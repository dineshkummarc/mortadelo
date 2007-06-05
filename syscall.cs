namespace Mortadelo {
	public struct Syscall {
		/* Syscall index within its parent log */
		public int index;

		/* Process ID and thread ID */
		public int pid;
		public int tid;

		/* Process name */
		public string execname;

		/* Timestamp in microseconds */
		public long timestamp;

		/* Name of syscall */
		public string name;

		/* "Main" arguments and extra info */
		public string arguments;
		public string extra_info;

		/* Do we have a result?  For example, exit_group() does not
		 * return and so does not have a result.
		 */
		public bool have_result;
		public int result;

		/* A syscall can execute and finish without a context switch.  In that case,
		 *     is_syscall_start == is_syscall_end == true;
		 *     end_index == start_index == index;
		 *
		 * Otherwise, there was a context switch and we have stuff like
		 *     index  pid  call
		 *        50  1234 foo(42 <unfinished ...>
		 *        51  2345 bar(57) = 3
		 *        52  1234 <... foo resumed> ) = 0
		 *
		 * In that case,
		 *        50: { index=50, pid=1234, name="foo",
		 *              is_syscall_start=true, end_index=52,
		 *              is_syscall_end=false, start_index=50
		 *              have_result=false }
		 *        51: { index=51, pid=2345, name="bar",
		 *              is_syscall_start=true, end_index=51,
		 *              is_syscall_end=true, start_index=51,
		 *              have_result=true, result=3 }
		 *        52: { index=52, pid=1234, name="foo",
		 *              is_syscall_start=false, end_index=52
		 *              is_syscall_end=true, start_index=50,
		 *              have_result=true, result=0 }
		 *
		 * Summary:
		 *
		 * If this the start of an imcomplete syscall,
		 *     is_syscall_start = true;
		 *     is_syscall_end = false;
		 *     start_index = [self index]
		 *     end_index = index in the parent log of the "syscall" that finishes this one
		 *
		 * If this is the end of an incomplete syscall,
		 *     is_syscall_start = false;
		 *     is_syscall_end = true;
		 *     start_index = index in the parent log of the "syscall" that started this one
		 *     end_index = [self]
		 */
		public bool is_syscall_start;
		public int end_index;

		public bool is_syscall_end;
		public int start_index;

		public void Clear ()
		{
			index            = -1;
			pid              = -1;
			tid              = -1;
			execname         = null;
			timestamp        = 0;
			name             = null;
			arguments        = null;
			extra_info       = null;

			have_result      = false;
			result           = -1;

			is_syscall_start = false;
			end_index        = -1;

			is_syscall_end   = false;
			start_index      = -1;
		}
	}
}
