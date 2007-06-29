namespace Mortadelo {

	public delegate void SyscallModifiedHandler (int num);
	public delegate void SyscallRemovedHandler (int num);
	public delegate void SyscallInsertedHandler (int num);

	public interface ILogProvider {
		/* Gets the number of system calls that this log stores */
		int GetNumSyscalls ();

		/* Gets a syscall by its index within the log.  Syscall.index is not necessarily equal to num */
		Syscall GetSyscall (int num);

		/* Gets the index within the log which corresponds to the syscall which has syscall.index == num,
		 * or returns -1 if the log doesn't contain the sought syscall.
		 *
		 * For filtered logs, this maps the base index to the filtered
		 * index.
		 */
		int GetSyscallByBaseIndex (int base_index);

		/* Events emitted when syscalls change */
		event SyscallModifiedHandler SyscallModified;
		event SyscallRemovedHandler SyscallRemoved;
		event SyscallInsertedHandler SyscallInserted;
	}

}
