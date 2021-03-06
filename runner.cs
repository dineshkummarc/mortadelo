/*
 * Mortadelo - a viewer for system calls
 *
 * runner.cs - Runs a child process and ties its output to a syscall parser and log aggregator
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
using GLib;
using Mono.Unix;
using Mono.Unix.Native;
using NUnit.Framework;

using unix = Mono.Unix.Native.Syscall;

namespace Mortadelo {
	public class AggregatorRunner {
		public AggregatorRunner ()
		{
			state = State.PreRun;
		}

		public void Run (Aggregator aggregator, string[] argv, string stdin_str)
		{
			if (state != State.PreRun)
				throw new ApplicationException ("Tried to Run() an AggregatorRunner which was not in PreRun state");

			if (aggregator == null)
				throw new ArgumentNullException ("aggregator");

			if (argv == null)
				throw new ArgumentNullException ("argv");

			this.aggregator = aggregator;

			try {
				int[] pipe;

				spawn = new Spawn ();

				pipe = new int[2];
				if (unix.pipe (pipe) != 0)
					throw new UnixIOException (Mono.Unix.Native.Stdlib.GetLastError ());

				Spawn.ChildSetupFunc child_setup_fn = delegate () {
					int process_group;
					UnixStream child_stream;
					StreamWriter child_writer;

					process_group = unix.setsid ();

					child_stream = new UnixStream (pipe[1], false);
					child_writer = new StreamWriter (child_stream);

					child_writer.Write ("{0}\n", process_group);
					child_writer.Close ();
				};

				spawn.SpawnAsyncWithPipes (null,
							   argv,
							   null,
							   GSpawnFlags.G_SPAWN_DO_NOT_REAP_CHILD | GSpawnFlags.G_SPAWN_SEARCH_PATH,
							   child_setup_fn,
							   out child_pid,
							   out child_stdin,
							   out child_stdout,
							   out child_stderr);

				child_watch_id = spawn.ChildWatchAdd (child_pid, child_watch_cb);

				UnixStream parent_stream;
				StreamReader parent_reader;
				string str;

				parent_stream = new UnixStream (pipe[0], false);
				parent_reader = new StreamReader (parent_stream);

				str = parent_reader.ReadLine ();
				parent_reader.Close ();

				child_process_group = int.Parse (str);
				if (child_process_group == -1)
					throw new ApplicationException ("Could not get the child process group");

				state = State.Running;

				stdout_reader = new UnixReader (child_stdout);
				stdout_reader.DataAvailable += stdout_reader_data_available_cb;
				stdout_reader.Closed += stdout_reader_closed_cb;

				stderr_reader = new UnixReader (child_stderr);
				stderr_reader.DataAvailable += stderr_reader_data_available_cb;
				stderr_reader.Closed += stderr_reader_closed_cb;

				line_reader = new LineReader ();
				line_reader.LineAvailable += line_reader_line_available_cb;

				if (stdin_str != null)
					write_stdin_to_child (stdin_str);
			} catch (GException e) {
				Console.WriteLine ("error when spawning: {0}", e);
				/* FIXME: report something better --- re-throw the exception here? */
				state = State.Error;
			}
		}

		public void Stop ()
		{
			int result;

			if (state == State.PreRun) {
				state = State.Stopped;
				return;
			}

			if (state == State.Stopped)
				return;

			if (state != State.Running)
				throw new ApplicationException ("Tried to Stop() an AggregatorRunner which was not in Running state");

			result = unix.kill (-child_process_group, Signum.SIGTERM);
			if (result == 0)
				state = State.Stopped;
			else {
				/* FIXME: do we need to reset our fields here?  Then again, child_watch_cb() may end
				 * up doing that for us.
				 */
				state = State.Error;
			}
		}

		void child_watch_cb (int pid, int status)
		{
			/* FIXME: we need to close the FDs! */
			child_pid = 0;
			child_stdin = -1;
			child_stdout = -1;
			child_stderr = -1;
			child_watch_id = 0;

			ChildExited (status);
		}

		void write_stdin_to_child (string stdin_str)
		{
			UnixStream stream;
			StreamWriter writer;

			try {
				stream = new UnixStream (child_stdin, true);
				writer = new StreamWriter (stream);

				writer.Write (stdin_str);
				writer.Close (); /* this will close the child_stdin fd */
			} catch {
				/* FIXME: broken pipe */
			}

			child_stdin = -1;
		}

		void stdout_reader_data_available_cb (byte[] buffer, int len)
		{
			// Console.WriteLine ("child stdout data available, len {0}", len);

			MemoryStream stream = new MemoryStream (buffer, 0, len);
			StreamReader stream_reader = new StreamReader (stream);

			line_reader.ReadLines (stream_reader);

			stream_reader.Close ();
		}

		void stdout_reader_closed_cb ()
		{
			// Console.WriteLine ("child stdout closed");
			line_reader.Close ();
		}

		void stderr_reader_data_available_cb (byte[] buffer, int len)
		{
			if (StderrDataAvailable != null)
				StderrDataAvailable (buffer, len);
		}

		void stderr_reader_closed_cb ()
		{
			// Console.WriteLine ("child stderr closed");
		}

		void line_reader_line_available_cb (string line)
		{
			// Console.WriteLine ("processing line: {0}", line);
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
		Spawn spawn;

		uint child_watch_id;
		int child_pid;
		int child_process_group;
		int child_stdin;
		int child_stdout;
		int child_stderr;

		UnixReader stdout_reader;
		UnixReader stderr_reader;
		LineReader line_reader;

		public delegate void ChildExitedHandler (int status);
		public event ChildExitedHandler ChildExited;

		public delegate void StderrDataAvailableHandler (byte[] buffer, int len);
		public event StderrDataAvailableHandler StderrDataAvailable;
	}

	[TestFixture]
	public class AggregatorRunnerTest {
		[SetUp]
		public void SetUp () {
			log = new Log ();
			parser = new SystemtapParser ();
			aggregator = new Aggregator (log, parser);
		}

		[Test]
		public void Aggregate () {
			string[] argv = { "/bin/cat" };
			string stdin_str = get_stdin_str ();

			AggregatorRunner runner = new AggregatorRunner ();
			runner.ChildExited += child_exited_cb;

			runner.Run (aggregator, argv, stdin_str);

			loop = new MainLoop ();
			loop.Run ();

			Assert.IsTrue (unix.WIFEXITED (child_status) && unix.WEXITSTATUS (child_status) == 0,
				       "Child exit status = 0");

			Assert.IsTrue (check_log (), "Contents of aggregated log");
		}

		string get_stdin_str ()
		{
			return ("start.open: 1180976736974992: gnome-panel (3630:3630): \"/proc/partitions\", O_RDONLY\n" +
				"return.open: 1180976736975010: gnome-panel (3630:3630): 27\n" +
				"start.open: 1181064007999786: hald-addon-stor (2882:2883): \"/dev/hdc\", O_RDONLY|O_EXCL|O_LARGEFILE|O_NONBLOCK\n" +
				"start.open: 1181064008000173: gimp-2.2 (27920:27920): \"/home/federico/.gimp-2.2/tool-options/gimp-free-select-tool.presetsysBTNg\", O_RDWR|O_CREAT|O_EXCL|O_LARGEFILE, 0600\n" +
				"return.open: 1181064008031945: NetworkManager (2539:26181): 19\n" +
				"return.open: 1181064008000205: gimp-2.2 (27920:27920): 7\n");
		}

		void child_exited_cb (int status)
		{
			child_status = status;
			loop.Quit ();
		}

		bool check_log ()
		{
			bool retval;
			Syscall[] expected;
			int i;
			int num_syscalls;

			retval = false;

			expected = new Syscall[6];

			expected[0].index            = 0;
			expected[0].pid              = 3630;
			expected[0].tid              = 3630;
			expected[0].execname         = "gnome-panel";
			expected[0].timestamp        = 1180976736974992;
			expected[0].name             = "open";
			expected[0].arguments        = "\"/proc/partitions\", O_RDONLY";
			expected[0].extra_info       = null;
			expected[0].have_result      = false;
			expected[0].result           = -1;
			expected[0].is_syscall_start = true;
			expected[0].end_index        = 1;
			expected[0].is_syscall_end   = false;
			expected[0].start_index      = -1;

			expected[1].index            = 1;
			expected[1].pid              = 3630;
			expected[1].tid              = 3630;
			expected[1].execname         = "gnome-panel";
			expected[1].timestamp        = 1180976736975010;
			expected[1].name             = "open";
			expected[1].arguments        = null;
			expected[1].extra_info       = null;
			expected[1].have_result      = true;
			expected[1].result           = 27;
			expected[1].is_syscall_start = false;
			expected[1].end_index        = -1;
			expected[1].is_syscall_end   = true;
			expected[1].start_index      = 0;

			expected[2].index            = 2;
			expected[2].pid              = 2882;
			expected[2].tid              = 2883;
			expected[2].execname         = "hald-addon-stor";
			expected[2].timestamp        = 1181064007999786;
			expected[2].name             = "open";
			expected[2].arguments        = "\"/dev/hdc\", O_RDONLY|O_EXCL|O_LARGEFILE|O_NONBLOCK";
			expected[2].extra_info       = null;
			expected[2].have_result      = false;
			expected[2].result           = -1;
			expected[2].is_syscall_start = true;
			expected[2].end_index        = -1;
			expected[2].is_syscall_end   = false;
			expected[2].start_index      = -1;

			expected[3].index            = 3;
			expected[3].pid              = 27920;
			expected[3].tid              = 27920;
			expected[3].execname         = "gimp-2.2";
			expected[3].timestamp        = 1181064008000173;
			expected[3].name             = "open";
			expected[3].arguments        = "\"/home/federico/.gimp-2.2/tool-options/gimp-free-select-tool.presetsysBTNg\", O_RDWR|O_CREAT|O_EXCL|O_LARGEFILE, 0600";
			expected[3].extra_info       = null;
			expected[3].have_result      = false;
			expected[3].result           = -1;
			expected[3].is_syscall_start = true;
			expected[3].end_index        = 5;
			expected[3].is_syscall_end   = false;
			expected[3].start_index      = -1;

			expected[4].index            = 4;
			expected[4].pid              = 2539;
			expected[4].tid              = 26181;
			expected[4].execname         = "NetworkManager";
			expected[4].timestamp        = 1181064008031945;
			expected[4].name             = "open";
			expected[4].arguments        = null;
			expected[4].extra_info       = null;
			expected[4].have_result      = true;
			expected[4].result           = 19;
			expected[4].is_syscall_start = false;
			expected[4].end_index        = -1;
			expected[4].is_syscall_end   = true;
			expected[4].start_index      = -1;

			expected[5].index            = 5;
			expected[5].pid              = 27920;
			expected[5].tid              = 27920;
			expected[5].execname         = "gimp-2.2";
			expected[5].timestamp        = 1181064008000205;
			expected[5].name             = "open";
			expected[5].arguments        = null;
			expected[5].extra_info       = null;
			expected[5].have_result      = true;
			expected[5].result           = 7;
			expected[5].is_syscall_start = false;
			expected[5].end_index        = -1;
			expected[5].is_syscall_end   = true;
			expected[5].start_index      = 3;

			for (i = 0; i < expected.Length; i++) {
				Syscall syscall = log.GetSyscall (i);

				if (!(syscall.Equals (expected[i])))
					goto bail;
			}

			num_syscalls = log.GetNumSyscalls ();
			if (num_syscalls != expected.Length)
				goto bail;

			retval = true;

		bail:
			return retval;
		}

		Log log;
		ISyscallParser parser;
		Aggregator aggregator;

		MainLoop loop;
		int child_status;
	}
}
