/*
 * Mortadelo - a viewer for system calls
 *
 * log.cs - Basic log of syscalls
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
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

namespace Mortadelo {
	public class Log : ILogProvider {
		List<Syscall> syscalls;

		public Log ()
		{
			syscalls = new List<Syscall> ();
		}

		public int GetNumSyscalls ()
		{
			return syscalls.Count;
		}

		public int AppendSyscall (Syscall syscall)
		{
			syscall.index = syscalls.Count;
			uniquify_strings (ref syscall);

			syscalls.Add (syscall);
			if (SyscallInserted != null)
				SyscallInserted (syscall.index);

			return syscall.index;
		}

		public Syscall GetSyscall (int num)
		{
			return syscalls[num];
		}

		public int GetSyscallByBaseIndex (int base_index)
		{
			if (base_index < 0 || base_index >= syscalls.Count)
				return -1;

			Debug.Assert (syscalls[base_index].index == base_index);
			return base_index; /* we are the full log, so our indices *are* the base indices */
		}

		public void ModifySyscall (int num, Syscall syscall)
		{
			syscall.index = num;
			uniquify_strings (ref syscall);

			syscalls[num] = syscall;

			if (SyscallModified != null)
				SyscallModified (num);
		}

		void uniquify_strings (ref Syscall syscall)
		{
			syscall.execname = unique_string (syscall.execname);
			syscall.name = unique_string (syscall.name);
			syscall.arguments = unique_string (syscall.arguments);
			syscall.extra_info = unique_string (syscall.extra_info);
		}

		string unique_string (string s)
		{
			return (s != null) ? String.Intern (s) : null;
		}

		public event SyscallModifiedHandler SyscallModified;
		public event SyscallRemovedHandler SyscallRemoved;
		public event SyscallInsertedHandler SyscallInserted;
	}
}
