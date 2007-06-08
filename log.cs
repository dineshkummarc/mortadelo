using System;
using System.Collections;
using System.Collections.Generic;

namespace Mortadelo {
	public class Log {
		List<Syscall> syscalls;

		public Log ()
		{
			syscalls = new List<Syscall> ();
			pool = new StringPool ();

			modified_hash = new Hashtable ();
		}

		public int GetNumSyscalls ()
		{
			return syscalls.Count;
		}

		public int AppendSyscall (Syscall syscall)
		{
			syscall.index = syscalls.Count;
			syscalls.Add (syscall);

			return syscall.index;
		}

		public Syscall GetSyscall (int num)
		{
			return syscalls[num];
		}

		public void ModifySyscall (int num, Syscall syscall)
		{
			syscall.index = num;
			syscalls[num] = syscall;

			modified_hash[num] = true;
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

		public void uniquify_strings (ref Syscall syscall)
		{
			syscall.execname = pool.AddString (syscall.execname);
		}

		StringPool pool;
		Hashtable modified_hash;
	}
}
