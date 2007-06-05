using System.Collections.Generic;

namespace Mortadelo {
	public class Log {
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
		}
	}
}
