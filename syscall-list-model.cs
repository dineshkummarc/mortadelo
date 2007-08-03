/*
 * Mortadelo - a viewer for system calls
 *
 * syscall-list-model.cs - ListStore derivative to glue a log of syscalls to a TreeView
 *
 * Copyright (C) 2007 Federico Mena-Quintero
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 *
 * Authors: Federico Mena Quintero <federico@novell.com>
 */

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
