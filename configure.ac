AC_INIT(mortadelo, 0.2)
AC_CONFIG_SRCDIR(main-window.cs)

AM_INIT_AUTOMAKE([tar-ustar])

AM_MAINTAINER_MODE

AC_PROG_INTLTOOL([0.21])

AC_PATH_PROG(MONO, mono)
AC_PATH_PROG(MCS, gmcs)

dnl -- Intl

ALL_LINGUAS="es"

GETTEXT_PACKAGE=mortadelo
AC_SUBST(GETTEXT_PACKAGE)
AC_DEFINE_UNQUOTED(GETTEXT_PACKAGE,"$GETTEXT_PACKAGE", [Gettext package])
AM_GLIB_GNU_GETTEXT

dnl --- Check for mono and gmcs

CS="C#"
if test ! -x "$MCS"; then
	AC_MSG_ERROR([No $CS compiler found])
fi

CSC=gmcs
AC_SUBST(CSC)

dnl -- Check for mono pc file

AC_MSG_CHECKING([for mono.pc])
if test -z `pkg-config --variable=prefix mono`; then
  AC_MSG_ERROR([missing the mono.pc file, usually found in the mono-devel package])
else
  AC_MSG_RESULT([found])
fi

GTKSHARP_REQUIRED=2.8.0
NUNIT_REQUIRED=1.2.0

PKG_CHECK_MODULES(MORTADELO, gtk-sharp-2.0 >= $GTKSHARP_REQUIRED mono-nunit >= $NUNIT_REQUIRED)
AC_SUBST(MORTADELO_CFLAGS)
AC_SUBST(MORTADELO_LIBS)

AC_CONFIG_FILES([mortadelo],[chmod +x mortadelo])

AC_OUTPUT([
Makefile
doc/Makefile
po/Makefile.in
])
