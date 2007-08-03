/*
 * Mortadelo - a viewer for system calls
 *
 * plain-formatter.cs - Trivially formats a Syscall for display
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

using System.Diagnostics;

namespace Mortadelo {
	public class PlainFormatter : ISyscallFormatter {
		public PlainFormatter ()
		{
		}

		public string Format (int syscall_index, Syscall syscall, SyscallVisibleField field)
		{
			switch (field) {
			case SyscallVisibleField.None:
				return null;

			case SyscallVisibleField.Process:
				return Util.FormatProcess (syscall.pid, syscall.tid, syscall.execname);

			case SyscallVisibleField.Timestamp:
				return Util.FormatTimestamp (syscall.timestamp);

			case SyscallVisibleField.Name:
				return syscall.name;

			case SyscallVisibleField.Arguments:
				return syscall.arguments;

			case SyscallVisibleField.ExtraInfo:
				return syscall.extra_info;

			case SyscallVisibleField.Result:
				return Util.FormatResult (syscall.have_result, syscall.result);

			default:
				Debug.Assert (false, "Not reached");
				return null;
			}
		}

		public bool UseMarkup ()
		{
			return false;
		}
	}
}
