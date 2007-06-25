using System;
using System.Collections;
using System.Collections.Generic;

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
