/*
 * Mortadelo - a viewer for system calls
 *
 * unix-reader.cs - Watches a file descriptor for data and reads it when available
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
using Mono.Unix;

namespace Mortadelo {
	public class UnixReader : IDisposable {
		public UnixReader (int fd)
		{
			stream = new UnixStream (fd, false);

			io_channel = new NDesk.GLib.IOChannel (fd);
			io_channel.Flags |= NDesk.GLib.IOFlags.Nonblock;
			watch_id = NDesk.GLib.IO.AddWatch (io_channel,
							   NDesk.GLib.IOCondition.In | NDesk.GLib.IOCondition.Hup,
							   io_callback);
		}

		public void Dispose ()
		{
			if (watch_id != 0) {
				GLib.Source.Remove (watch_id);
				watch_id = 0;
			}
		}

		bool io_callback (NDesk.GLib.IOChannel source, NDesk.GLib.IOCondition condition, IntPtr data)
		{
			if ((condition & NDesk.GLib.IOCondition.In) != 0)
				read ();

			if ((condition & NDesk.GLib.IOCondition.Hup) != 0) {
				Closed ();
				watch_id = 0;
				return false;
			}

			return true;
		}

		void read ()
		{
			byte[] buffer = new byte[BUFFER_SIZE];
			int num_read;

			do {
				try {
					num_read = stream.Read (buffer, 0, buffer.Length);
				} catch (Mono.Unix.UnixIOException e) {
					if (e.ErrorCode == Mono.Unix.Native.Errno.EWOULDBLOCK)
						num_read = 0;
					else
						throw (e);
					/* UnixStream already handles EINTR for us */
				}

				if (num_read > 0)
					DataAvailable (buffer, num_read);
			} while (num_read > 0);
		}

		const int BUFFER_SIZE = 65536;

		UnixStream stream;
		uint watch_id;
		NDesk.GLib.IOChannel io_channel;

		public delegate void DataAvailableDelegate (byte[] buffer, int len);
		public delegate void ClosedDelegate ();

		public event DataAvailableDelegate DataAvailable;
		public event ClosedDelegate Closed;
	}
}
