using System;
using System.IO;

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

				unix_reader = new UnixReader (child_stdout);
				unix_reader.DataAvailable += unix_reader_data_available_cb;
				unix_reader.Closed += unix_reader_closed_cb;

				line_reader = new LineReader ();
				line_reader.LineAvailable += line_reader_line_available_cb;

				write_script_to_child ();
			} catch (GException e) {
				/* FIXME: print the error */
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
		}

		void child_watch_cb (int pid, int status, IntPtr user_data)
		{
			/* FIXME */
		}

		void unix_reader_data_available_cb (byte[] buffer, int len)
		{
			MemoryStream stream = new MemoryStream (buffer, 0, len);
			StreamReader stream_reader = new StreamReader (stream);

			line_reader.ReadLines (stream_reader);

			stream_reader.Close ();
		}

		void unix_reader_closed_cb ()
		{
			line_reader.Close ();
		}

		void line_reader_line_available_cb (string line)
		{
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
		UnixReader unix_reader;
		LineReader line_reader;

		int child_pid;
		int child_stdin;
		int child_stdout;
		int child_stderr;
	}
}
