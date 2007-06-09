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
			uniquify_strings (ref syscall);

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
			uniquify_strings (ref syscall);

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

		void uniquify_strings (ref Syscall syscall)
		{
			syscall.execname = pool.AddString (syscall.execname);
			syscall.name = pool.AddString (syscall.name);
			syscall.arguments = pool.AddString (syscall.arguments);
			syscall.extra_info = pool.AddString (syscall.extra_info);
		}

		StringPool pool;
		Hashtable modified_hash;
	}
}
