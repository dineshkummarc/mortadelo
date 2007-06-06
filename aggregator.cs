using System;
using System.Collections;
using NUnit.Framework;

namespace Mortadelo {
	public class Aggregator {
		public Aggregator (Log log, ISyscallParser parser)
		{
			this.log = log;
			this.parser = parser;

			tid_to_pending_index = new Hashtable ();
		}

		public void ProcessLine (string str)
		{
			bool parsed;
			Syscall syscall;

			parsed = parser.Parse (str, out syscall);
			if (!parsed)
				return;

			if (syscall.is_syscall_start && !syscall.is_syscall_end)
				add_start_syscall (syscall);
			else if (!syscall.is_syscall_start && syscall.is_syscall_end)
				add_end_syscall (syscall);
		}

		void add_start_syscall (Syscall syscall)
		{
			int new_idx;

			if (tid_to_pending_index.ContainsKey (syscall.tid)) {
				int pending_idx;
				Syscall pending;
				string str;

				pending_idx = (int) tid_to_pending_index[syscall.tid];
				pending = log.GetSyscall (pending_idx);

				str = String.Format ("Got syscall start for '{0}' when TID {1} already had an unfinished call for {2}",
						     syscall.name,
						     syscall.tid,
						     pending.name);
				throw new ApplicationException (str);
			}

			new_idx = log.AppendSyscall (syscall);
			tid_to_pending_index[syscall.tid] = new_idx;
		}

		void add_end_syscall (Syscall syscall)
		{
			int pending_idx;
			Syscall pending;
			int new_idx;

			pending_idx = -1;

			if (tid_to_pending_index.ContainsKey (syscall.tid)) {
				pending_idx = (int) tid_to_pending_index[syscall.tid];
				pending = log.GetSyscall (pending_idx);

				if (pending.name != syscall.name) {
					string str;

					str = String.Format ("Got syscall end for '{0}' when TID {1} had an unfinished call for {2}",
							     syscall.name,
							     syscall.tid,
							     pending.name);
					throw new ApplicationException (str);
				}

				syscall.start_index = pending_idx;
				tid_to_pending_index.Remove (syscall.tid);
			} else
				pending = new Syscall ();

			new_idx = log.AppendSyscall (syscall);

			if (pending_idx != -1) {
				pending.end_index = new_idx;
				log.ModifySyscall (pending_idx, pending);
			}
		}

		Log log;
		ISyscallParser parser;

		Hashtable tid_to_pending_index;
	}

	[TestFixture]
	public class AggregatorTest {

		[SetUp]
		public void Setup ()
		{
			parser = new SystemtapParser ();
			log = new Log ();
			aggregator = new Aggregator (log, parser);
		}

		[Test]
		public void OpenTest () {
			string[] lines = {
				"open: 1180976736974992: gnome-panel (3630:3630): \"/proc/partitions\", O_RDONLY\n",
				"open.return: 1180976736975010: gnome-panel (3630:3630): 27\n",
				"open: 1181064007999786: hald-addon-stor (2882:2883): \"/dev/hdc\", O_RDONLY|O_EXCL|O_LARGEFILE|O_NONBLOCK\n",
				"open: 1181064008000173: gimp-2.2 (27920:27920): \"/home/federico/.gimp-2.2/tool-options/gimp-free-select-tool.presetsysBTNg\", O_RDWR|O_CREAT|O_EXCL|O_LARGEFILE, 0600\n",
				"open.return: 1181064008031945: NetworkManager (2539:26181): 19\n",
				"open.return: 1181064008000205: gimp-2.2 (27920:27920): 7\n",
			};

			Syscall[] expected;
			Syscall[] syscall;
			int i;
			int num_syscalls;

			foreach (string l in lines)
				aggregator.ProcessLine (l);

			expected = new Syscall[lines.Length];
			syscall = new Syscall[lines.Length];

			for (i = 0; i < lines.Length; i++) {
				syscall[i] = log.GetSyscall (i);
				expected[i].Clear ();
			}

			num_syscalls = log.GetNumSyscalls ();

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

			for (i = 0; i < lines.Length; i++) {
				string str;

				str = String.Format ("Start of open syscall ({0})", i);
				Assert.AreEqual (syscall[i], expected[i], str);

				str = String.Format ("Return of open syscall ({0})", i);
				Assert.AreEqual (syscall[i], expected[i], str);
			}

			Assert.AreEqual (num_syscalls, lines.Length, "Number of parsed syscalls");
		}

		SystemtapParser parser;
		Log log;
		Aggregator aggregator;

		public static void Main ()
		{
		}
	}
}
