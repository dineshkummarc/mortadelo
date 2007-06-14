using System;
using System.Diagnostics;
using System.IO;
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

			build_tree_view ();
			vbox.PackStart (scrolled_window, true, true, 0);

			build_statusbar ();
			vbox.PackStart (statusbar, false, false, 0);

			vbox.ShowAll ();

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
				      "   </menubar>\n" +
				      "   <toolbar name='toolbar'>\n" +
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

		ActionEntry[] build_normal_action_entries ()
		{
			ActionEntry[] entries = {
				new ActionEntry ("file-menu",
						 null,
						 Mono.Unix.Catalog.GetString ("_File"),
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
				new ActionEntry ("quit",
						 Stock.Quit,
						 Mono.Unix.Catalog.GetString ("_Quit"),
						 "<control>Q",
						 Mono.Unix.Catalog.GetString ("Exit the program"),
						 quit_cb),
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
			if (record_mode == RecordMode.Recording)
				stop_recording ();

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
			switch (view_mode) {
			case ViewMode.Compact:
				Debug.Assert (compact_log != null);
				return compact_log;

			case ViewMode.Full:
				return full_log;
			}

			Debug.Assert (false, "not reached");
			return null;
		}

		ILogProvider create_derived_log ()
		{
			switch (view_mode) {
			case ViewMode.Compact:
				compact_log = new CompactLog (full_log);
				return compact_log;

			case ViewMode.Full:
				compact_log = null;
				return full_log;
			}

			Debug.Assert (false, "not reached");
			return null;
		}

		void set_derived_model ()
		{
			ILogProvider derived;

			derived = create_derived_log ();

			model = new SyscallListModel (derived);
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
			if (view_mode == ViewMode.Compact)
				compact_log.Update (); /* this is lame... should CompactLog update itself from
							* signals from the base Log?
							*/

			model.Update ();
			update_statusbar_with_syscall_count ();
			return true;
		}

		void update_statusbar_with_syscall_count ()
		{
			int full_num, derived_num;
			string str;

			full_num = full_log.GetNumSyscalls ();
			derived_num = get_derived_log ().GetNumSyscalls ();

			if (full_num == derived_num)
				str = String.Format ("Total system calls: {0}", full_num);
			else
				str = String.Format ("Displayed system calls: {0}    Total system calls: {1}", derived_num, full_num);

			PopStatus ("info");
			PushStatus ("info", str);
		}

		public void PushStatus (string context, string str)
		{
			uint id;

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

		MenuBar menubar;
		Toolbar toolbar;
		Statusbar statusbar;

		ScrolledWindow scrolled_window;
		SyscallTreeView tree_view;

		Log full_log;
		CompactLog compact_log;

		SystemtapRunner runner;
		SyscallListModel model;

		uint update_timeout_id;

		bool setting_record_mode;
	}
}
