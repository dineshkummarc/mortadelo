using System;
using Mono.Unix;

namespace Mortadelo {
	public class UnixReader {
		public UnixReader (int fd)
		{
			stream = new UnixStream (fd, false);

			io_channel = new NDesk.GLib.IOChannel (fd);
			watch_id = AddWatch (io_channel, NDesk.GLib.IOCondition.In | NDesk.GLib.IOCondition.Hup, io_callback);
		}

		bool io_callback (NDesk.GLib.IOChannel source, NDesk.GLib.IOCondition condition, IntPtr data)
		{
			if (condition & NDesk.GLib.IOCondition.In)
				read ();

			if (condition & NDesk.GLib.IOCondition.Hup) {
				Closed ();
				return false;
			}

			return true;
		}

		void read ()
		{
			byte[BUFFER_SIZE] buffer;
			int num_read;

			while ((num_read = stream.Read (buffer, 0, buffer.Length)) != 0) {
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
