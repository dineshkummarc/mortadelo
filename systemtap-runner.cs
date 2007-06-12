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
			runner = new AggregatorRunner (build_systemtap_argv (), build_script (), aggregator);
			runner.ChildExited += child_exited_cb;
		}

		public void Run ()
		{
			runner.Run ();
		}

		public void Stop ()
		{
			runner.Stop ();
		}

		string[] build_systemtap_argv ()
		{
			string[] argv = new string[2];

			argv[0] = "stap";
			argv[1] = "-";

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

		void child_exited_cb (int status)
		{
			ChildExited (status);
		}

		Aggregator aggregator;
		SystemtapParser parser;
		AggregatorRunner runner;

		public delegate void ChildExitedHandler (int status);
		public event ChildExitedHandler ChildExited;
	}
}
