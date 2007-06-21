namespace Mortadelo {

	public delegate void SyscallAddedHandler ();
	public delegate void SyscallModifiedHandler (int num);

	public interface ILogProvider {
		int GetNumSyscalls ();
		Syscall GetSyscall (int num);

		event SyscallAddedHandler SyscallAdded;
		event SyscallModifiedHandler SyscallModified;
	}

}
