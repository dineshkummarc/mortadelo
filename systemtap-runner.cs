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

			state = State.PreRun;
		}

		public void Run ()
		{
			if (state != State.PreRun)
				throw new ApplicationException ("Tried to Run() a SystemtapRunner which was not in PreRun state");

			try {
				Spawn.SpawnAsyncWithPipes (null,
							   build_systemtap_argv (),
							   null,
							   Spawn.G_SPAWN_DO_NOT_REAP_CHILD | Spawn.G_SPAWN_SEARCH_PATH,
							   out child_pid,
							   out child_stdin,
							   out child_stdout,
							   out child_stderr);
				child_watch_id = Spawn.ChildWatchAdd (child_pid, child_watch_cb);

				state = State.Running;

				stdout_reader = new UnixReader (child_stdout);
				stdout_reader.DataAvailable += stdout_reader_data_available_cb;
				stdout_reader.Closed += stdout_reader_closed_cb;

				stderr_reader = new UnixReader (child_stderr);
				stderr_reader.DataAvailable += stderr_reader_data_available_cb;
				stderr_reader.Closed += stderr_reader_closed_cb;

				line_reader = new LineReader ();
				line_reader.LineAvailable += line_reader_line_available_cb;

				write_script_to_child ();
			} catch (GException e) {
				Console.WriteLine ("error when spawning");
				state = State.Error;
			}
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

		void write_script_to_child ()
		{
			string script;
			UnixStream stream;
			StreamWriter writer;

			script = build_script ();
			stream = new UnixStream (child_stdin, true);
			writer = new StreamWriter (stream);

			writer.Write (script);
			writer.Close (); /* this will close the child_stdin fd */
			child_stdin = -1;

			Console.WriteLine ("wrote the script to the child");
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

		void child_watch_cb (int pid, int status, IntPtr user_data)
		{
			Console.WriteLine ("CHILD WATCH pid {0}, status {1}", pid, status);
			/* FIXME */
		}

		void stdout_reader_data_available_cb (byte[] buffer, int len)
		{
//			Console.WriteLine ("child stdout data available, len {0}", len);

			MemoryStream stream = new MemoryStream (buffer, 0, len);
			StreamReader stream_reader = new StreamReader (stream);

			line_reader.ReadLines (stream_reader);

			stream_reader.Close ();
		}

		void stdout_reader_closed_cb ()
		{
//			Console.WriteLine ("child stdout closed");
			line_reader.Close ();
		}

		void stderr_reader_data_available_cb (byte[] buffer, int len)
		{
//			Console.WriteLine ("child stderr data available, len {0}", len);

			MemoryStream stream = new MemoryStream (buffer, 0, len);
			StreamReader stream_reader = new StreamReader (stream);

			string str = stream_reader.ReadToEnd ();
//			Console.WriteLine ("child stderr --------------\n{0}\n-------------------------", str);

			stream_reader.Close ();
		}

		void stderr_reader_closed_cb ()
		{
//			Console.WriteLine ("child stderr closed");
			line_reader.Close ();
		}

		void line_reader_line_available_cb (string line)
		{
//			Console.WriteLine ("processing line: {0}", line);
			aggregator.ProcessLine (line);
		}

		enum State {
			PreRun,
			Running,
			Stopped,
			Error
		}

		State state;

		Aggregator aggregator;
		SystemtapParser parser;
		uint child_watch_id;
		UnixReader stdout_reader;
		UnixReader stderr_reader;
		LineReader line_reader;

		int child_pid;
		int child_stdin;
		int child_stdout;
		int child_stderr;

		public static void Main ()
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
