/*
 * Mortadelo - a viewer for system calls
 *
 * systemtap-parser.cs - Parses the output of a certain Systemtap scripts to turn it into Syscall structures
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
using NUnit.Framework;

namespace Mortadelo {
	public class SystemtapParser : ISyscallParser {
		public SystemtapParser ()
		{
			// start.open: 1180976736974992: gnome-panel (3630:3630): "/proc/partitions", O_RDONLY
			syscall_regex = new Regex (@"start\.(?<name>.+): (?<timestamp>\d+): (?<execname>.+) \((?<pid>\d+):(?<tid>\d+)\): (?<arguments>.*)");
			// return.open: 1180976736975010: gnome-panel (3630:3630): 27
			syscall_return_regex = new Regex (@"return\.(?<name>.+): (?<timestamp>\d+): (?<execname>.+) \((?<pid>\d+):(?<tid>\d+)\): (?<result>.*)");
		}

		public bool Parse (string str, out Syscall syscall)
		{
			if (str == null)
				throw new ArgumentNullException ("str");

			if (try_parse_syscall (str, out syscall)
			    || try_parse_syscall_return (str, out syscall))
				return true;
			else
				return false;
		}

		bool try_parse_syscall (string str, out Syscall syscall)
		{
			Syscall s;
			Match m;
			string name_str, timestamp_str, execname_str, pid_str, tid_str, arguments_str;
			bool retval;

			s = new Syscall ();
			s.Clear ();

			m = syscall_regex.Match (str);
			if (!m.Success) {
				retval = false;
				goto bail;
			}

			name_str      = m.Groups["name"].Value;
			timestamp_str = m.Groups["timestamp"].Value;
			execname_str  = m.Groups["execname"].Value;
			pid_str       = m.Groups["pid"].Value;
			tid_str       = m.Groups["tid"].Value;
			arguments_str = m.Groups["arguments"].Value;

			s.name      = name_str;
			s.timestamp = long.Parse (timestamp_str);
			s.execname  = execname_str;
			s.pid       = int.Parse (pid_str);
			s.tid       = int.Parse (tid_str);
			s.arguments = arguments_str;

			s.have_result = false;

			s.is_syscall_start = true;

			retval = true;

		bail:
			syscall = s;

			return retval;
		}

		bool try_parse_syscall_return (string str, out Syscall syscall)
		{
			Syscall s;
			Match m;
			string name_str, timestamp_str, execname_str, pid_str, tid_str, result_str;
			bool retval;

			s = new Syscall ();
			s.Clear ();

			m = syscall_return_regex.Match (str);
			if (!m.Success) {
				retval = false;
				goto bail;
			}

			name_str      = m.Groups["name"].Value;
			timestamp_str = m.Groups["timestamp"].Value;
			execname_str  = m.Groups["execname"].Value;
			pid_str       = m.Groups["pid"].Value;
			tid_str       = m.Groups["tid"].Value;
			result_str    = m.Groups["result"].Value;

			s.name      = name_str;
			s.timestamp = long.Parse (timestamp_str);
			s.execname  = execname_str;
			s.pid       = int.Parse (pid_str);
			s.tid       = int.Parse (tid_str);

			s.have_result = true;
			s.result = int.Parse (result_str);

			s.is_syscall_end = true;

			retval = true;

		bail:
			syscall = s;

			return retval;
		}

		Regex syscall_regex;
		Regex syscall_return_regex;
	}

	[TestFixture]
	public class SystemtapParserTest {
		SystemtapParser parser;

		[SetUp]
		public void Setup ()
		{
			parser = new SystemtapParser ();
		}

		[Test]
		public void Open ()
		{
			bool result;
			Syscall syscall;
			Syscall expected;

			result = parser.Parse ("start.open: 1180976736974992: gnome-panel (3630:3630): \"/proc/partitions\", O_RDONLY",
					       out syscall);

			Assert.IsTrue (result, "Parse open - presence of a match");

			expected = new Syscall ();
			expected.Clear ();
			expected.name = "open";
			expected.timestamp = 1180976736974992;
			expected.execname = "gnome-panel";
			expected.pid = 3630;
			expected.tid = 3630;
			expected.arguments = "\"/proc/partitions\", O_RDONLY";
			expected.is_syscall_start = true;

			Assert.AreEqual (expected, syscall, "Parse open - syscall contents");
		}

		[Test]
		public void OpenReturn ()
		{
			bool result;
			Syscall syscall;
			Syscall expected;

			result = parser.Parse ("return.open: 1180976736975010: gnome-panel (3630:3630): 27",
					       out syscall);

			Assert.IsTrue (result, "Parse open.return - presence of a match");

			expected = new Syscall ();
			expected.Clear ();
			expected.name = "open";
			expected.timestamp = 1180976736975010;
			expected.execname = "gnome-panel";
			expected.pid = 3630;
			expected.tid = 3630;
			expected.have_result = true;
 			expected.result = 27;
			expected.is_syscall_end = true;

			Assert.AreEqual (expected, syscall, "Parse open.return - syscall contents");
		}
	}
}
