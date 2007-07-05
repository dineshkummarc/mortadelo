using System;
using System.IO;
using System.Text;
using GLib;
using Mono.Unix;
using Gtk;

namespace Mortadelo {
	public class SystemtapRunner : AggregatorRunner {
		public SystemtapRunner (Log log):
			base ()
		{
			parser = new SystemtapParser ();
			aggregator = new Aggregator (log, parser);
		}

		public void Run ()
		{
			base.Run (aggregator, build_systemtap_argv (), build_script ());
		}

		string[] build_systemtap_argv ()
		{
			string[] argv = new string[2];

			argv[0] = "stap";
			argv[1] = "-";

			return argv;
		}

		string build_script ()
		{
			StringBuilder builder = new StringBuilder ();

			builder.Append (@"
probe syscallgroup.filename_begin = 
	syscall.access,
	syscall.acct,
	syscall.chdir,
	syscall.chmod,
	syscall.chown,
#	syscall.chown32,
	syscall.chroot,
	syscall.creat,
	syscall.execve,
#	syscall.faccessat
#	syscall.fchmodat,
#	syscall.fchownat,
#	syscall.fstatat64,
#	syscall.futimesat,
	syscall.getcwd,
	syscall.getxattr,
	syscall.lchown,
#	syscall.lchown32,
	syscall.lgetxattr,
	syscall.link,
#	syscall.linkat,
	syscall.listxattr,
	syscall.llistxattr,
	syscall.lremovexattr,
	syscall.lsetxattr,
	syscall.lstat,
#	syscall.lstat64,
	syscall.mkdir,
	syscall.mkdirat,
	syscall.mknod,
#	syscall.mknodat,
	syscall.mount,
#	syscall.oldlstat,
#	syscall.oldstat,
#	syscall.oldumount,
	syscall.open,
#	syscall.openat,
	syscall.pivot_root,
	syscall.readlink,
#	syscall.readlinkat,
	syscall.removexattr,
	syscall.rename,
#	syscall.renameat,
	syscall.rmdir,
	syscall.setxattr,
	syscall.stat,
#	syscall.stat64,
	syscall.statfs,
	syscall.statfs64,
	syscall.swapon,
	syscall.symlink,
#	syscall.symlinkat,
	syscall.truncate,
#	syscall.truncate64,
	syscall.umount,
	syscall.unlink,
#	syscall.unlinkat,
	syscall.uselib,
	syscall.utime,
	syscall.utimes
{
}

probe syscallgroup.filename_end = 
	syscall.access.return,
	syscall.acct.return,
	syscall.chdir.return,
	syscall.chmod.return,
	syscall.chown.return,
#	syscall.chown32.return,
	syscall.chroot.return,
	syscall.creat.return,
	syscall.execve.return,
#	syscall.faccessat.return
#	syscall.fchmodat.return,
#	syscall.fchownat.return,
#	syscall.fstatat64.return,
#	syscall.futimesat.return,
	syscall.getcwd.return,
	syscall.getxattr.return,
	syscall.lchown.return,
#	syscall.lchown32.return,
	syscall.lgetxattr.return,
	syscall.link.return,
#	syscall.linkat.return,
	syscall.listxattr.return,
	syscall.llistxattr.return,
	syscall.lremovexattr.return,
	syscall.lsetxattr.return,
	syscall.lstat.return,
#	syscall.lstat64.return,
	syscall.mkdir.return,
	syscall.mkdirat.return,
	syscall.mknod.return,
#	syscall.mknodat.return,
	syscall.mount.return,
#	syscall.oldlstat.return,
#	syscall.oldstat.return,
#	syscall.oldumount.return,
	syscall.open.return,
#	syscall.openat.return,
	syscall.pivot_root.return,
	syscall.readlink.return,
#	syscall.readlinkat.return,
	syscall.removexattr.return,
	syscall.rename.return,
#	syscall.renameat.return,
	syscall.rmdir.return,
	syscall.setxattr.return,
	syscall.stat.return,
#	syscall.stat64.return,
	syscall.statfs.return,
	syscall.statfs64.return,
	syscall.swapon.return,
	syscall.symlink.return,
#	syscall.symlinkat.return,
	syscall.truncate.return,
#	syscall.truncate64.return,
	syscall.umount.return,
	syscall.unlink.return,
#	syscall.unlinkat.return,
	syscall.uselib.return,
	syscall.utime.return,
	syscall.utimes.return
{
}

probe syscallgroup.filename_begin {
	printf (""start.%s: %d: %s (%d:%d): %s\n"", name, gettimeofday_us (), execname (), pid (), tid (), argstr);
}

probe syscallgroup.filename_end {
	printf (""return.%s: %d: %s (%d:%d): %d\n"", name, gettimeofday_us (), execname (), pid (), tid (), $return);
}
");

			return builder.ToString ();
		}

		Aggregator aggregator;
		SystemtapParser parser;
	}
}
