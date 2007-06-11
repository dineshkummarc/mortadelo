using System;
using Gtk;
using Mono.Unix;

namespace Mortadelo {
	public class MainWindow : Window {
		public MainWindow () : base (WindowType.Toplevel)
		{
			Title = Mono.Unix.Catalog.GetString ("Viewer for system calls");

			build_window ();
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
			action_group.Add (record_entries, (int) RecordMode.Stopped, record_mode_changed_cb);
			action_group.Add (view_entries, (int) ViewMode.Full, view_mode_changed_cb);

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
						 null,
						 Mono.Unix.Catalog.GetString ("Load a log of system calls from a file"),
						 open_cb),
				new ActionEntry ("save-as",
						 Stock.SaveAs,
						 Mono.Unix.Catalog.GetString ("Save _As..."),
						 null,
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
						 null,
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
			/* FIXME */
		}

		void save_as_cb (object o, EventArgs args)
		{
			/* FIXME */
		}

		void clear_cb (object o, EventArgs args)
		{
			/* FIXME */
		}

		void quit_cb (object o, EventArgs args)
		{
			/* FIXME: stop logging, then quit */
		}

		void record_mode_changed_cb (object o, ChangedArgs args)
		{
			RadioAction current = args.Current;

			set_record_mode ((RecordMode) current.Value);
		}

		void view_mode_changed_cb (object o, ChangedArgs args)
		{
			RadioAction current = args.Current;

			set_view_mode ((ViewMode) current.Value);
		}

		void set_record_mode (RecordMode mode)
		{
			/* FIXME */
		}

		void set_view_mode (ViewMode mode)
		{
			/* FIXME */
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
	}

	public class Driver {
		public static void Main ()
		{
			MainWindow window;

			Application.Init ();

			window = new MainWindow ();
			window.ShowAll ();

			Application.Run ();
		}
	}
}
