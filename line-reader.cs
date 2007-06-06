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
				Assert.AreEqual (line, expected_lines[line_num]);
				line_num++;
			});

			reader.ReadLines (stream);
			reader.Close ();
		}
	}
}
