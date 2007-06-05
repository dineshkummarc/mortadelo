sources = 			\
	errno.cs		\
	log.cs			\
	parser.cs		\
	systemtap-parser.cs	\
	syscall.cs

mortadelo.exe: $(sources)
	mcs -warn:4 -out:$@ $(sources) -pkg:mono-nunit
