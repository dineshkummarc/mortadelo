/*
 * Mortadelo - a viewer for system calls
 *
 * memory-profile.cs - Simple program to test memory consumption of the syscall logs
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
using System.IO;

namespace Mortadelo {
	public class MemoryProfile {
		public static int Main (string[] args)
		{
			if (args.Length != 1) {
				Console.WriteLine ("usage: mortadelo-memory-profile <systemtap-log-file>");
				return 1;
			}

			ISyscallParser parser;
			LogIO io;
			StreamReader reader;
			Stream stream;
			Log log;

			parser = new SystemtapParser ();
			io = new LogIO ();
			reader = new StreamReader (args[0]);
			stream = reader.BaseStream;
			log = io.Load (reader, parser);

			Console.WriteLine ("{0} syscalls processed ({1:0.00} KB from the file)",
					   log.GetNumSyscalls (),
					   stream.Position / 1024.0);

			return 0;
		}
	}
}
