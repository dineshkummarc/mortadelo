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

		public void SetModelAndLog (SyscallListModel model, ILogProvider log)
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
			append_text_column (Mono.Unix.Catalog.GetString ("Result"),	Columns.Result, false);
			append_text_column (Mono.Unix.Catalog.GetString ("Arguments"),	Columns.Arguments, true);
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

		ILogProvider log;
	}
}
