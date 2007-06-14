using System;
using System.Collections;
using System.Collections.Generic;

namespace Mortadelo {
	public class Log : ILogProvider {
		List<Syscall> syscalls;

		public Log ()
		{
			syscalls = new List<Syscall> ();
			pool = new StringPool ();
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

			if (SyscallModified != null)
				SyscallModified (num);
		}

		void uniquify_strings (ref Syscall syscall)
		{
			syscall.execname = pool.AddString (syscall.execname);
			syscall.name = pool.AddString (syscall.name);
			syscall.arguments = pool.AddString (syscall.arguments);
			syscall.extra_info = pool.AddString (syscall.extra_info);
		}

		StringPool pool;

		public event SyscallModifiedHandler SyscallModified;
	}
}
