namespace Mortadelo {

	public interface ISyscallParser {
		bool Parse (string str, out Syscall syscall);
	}

}
