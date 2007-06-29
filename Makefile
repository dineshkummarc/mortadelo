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

mortadelo_sources = 		\
	$(shared_sources)	\
	main.cs			\
	main-window.cs		\
	syscall-tree-view.cs	\

memprof_sources =		\
	$(shared_sources)	\
	memory-profile.cs

all: mortadelo.exe mortadelo-memory-profile.exe

mortadelo.exe: $(mortadelo_sources)
	gmcs -debug -define:DEBUG -define:TRACE -warn:4 -out:$@ $(mortadelo_sources) -pkg:mono-nunit -pkg:glib-sharp-2.0 -r:Mono.Posix -pkg:gtk-sharp-2.0

mortadelo-memory-profile.exe: $(memprof_sources)
	gmcs -debug -define:DEBUG -define:TRACE -warn:4 -out:$@ $(memprof_sources) -pkg:mono-nunit -pkg:glib-sharp-2.0 -r:Mono.Posix -pkg:gtk-sharp-2.0

.PHONY: check upload all

check: mortadelo.exe
	nunit-console2 mortadelo.exe

upload:
	git repack -d
	rm -rf /tmp/mortadelo
	git clone --bare -l . /tmp/mortadelo
	git --bare --git-dir=/tmp/mortadelo update-server-info
	rsync -vaz -e 'ssh' --delete /tmp/mortadelo www.gnome.org:~/git
