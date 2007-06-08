using System.IO;

namespace Mortadelo {

	public interface ISyscallSerializer {
		void Serialize (TextWriter writer, Syscall syscall);
	}

}
