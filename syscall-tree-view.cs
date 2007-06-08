using System;
using System.Diagnostics;
using Gtk;
using Mono.Unix;

namespace Mortadelo {
	public class SyscallTreeView : TreeView {
		public SyscallTreeView () : base ()
		{
			setup_columns ();
		}

		void SetModelAndLog (SyscallListModel model, Log log)
		{
			this.Model = model;
			this.log = log;
		}

		void setup_columns ()
		{
			append_text_column (Mono.Unix.Catalog.GetString ("#"),		Columns.Index, false);
			append_text_column (Mono.Unix.Catalog.GetString ("Time"),	Columns.Timestamp, false);
			append_text_column (Mono.Unix.Catalog.GetString ("Process"),	Columns.Process, true);
			append_text_column (Mono.Unix.Catalog.GetString ("Syscall"),	Columns.SyscallName, false);
			append_text_column (Mono.Unix.Catalog.GetString ("Arguments"),	Columns.Arguments, true);
			append_text_column (Mono.Unix.Catalog.GetString ("Result"),	Columns.Result, true);
		}

		void append_text_column (string title, Columns id, bool resizable)
		{
			TreeViewColumn tree_col;

			tree_col = AppendColumn (title, new CellRendererText (),
						 delegate (TreeViewColumn column, CellRenderer renderer, TreeModel model,
							   TreeIter iter)
						 {
							 data_func (column, renderer, model, iter, id);
						 });

			tree_col.Resizable = resizable;
		}

		void data_func (TreeViewColumn column, CellRenderer renderer, TreeModel model, TreeIter iter, Columns id)
		{
			CellRendererText text_renderer = renderer as CellRendererText;
			int syscall_index;
			Syscall syscall;
			string text;

			syscall_index = (int) model.GetValue (iter, 0);
			syscall = log.GetSyscall (syscall_index);

			switch (id) {
			case Columns.Index:
				text = String.Format ("{0}", syscall.index);
				Debug.Assert (syscall_index == syscall.index, "Index of syscalls matches");
				break;

			case Columns.Timestamp:
				text = String.Format ("{0}", syscall.timestamp);
				break;

			case Columns.Process:
				text = String.Format ("{0}:{1}{2}{3}",
						      syscall.execname,
						      syscall.pid,
						      (syscall.pid == syscall.tid) ? "" : ":",
						      (syscall.pid == syscall.tid) ? "" : syscall.tid.ToString ());
				break;

			case Columns.SyscallName:
				text = syscall.name;
				break;

			case Columns.Arguments:
				text = syscall.arguments;
				break;

			case Columns.Result:
				if (syscall.have_result) {
					if (syscall.result < 0) {
						string name, description;
						if (Errno.GetErrno (-syscall.result, out name, out description))
							text = name;
						else {
							/* unknown errno code */
							text = Mono.Unix.Catalog.GetString ("UNKNOWN");
						}
					} else
						text = String.Format ("{0}", syscall.result);
				} else
					text = "?";

				break;

			default:
				Debug.Assert (false, "not reached");
				text = null;
				break;
			}

			text_renderer.Text = text;
			text_renderer.Foreground = (syscall.have_result && syscall.result < 0) ? "#ff0000" : "#000000";
		}

		enum Columns {
			Index,
			Timestamp,
			Process,
			SyscallName,
			Arguments,
			Result
		}

		Log log;

		public static void Main () {
			SystemtapRunner runner;
			Log log;

			Application.Init ();

			log = new Log ();
			runner = new SystemtapRunner (log);

			runner.Run ();

			Window w;
			SyscallTreeView tree;
			ScrolledWindow sw;
			SyscallListModel model;

			w = new Window ("hola");

			sw = new ScrolledWindow (null, null);
			w.Add (sw);

			model = new SyscallListModel (log);
			tree = new SyscallTreeView ();
			tree.SetModelAndLog (model, log);
			sw.Add (tree);

			w.ShowAll ();

			GLib.Timeout.Add (1000, delegate {
				int num;

				num = log.GetNumSyscalls ();
				Console.WriteLine ("syscalls: {0}", num);
				model.Update ();
				return true;
			});

			Application.Run ();
		}
	}
}
