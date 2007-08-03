/*
 * Mortadelo - a viewer for system calls
 *
 * main.cs - main program
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
using Gtk;
using Mono.Unix.Native;
using Mono.Unix;

using unix = Mono.Unix.Native.Syscall;

namespace Mortadelo {

	public class MortadeloMain {
		public static int Main (string[] args)
		{
			MainWindow window;

			Application.Init ();

			if (user_is_l33t ()) {
				window = new MainWindow ();
				window.ShowAll ();

				Application.Run ();
				return 0;
			} else
				return 1;
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
