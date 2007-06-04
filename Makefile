sources = 		\
	errno.cs	\
	log.cs		\
	syscall.cs

mortadelo.exe: $(sources)
	mcs -warn:4 -out:$@ $<
