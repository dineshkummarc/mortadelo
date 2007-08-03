/*
 * Mortadelo - a viewer for system calls
 *
 * log-provider.cs - Abstraction for a syscall log
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

namespace Mortadelo {

	public delegate void SyscallModifiedHandler (int num);
	public delegate void SyscallRemovedHandler (int num);
	public delegate void SyscallInsertedHandler (int num);

	public interface ILogProvider {
		/* Gets the number of system calls that this log stores */
		int GetNumSyscalls ();

		/* Gets a syscall by its index within the log.  Syscall.index is not necessarily equal to num */
		Syscall GetSyscall (int num);

		/* Gets the index within the log which corresponds to the syscall which has syscall.index == num,
		 * or returns -1 if the log doesn't contain the sought syscall.
		 *
		 * For filtered logs, this maps the base index to the filtered
		 * index.
		 */
		int GetSyscallByBaseIndex (int base_index);

		/* Events emitted when syscalls change */
		event SyscallModifiedHandler SyscallModified;
		event SyscallRemovedHandler SyscallRemoved;
		event SyscallInsertedHandler SyscallInserted;
	}

}
