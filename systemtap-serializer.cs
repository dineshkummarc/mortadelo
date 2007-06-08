using System;
using System.IO;

namespace Mortadelo {

	public class SystemtapSerializer : ISyscallSerializer {
		public SystemtapSerializer ()
		{
		}

		public void Serialize (TextWriter writer, Syscall syscall)
		{
			if (syscall.name == "open" && syscall.is_syscall_start) {
				writer.Write ("open: {0}: {1} ({2}:{3}): {4}\n",
					      syscall.timestamp,
					      syscall.execname,
					      syscall.pid,
					      syscall.tid,
					      syscall.arguments);
			}

			if (syscall.name == "open" && syscall.is_syscall_end) {
				writer.Write ("open.return: {0}: {1} ({2}:{3}): {4}\n",
					      syscall.timestamp,
					      syscall.execname,
					      syscall.pid,
					      syscall.tid,
					      syscall.result);
			}
		}
	}

}
