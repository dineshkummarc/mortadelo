using System;
using System.Diagnostics;
using Gtk;
using Mono.Unix;

namespace Mortadelo {
	/* This is lame; CellRendererText does not have a binding for the
	 * CellBackgroundSet property, and SetProperty() is a protected member.
	 * So we need to hack our own class rather than being able to call the
	 * stupid method directly.
	 */
	public class TextRenderer : CellRendererText {
		public bool CellBackgroundSet {
			set {
				SetProperty ("cell-background-set", new GLib.Value (value));
			}
		}
	}

	public class SyscallTreeView : TreeView {
		public SyscallTreeView () : base ()
		{
			paired_row = -1;
			setup_columns ();
		}

		public void SetModelAndLog (SyscallListModel model, ILogProvider log)
		{
			paired_row = -1;

			this.log = log;
			this.Model = model;
		}

		public void SetFormatter (ISyscallFormatter formatter)
		{
			this.formatter = formatter;
		}

		public void SetPairedRow (int row)
		{
			int old_paired_row;

			old_paired_row = paired_row;
			paired_row = row;

			if (Model == null)
				return;

			if (old_paired_row != -1)
				emit_row_changed (old_paired_row);

			if (paired_row != -1)
				emit_row_changed (paired_row);
		}

		void emit_row_changed (int row)
		{
			TreePath path;
			TreeIter iter;

			path = new TreePath (new int[] { row });
			if (!Model.GetIter (out iter, path))
				Debug.Assert (false, "not reached");

			Model.EmitRowChanged (path, iter);
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

			tree_col = AppendColumn (title, new TextRenderer (),
						 delegate (TreeViewColumn column, CellRenderer renderer, TreeModel model,
							   TreeIter iter)
						 {
							 data_func (column, renderer, model, iter, id);
						 });

			tree_col.Resizable = resizable;
		}

		void ensure_formatter ()
		{
			if (formatter == null)
				formatter = new PlainFormatter ();
		}

		void data_func (TreeViewColumn column, CellRenderer renderer, TreeModel model, TreeIter iter, Columns id)
		{
			TextRenderer text_renderer = renderer as TextRenderer;
			int syscall_index;
			Syscall syscall;
			string text;
			TreePath path;

			ensure_formatter ();

			path = model.GetPath (iter);
			Debug.Assert (path != null, "Get a path from an iter");
			syscall_index = path.Indices[0];

			syscall = log.GetSyscall (syscall_index);

			switch (id) {
			case Columns.Index:
				text = String.Format ("{0}", syscall.index);
				Debug.Assert (syscall_index == syscall.index, "Index of syscalls matches");
				break;

			case Columns.Timestamp:
				text = formatter.Format (syscall_index, syscall, SyscallVisibleField.Timestamp);
				break;

			case Columns.Process:
				text = formatter.Format (syscall_index, syscall, SyscallVisibleField.Process);
				break;

			case Columns.SyscallName:
				text = formatter.Format (syscall_index, syscall, SyscallVisibleField.Name);
				break;

			case Columns.Arguments:
				text = formatter.Format (syscall_index, syscall, SyscallVisibleField.Arguments);
				break;

			case Columns.Result:
				text = formatter.Format (syscall_index, syscall, SyscallVisibleField.Result);
				break;

			default:
				Debug.Assert (false, "not reached");
				text = null;
				break;
			}

			if (formatter.UseMarkup ())
				text_renderer.Markup = text;
			else
				text_renderer.Text = text;

			text_renderer.Foreground = (syscall.have_result && syscall.result < 0) ? "#ff0000" : "#000000";

			if (syscall_index == paired_row) {
				text_renderer.CellBackgroundGdk = get_paired_gdkcolor ();
				text_renderer.CellBackgroundSet = true;
			} else
				text_renderer.CellBackgroundSet = false;
		}

		Gdk.Color get_paired_gdkcolor ()
		{
			int r, g, b;
			Gdk.Color x, y;

			x = Style.BaseColors[(int) StateType.Normal];
			y = Style.BaseColors[(int) StateType.Selected];

			r = (x.Red   + y.Red) / 2;
			g = (x.Green + y.Green) / 2;
			b = (x.Blue  + y.Blue) / 2;

			return new Gdk.Color ((byte) (r >> 8), (byte) (g >> 8), (byte) (b >> 8));
		}

		enum Columns {
			Index,
			Timestamp,
			Process,
			SyscallName,
			Arguments,
			Result
		}

		ISyscallFormatter formatter;
		ILogProvider log;

		int paired_row;
	}
}
