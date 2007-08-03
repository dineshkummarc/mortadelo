/*
 * Mortadelo - a viewer for system calls
 *
 * timer-throttle.cs - Throttles frequent actions based on a timeout
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
