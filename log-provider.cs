namespace Mortadelo {

	public delegate void SyscallModifiedHandler (int num);

	public interface ILogProvider {
		int GetNumSyscalls ();
		Syscall GetSyscall (int num);

		event SyscallModifiedHandler SyscallModified;
	}

}
