using System;
using Gtk;
using Mono.Unix.Native;
using Mono.Unix;

using unix = Mono.Unix.Native.Syscall;

namespace Mortadelo {

	public class MortadeloMain {
		public static void Main (string[] args)
		{
			MainWindow window;

			Application.Init ();

			if (user_is_l33t ()) {
				window = new MainWindow ();
				window.ShowAll ();

				Application.Run ();
			}
		}

		static bool user_is_l33t ()
		{
			if (unix.getuid () == 0)
				return true;

			MessageDialog msg = new MessageDialog (
				null,
				DialogFlags.Modal,
				MessageType.Error,
				ButtonsType.Close,
				Mono.Unix.Catalog.GetString ("This program can only be run by the system administrator."));

			msg.Run ();

			return false;
		}

	}

}
