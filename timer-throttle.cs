using System;
using System.Diagnostics;
using GLib;

namespace Mortadelo {

	public class TimerThrottle {
		public TimerThrottle (int msec)
		{
			this.msec = msec;
			this.timeout_id = 0;
		}

		public void Start ()
		{
			if (timeout_id != 0)
				GLib.Source.Remove (timeout_id);

			timeout_id = GLib.Timeout.Add ((uint) msec, timeout_cb);
		}

		public void Stop ()
		{
			if (timeout_id == 0)
				return;

			GLib.Source.Remove (timeout_id);
			timeout_id = 0;
		}

		bool timeout_cb ()
		{
			if (Trigger != null)
				Trigger ();

			timeout_id = 0;
			return false;
		}

		int msec; /* milliseconds */
		uint timeout_id;

		public delegate void TriggerHandler ();
		public event TriggerHandler Trigger;
	}

}
