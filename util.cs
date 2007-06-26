using System;
using System.Text.RegularExpressions;

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

		/* Builds a Regex from a string, and configures it to do
		 * case-sensitive or case-insensitive matches in the same way as
		 * Emacs.  If the string is all lowercase, the search is
		 * case-insensitive.  If the string has uppercase characters,
		 * the search is case-sensitive.
		 */
		public static Regex MakeRegex (string str)
		{
			RegexOptions options;

			options = RegexOptions.None;

			if (is_lowercase (str))
				options = options | RegexOptions.IgnoreCase;

			return new Regex (str, options);
		}

		public static bool is_lowercase (string str)
		{
			foreach (char ch in str) {
				if (Char.IsUpper (ch))
					return false;
			}

			return true;
		}
	}
}
