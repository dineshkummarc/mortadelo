using System;

namespace Mortadelo {

	public interface ISyscallFormatter {
		string Format (int syscall_index, Syscall syscall, SyscallVisibleField field);
		bool UseMarkup ();
	}
}
