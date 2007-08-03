/*
 * Mortadelo - a viewer for system calls
 *
 * log-modification-accumulator.cs - Collects modifications from a log to consult them later
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
using System.Collections;
using System.Collections.Generic;

namespace Mortadelo {

	public class LogModificationAccumulator {
		public LogModificationAccumulator (ILogProvider log)
		{
			log.SyscallModified += syscall_modified_cb;

			modified_hash = new Hashtable ();
		}

		public int[] GetModifiedIndexes ()
		{
			List<int> modified_list;
			int[] modified_array;
			int n;

			modified_list = new List<int> ();

			foreach (int i in modified_hash.Keys)
				modified_list.Add (i);

			modified_hash.Clear ();

			n = modified_list.Count;
			modified_array = new int[n];

			for (int i = 0; i < n; i++)
				modified_array[i] = modified_list[i];

			Array.Sort (modified_array);
			return modified_array;
		}

		void syscall_modified_cb (int num)
		{
			modified_hash[num] = true;
		}

		Hashtable modified_hash;
	}
	
}
