using System;
using Mono.Unix;

namespace Mortadelo {
	public class UnixReader : IDisposable {
		public UnixReader (int fd)
		{
			stream = new UnixStream (fd, false);

			io_channel = new NDesk.GLib.IOChannel (fd);
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
