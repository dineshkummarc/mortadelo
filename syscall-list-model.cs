using System;
using System.Diagnostics;
using GLib;
using Gtk;

namespace Mortadelo {
	public class SyscallListModel : ListStore {
		public SyscallListModel (ILogProvider log) : base (typeof (int))
		{
			if (log == null)
				throw new ArgumentNullException ("log");

			this.log = log;

			log.SyscallInserted += log_syscall_inserted_cb;
			log.SyscallRemoved += log_syscall_removed_cb;
			log.SyscallModified += log_syscall_modified_cb;

			populate ();
		}

		void populate ()
		{
			int new_num_rows = log.GetNumSyscalls ();
			int i;

			for (i = 0; i < new_num_rows; i++)
				log_syscall_inserted_cb (i);
		}

		void log_syscall_inserted_cb (int num)
		{
			TreeIter iter;

			iter = Insert (num);
			SetValue (iter, 0, 0);
		}

		void log_syscall_removed_cb (int num)
		{
			TreePath path;
			TreeIter iter;

			path = new TreePath (new int[] { num });

			if (!GetIter (out iter, path))
				Debug.Assert (false, "Get an iter in the list model to remove it");

			if (!Remove (ref iter))
				Debug.Assert (false, "Remove an iter");
		}

		void log_syscall_modified_cb (int num)
		{
			TreePath path;
			TreeIter iter;

			path = new TreePath (new int[] { num });

			if (!GetIter (out iter, path))
				Debug.Assert (false, "Get an iter in the list model to modify it");

			SetValue (iter, 0, 0);
		}

		ILogProvider log;
	}
}
