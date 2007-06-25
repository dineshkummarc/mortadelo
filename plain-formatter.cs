using System.Diagnostics;

namespace Mortadelo {
	public class PlainFormatter : ISyscallFormatter {
		public PlainFormatter ()
		{
		}

		public string Format (int syscall_index, Syscall syscall, SyscallVisibleField field)
		{
			switch (field) {
			case SyscallVisibleField.None:
				return null;

			case SyscallVisibleField.Process:
				return Util.FormatProcess (syscall.pid, syscall.tid, syscall.execname);

			case SyscallVisibleField.Timestamp:
				return Util.FormatTimestamp (syscall.timestamp);

			case SyscallVisibleField.Name:
				return syscall.name;

			case SyscallVisibleField.Arguments:
				return syscall.arguments;

			case SyscallVisibleField.ExtraInfo:
				return syscall.extra_info;

			case SyscallVisibleField.Result:
				return Util.FormatResult (syscall.have_result, syscall.result);

			default:
				Debug.Assert (false, "Not reached");
				return null;
			}
		}

		public bool UseMarkup ()
		{
			return false;
		}
	}
}
