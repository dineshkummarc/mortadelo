/*
 * Mortadelo - a viewer for system calls
 *
 * systemtap-serializer.cs - Takes a syscall and serializes it to systemtap-like output
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
using System.IO;

namespace Mortadelo {

	public class SystemtapSerializer : ISyscallSerializer {
		public SystemtapSerializer ()
		{
		}

		public void Serialize (TextWriter writer, Syscall syscall)
		{
			if (syscall.name == "open" && syscall.is_syscall_start) {
				writer.Write ("start.open: {0}: {1} ({2}:{3}): {4}\n",
					      syscall.timestamp,
					      syscall.execname,
					      syscall.pid,
					      syscall.tid,
					      syscall.arguments);
			}

			if (syscall.name == "open" && syscall.is_syscall_end) {
				writer.Write ("return.open: {0}: {1} ({2}:{3}): {4}\n",
					      syscall.timestamp,
					      syscall.execname,
					      syscall.pid,
					      syscall.tid,
					      syscall.result);
			}
		}
	}

}
