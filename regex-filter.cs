using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Mortadelo {
	public class RegexFilter : ISyscallFilter {
		public RegexFilter (RegexCache regex_cache)
		{
			if (regex_cache == null)
				throw new ArgumentNullException ("regex_cache");

			this.regex_cache = regex_cache;
			has_match = false;
			matches = null;
		}

		public bool Match (Syscall syscall)
		{
			int num_matches;
			matches = new List<SyscallMatch> ();

			num_matches = 0;

			num_matches += add_match (match_string (SyscallVisibleField.Process,
								Util.FormatProcess (syscall.pid,
										    syscall.tid,
										    syscall.execname)));

			num_matches += add_match (match_timestamp (SyscallVisibleField.Timestamp, syscall.timestamp));

			num_matches += add_match (match_string (SyscallVisibleField.Name, syscall.name));
			num_matches += add_match (match_string (SyscallVisibleField.Arguments, syscall.arguments));
			num_matches += add_match (match_string (SyscallVisibleField.ExtraInfo, syscall.extra_info));

			num_matches += add_match (match_result (SyscallVisibleField.Result,
								syscall.have_result, syscall.result));

			has_match = (num_matches > 0);
			return has_match;
		}

		SyscallMatch match_timestamp (SyscallVisibleField field, long timestamp)
		{
			return match_string (field, Util.FormatTimestamp (timestamp));
		}

		SyscallMatch match_string (SyscallVisibleField field, string str)
		{
			SyscallMatch match = new SyscallMatch (SyscallVisibleField.None, 0, 0);
			Match m;

			if (str == null)
				return match;

			m = regex_cache.GetMatch (str);
			if (m.Success) {
				match.field = field;
				match.start_pos = m.Index;
				match.length = m.Length;
			}

			return match;
		}

		SyscallMatch match_result (SyscallVisibleField field, bool have_result, int result)
		{
			return match_string (field, Util.FormatResult (have_result, result));
		}

		int add_match (SyscallMatch match)
		{
			if (match.field == SyscallVisibleField.None)
				return 0;

			matches.Add (match);
			return 1;
		}

		public List<SyscallMatch> GetMatches ()
		{
			if (!has_match)
				return null;

			return matches;
		}

		RegexCache regex_cache;
		bool has_match;
		List<SyscallMatch> matches;
	}

	[TestFixture]
	public class RegexFilterTest {
		[SetUp]
		public void setup ()
		{
			syscall = new Syscall ();

			syscall.pid              = 1234;
			syscall.tid              = 2345;
			syscall.execname         = "gnome-panel";
			syscall.timestamp        = 1180976736974992; /* 12:05:36.974992 */
			syscall.name             = "open";
			syscall.arguments        = "\"/proc/partitions\", O_RDONLY";
			syscall.extra_info       = "yadda yadda";
			syscall.have_result      = false;
			syscall.result           = -1;

			process_regex = "2.4"; /* will match "gnome-panel:1234:2345" */
			process_expected_matches = new List<SyscallMatch> ();
			process_expected_matches.Add (new SyscallMatch (SyscallVisibleField.Process, 13, 3));

			timestamp_regex = "9.*99";
			timestamp_expected_matches = new List<SyscallMatch> ();
			timestamp_expected_matches.Add (new SyscallMatch (SyscallVisibleField.Timestamp, 9, 5));

			name_regex = "pen";
			name_expected_matches = new List<SyscallMatch> ();
			name_expected_matches.Add (new SyscallMatch (SyscallVisibleField.Name, 1, 3));

			arguments_regex = "O_RDONLY";
			arguments_expected_matches = new List<SyscallMatch> ();
			arguments_expected_matches.Add (new SyscallMatch (SyscallVisibleField.Arguments, 20, 8));

			extra_info_regex = "dd";
			extra_info_expected_matches = new List<SyscallMatch> ();
			extra_info_expected_matches.Add (new SyscallMatch (SyscallVisibleField.ExtraInfo, 2, 2));

			result_regex = "\\?";
			result_expected_matches = new List<SyscallMatch> ();
			result_expected_matches.Add (new SyscallMatch (SyscallVisibleField.Result, 0, 1));

			multi_regex = ".";
			multi_expected_matches = new List<SyscallMatch> ();
			multi_expected_matches.Add (new SyscallMatch (SyscallVisibleField.Process, 0, 1));
			multi_expected_matches.Add (new SyscallMatch (SyscallVisibleField.Timestamp, 0, 1));
			multi_expected_matches.Add (new SyscallMatch (SyscallVisibleField.Name, 0, 1));
			multi_expected_matches.Add (new SyscallMatch (SyscallVisibleField.Arguments, 0, 1));
			multi_expected_matches.Add (new SyscallMatch (SyscallVisibleField.ExtraInfo, 0, 1));
			multi_expected_matches.Add (new SyscallMatch (SyscallVisibleField.Result, 0, 1));
		}

		[Test]
		public void test_nonexistent ()
		{
			string str = "this string does not occur";
			RegexFilter filter = new RegexFilter (new RegexCache (new Regex (str)));

			match (filter, str, null);
		}

		[Test]
		public void test_matches ()
		{
			generic_test (process_regex,	process_expected_matches);
			generic_test (timestamp_regex,	timestamp_expected_matches);
			generic_test (name_regex,	name_expected_matches);
			generic_test (arguments_regex,	arguments_expected_matches);
			generic_test (extra_info_regex,	extra_info_expected_matches);
			generic_test (result_regex,	result_expected_matches);
			generic_test (multi_regex,	multi_expected_matches);
		}

		void generic_test (string str_regex, List<SyscallMatch> expected)
		{
			RegexFilter filter;

			filter = new RegexFilter (new RegexCache (new Regex (str_regex)));
			match (filter, str_regex, expected);
		}

		void match (RegexFilter filter, string str_regex, List<SyscallMatch> expected)
		{
			if (filter.Match (syscall)) {
				List<SyscallMatch> matches;

				matches = filter.GetMatches ();
				if (expected == null)
					Assert.IsNull (matches, String.Format ("empty list of matches for \"{0}\"", str_regex));
				else {
					int i;

					Assert.IsNotNull (matches,
							  String.Format ("non-null list of matches for \"{0}\"", str_regex));
					Assert.AreEqual (expected.Count, matches.Count,
							 String.Format ("number of matches for \"{0}\"", str_regex));

					for (i = 0; i < expected.Count; i++)
						Assert.AreEqual (expected[i], matches[i],
								 String.Format ("match number {0} for \"{1}\"", i, str_regex));
				}
			} else
				Assert.IsNull (expected, String.Format ("no matches for \"{0}\"", str_regex));
		}

		Syscall syscall;

		string process_regex;
		List<SyscallMatch> process_expected_matches;

		string timestamp_regex;
		List<SyscallMatch> timestamp_expected_matches;

		string name_regex;
		List<SyscallMatch> name_expected_matches;

		string arguments_regex;
		List<SyscallMatch> arguments_expected_matches;

		string extra_info_regex;
		List<SyscallMatch> extra_info_expected_matches;

		string result_regex;
		List<SyscallMatch> result_expected_matches;

		string multi_regex;
		List<SyscallMatch> multi_expected_matches;
	}
}
