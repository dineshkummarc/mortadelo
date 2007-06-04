sources = 	\
	syscall.cs

mortadelo.exe: $(sources)
	mcs -warn:4 -out:$@ $<
