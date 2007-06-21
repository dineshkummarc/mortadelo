using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Mortadelo {
	public class SubstringFilter : ISyscallFilter {
		public SubstringFilter (string needle)
		{
			if (needle == null)
				throw new ArgumentNullException ("needle");

			this.needle = needle;
			has_match = false;
			matches = new List<SyscallMatch> ();
		}

		public bool Match (Syscall syscall)
		{
			has_match = false;
			matches.Clear ();

			has_match = has_match || add_match (match_int (SyscallVisibleField.Pid, syscall.pid));
			has_match = has_match || add_match (match_int (SyscallVisibleField.Tid, syscall.tid));

			has_match = has_match || add_match (match_timestamp (SyscallVisibleField.Timestamp, syscall.timestamp));

			has_match = has_match || add_match (match_string (SyscallVisibleField.Execname, syscall.execname));
			has_match = has_match || add_match (match_string (SyscallVisibleField.Arguments, syscall.arguments));
			has_match = has_match || add_match (match_string (SyscallVisibleField.ExtraInfo, syscall.extra_info));

			has_match = has_match || add_match (match_result (SyscallVisibleField.Result,
									  syscall.have_result, syscall.result));

			return has_match;
		}

		SyscallMatch match_int (SyscallVisibleField field, int number)
		{
			return match_string (field, number.ToString ());
		}

		SyscallMatch match_timestamp (SyscallVisibleField field, long timestamp)
		{
			return match_string (field, Util.FormatTimestamp (timestamp));
		}

		SyscallMatch match_string (SyscallVisibleField field, string str)
		{
			SyscallMatch match = new SyscallMatch (SyscallVisibleField.None, 0, 0);
			int pos;

			pos = (str != null) ? str.IndexOf (needle) : -1;
			if (pos >= 0) {
				match.field = field;
				match.start_pos = pos;
				match.length = str.Length;
			}

			return match;
		}

		SyscallMatch match_result (SyscallVisibleField field, bool have_result, int result)
		{
			return match_string (field, Util.FormatResult (have_result, result));
		}

		bool add_match (SyscallMatch match)
		{
			if (match.field == SyscallVisibleField.None)
				return false;

			matches.Add (match);
			return true;
		}

		public List<SyscallMatch> GetMatches ()
		{
			if (!has_match)
				return null;

			return matches;
		}

		string needle;
		bool has_match;
		List<SyscallMatch> matches;
	}

	[TestFixture]
	public class SubstringFilterTest {
		[SetUp]
		public void setup ()
		{
			syscall = new Syscall ();

			syscall.pid              = 1234;
			syscall.tid              = 2345;
			syscall.execname         = "gnome-panel";
			syscall.timestamp        = 1180976736974992;
			syscall.name             = "open";
			syscall.arguments        = "\"/proc/partitions\", O_RDONLY";
			syscall.extra_info       = null;
			syscall.have_result      = false;
			syscall.result           = -1;

			pid_tid_expected_matches = new List<SyscallMatch> ();
			pid_tid_expected_matches.Add (new SyscallMatch (SyscallVisibleField.Pid, 1, 3));
			pid_tid_expected_matches.Add (new SyscallMatch (SyscallVisibleField.Tid, 0, 3));

			execname_expected_matches = new List <SyscallMatch> ();
			execname_expected_matches.Add (new SyscallMatch (SyscallVisibleField.Execname, 6, 5));
		}

		[Test]
		public void test_nonexistent ()
		{
			string substring = "this string does not occur";
			SubstringFilter filter = new SubstringFilter (substring);

			match (filter, substring, null);
		}

		[Test]
		public void test_match ()
		{
			SubstringFilter filter;

			filter = new SubstringFilter ("234");
			match (filter, "234", pid_tid_expected_matches);

			filter = new SubstringFilter ("panel");
			match (filter, "panel", execname_expected_matches);
		}

		void match (SubstringFilter filter, string substring, List<SyscallMatch> expected)
		{
			if (filter.Match (syscall)) {
				List<SyscallMatch> matches;

				matches = filter.GetMatches ();
				Assert.AreEqual (expected, matches, "list of matches");
			} else
				Assert.IsNull (expected, String.Format ("no matches for \"{0}\"", substring));
		}

		Syscall syscall;
		List<SyscallMatch> pid_tid_expected_matches;
		List<SyscallMatch> execname_expected_matches;
	}
}
