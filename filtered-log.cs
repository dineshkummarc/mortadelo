using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Mortadelo {

	public class FilteredLog : ILogProvider {
		public FilteredLog (ILogProvider log, ISyscallFilter filter)
		{
			if (log == null)
				throw new ArgumentNullException ("log");

			if (filter == null)
				throw new ArgumentNullException ("filter");

			this.log = log;
			log.SyscallInserted += full_log_syscall_inserted_cb;
			log.SyscallRemoved += full_log_syscall_removed_cb;
			log.SyscallModified += full_log_syscall_modified_cb;

			this.filter = filter;

			num_updated_syscalls = 0;
			filtered_syscalls = new List<FilteredSyscall> ();

			orig_to_mapped_index = new Hashtable ();

			populate ();
		}

		public int GetNumSyscalls ()
		{
			return filtered_syscalls.Count;
		}

		public Syscall GetSyscall (int num)
		{
			return filtered_syscalls[num].syscall;
		}

		void populate ()
		{
			int num;
			int i;

			num = log.GetNumSyscalls ();
			for (i = 0; i < num; i++)
				process_syscall (i);

			num_updated_syscalls = i;
		}

		void process_syscall (int num)
		{
			int new_index;
			Syscall syscall;
			FilteredSyscall filtered;

			syscall = log.GetSyscall (num);

			if (!filter.Match (syscall))
				return;

			new_index = filtered_syscalls.Count;
			Debug.Assert (!orig_to_mapped_index.ContainsKey (num));
			orig_to_mapped_index[num] = new_index;

			filtered = new FilteredSyscall (syscall, filter.GetMatches ());

			filtered_syscalls.Add (filtered);

			if (SyscallInserted != null)
				SyscallInserted (new_index);
		}

		void full_log_syscall_inserted_cb (int num)
		{
			int new_num_syscalls = log.GetNumSyscalls ();

			Debug.Assert (new_num_syscalls == num_updated_syscalls + 1, "Number of updated syscalls");
			Debug.Assert (num == new_num_syscalls - 1, "FilteredLog only supports appends to the base model");

			num_updated_syscalls = new_num_syscalls;
			process_syscall (num_updated_syscalls - 1);
		}

		void full_log_syscall_modified_cb (int num)
		{
			Syscall syscall;
			int mapped_idx;

			syscall = log.GetSyscall (num);

			if (orig_to_mapped_index.ContainsKey (num))
				mapped_idx = (int) orig_to_mapped_index[num];
			else
				mapped_idx = -1;

			if (filter.Match (syscall)) {
				List<SyscallMatch> matches;

				matches = filter.GetMatches ();

				if (mapped_idx == -1) {
					/* The syscall didn't exist, so add it as a new one */
					new_syscall_inserted (syscall, matches, num);
				} else {
					/* The syscall already existed; just modify it. */
					existing_syscall_modified (syscall, matches, num, mapped_idx);
				}
			} else {
				/* The syscall doesn't match.  If it matched before, remove it */
				if (mapped_idx != -1)
					existing_syscall_removed (num, mapped_idx);
			}
		}

		void full_log_syscall_removed_cb (int num)
		{
			Debug.Assert (false, "FilteredLog does not support removals from the base model yet");
		}

		void new_syscall_inserted (Syscall syscall, List<SyscallMatch> matches, int orig_idx)
		{
			int[] original_indices;
			int orig_insert_pos;
			int i;

			original_indices = get_original_indices ();
			orig_insert_pos = Array.BinarySearch (original_indices, orig_idx);

			Debug.Assert (orig_insert_pos < 0); /* This row didn't exist!  We'll insert it. */
			orig_insert_pos = ~orig_insert_pos;

			/* Shift all the subsequent rows down */

			for (i = original_indices.Length - 1; i >= orig_insert_pos; i--) {
				int o;
				int m;

				o = original_indices[i];
				m = (int) orig_to_mapped_index[o];
				orig_to_mapped_index.Remove (o);
				orig_to_mapped_index[o + 1] = m + 1;
			}

			/* Insert the new row */

			filtered_syscalls.Insert (orig_insert_pos, new FilteredSyscall (syscall, matches));
			orig_to_mapped_index[orig_idx] = orig_insert_pos;

			/* Notify upstream */
			if (SyscallInserted != null)
				SyscallInserted (orig_insert_pos);
		}

		int[] get_original_indices ()
		{
			int num_original_indices;
			int[] original_indices;
			int i;

			num_original_indices = orig_to_mapped_index.Count;
			original_indices = new int[num_original_indices];

			i = 0;

			foreach (int idx in orig_to_mapped_index.Keys)
				original_indices[i++] = idx;

			Array.Sort (original_indices);

			return original_indices;
		}

		void existing_syscall_modified (Syscall syscall, List<SyscallMatch> matches, int orig_idx, int mapped_idx)
		{
			filtered_syscalls[mapped_idx] = new FilteredSyscall (syscall, matches);

			/* Notify upstream */
			if (SyscallModified != null)
				SyscallModified (mapped_idx);
		}

		void existing_syscall_removed (int orig_idx, int mapped_idx)
		{
			int i;
			int num;

			/* Remove the old row... */

			filtered_syscalls.RemoveAt (mapped_idx);
			orig_to_mapped_index.Remove (orig_idx);

			/* ... shift the following rows upward ... */

			num = log.GetNumSyscalls ();
			for (i = orig_idx + 1; i < num; i++)
				if (orig_to_mapped_index.ContainsKey (i)) {
					int m;

					m = (int) orig_to_mapped_index[i];
					orig_to_mapped_index.Remove (i);
					orig_to_mapped_index[i - 1] = m - 1;
				}

			/* ... and notify upstream. */

			if (SyscallRemoved != null)
				SyscallRemoved (mapped_idx);
		}

		internal struct FilteredSyscall {
			internal Syscall syscall;
			internal List<SyscallMatch> matches;

			internal FilteredSyscall (Syscall syscall, List<SyscallMatch> matches)
			{
				this.syscall = syscall;
				this.matches= matches;
			}
		}

		ILogProvider log;
		int num_updated_syscalls;
		ISyscallFilter filter;

		List<FilteredSyscall> filtered_syscalls;
		Hashtable orig_to_mapped_index;

		public event SyscallInsertedHandler SyscallInserted;
		public event SyscallModifiedHandler SyscallModified;
		public event SyscallRemovedHandler SyscallRemoved;
	}

	[TestFixture]
	public class FilteredLogTest {
		[SetUp]
		public void Setup ()
		{
			parser = new SystemtapParser ();
			log = new Log ();
			compact_log = new CompactLog (log);

			filtered_log = new FilteredLog (compact_log, new RegexFilter (new Regex ("\\?")));
			aggregator = new Aggregator (log, parser);

			make_expected_syscalls ();
		}

		[Test]
		public void TestFilteredLog () {
			string[] lines = {
				"open: 1180976736974992: gnome-panel (3630:3630): \"/proc/partitions\", O_RDONLY\n",
				"open.return: 1180976736975010: gnome-panel (3630:3630): 27\n",
				"open: 1181064007999786: hald-addon-stor (2882:2883): \"/dev/hdc\", O_RDONLY|O_EXCL|O_LARGEFILE|O_NONBLOCK\n",
				"open: 1181064008000173: gimp-2.2 (27920:27920): \"/home/federico/.gimp-2.2/tool-options/gimp-free-select-tool.presetsysBTNg?\", O_RDWR|O_CREAT|O_EXCL|O_LARGEFILE, 0600\n",
				"open.return: 1181064008031945: NetworkManager (2539:26181): 19\n",
				"open.return: 1181064008000205: gimp-2.2 (27920:27920): 7\n",
			};

			int[] expected_added = {
				0,
				-1,
				0,
				1,
				-1,
				-1
			};

			int[] expected_modified = {
				-1,
				-1,
				-1,
				-1,
				-1,
				1
			};

			int[] expected_removed = {
				-1,
				0,
				-1,
				-1,
				-1,
				-1
			};

			int i;

			int modified_idx;
			int added_idx;
			int removed_idx;

			filtered_log.SyscallInserted += delegate (int num) {
				added_idx = num;
			};

			filtered_log.SyscallModified += delegate (int num) {
				modified_idx = num;
			};

			filtered_log.SyscallRemoved += delegate (int num) {
				removed_idx = num;
			};

			for (i = 0; i < lines.Length; i++) {
				modified_idx = -1;
				added_idx = -1;
				removed_idx = -1;

				aggregator.ProcessLine (lines[i]);

				Assert.AreEqual (expected_added[i], added_idx,
						 String.Format ("Emission of SyscallAdded for full syscall {0}", i));

				Assert.AreEqual (expected_modified[i], modified_idx,
						 String.Format ("Emission of SyscallModified after processing full syscall {0}", i));

				Assert.AreEqual (expected_removed[i], removed_idx,
						 String.Format ("Emission of SyscallRemoved for full syscall {0}", i));
			}

			for (i = 0; i < expected_syscalls.Length; i++) {
				Assert.AreEqual (expected_syscalls[i], filtered_log.GetSyscall (i),
						 String.Format ("Contents of filtered syscall {0}", i));
			}

			Assert.AreEqual (expected_syscalls.Length, filtered_log.GetNumSyscalls (), "Number of filtered syscalls");
		}

		void make_expected_syscalls ()
		{
			int i;

			expected_syscalls = new Syscall[2];
			for (i = 0; i < expected_syscalls.Length; i++)
				expected_syscalls[i].Clear ();

			expected_syscalls[0].index            = 2;
			expected_syscalls[0].pid              = 2882;
			expected_syscalls[0].tid              = 2883;
			expected_syscalls[0].execname         = "hald-addon-stor";
			expected_syscalls[0].timestamp        = 1181064007999786;
			expected_syscalls[0].name             = "open";
			expected_syscalls[0].arguments        = "\"/dev/hdc\", O_RDONLY|O_EXCL|O_LARGEFILE|O_NONBLOCK";
			expected_syscalls[0].extra_info       = null;
			expected_syscalls[0].have_result      = false;
			expected_syscalls[0].result           = -1;
			expected_syscalls[0].is_syscall_start = true;
			expected_syscalls[0].end_index        = -1;
			expected_syscalls[0].is_syscall_end   = false;
			expected_syscalls[0].start_index      = 1;

			expected_syscalls[1].index            = 3;
			expected_syscalls[1].pid              = 27920;
			expected_syscalls[1].tid              = 27920;
			expected_syscalls[1].execname         = "gimp-2.2";
			expected_syscalls[1].timestamp        = 1181064008000173;
			expected_syscalls[1].name             = "open";
			expected_syscalls[1].arguments        = "\"/home/federico/.gimp-2.2/tool-options/gimp-free-select-tool.presetsysBTNg?\", O_RDWR|O_CREAT|O_EXCL|O_LARGEFILE, 0600";
			expected_syscalls[1].extra_info       = null;
			expected_syscalls[1].have_result      = true;
			expected_syscalls[1].result           = 7;
			expected_syscalls[1].is_syscall_start = true;
			expected_syscalls[1].end_index        = 2;
			expected_syscalls[1].is_syscall_end   = true;
			expected_syscalls[1].start_index      = 2;
		}

		SystemtapParser parser;
		Log log;
		CompactLog compact_log;
		FilteredLog filtered_log;
		Aggregator aggregator;

		Syscall[] expected_syscalls;
	}

}
