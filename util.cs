/*
 * Mortadelo - a viewer for system calls
 *
 * util.cs - Miscellaneous utility functions
 *
 * Copyright (C) 2007 Federico Mena-Quintero
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 *
 * Authors: Federico Mena Quintero <federico@novell.com>
 */

using System;
using System.Text.RegularExpressions;
using Mono.Unix;
using Mono.Unix.Native;

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
			DateTime dt;
			long sec, usec;

			sec = timestamp / 1000000;
			usec = timestamp % 1000000;

			dt = NativeConvert.FromTimeT (sec);

			return String.Format (Mono.Unix.Catalog.GetString ("{0:d2}:{1:d2}:{2:d2}.{3:d6}"),
					      dt.Hour,
					      dt.Minute,
					      dt.Second,
					      usec);
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
