IKVMC=ikvmc
IKVMC_FLAGS=-target:library

DMCS=dmcs
DMCS_FLAGS=

MSBUILD=xbuild
MSBUILD_FLAGS=

ANT=ant

RHINO_BUILDFILE=Rhino/build.xml
RHINO_OUTPUT=Rhino/build/rhino1_7R5pre/js.jar

SSR_DLLS=ServiceStack.Common.dll ServiceStack.Interfaces.dll ServiceStack.Redis.dll ServiceStack.Text.dll

SSR_PROJ_DIR=ServiceStack.Redis/src/ServiceStack.Redis
SSR_PROJ_FILE=ServiceStack.Redis.csproj

SSR_OUTPUT_DIR=$(SSR_PROJ_DIR)/bin/Release

Libs/Rhino.jar:
	$(ANT) -f $(RHINO_BUILDFILE) jar
	cp $(RHINO_OUTPUT) Libs/Rhino.jar

Libs/Rhino.dll: Libs/Rhino.jar
	$(IKVMC) $(IKVMC_FLAGS) Libs/Rhino.jar -out:Libs/Rhino.dll

$(addprefix Libs/,$(SSR_DLLS)): 
	$(MSBUILD) $(MSBUILD_FLAGS) ServiceStack.Redis/src/ServiceStack.Redis/ServiceStack.Redis.csproj /p:Configuration=Release
	cp $(addprefix $(SSR_OUTPUT_DIR)/,$(SSR_DLLS)) Libs/


TestVisorDeps: Libs/Rhino.dll $(addprefix Libs/,$(SSR_DLLS))
	
