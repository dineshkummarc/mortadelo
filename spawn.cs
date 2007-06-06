using System;
using System.Runtime.InteropServices;
using GLib;

namespace Mortadelo {
	public class Spawn {
		public const int G_SPAWN_LEAVE_DESCRIPTORS_OPEN = 1 << 0;
		public const int G_SPAWN_DO_NOT_REAP_CHILD      = 1 << 1;
		public const int G_SPAWN_SEARCH_PATH            = 1 << 2;
		public const int G_SPAWN_STDOUT_TO_DEV_NULL     = 1 << 3;
		public const int G_SPAWN_STDERR_TO_DEV_NULL     = 1 << 4;
		public const int G_SPAWN_CHILD_INHERITS_STDIN   = 1 << 5;
		public const int G_SPAWN_FILE_AND_ARGV_ZERO     = 1 << 6;

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

		public static void SpawnAsyncWithPipes (string working_directory,
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

		public delegate void ChildWatchFunc (int pid, int status, IntPtr user_data);

		[DllImport ("glib-2.0")]
		static extern uint g_child_watch_add (int pid, ChildWatchFunc watch_func, IntPtr user_data);

		public static uint ChildWatchAdd (int pid, ChildWatchFunc watch_func)
		{
			return g_child_watch_add (pid, watch_func, IntPtr.Zero);
		}
	}
}
