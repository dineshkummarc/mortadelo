SUBDIRS = doc po

EXTRA_LIBS =			\
	-pkg:mono-nunit		\
	-pkg:glib-sharp-2.0	\
	-r:Mono.Posix		\
	-pkg:gtk-sharp-2.0

shared_sources =				\
	$(srcdir)/aggregator.cs				\
	$(srcdir)/compact-log.cs				\
	$(srcdir)/errno.cs				\
	$(srcdir)/filter.cs				\
	$(srcdir)/filter-formatter.cs			\
	$(srcdir)/filtered-log.cs				\
	$(srcdir)/GLib.IO.cs				\
	$(srcdir)/line-reader.cs				\
	$(srcdir)/log.cs					\
	$(srcdir)/log-io.cs				\
	$(srcdir)/log-provider.cs				\
	$(srcdir)/log-modification-accumulator.cs		\
	$(srcdir)/parser.cs				\
	$(srcdir)/plain-formatter.cs			\
	$(srcdir)/regex-cache.cs				\
	$(srcdir)/regex-filter.cs				\
	$(srcdir)/runner.cs				\
	$(srcdir)/serializer.cs				\
	$(srcdir)/spawn.cs				\
	$(srcdir)/systemtap-parser.cs			\
	$(srcdir)/systemtap-runner.cs			\
	$(srcdir)/systemtap-serializer.cs			\
	$(srcdir)/syscall.cs				\
	$(srcdir)/syscall-formatter.cs			\
	$(srcdir)/syscall-list-model.cs			\
	$(srcdir)/timer-throttle.cs			\
	$(srcdir)/unix-reader.cs				\
	$(srcdir)/util.cs

mortadelo_specific_sources =	\
	$(srcdir)/main.cs			\
	$(srcdir)/main-window.cs		\
	$(srcdir)/syscall-tree-view.cs

mortadelo_sources = 			\
	$(shared_sources)		\
	$(mortadelo_specific_sources)

memprof_specific_sources =	\
	$(srcdir)/memory-profile.cs

memprof_sources =			\
	$(shared_sources)		\
	$(memprof_specific_sources)

mortadelodir=$(pkglibdir)
mortadelo_SCRIPTS = mortadelo.exe

bin_SCRIPTS = mortadelo

intltool_bullshit =		\
	intltool-extract.in	\
	intltool-merge.in	\
	intltool-update.in

EXTRA_DIST =				\
	$(shared_sources)		\
	$(mortadelo_specific_sources)	\
	$(memprof_specific_sources)	\
	$(intltool_bullshit)

CLEANFILES =			\
	mortadelo.exe		\
	mortadelo.exe.mdb	\
	mortadelo

DISTCLEANFILES =		\
	intltool-extract	\
	intltool-merge		\
	intltool-update		\
	TestResult.xml

mortadelo.exe: $(mortadelo_sources)
	$(CSC) -warn:4 $(MORTADELO_LIBS) -out:$@ $(mortadelo_sources) $(EXTRA_LIBS)

all: mortadelo.exe

check: mortadelo.exe
	nunit-console2 mortadelo.exe

upload:
	git push --force --all ssh://www.gnome.org/~federico/public_html/git/mortadelo

.PHONY: check upload
