using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Gtk;
using Mono.Unix;

namespace Mortadelo {
	public class MainWindow : Window {

		const ViewMode DEFAULT_VIEW_MODE = ViewMode.Compact;

		public MainWindow () : base (WindowType.Toplevel)
		{
			Title = Mono.Unix.Catalog.GetString ("Viewer for system calls");

			view_mode = DEFAULT_VIEW_MODE;
			full_log = null;
			compact_log = null;

			statusbar_has_transient_message = false;
			statusbar_transient_timeout_id = 0;

			build_window ();

			set_record_mode (RecordMode.Recording);
		}

		void build_window ()
		{
			VBox vbox;

			build_action_group ();
			build_ui_manager ();

			vbox = new VBox (false, 0);
			Add (vbox);

			build_menubar ();
			vbox.PackStart (menubar, false, false, 0);

			build_toolbar ();
			vbox.PackStart (toolbar, false, false, 0);

			build_filter_box ();
			vbox.PackStart (filter_box, false, false, 0);

			build_tree_view ();
			vbox.PackStart (scrolled_window, true, true, 0);

			build_statusbar ();
			vbox.PackStart (statusbar, false, false, 0);

			filter_entry.GrabFocus ();

			vbox.ShowAll ();
			filter_error_label.Hide ();
		}

		void build_action_group ()
		{
			ActionEntry [] normal_entries;
			RadioActionEntry [] record_entries;
			RadioActionEntry [] view_entries;

			normal_entries = build_normal_action_entries ();
			record_entries = build_record_action_entries ();
			view_entries = build_view_action_entries ();

			action_group = new ActionGroup ("actions");
			/* FIXME: action_group.TranslationDomain = "mortadelo"; */
			/* FIXME: action_group.TranslateFunc = translate_action_cb; */

			action_group.Add (normal_entries);
			action_group.Add (record_entries, (int) RecordMode.Recording, record_mode_changed_cb);
			action_group.Add (view_entries, (int) DEFAULT_VIEW_MODE, view_mode_changed_cb);

			fix_action_short_names ();
		}

		void build_ui_manager ()
		{
			string xml = ("<ui>\n" +
				      "  <menubar name='main-menu'>\n" +
				      "     <menu action='file-menu'>\n" +
				      "       <menuitem action='open'/>\n" +
				      "       <menuitem action='save-as'/>\n" +
				      "       <separator/>\n" +
				      "       <menuitem action='quit'/>\n" +
				      "     </menu>\n" +
				      "     <menu action='help-menu'>\n" +
				      "       <menuitem action='about'/>\n" +
				      "     </menu>\n" +
				      "   </menubar>\n" +
				      "   <toolbar name='toolbar'>\n" +
				      "     <toolitem action='next-error'/>\n" +
				      "     <toolitem action='clear'/>\n" +
				      "     <separator/>\n" +
				      "     <toolitem action='record-stop'/>\n" +
				      "     <toolitem action='record-record'/>\n" +
				      "     <separator/>\n" +
				      "     <toolitem action='view-compact'/>\n" +
				      "     <toolitem action='view-full'/>\n" +
				      "   </toolbar>\n" +
				      "</ui>\n");

			ui_manager = new UIManager ();

			ui_manager.InsertActionGroup (action_group, 0);

			AccelGroup accel_group = ui_manager.AccelGroup;
			AddAccelGroup (accel_group);

			ui_manager.AddUiFromString (xml); /* FIXME: this could throw an exception */
		}

		void build_menubar ()
		{
			menubar = ui_manager.GetWidget ("/main-menu") as MenuBar;
		}

		void build_toolbar ()
		{
			toolbar = ui_manager.GetWidget ("/toolbar") as Toolbar;
		}

		void build_filter_box ()
		{
			HBox hbox;
			Label label;

			filter_box = new VBox (false, 12);

			hbox = new HBox (false, 12);
			filter_box.PackStart (hbox, false, false, 0);

			label = new Label ("Filter:");
			hbox.PackStart (label, false, false, 0);

			filter_entry = new Entry ();
			hbox.PackStart (filter_entry, true, true, 0);

			label.MnemonicWidget = filter_entry;

			filter_throttle = new TimerThrottle (FILTER_THROTTLE_MSEC);
			filter_throttle.Trigger += filter_throttle_trigger_cb;

			filter_entry.Changed += delegate (object o, EventArgs args) {
				filter_throttle.Start ();
			};

			filter_error_label = new Label ();
			filter_error_label.Xalign = 0.0f;
			filter_box.PackStart (filter_error_label, false, false, 0);
		}

		void filter_throttle_trigger_cb ()
		{
			string text;

			text = filter_entry.Text;

			if (text == "") {
				filter_mode = FilterMode.Unfiltered;
				filter_error_label.Hide ();
			} else
				filter_mode = FilterMode.Filtered;

			set_derived_model ();
		}

		void build_tree_view ()
		{
			scrolled_window = new ScrolledWindow (null, null);
			scrolled_window.ShadowType = ShadowType.EtchedIn;
			scrolled_window.HscrollbarPolicy = PolicyType.Automatic;
			scrolled_window.VscrollbarPolicy = PolicyType.Always;

			tree_view = new SyscallTreeView ();
			scrolled_window.Add (tree_view);
		}

		void build_statusbar ()
		{
			statusbar = new Statusbar ();
			statusbar.HasResizeGrip = true;
		}

		enum RecordMode {
			Stopped,
			Recording
		}

		enum ViewMode {
			Compact,
			Full
		}

		enum FilterMode {
			Unfiltered,
			Filtered
		}

		ActionEntry[] build_normal_action_entries ()
		{
			ActionEntry[] entries = {
				new ActionEntry ("file-menu",
						 null,
						 Mono.Unix.Catalog.GetString ("_File"),
						 null,
						 null,
						 null),
				new ActionEntry ("help-menu",
						 null,
						 Mono.Unix.Catalog.GetString ("_Help"),
						 null,
						 null,
						 null),
				new ActionEntry ("open",
						 Stock.Open,
						 Mono.Unix.Catalog.GetString ("_Open..."),
						 "<control>O",
						 Mono.Unix.Catalog.GetString ("Load a log of system calls from a file"),
						 open_cb),
				new ActionEntry ("save-as",
						 Stock.SaveAs,
						 Mono.Unix.Catalog.GetString ("Save _As..."),
						 "<control><shift>S",
						 Mono.Unix.Catalog.GetString ("Save the displayed log to a file"),
						 save_as_cb),
				new ActionEntry ("clear",
						 Stock.Clear,
						 Mono.Unix.Catalog.GetString ("C_lear"),
						 "<control>L",
						 Mono.Unix.Catalog.GetString ("Clear the current log"),
						 clear_cb),
				new ActionEntry ("next-error",
						 Stock.JumpTo,
						 Mono.Unix.Catalog.GetString ("_Next Error"),
						 "<control>N",
						 Mono.Unix.Catalog.GetString ("Jumps to the next system call that returned an error"),
						 next_error_cb),
				new ActionEntry ("quit",
						 Stock.Quit,
						 Mono.Unix.Catalog.GetString ("_Quit"),
						 "<control>Q",
						 Mono.Unix.Catalog.GetString ("Exit the program"),
						 quit_cb),
				new ActionEntry ("about",
						 Stock.About,
						 Mono.Unix.Catalog.GetString ("_About Mortadelo"),
						 null,
						 Mono.Unix.Catalog.GetString ("Show version information about this program"),
						 help_about_cb)
			};

			return entries;
		}

		RadioActionEntry[] build_record_action_entries ()
		{
			RadioActionEntry[] entries = {
				new RadioActionEntry ("record-stop",
						      Stock.MediaStop,
						      Mono.Unix.Catalog.GetString ("S_top Recording"),
						      "<control>T",
						      Mono.Unix.Catalog.GetString ("Stop recording system calls"),
						      (int) RecordMode.Stopped),
				new RadioActionEntry ("record-record",
						      Stock.MediaRecord,
						      Mono.Unix.Catalog.GetString ("Start _Recording"),
						      "<control>R",
						      Mono.Unix.Catalog.GetString ("Start recording system calls"),
						      (int) RecordMode.Recording)
			};

			return entries;
		}

		RadioActionEntry[] build_view_action_entries ()
		{
			RadioActionEntry[] entries = {
				new RadioActionEntry ("view-compact",
						      Stock.ZoomOut,
						      Mono.Unix.Catalog.GetString ("_Compact"),
						      "<control>1",
						      Mono.Unix.Catalog.GetString ("Compact view (start/end of system call in the same line)"),
						      (int) ViewMode.Compact),
				new RadioActionEntry ("view-full",
						      Stock.Zoom100,
						      Mono.Unix.Catalog.GetString ("_Full"),
						      "<control>2",
						      Mono.Unix.Catalog.GetString ("Full view (start/end of system call in separate lines"),
						      (int) ViewMode.Full)
			};

			return entries;
		}

		void fix_action_short_names ()
		{
			Action a;

			a = action_group.GetAction ("record-stop");
			a.ShortLabel = Mono.Unix.Catalog.GetString ("Stop");

			a = action_group.GetAction ("record-record");
			a.ShortLabel = Mono.Unix.Catalog.GetString ("Record");
		}

		void open_cb (object o, EventArgs args)
		{
			FileChooserDialog chooser;

			chooser = new FileChooserDialog (Mono.Unix.Catalog.GetString ("Open a log of system calls"),
							 this,
							 FileChooserAction.Open);

			chooser.AddButton (Stock.Cancel, ResponseType.Cancel);
			chooser.AddButton (Stock.Open, ResponseType.Accept);
			chooser.DefaultResponse = ResponseType.Accept;

			if (chooser.Run () == (int) ResponseType.Accept)
				do_open (chooser.Filename);

			chooser.Destroy ();
		}

		void do_open (string filename)
		{
			LogIO io;
			StreamReader reader;
			Log new_log;

			try {
				io = new LogIO ();
				reader = new StreamReader (filename);
				new_log = io.Load (reader, new SystemtapParser ());
				reader.Close ();
			} catch (Exception e) {
				Console.WriteLine ("exception while loading: {0}", e);
				/* FIXME */
				return;
			}

			set_record_mode (RecordMode.Stopped);

			full_log = new_log;

			set_derived_model ();
		}

		void save_as_cb (object o, EventArgs args)
		{
			FileChooserDialog chooser;

			chooser = new FileChooserDialog (Mono.Unix.Catalog.GetString ("Save log of system calls"),
							 this,
							 FileChooserAction.Save);

			chooser.AddButton (Stock.Cancel, ResponseType.Cancel);
			chooser.AddButton (Stock.Save, ResponseType.Accept);
			chooser.DefaultResponse = ResponseType.Accept;

			if (chooser.Run () == (int) ResponseType.Accept)
				do_save_as (chooser.Filename);

			chooser.Destroy ();
		}

		void do_save_as (string filename)
		{
			LogIO io;
			StreamWriter writer;

			io = new LogIO ();

			try {
				writer = new StreamWriter (filename);
				io.Save (writer, full_log, new SystemtapSerializer ());
				writer.Close ();
			} catch (Exception e) {
				Console.WriteLine ("exception while saving: {0}", e);
				/* FIXME */
			}
		}

		void clear_cb (object o, EventArgs args)
		{
			RecordMode old_mode;

			old_mode = record_mode;
			set_record_mode (RecordMode.Stopped);

			full_log = new Log ();
			set_derived_model ();

			set_record_mode (old_mode);
		}

		void quit_cb (object o, EventArgs args)
		{
			do_quit ();
		}

		void do_quit ()
		{
			filter_throttle.Stop ();

			if (record_mode == RecordMode.Recording)
				stop_recording ();

			if (statusbar_has_transient_message) {
				Debug.Assert (statusbar_transient_timeout_id != 0);
				GLib.Source.Remove (statusbar_transient_timeout_id);
				statusbar_transient_timeout_id = 0;
			}

			Application.Quit ();
		}

		void record_mode_changed_cb (object o, ChangedArgs args)
		{
			RadioAction current = args.Current;

			if (setting_record_mode)
				return;

			set_record_mode ((RecordMode) current.Value);
		}

		void view_mode_changed_cb (object o, ChangedArgs args)
		{
			RadioAction current = args.Current;

			set_view_mode ((ViewMode) current.Value);
		}

		ILogProvider get_derived_log ()
		{
			ILogProvider sublog;

			switch (view_mode) {
			case ViewMode.Compact:
				Debug.Assert (compact_log != null);
				sublog = compact_log;
				break;

			case ViewMode.Full:
				sublog = full_log;
				break;

			default:
				Debug.Assert (false, "not reached");
				sublog = null;
				break;
			}

			switch (filter_mode) {
			case FilterMode.Unfiltered:
				return sublog;

			case FilterMode.Filtered:
				return filtered_log;
			}

			Debug.Assert (false, "not reached");
			return null;
		}

		ILogProvider create_derived_log ()
		{
			ILogProvider sublog;

			switch (view_mode) {
			case ViewMode.Compact:
				compact_log = new CompactLog (full_log);
				sublog = compact_log;
				break;

			case ViewMode.Full:
				compact_log = null;
				sublog = full_log;
				break;

			default:
				Debug.Assert (false, "not reached");
				sublog = null;
				break;
			}

			switch (filter_mode) {
			case FilterMode.Unfiltered:
				filtered_log = null;
				return sublog;

			case FilterMode.Filtered:
				/* FIXME: what if compiling the regex fails? */
				Regex regex = make_regex (filter_entry.Text);

				if (regex != null)
					filtered_log = new FilteredLog (sublog, new RegexFilter (new RegexCache (regex)));
				else
					filtered_log = null;

				return filtered_log;
			}

			Debug.Assert (false, "not reached");
			return null;
		}

		Regex make_regex (string text)
		{
			Regex regex;

			regex = null;

			try {
				regex = Util.MakeRegex (text);
				filter_error_label.Hide ();
			} catch (Exception e) {
				filter_error_label.Text = e.Message;
				filter_error_label.Show ();
			}

			return regex;
		}

		void set_derived_model ()
		{
			ILogProvider derived;

			derived = create_derived_log ();
			if (derived != null) {
				model = new SyscallListModel (derived);

				if (filter_mode == FilterMode.Filtered)
					tree_view.SetFormatter (new FilterFormatter (filtered_log));
				else
					tree_view.SetFormatter (null);

			} else
				model = null;

			tree_view.SetModelAndLog (model, derived);

			update_statusbar_with_syscall_count ();
		}

		void set_record_mode (RecordMode mode)
		{
			Action action;

			if (mode == record_mode)
				return;

			switch (mode) {
			case RecordMode.Stopped:
				stop_recording ();
				action = action_group.GetAction ("record-stop");
				break;

			case RecordMode.Recording:
				start_recording ();
				action = action_group.GetAction ("record-record");
				break;

			default:
				Debug.Assert (false, "not reached");
				action = null;
				break;
			}

			setting_record_mode = true;
			action.Activate ();
			setting_record_mode = false;
		}

		void start_recording ()
		{
			Debug.Assert (record_mode == RecordMode.Stopped, "must be stopped");

			if (full_log == null)
				full_log = new Log ();

			runner = new SystemtapRunner (full_log);
			runner.ChildExited += runner_child_exited_cb;

			runner.Run (); /* FIXME: catch exceptions? */

			set_derived_model ();

			update_timeout_id = GLib.Timeout.Add (1000, update_timeout_cb);

			record_mode = RecordMode.Recording;
		}

		void runner_child_exited_cb (int status)
		{
			/* FIXME: check WIFEXITED(), WEXITSTATUS, etc. */
			/* FIXME: need to add a stderr capturer in AggregatorRunner / SystemtapRunner */
		}

		void stop_recording ()
		{
			Debug.Assert (record_mode == RecordMode.Recording, "must be recording");

			runner.Stop ();
			GLib.Source.Remove (update_timeout_id);
			update_timeout_id = 0;

			record_mode = RecordMode.Stopped;
		}

		void set_view_mode (ViewMode mode)
		{
			if (mode == view_mode)
				return;

			view_mode = mode;

			set_derived_model ();
		}

		bool update_timeout_cb ()
		{
			update_statusbar_with_syscall_count ();
			return true;
		}

		void update_statusbar_with_syscall_count ()
		{
			ILogProvider derived;
			int full_num, derived_num;
			string str;

			full_num = full_log.GetNumSyscalls ();

			derived = get_derived_log ();
			if (derived == null)
				derived_num = 0;
			else
				derived_num = derived.GetNumSyscalls ();

			if (full_num == derived_num)
				str = String.Format ("Total system calls: {0}", full_num);
			else
				str = String.Format ("Displayed system calls: {0}    Total system calls: {1}", derived_num, full_num);

			PopStatus ("info");
			PushStatus ("info", str);
		}

		void help_about_cb (object o, EventArgs a)
		{
			if (about_dialog == null) {
				about_dialog = new AboutDialog ();

				about_dialog.Authors = new string[] { "Federico Mena-Quintero <federico@novell.com>" };
				about_dialog.Comments = Mono.Unix.Catalog.GetString (
					"This program lets you view the system calls from all processes in the system.");
				about_dialog.Copyright = Mono.Unix.Catalog.GetString (
					"Copyright (C) 2007 Federico Mena-Quintero");
				about_dialog.License = "GPL"; /* FIXME: include the text of the GPL */
				about_dialog.Name = "Mortadelo";
				about_dialog.Version = Util.Version;

				about_dialog.TransientFor = this;

				about_dialog.DeleteEvent += delegate (object obj, DeleteEventArgs args) {
					about_dialog.Hide ();
					args.RetVal = true;
				};
			}

			about_dialog.PresentWithTime (Gtk.Global.CurrentEventTime);
		}

		void next_error_cb (object o, EventArgs args)
		{
			int error_idx;

			error_idx = find_next_error_idx ();

			if (error_idx == -1)
				warn_about_no_next_error ();
			else
				select_index (error_idx);
		}

		
		int find_next_error_idx ()
		{
			int selected;

			selected = get_selected_index ();
			return look_for_error_starting_at (selected + 1);
		}

		int get_selected_index ()
		{
			TreeModel model;
			TreeIter iter;
			int selected_idx;

			if (tree_view.Selection.GetSelected (out model, out iter)) {
				TreePath path;

				path = model.GetPath (iter);
				Debug.Assert (path != null);

				selected_idx = path.Indices[0];
			} else
				selected_idx = -1;

			return selected_idx;
		}

		int look_for_error_starting_at (int idx)
		{
			ILogProvider derived;
			int num;

			derived = get_derived_log ();
			num = derived.GetNumSyscalls ();

			for (; idx < num; idx++) {
				Syscall syscall;

				syscall = derived.GetSyscall (idx);
				if (syscall.have_result && syscall.result < 0)
					return idx;
			}

			return -1;
		}

		void warn_about_no_next_error ()
		{
			this.Display.Beep (); /* wheeee! */

			PushTransientStatus (Mono.Unix.Catalog.GetString ("No further errors"));
		}

		void select_index (int idx)
		{
			TreePath path = new TreePath (new int[] { idx });
			TreePath first, last;
			bool use_align;
			float row_align, col_align;

			tree_view.Selection.SelectPath (path);

			if (tree_view.GetVisibleRange (out first, out last)
			    && (path.Compare (first) == -1 || path.Compare (last) == 1)) {
				use_align = true;
				row_align = 0.5f;
				col_align = 0.0f;
			} else {
				use_align = false;
				row_align = 0.0f;
				col_align = 0.0f;
			}

			tree_view.ScrollToCell (path, null, use_align, row_align, col_align);
		}

		public void PushTransientStatus (string str)
		{
			if (statusbar_has_transient_message) {
				Debug.Assert (statusbar_transient_timeout_id != 0);
				PopStatus ("transient");
				GLib.Source.Remove (statusbar_transient_timeout_id);
				statusbar_transient_timeout_id = 0;
				statusbar_has_transient_message = false;
			} else
				Debug.Assert (statusbar_transient_timeout_id == 0);

			PushStatus ("transient", str);

			statusbar_has_transient_message = true;
			statusbar_transient_timeout_id = GLib.Timeout.Add (STATUSBAR_TRANSIENT_DURATION_MSEC,
									   statusbar_transient_timeout_cb);
		}

		bool statusbar_transient_timeout_cb ()
		{
			PopStatus ("transient");
			statusbar_transient_timeout_id = 0;
			statusbar_has_transient_message = false;

			update_statusbar_with_syscall_count ();
			return false;
		}

		public void PushStatus (string context, string str)
		{
			uint id;

			if (statusbar_has_transient_message)
				return;

			id = statusbar.GetContextId (context);
			statusbar.Push (id, str);
		}

		public void PopStatus (string context)
		{
			uint id;

			id = statusbar.GetContextId (context);
			statusbar.Pop (id);
		}

		protected override bool OnDeleteEvent (Gdk.Event ev)
		{
			do_quit ();
			return true;
		}

		ActionGroup action_group;
		UIManager ui_manager;

		RecordMode record_mode;
		ViewMode view_mode;
		FilterMode filter_mode;

		MenuBar menubar;
		Toolbar toolbar;
		Statusbar statusbar;
		Box filter_box;
		Entry filter_entry;
		Label filter_error_label;

		ScrolledWindow scrolled_window;
		SyscallTreeView tree_view;

		AboutDialog about_dialog;

		const int FILTER_THROTTLE_MSEC = 300;
		TimerThrottle filter_throttle;

		bool statusbar_has_transient_message;
		uint statusbar_transient_timeout_id;
		const int STATUSBAR_TRANSIENT_DURATION_MSEC = 2000;

		Log full_log;
		CompactLog compact_log;
		FilteredLog filtered_log;

		SystemtapRunner runner;
		SyscallListModel model;

		uint update_timeout_id;

		bool setting_record_mode;
	}
}
