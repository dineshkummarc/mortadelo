using System;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Mortadelo {
	public class RegexCache {
		public RegexCache (Regex regex)
		{
			if (regex == null)
				throw new ArgumentNullException ("regex");

			this.regex = regex;
			string_to_match_cache = new Hashtable ();
		}

		public Match GetMatch (string str)
		{
			Match m;

			if (string_to_match_cache.ContainsKey (str))
				m = string_to_match_cache[str] as Match;
			else {
				m = regex.Match (str);
				string_to_match_cache[str] = m;
			}

			return m;
		}

		Regex regex;
		Hashtable string_to_match_cache;
	}
}
