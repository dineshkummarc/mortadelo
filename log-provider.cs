namespace Mortadelo {

	public delegate void SyscallModifiedHandler (int num);
	public delegate void SyscallRemovedHandler (int num);
	public delegate void SyscallInsertedHandler (int num);

	public interface ILogProvider {
		int GetNumSyscalls ();
		Syscall GetSyscall (int num);

		event SyscallModifiedHandler SyscallModified;
		event SyscallRemovedHandler SyscallRemoved;
		event SyscallInsertedHandler SyscallInserted;
	}

}
