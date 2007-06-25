using System;
using System.Diagnostics;
using System.Text;

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

			return plain_text;
		}

		string highlight_in_string (string plain, SyscallMatch match)
		{
			StringBuilder builder;

			builder = new StringBuilder (plain, 0, match.start_pos, 0);
			builder.Append ("<b>");
			builder.Append (plain, match.start_pos, match.length);
			builder.Append ("</b>");
			builder.Append (plain, match.start_pos + match.length, plain.Length - (match.start_pos + match.length));

			return builder.ToString ();
		}

		FilteredLog log;
	}
}
