using System;

namespace Mortadelo {
	public class Errno {
		public struct Err {
			public int code;
			public string name;
			public string description;

			public Err (int code, string name, string description)
			{
				this.code = code;
				this.name = name;
				this.description = description;
			}
		}

		public static Err[] errors = new Err[] {
			new Err (0, "OK",			"Success"),

			/* /usr/include/asm-generic/errno-base.h */
			new Err (1, "EPERM",			"Operation not permitted"),
			new Err (2, "ENOENT",			"No such file or directory"),
			new Err (3, "ESRCH",			"No such process"),
			new Err (4, "EINTR",			"Interrupted system call"),
			new Err (5, "EIO",			"I/O error"),
			new Err (6, "ENXIO",			"No such device or address"),
			new Err (7, "E2BIG",			"Argument list too long"),
			new Err (8, "ENOEXEC",			"Exec format error"),
			new Err (9, "EBADF",			"Bad file number"),
			new Err (10, "ECHILD",			"No child processes"),
			new Err (11, "EAGAIN",			"Try again"),
			new Err (12, "ENOMEM",			"Out of memory"),
			new Err (13, "EACCES",			"Permission denied"),
			new Err (14, "EFAULT",			"Bad address"),
			new Err (15, "ENOTBLK",			"Block device required"),
			new Err (16, "EBUSY",			"Device or resource busy"),
			new Err (17, "EEXIST",			"File exists"),
			new Err (18, "EXDEV",			"Cross-device link"),
			new Err (19, "ENODEV",			"No such device"),
			new Err (20, "ENOTDIR",			"Not a directory"),
			new Err (21, "EISDIR",			"Is a directory"),
			new Err (22, "EINVAL",			"Invalid argument"),
			new Err (23, "ENFILE",			"File table overflow"),
			new Err (24, "EMFILE",			"Too many open files"),
			new Err (25, "ENOTTY",			"Not a typewriter"),
			new Err (26, "ETXTBSY",			"Text file busy"),
			new Err (27, "EFBIG",			"File too large"),
			new Err (28, "ENOSPC",			"No space left on device"),
			new Err (29, "ESPIPE",			"Illegal seek"),
			new Err (30, "EROFS",			"Read-only file system"),
			new Err (31, "EMLINK",			"Too many links"),
			new Err (32, "EPIPE",			"Broken pipe"),
			new Err (33, "EDOM",			"Math argument out of domain of func"),
			new Err (34, "ERANGE",			"Math result not representable"),

			/* /usr/include/asm-generic/errno.h */
			new Err (35, "EDEADLK",			"Resource deadlock would occur"),
			new Err (36, "ENAMETOOLONG",		"File name too long"),
			new Err (37, "ENOLCK",			"No record locks available"),
			new Err (38, "ENOSYS",			"Function not implemented"),
			new Err (39, "ENOTEMPTY",		"Directory not empty"),
			new Err (40, "ELOOP",			"Too many symbolic links encountered"),
			new Err (41, "*** unknown ***",		"*** unknown ***"),
			new Err (42, "ENOMSG",			"No message of desired type"),
			new Err (43, "EIDRM",			"Identifier removed"),
			new Err (44, "ECHRNG",			"Channel number out of range"),
			new Err (45, "EL2NSYNC",		"Level 2 not synchronized"),
			new Err (46, "EL3HLT",			"Level 3 halted"),
			new Err (47, "EL3RST",			"Level 3 reset"),
			new Err (48, "ELNRNG",			"Link number out of range"),
			new Err (49, "EUNATCH",			"Protocol driver not attached"),
			new Err (50, "ENOCSI",			"No CSI structure available"),
			new Err (51, "EL2HLT",			"Level 2 halted"),
			new Err (52, "EBADE",			"Invalid exchange"),
			new Err (53, "EBADR",			"Invalid request descriptor"),
			new Err (54, "EXFULL",			"Exchange full"),
			new Err (55, "ENOANO",			"No anode"),
			new Err (56, "EBADRQC",			"Invalid request code"),
			new Err (57, "EBADSLT",			"Invalid slot"),
			new Err (58, "*** unknown ***",		"*** unknown ***"),
			new Err (59, "EBFONT",			"Bad font file format"),
			new Err (60, "ENOSTR",			"Device not a stream"),
			new Err (61, "ENODATA",			"No data available"),
			new Err (62, "ETIME",			"Timer expired"),
			new Err (63, "ENOSR",			"Out of streams resources"),
			new Err (64, "ENONET",			"Machine is not on the network"),
			new Err (65, "ENOPKG",			"Package not installed"),
			new Err (66, "EREMOTE",			"Object is remote"),
			new Err (67, "ENOLINK",			"Link has been severed"),
			new Err (68, "EADV",			"Advertise error"),
			new Err (69, "ESRMNT",			"Srmount error"),
			new Err (70, "ECOMM",			"Communication error on send"),
			new Err (71, "EPROTO",			"Protocol error"),
			new Err (72, "EMULTIHOP",		"Multihop attempted"),
			new Err (73, "EDOTDOT",			"RFS specific error"),
			new Err (74, "EBADMSG",			"Not a data message"),
			new Err (75, "EOVERFLOW",		"Value too large for defined data type"),
			new Err (76, "ENOTUNIQ",		"Name not unique on network"),
			new Err (77, "EBADFD",			"File descriptor in bad state"),
			new Err (78, "EREMCHG",			"Remote address changed"),
			new Err (79, "ELIBACC",			"Can not access a needed shared library"),
			new Err (80, "ELIBBAD",			"Accessing a corrupted shared library"),
			new Err (81, "ELIBSCN",			".lib section in a.out corrupted"),
			new Err (82, "ELIBMAX",			"Attempting to link in too many shared libraries"),
			new Err (83, "ELIBEXEC",		"Cannot exec a shared library directly"),
			new Err (84, "EILSEQ",			"Illegal byte sequence"),
			new Err (85, "ERESTART",		"Interrupted system call should be restarted"),
			new Err (86, "ESTRPIPE",		"Streams pipe error"),
			new Err (87, "EUSERS",			"Too many users"),
			new Err (88, "ENOTSOCK",		"Socket operation on non-socket"),
			new Err (89, "EDESTADDRREQ",		"Destination address required"),
			new Err (90, "EMSGSIZE",		"Message too long"),
			new Err (91, "EPROTOTYPE",		"Protocol wrong type for socket"),
			new Err (92, "ENOPROTOOPT",		"Protocol not available"),
			new Err (93, "EPROTONOSUPPORT",		"Protocol not supported"),
			new Err (94, "ESOCKTNOSUPPORT",		"Socket type not supported"),
			new Err (95, "EOPNOTSUPP",		"Operation not supported on transport endpoint"),
			new Err (96, "EPFNOSUPPORT",		"Protocol family not supported"),
			new Err (97, "EAFNOSUPPORT",		"Address family not supported by protocol"),
			new Err (98, "EADDRINUSE",		"Address already in use"),
			new Err (99, "EADDRNOTAVAIL",		"Cannot assign requested address"),
			new Err (100, "ENETDOWN",		"Network is down"),
			new Err (101, "ENETUNREACH",		"Network is unreachable"),
			new Err (102, "ENETRESET",		"Network dropped connection because of reset"),
			new Err (103, "ECONNABORTED",		"Software caused connection abort"),
			new Err (104, "ECONNRESET",		"Connection reset by peer"),
			new Err (105, "ENOBUFS",		"No buffer space available"),
			new Err (106, "EISCONN",		"Transport endpoint is already connected"),
			new Err (107, "ENOTCONN",		"Transport endpoint is not connected"),
			new Err (108, "ESHUTDOWN",		"Cannot send after transport endpoint shutdown"),
			new Err (109, "ETOOMANYREFS",		"Too many references: cannot splice"),
			new Err (110, "ETIMEDOUT",		"Connection timed out"),
			new Err (111, "ECONNREFUSED",		"Connection refused"),
			new Err (112, "EHOSTDOWN",		"Host is down"),
			new Err (113, "EHOSTUNREACH",		"No route to host"),
			new Err (114, "EALREADY",		"Operation already in progress"),
			new Err (115, "EINPROGRESS",		"Operation now in progress"),
			new Err (116, "ESTALE",			"Stale NFS file handle"),
			new Err (117, "EUCLEAN",		"Structure needs cleaning"),
			new Err (118, "ENOTNAM",		"Not a XENIX named type file"),
			new Err (119, "ENAVAIL",		"No XENIX semaphores available"),
			new Err (120, "EISNAM",			"Is a named type file"),
			new Err (121, "EREMOTEIO",		"Remote I/O error"),
			new Err (122, "EDQUOT",			"Quota exceeded"),
			new Err (123, "ENOMEDIUM",		"No medium found"),
			new Err (124, "EMEDIUMTYPE",		"Wrong medium type"),
			new Err (125, "ECANCELED",		"Operation Canceled"),
			new Err (126, "ENOKEY",			"Required key not available"),
			new Err (127, "EKEYEXPIRED",		"Key has expired"),
			new Err (128, "EKEYREVOKED",		"Key has been revoked"),
			new Err (129, "EKEYREJECTED",		"Key was rejected by service"),
			new Err (130, "EOWNERDEAD",		"Owner died"),
			new Err (131, "ENOTRECOVERABLE",	"State not recoverable")
		};

		public static bool GetErrno (int num, out string name, out string description)
		{
			if (num < 0 || num >= errors.Length) {
				name = null;
				description = null;
				return false;
			}

			if (errors[num].code != num) {
				string str = String.Format ("Error in internal table of errno mappings (requested error {0})", num);
				throw new ApplicationException (str);
			}

			name = errors[num].name;
			description = errors[num].description;
			return true;
		}
	}
}
