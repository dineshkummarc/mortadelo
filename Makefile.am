SUBDIRS = doc

EXTRA_LIBS =			\
	-pkg:mono-nunit		\
	-pkg:glib-sharp-2.0	\
	-r:Mono.Posix		\
	-pkg:gtk-sharp-2.0

shared_sources =				\
	aggregator.cs				\
	compact-log.cs				\
	errno.cs				\
	filter.cs				\
	filter-formatter.cs			\
	filtered-log.cs				\
	GLib.IO.cs				\
	line-reader.cs				\
	log.cs					\
	log-io.cs				\
	log-provider.cs				\
	log-modification-accumulator.cs		\
	parser.cs				\
	plain-formatter.cs			\
	regex-cache.cs				\
	regex-filter.cs				\
	runner.cs				\
	serializer.cs				\
	spawn.cs				\
	systemtap-parser.cs			\
	systemtap-runner.cs			\
	systemtap-serializer.cs			\
	syscall.cs				\
	syscall-formatter.cs			\
	syscall-list-model.cs			\
	timer-throttle.cs			\
	unix-reader.cs				\
	util.cs

mortadelo_specific_sources =	\
	main.cs			\
	main-window.cs		\
	syscall-tree-view.cs

mortadelo_sources = 			\
	$(shared_sources)		\
	$(mortadelo_specific_sources)

memprof_specific_sources =	\
	memory-profile.cs

memprof_sources =			\
	$(shared_sources)		\
	$(memprof_specific_sources)

mortadelodir=$(pkglibdir)
mortadelo_SCRIPTS = mortadelo.exe

bin_SCRIPTS = mortadelo

EXTRA_DIST =				\
	$(shared_sources)		\
	$(mortadelo_specific_sources)	\
	$(memprof_specific_sources)

CLEANFILES =			\
	mortadelo.exe		\
	mortadelo.exe.mdb	\
	mortadelo

mortadelo.exe: $(mortadelo_sources)
	$(CSC) -warn:4 $(MORTADELO_LIBS) -out:$@ $(mortadelo_sources) $(EXTRA_LIBS)

all: mortadelo.exe

check: mortadelo.exe
	nunit-console2 mortadelo.exe

.PHONY: check
