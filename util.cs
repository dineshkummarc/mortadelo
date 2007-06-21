using System;

namespace Mortadelo {
	public static class Util {
		public static string FormatProcess (int pid, int tid, string execname)
		{
			return String.Format ("{0}:{1}{2}{3}",
					      execname,
					      pid,
					      (pid == tid) ? "" : ":",
					      (pid == tid) ? "" : tid.ToString ());
		}

		public static string FormatTimestamp (long timestamp)
		{
			/* FIXME */
			return timestamp.ToString ();
		}

		public static string FormatResult (bool have_result, int result)
		{
			if (have_result) {
				if (result < 0) {
					string name, description;
					if (Errno.GetErrno (-result, out name, out description))
						return name;
					else {
						/* unknown errno code */
						return Mono.Unix.Catalog.GetString ("UNKNOWN");
					}
				} else
					return result.ToString ();
			} else
				return "?";
		}
	}
}
