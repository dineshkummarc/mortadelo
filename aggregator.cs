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
				"open: 1180976736974992: gnome-panel (3630:3630): \"/proc/partitions\", O_RDONLY",
				"open.return: 1180976736975010: gnome-panel (3630:3630): 27"
			};

			Syscall expected0;
			Syscall expected1;
			Syscall syscall0;
			Syscall syscall1;

			foreach (string l in lines)
				aggregator.ProcessLine (l);

			syscall0 = log.GetSyscall (0);
			syscall1 = log.GetSyscall (1);

			expected0 = new Syscall ();
			expected0.index            = 0;
			expected0.pid              = 3630;
			expected0.tid              = 3630;
			expected0.execname         = "gnome-panel";
			expected0.timestamp        = 1180976736974992;
			expected0.name             = "open";
			expected0.arguments        = "\"/proc/partitions\", O_RDONLY";
			expected0.extra_info       = null;
			expected0.have_result      = false;
			expected0.result           = -1;
			expected0.is_syscall_start = true;
			expected0.end_index        = 1;
			expected0.is_syscall_end   = false;
			expected0.start_index      = -1;

			expected1 = new Syscall ();
			expected1.index            = 1;
			expected1.pid              = 3630;
			expected1.tid              = 3630;
			expected1.execname         = "gnome-panel";
			expected1.timestamp        = 1180976736975010;
			expected1.name             = "open";
			expected1.arguments        = null;
			expected1.extra_info       = null;
			expected1.have_result      = true;
			expected1.result           = 27;
			expected1.is_syscall_start = false;
			expected1.end_index        = -1;
			expected1.is_syscall_end   = true;
			expected1.start_index      = 0;

			Assert.AreEqual (syscall0, expected0, "Start of open syscall");
			Assert.AreEqual (syscall1, expected1, "Return of open syscall");
		}

		SystemtapParser parser;
		Log log;
		Aggregator aggregator;
	}
}
