sources = 			\
	aggregator.cs		\
	errno.cs		\
	GLib.IO.cs		\
	line-reader.cs		\
	log.cs			\
	log-io.cs		\
	parser.cs		\
	runner.cs		\
	serializer.cs		\
	spawn.cs		\
	systemtap-parser.cs	\
	systemtap-runner.cs	\
	systemtap-serializer.cs	\
	syscall.cs		\
	syscall-list-model.cs	\
	syscall-tree-view.cs	\
	unix-reader.cs

mortadelo.exe: $(sources)
	gmcs -warn:4 -out:$@ $(sources) -pkg:mono-nunit -r:Mono.C5 -pkg:glib-sharp-2.0 -r:Mono.Posix -pkg:gtk-sharp-2.0

.PHONY: check upload

check: mortadelo.exe
	nunit-console2 mortadelo.exe

upload:
	git push --all --force ssh://www.gnome.org/~federico/public_html/git/mortadelo
