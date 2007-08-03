/*
 * Mortadelo - a viewer for system calls
 *
 * line-reader.cs - Reads newline-terminated lines from a TextReader
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
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Mortadelo {
	public class LineReader {
		public LineReader ()
		{
			line = new StringBuilder ();
			closed = false;
		}

		public void ReadLines (TextReader text_reader)
		{
			int ch;

			if (closed)
				throw new ApplicationException ("Tried to read lines from a LineReader which is already closed");

			while ((ch = text_reader.Read()) != -1) {
				line.Append ((char) ch);

				if (ch == '\n') {
					LineAvailable (line.ToString ());
					line.Remove (0, line.Length);
				}
			}
		}

		public void Close ()
		{
			closed = true;
			LineAvailable (line.ToString ());
		}

		StringBuilder line;
		bool closed;

		public delegate void LineAvailableDelegate (string line);

		public event LineAvailableDelegate LineAvailable;
	}

	[TestFixture]
	public class LineReaderTest {
		[Test]
		public void Lines ()
		{
			string str = "This is a test\nof using a line reader\nwith no terminating newline.";
			string[] expected_lines = {
				"This is a test\n",
				"of using a line reader\n",
				"with no terminating newline."
			};
			StringReader stream = new StringReader (str);
			LineReader reader = new LineReader ();
			int line_num;

			line_num = 0;

			reader.LineAvailable += new LineReader.LineAvailableDelegate (delegate (string line) {
				Assert.AreEqual (expected_lines[line_num], line, "Contents of a line");
				line_num++;
			});

			reader.ReadLines (stream);
			reader.Close ();
		}
	}
}
