using System;
using Mono.Unix;

namespace Mortadelo {
	public class UnixReader {
		public UnixReader (int fd)
		{
			stream = new UnixStream (fd, false);

			io_channel = new NDesk.GLib.IOChannel (fd);
			watch_id = NDesk.GLib.IO.AddWatch (io_channel,
							   NDesk.GLib.IOCondition.In | NDesk.GLib.IOCondition.Hup,
							   io_callback);
		}

		bool io_callback (NDesk.GLib.IOChannel source, NDesk.GLib.IOCondition condition, IntPtr data)
		{
			if ((condition & NDesk.GLib.IOCondition.In) != 0)
				read ();

			if ((condition & NDesk.GLib.IOCondition.Hup) != 0) {
				Closed ();
				return false;
			}

			return true;
		}

		void read ()
		{
			byte[] buffer = new byte[BUFFER_SIZE];
			int num_read;

			while ((num_read = stream.Read (buffer, 0, buffer.Length)) != 0)
				DataAvailable (buffer, num_read);
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
