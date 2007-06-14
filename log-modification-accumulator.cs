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
