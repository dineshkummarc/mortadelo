sources = 			\
	aggregator.cs		\
	errno.cs		\
	GLib.IO.cs		\
	line-reader.cs		\
	log.cs			\
	parser.cs		\
	runner.cs		\
	spawn.cs		\
	systemtap-parser.cs	\
	systemtap-runner.cs	\
	syscall.cs		\
	syscall-list-model.cs	\
	syscall-tree-view.cs	\
	unix-reader.cs

mortadelo.exe: $(sources)
	gmcs -warn:4 -out:$@ $(sources) -pkg:mono-nunit -r:Mono.C5 -pkg:glib-sharp-2.0 -r:Mono.Posix -pkg:gtk-sharp-2.0

.PHONY: check

check: mortadelo.exe
	nunit-console2 mortadelo.exe
