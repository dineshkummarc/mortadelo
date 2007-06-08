using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Mortadelo {
	public class LogIO {
		public LogIO (ISyscallParser parser)
		{
			if (parser == null)
				throw new ArgumentNullException ("parser");

			this.parser = parser;
		}

		public Log Load (TextReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			Log log;
			Aggregator aggregator;
			LineReader line_reader;

			log = new Log ();
			aggregator = new Aggregator (log, parser);

			line_reader = new LineReader ();

			line_reader.LineAvailable += delegate (string line) {
				aggregator.ProcessLine (line);
			};

			line_reader.ReadLines (reader);
			line_reader.Close ();

			return log;
		}

		public void Save (TextWriter writer, Log log, ISyscallSerializer serializer)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");

			int num_syscalls;
			int i;

			num_syscalls = log.GetNumSyscalls ();

			for (i = 0; i < num_syscalls; i++) {
				Syscall syscall;

				syscall = log.GetSyscall (i);
				serializer.Serialize (writer, syscall);
			}
		}

		ISyscallParser parser;
	}

	[TestFixture]
	public class LogIOTest {
		[Test]
		public void Roundtrip ()
		{
			string string_log = get_systemtap_log ();

			/* Load */

			ISyscallParser parser = new SystemtapParser ();
			LogIO io;
			Log log;

			io = new LogIO (parser);

			log = io.Load (new StringReader (string_log));

			/* Save */

			StringBuilder builder = new StringBuilder ();
			StringWriter writer = new StringWriter (builder);

			io.Save (writer, log, new SystemtapSerializer ());
			writer.Close ();

			/* Compare */

			Assert.AreEqual (string_log, builder.ToString (), "Systemtap Load/Save roundtrip");
		}

		string get_systemtap_log ()
		{
			return ("open: 1180976736974992: gnome-panel (3630:3630): \"/proc/partitions\", O_RDONLY\n" +
				"open.return: 1180976736975010: gnome-panel (3630:3630): 27\n" +
				"open: 1181064007999786: hald-addon-stor (2882:2883): \"/dev/hdc\", O_RDONLY|O_EXCL|O_LARGEFILE|O_NONBLOCK\n" +
				"open: 1181064008000173: gimp-2.2 (27920:27920): \"/home/federico/.gimp-2.2/tool-options/gimp-free-select-tool.presetsysBTNg\", O_RDWR|O_CREAT|O_EXCL|O_LARGEFILE, 0600\n" +
				"open.return: 1181064008031945: NetworkManager (2539:26181): 19\n" +
				"open.return: 1181064008000205: gimp-2.2 (27920:27920): 7\n");
		}
	}
}
