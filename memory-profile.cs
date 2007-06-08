using System;
using System.IO;

namespace Mortadelo {
	public class MemoryProfile {
		public static int Main (string[] args)
		{
			if (args.Length != 1) {
				Console.WriteLine ("usage: mortadelo-memory-profile <systemtap-log-file>");
				return 1;
			}

			ISyscallParser parser = new SystemtapParser ();
			LogIO io;
			StreamReader reader;
			Stream stream;
			Log log;

			io = new LogIO (parser);
			reader = new StreamReader (args[0]);
			stream = reader.BaseStream;
			log = io.Load (reader);

			Console.WriteLine ("{0} syscalls processed ({1:0.00} KB from the file)",
					   log.GetNumSyscalls (),
					   stream.Position / 1024.0);

			return 0;
		}
	}
}
