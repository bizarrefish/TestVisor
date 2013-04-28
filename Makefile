IKVMC=ikvmc
IKVMC_FLAGS=-target:library

DMCS=dmcs
DMCS_FLAGS=

ANT=ant

RHINO_BUILDFILE=Rhino/build.xml
RHINO_OUTPUT=Rhino/build/rhino1_7R5pre/js.jar

Libs/Rhino.jar:
	$(ANT) -f $(RHINO_BUILDFILE) jar
	cp $(RHINO_OUTPUT) Libs/Rhino.jar

Libs/Rhino.dll: Libs/Rhino.jar
	$(IKVMC) $(IKVMC_FLAGS) Libs/Rhino.jar -out:Libs/Rhino.dll


TestVisorDeps: Libs/Rhino.dll
	
