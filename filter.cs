/*
 * Mortadelo - a viewer for system calls
 *
 * filter.cs - Abstraction to filter syscalls based on certain criteria
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

namespace Mortadelo {
	public enum SyscallVisibleField {
		None,
		Process,
		Timestamp,
		Name,
		Arguments,
		ExtraInfo,
		Result
	}

	public struct SyscallMatch {
		public SyscallVisibleField field;
		public int start_pos;
		public int length;

		public SyscallMatch (SyscallVisibleField field, int start_pos, int length)
		{
			this.field = field;
			this.start_pos = start_pos;
			this.length = length;
		}

		public override string ToString ()
		{
			return String.Format ("{{ field = {0}, start_pos = {1}, length = {2} }}",
					      field,
					      start_pos,
					      length);
		}

		public override bool Equals (object o)
		{
			SyscallMatch m;

			if (!(o is SyscallMatch))
				return false;

			m = (SyscallMatch) o;

			return (field == m.field
				&& start_pos == m.start_pos
				&& length == m.length);
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
	}

	public interface ISyscallFilter {
		bool Match (Syscall syscall);
		List<SyscallMatch> GetMatches ();
	}
}
