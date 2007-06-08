using System;
using System.Diagnostics;
using GLib;
using Gtk;

namespace Mortadelo {
	public class SyscallListModel : ListStore {
		public SyscallListModel (Log log) : base (typeof (int))
		{
			if (log == null)
				throw new ArgumentNullException ("log");

			this.log = log;
			num_updated_rows = 0;

			update_new_rows ();
		}

		public void Update ()
		{
			update_new_rows ();
			update_changed_rows ();
		}

		void update_new_rows ()
		{
			int old_num_updated_rows = num_updated_rows;
			int new_num_rows = log.GetNumSyscalls ();
			int i;

			if (new_num_rows == num_updated_rows)
				return;

			old_num_updated_rows = num_updated_rows;
			num_updated_rows = new_num_rows;

			Debug.Assert (new_num_rows > old_num_updated_rows, "Number of rows in sync");

			for (i = old_num_updated_rows; i < new_num_rows; i++)
				append_row_and_emit (i);
		}

		void update_changed_rows ()
		{
			int[] modified;
			int i;

			modified = log.GetModifiedIndexes ();

			for (i = 0; i < modified.Length; i++)
				emit_row_changed (modified[i]);
		}

		void append_row_and_emit (int n)
		{
			AppendValues (n);
		}

		void emit_row_changed (int n)
		{
			TreePath path;
			TreeIter iter;

			path = new TreePath (new int[] { n });

			if (!GetIter (out iter, path))
				Debug.Assert (false, "Get an iter in the list model to change its values");

			SetValue (iter, 0, n);
		}

		Log log;
		int num_updated_rows;
	}
}
