/*
 * Mortadelo - a viewer for system calls
 *
 * regex-cache.cs - Memoizer for the results of regex matches
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
