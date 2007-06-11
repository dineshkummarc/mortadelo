using System;
using Gtk;

namespace Mortadelo {

	public class MortadeloMain {
		public static void Main (string[] args)
		{
			MainWindow window;

			Application.Init ();

			window = new MainWindow ();
			window.ShowAll ();

			Application.Run ();
		}
	}

}
