using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;

namespace Mortadelo {

	public class CompactLog : ILogProvider {
		public CompactLog (ILogProvider log)
		{
			if (log == null)
				throw new ArgumentNullException ("log");

			this.log = log;

			num_updated_syscalls = 0;
			syscalls = new List<MappedSyscall> ();

			orig_to_mapped_index = new Hashtable ();
		}

		public void Update ()
		{
			int old_num_updated_syscalls = num_updated_syscalls;
			int new_num_syscalls = log.GetNumSyscalls ();
			int i;

			Debug.Assert (new_num_syscalls >= old_num_updated_syscalls);

			if (old_num_updated_syscalls == new_num_syscalls)
				return;

			num_updated_syscalls = new_num_syscalls;

			for (i = old_num_updated_syscalls; i < new_num_syscalls; i++)
				process_full_syscall (i);
		}

		public int GetNumSyscalls ()
		{
			return syscalls.Count;
		}

		public Syscall GetSyscall (int num)
		{
			return syscalls[num].syscall;
		}

		void process_full_syscall (int num)
		{
			Syscall syscall;

			syscall = log.GetSyscall (num);

			if (syscall.is_syscall_start)
				add_syscall (syscall);
			else if (syscall.is_syscall_end)
				add_end_syscall (syscall);
			else
				Debug.Assert (false, "not reached");
		}

		void add_syscall (Syscall syscall)
		{
			MappedSyscall mapped;

			mapped = new MappedSyscall ();

			mapped.original_idx = syscall.index;
			syscall.index = syscalls.Count;

			Debug.Assert (!orig_to_mapped_index.ContainsKey (mapped.original_idx));
			orig_to_mapped_index[mapped.original_idx] = syscall.index;

			if (syscall.is_syscall_start)
				syscall.start_index = syscall.index;

			if (syscall.is_syscall_end)
				syscall.end_index = syscall.index;

			mapped.syscall = syscall;

			syscalls.Add (mapped);
			orig_to_mapped_index[mapped.original_idx] = syscall.index;
		}

		void add_end_syscall (Syscall syscall)
		{
			if (syscall.start_index == -1) {
				/* We don't have a corresponding start syscall,
				 * so we just add this end syscall as-is.
				 */

				add_syscall (syscall);
			} else {
				/* Find the corresponding start syscall, and
				 * modify it to know the end syscall's result.
				 */
				int mapped_start_idx = (int) orig_to_mapped_index[syscall.start_index];
				MappedSyscall mapped_start = syscalls[mapped_start_idx];

				Debug.Assert (mapped_start.syscall.name == syscall.name
					      && mapped_start.syscall.pid == syscall.pid
					      && mapped_start.syscall.tid == syscall.tid
					      && !mapped_start.syscall.have_result
					      && mapped_start.syscall.is_syscall_start
					      && !mapped_start.syscall.is_syscall_end
					      && mapped_start.syscall.end_index == -1);

				mapped_start.syscall.have_result = syscall.have_result;
				mapped_start.syscall.result = syscall.result;
				mapped_start.syscall.is_syscall_end = true;
				mapped_start.syscall.end_index = mapped_start_idx;

				syscalls[mapped_start_idx] = mapped_start;

				if (SyscallModified != null)
					SyscallModified (mapped_start_idx);
			}
		}

		ILogProvider log;
		int num_updated_syscalls;

		struct MappedSyscall {
			public int original_idx;

			public Syscall syscall;
		}

		List<MappedSyscall> syscalls;
		Hashtable orig_to_mapped_index;

		public event SyscallModifiedHandler SyscallModified;
	}

	[TestFixture]
	public class CompactLogTest {
		[SetUp]
		public void Setup ()
		{
			parser = new SystemtapParser ();
			log = new Log ();
			compact_log = new CompactLog (log);
			aggregator = new Aggregator (log, parser);

			make_expected_syscalls ();
		}

		[Test]
		public void TestCompactLog ()
		{
			string[] lines = {
				"open: 1180976736974992: gnome-panel (3630:3630): \"/proc/partitions\", O_RDONLY\n",
				"open.return: 1180976736975010: gnome-panel (3630:3630): 27\n",
				"open: 1181064007999786: hald-addon-stor (2882:2883): \"/dev/hdc\", O_RDONLY|O_EXCL|O_LARGEFILE|O_NONBLOCK\n",
				"open: 1181064008000173: gimp-2.2 (27920:27920): \"/home/federico/.gimp-2.2/tool-options/gimp-free-select-tool.presetsysBTNg\", O_RDWR|O_CREAT|O_EXCL|O_LARGEFILE, 0600\n",
				"open.return: 1181064008031945: NetworkManager (2539:26181): 19\n",
				"open.return: 1181064008000205: gimp-2.2 (27920:27920): 7\n",
			};

			int[] expected_generated = {
				1,
				0,
				1,
				1,
				1,
				0
			};

			int[] expected_modified = {
				-1,
				0,
				-1,
				-1,
				-1,
				2
			};

			int i;

			int generated_syscalls = 0;
			int modified_idx;

			compact_log.SyscallModified += delegate (int num) {
				modified_idx = num;
			};

			for (i = 0; i < lines.Length; i++) {
				int new_generated_syscalls;
				string str;

				modified_idx = -1;
				aggregator.ProcessLine (lines[i]);
				compact_log.Update ();
				new_generated_syscalls = compact_log.GetNumSyscalls ();

				str = String.Format ("Compact syscalls generated after processing full syscall {0}", i);
				Assert.AreEqual (expected_generated[i], new_generated_syscalls - generated_syscalls, str);

				str = String.Format ("Compact syscall modified after processing full syscall {0}", i);
				Assert.AreEqual (expected_modified[i], modified_idx, str);

				generated_syscalls = new_generated_syscalls;
			}

			for (i = 0; i < expected_syscalls.Length; i++) {
				string str;

				str = String.Format ("Contents of compact syscall {0}", i);
				Assert.AreEqual (expected_syscalls[i], compact_log.GetSyscall (i), str);
			}

			Assert.AreEqual (expected_syscalls.Length, compact_log.GetNumSyscalls (), "Number of compacted syscalls");
		}

		void make_expected_syscalls ()
		{
			int i;

			expected_syscalls = new Syscall[4];
			for (i = 0; i < expected_syscalls.Length; i++)
				expected_syscalls[i].Clear ();

			expected_syscalls[0].index            = 0;
			expected_syscalls[0].pid              = 3630;
			expected_syscalls[0].tid              = 3630;
			expected_syscalls[0].execname         = "gnome-panel";
			expected_syscalls[0].timestamp        = 1180976736974992;
			expected_syscalls[0].name             = "open";
			expected_syscalls[0].arguments        = "\"/proc/partitions\", O_RDONLY";
			expected_syscalls[0].extra_info       = null;
			expected_syscalls[0].have_result      = true;
			expected_syscalls[0].result           = 27;
			expected_syscalls[0].is_syscall_start = true;
			expected_syscalls[0].end_index        = 0;
			expected_syscalls[0].is_syscall_end   = true;
			expected_syscalls[0].start_index      = 0;

			expected_syscalls[1].index            = 1;
			expected_syscalls[1].pid              = 2882;
			expected_syscalls[1].tid              = 2883;
			expected_syscalls[1].execname         = "hald-addon-stor";
			expected_syscalls[1].timestamp        = 1181064007999786;
			expected_syscalls[1].name             = "open";
			expected_syscalls[1].arguments        = "\"/dev/hdc\", O_RDONLY|O_EXCL|O_LARGEFILE|O_NONBLOCK";
			expected_syscalls[1].extra_info       = null;
			expected_syscalls[1].have_result      = false;
			expected_syscalls[1].result           = -1;
			expected_syscalls[1].is_syscall_start = true;
			expected_syscalls[1].end_index        = -1;
			expected_syscalls[1].is_syscall_end   = false;
			expected_syscalls[1].start_index      = 1;

			expected_syscalls[2].index            = 2;
			expected_syscalls[2].pid              = 27920;
			expected_syscalls[2].tid              = 27920;
			expected_syscalls[2].execname         = "gimp-2.2";
			expected_syscalls[2].timestamp        = 1181064008000173;
			expected_syscalls[2].name             = "open";
			expected_syscalls[2].arguments        = "\"/home/federico/.gimp-2.2/tool-options/gimp-free-select-tool.presetsysBTNg\", O_RDWR|O_CREAT|O_EXCL|O_LARGEFILE, 0600";
			expected_syscalls[2].extra_info       = null;
			expected_syscalls[2].have_result      = true;
			expected_syscalls[2].result           = 7;
			expected_syscalls[2].is_syscall_start = true;
			expected_syscalls[2].end_index        = 2;
			expected_syscalls[2].is_syscall_end   = true;
			expected_syscalls[2].start_index      = 2;

			expected_syscalls[3].index            = 3;
			expected_syscalls[3].pid              = 2539;
			expected_syscalls[3].tid              = 26181;
			expected_syscalls[3].execname         = "NetworkManager";
			expected_syscalls[3].timestamp        = 1181064008031945;
			expected_syscalls[3].name             = "open";
			expected_syscalls[3].arguments        = null;
			expected_syscalls[3].extra_info       = null;
			expected_syscalls[3].have_result      = true;
			expected_syscalls[3].result           = 19;
			expected_syscalls[3].is_syscall_start = false;
			expected_syscalls[3].end_index        = 3;
			expected_syscalls[3].is_syscall_end   = true;
			expected_syscalls[3].start_index      = -1;
		}

		SystemtapParser parser;
		Log log;
		CompactLog compact_log;
		Aggregator aggregator;

		Syscall[] expected_syscalls;
	}
}
