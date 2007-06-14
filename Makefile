shared_sources =				\
	aggregator.cs				\
	errno.cs				\
	GLib.IO.cs				\
	line-reader.cs				\
	log.cs					\
	log-io.cs				\
	log-provider.cs				\
	log-modification-accumulator.cs		\
	parser.cs				\
	runner.cs				\
	serializer.cs				\
	spawn.cs				\
	string-pool.cs				\
	systemtap-parser.cs			\
	systemtap-runner.cs			\
	systemtap-serializer.cs			\
	syscall.cs				\
	syscall-list-model.cs			\
	unix-reader.cs

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
	gmcs -debug -warn:4 -out:$@ $(mortadelo_sources) -pkg:mono-nunit -r:Mono.C5 -pkg:glib-sharp-2.0 -r:Mono.Posix -pkg:gtk-sharp-2.0

mortadelo-memory-profile.exe: $(memprof_sources)
	gmcs -debug -warn:4 -out:$@ $(memprof_sources) -pkg:mono-nunit -r:Mono.C5 -pkg:glib-sharp-2.0 -r:Mono.Posix -pkg:gtk-sharp-2.0

.PHONY: check upload all

check: mortadelo.exe
	nunit-console2 mortadelo.exe

upload:
	git push --all --force ssh://www.gnome.org/~federico/public_html/git/mortadelo
