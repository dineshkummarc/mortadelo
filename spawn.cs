using System;
using System.IO;
using System.Runtime.InteropServices;
using GLib;
using Mono.Unix;
using Mono.Unix.Native;
using NUnit.Framework;

namespace Mortadelo {
	public class Spawn {
		public const int G_SPAWN_LEAVE_DESCRIPTORS_OPEN = 1 << 0;
		public const int G_SPAWN_DO_NOT_REAP_CHILD      = 1 << 1;
		public const int G_SPAWN_SEARCH_PATH            = 1 << 2;
		public const int G_SPAWN_STDOUT_TO_DEV_NULL     = 1 << 3;
		public const int G_SPAWN_STDERR_TO_DEV_NULL     = 1 << 4;
		public const int G_SPAWN_CHILD_INHERITS_STDIN   = 1 << 5;
		public const int G_SPAWN_FILE_AND_ARGV_ZERO     = 1 << 6;

		public Spawn ()
		{
		}

		[DllImport ("glib-2.0")]
		static extern bool g_spawn_async_with_pipes (string working_directory,
							     IntPtr argv,
							     IntPtr envp,
							     int    flags,
							     IntPtr child_setup_fn,
							     IntPtr user_data,
							     out int child_pid,
							     out int stdin,
							     out int stdout,
							     out int stderr,
							     out IntPtr error);

		static IntPtr make_native_string_array (string[] strs)
		{
			IntPtr native;
			int i;

			if (strs == null)
				return IntPtr.Zero;

			native = Marshal.AllocHGlobal ((strs.Length + 1) * IntPtr.Size);

			for (i = 0; i < strs.Length; i++)
				Marshal.WriteIntPtr (native, i * IntPtr.Size, GLib.Marshaller.StringToPtrGStrdup (strs[i]));

                        Marshal.WriteIntPtr (native, strs.Length * IntPtr.Size, IntPtr.Zero); /* null terminator */

			return native;
		}

		static void free_native_string_array (IntPtr native, string[] original_array)
		{
			int i;

			if (native == IntPtr.Zero)
				return;

			for (i = 0; i < original_array.Length; i++)
				GLib.Marshaller.Free (Marshal.ReadIntPtr (native, i * IntPtr.Size));

			Marshal.FreeHGlobal (native);
		}

		public void SpawnAsyncWithPipes (string working_directory,
						 string[] argv,
						 string[] envp,
						 int flags,
						 out int child_pid,
						 out int stdin,
						 out int stdout,
						 out int stderr)
		{
			bool result;
			IntPtr argv_native;
			IntPtr envp_native;
			IntPtr error;

			argv_native = make_native_string_array (argv);
			envp_native = make_native_string_array (envp);

			result = g_spawn_async_with_pipes (working_directory,
							   argv_native,
							   envp_native,
							   flags,
							   IntPtr.Zero,
							   IntPtr.Zero,
							   out child_pid,
							   out stdin,
							   out stdout,
							   out stderr,
							   out error);

			free_native_string_array (argv_native, argv);
			free_native_string_array (envp_native, envp);

			if (!result)
				throw new GException (error);
		}

		public delegate void ChildWatchFunc (int pid, int status);

		delegate void GChildWatchFunc (int pid, int status, IntPtr user_data);

		[DllImport ("glib-2.0")]
		static extern uint g_child_watch_add (int pid, GChildWatchFunc watch_func, IntPtr user_data);

		public uint ChildWatchAdd (int pid, ChildWatchFunc watch_func)
		{
			this.watch_func = new ChildWatchFunc (watch_func);

			/* We hold this proxy in an object field.  Otherwise,
			 * the GC would collect the trampoline shortly after the
			 * invocation of g_child_watch_add(); this would crash
			 * the program later when Glib wants to call our defunct
			 * callback trampoline.
			 */
			watch_func_proxy = new GChildWatchFunc (watch_func_proxy_cb);

			return g_child_watch_add (pid, watch_func_proxy, IntPtr.Zero);
		}

		public void watch_func_proxy_cb (int pid, int status, IntPtr user_data)
		{
			watch_func (pid, status);
		}

		ChildWatchFunc watch_func;
		GChildWatchFunc watch_func_proxy;
	}

	[TestFixture]
	public class SpawnTest {
		[Test]
		public void TestSpawn ()
		{
			Spawn spawn;
			UnixReader reader;
			string[] argv = { "/bin/cat" };
			UnixStream stream;
			StreamWriter writer;
			int stdin, stdout, stderr;

			spawn = new Spawn ();

			spawn.SpawnAsyncWithPipes (null,
						   argv,
						   null,
						   Spawn.G_SPAWN_DO_NOT_REAP_CHILD,
						   out pid,
						   out stdin, out stdout, out stderr);

			pids_matched = false;
			exit_status_is_good = false;
			spawn.ChildWatchAdd (pid, child_watch_cb);

			stream = new UnixStream (stdin, true);
			writer = new StreamWriter (stream);

			writer.Write ("Hello, world!");
			writer.Close (); /* this will close the stdin fd */

			reader = new UnixReader (stdout);
			reader.DataAvailable += data_available_cb;
			reader.Closed += closed_cb;

			string_equal = false;
			closed = false;

			loop = new MainLoop ();
			loop.Run ();

			Assert.IsTrue (string_equal, "Read the correct string");
			Assert.IsTrue (closed, "UnixReader got closed");
			Assert.IsTrue (pids_matched, "PID of child process");
			Assert.IsTrue (exit_status_is_good, "Exit status of child process");
		}

		void data_available_cb (byte[] buffer, int len)
		{
			MemoryStream stream = new MemoryStream (buffer, 0, len);
			StreamReader reader = new StreamReader (stream);
			string str;

			str = reader.ReadToEnd ();
			if (str == "Hello, world!")
				string_equal = true;
		}

		void closed_cb ()
		{
			closed = true;
		}

		void child_watch_cb (int child_pid, int status)
		{
			if (child_pid == pid)
				pids_matched = true;

			if (Mono.Unix.Native.Syscall.WIFEXITED (status)
			    && Mono.Unix.Native.Syscall.WEXITSTATUS (status) == 0)
				exit_status_is_good = true;

			loop.Quit ();
		}

		int pid;
		bool string_equal;
		bool closed;
		bool pids_matched;
		bool exit_status_is_good;
		MainLoop loop;

		public static void Main () {}
	}
}
