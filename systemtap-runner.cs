using System;
using System.IO;
using System.Text;
using GLib;
using Mono.Unix;
using Gtk;

namespace Mortadelo {
	public class SystemtapRunner {
		public SystemtapRunner (Log log)
		{
			parser = new SystemtapParser ();
			aggregator = new Aggregator (log, parser);
		}


		public void Stop ()
		{
			if (state != State.Running)
				throw new ApplicationException ("Tried to Stop() a SystemtapRunner which was not in Running state");

			/* FIXME: kill the child */
		}

		string[] build_systemtap_argv ()
		{
			string[] argv = new string[3];

			argv[0] = "stap";
			argv[1] = "stap";
			argv[2] = "-";

			return argv;
		}

		string build_script ()
		{
			StringBuilder builder = new StringBuilder ();

			builder.Append (@"probe syscall.open {
						printf (""open: %d: %s (%d:%d): %s\n"",
							gettimeofday_us (), execname (), pid (), tid (), argstr);
					  }

					  probe syscall.open.return {
						printf (""open.return: %d: %s (%d:%d): %d\n"",
							gettimeofday_us (), execname (), pid (), tid (), $return);
					  }");

			return builder.ToString ();
		}

		Aggregator aggregator;
		SystemtapParser parser;

		public static void MainMain ()
		{
			SystemtapRunner runner;
			Log log;

			Application.Init ();

			log = new Log ();
			runner = new SystemtapRunner (log);

			runner.Run ();

			Window w;

			w = new Window ("hola");
			Button b;

			b = new Button ("hola mundo");
			w.Add (b);
			w.ShowAll ();

			GLib.Timeout.Add (5000, delegate {
				int num;

				num = log.GetNumSyscalls ();
				Console.WriteLine ("syscalls: {0}", num);
				return true;
			});

			Application.Run ();
		}
	}
}
