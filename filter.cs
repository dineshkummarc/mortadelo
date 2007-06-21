using System;
using System.Collections.Generic;

namespace Mortadelo {
	public enum SyscallVisibleField {
		None,
		Process,
		Timestamp,
		Name,
		Arguments,
		ExtraInfo,
		Result
	}

	public struct SyscallMatch {
		public SyscallVisibleField field;
		public int start_pos;
		public int length;

		public SyscallMatch (SyscallVisibleField field, int start_pos, int length)
		{
			this.field = field;
			this.start_pos = start_pos;
			this.length = length;
		}

		public override string ToString ()
		{
			return String.Format ("{{ field = {0}, start_pos = {1}, length = {2} }}",
					      field,
					      start_pos,
					      length);
		}

		public override bool Equals (object o)
		{
			SyscallMatch m;

			if (!(o is SyscallMatch))
				return false;

			m = (SyscallMatch) o;

			return (field == m.field
				&& start_pos == m.start_pos
				&& length == m.length);
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
	}

	public interface ISyscallFilter {
		bool Match (Syscall syscall);
		List<SyscallMatch> GetMatches ();
	}
}
