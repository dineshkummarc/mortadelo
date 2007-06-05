sources = 			\
	aggregator.cs		\
	errno.cs		\
	log.cs			\
	parser.cs		\
	systemtap-parser.cs	\
	syscall.cs

mortadelo.exe: $(sources)
	gmcs -warn:4 -out:$@ $(sources) -pkg:mono-nunit -r:Mono.C5

.PHONY: check

check: mortadelo.exe
	nunit-console2 mortadelo.exe
