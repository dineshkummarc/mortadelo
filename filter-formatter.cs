/*
 * Mortadelo - a viewer for system calls
 *
 * filter-formatter.cs - Adds Pango markup to display the results of a filter operation
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
using System.Diagnostics;
using System.Text;
using GLib;

namespace Mortadelo {

	public class FilterFormatter : ISyscallFormatter {
		public FilterFormatter (FilteredLog log)
		{
			if (log == null)
				throw new ArgumentNullException ("log");

			this.log = log;
		}

		public string Format (int syscall_index, Syscall syscall, SyscallVisibleField field)
		{
			string text;

			switch (field) {
			case SyscallVisibleField.None:
				return null;

			case SyscallVisibleField.Process:
				text = Util.FormatProcess (syscall.pid, syscall.tid, syscall.execname);
				break;

			case SyscallVisibleField.Timestamp:
				text = Util.FormatTimestamp (syscall.timestamp);
				break;

			case SyscallVisibleField.Name:
				text = syscall.name;
				break;

			case SyscallVisibleField.Arguments:
				text = syscall.arguments;
				break;

			case SyscallVisibleField.ExtraInfo:
				text = syscall.extra_info;
				break;

			case SyscallVisibleField.Result:
				text = Util.FormatResult (syscall.have_result, syscall.result);
				break;

			default:
				Debug.Assert (false, "Not reached");
				return null;
			}

			return highlight_matches (field, syscall_index, text);
		}

		public bool UseMarkup ()
		{
			return true;
		}

		string highlight_matches (SyscallVisibleField field, int syscall_index, string plain_text)
		{
			foreach (SyscallMatch match in log.GetMatches (syscall_index)) {
				if (match.field == field)
					return highlight_in_string (plain_text, match);
			}

			return GLib.Markup.EscapeText (plain_text);
		}

		string highlight_in_string (string plain, SyscallMatch match)
		{
			StringBuilder builder;

			builder = new StringBuilder (GLib.Markup.EscapeText (plain.Substring (0, match.start_pos)));
			builder.Append ("<b>");
			builder.Append (GLib.Markup.EscapeText (plain.Substring (match.start_pos, match.length)));
			builder.Append ("</b>");
			builder.Append (GLib.Markup.EscapeText (plain.Substring (match.start_pos + match.length,
										 plain.Length - (match.start_pos + match.length))));

			return builder.ToString ();
		}

		FilteredLog log;
	}
}
