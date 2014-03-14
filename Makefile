CONFIGURATION=Release
XBUILD=xbuild
XBUILDOPTS=/t:Build /p:WarningLevel=0
MDTOOL="/Applications/Xamarin Studio.app/Contents/MacOS/mdtool"
MDTOOLOPTS=build -c:$(CONFIGURATION)
IMPORT=mono bin/import.exe

xbuild = \
	$(XBUILD) $(XBUILDOPTS) /p:Configuration=$(CONFIGURATION) $(1)

mdbuild = \
	$(MDTOOL) $(MDTOOLOPTS) -t:$(1) -p:$(2) GameStack.sln

import = \
	$(IMPORT) $(1) $(2)

all: pipeline import desktop

pipeline:
	$(call xbuild,pipeline/GameStack.Pipeline.csproj)
	cp -f external/desktop/libassimp.* bin/

import:
	$(call xbuild,pipeline/import/Import.csproj)

ios:
	$(call mdbuild,Clean,GameStack.iOS.Bindings)
	$(call mdbuild,Clean,GameStack.iOS)
	$(call mdbuild,Build,GameStack.iOS.Bindings)
	$(call mdbuild,Build,GameStack.iOS)

android:
	$(call mdbuild,Clean,GameStack.Android.Bindings)
	$(call mdbuild,Clean,GameStack.Android)
	$(call mdbuild,Build,GameStack.Android.Bindings)
	$(call mdbuild,Build,GameStack.Android)

desktop:
	$(call xbuild,Desktop/bindings/GameStack.Desktop.Bindings.csproj)
	$(call xbuild,Desktop/GameStack.Desktop.csproj)

clean:
	find . -type d \( -name bin -o -name obj -o -name assets \) |xargs rm -rf ; rm -rf lib/ ; rm -rf bin/ 

.PHONY: all pipeline import ios android desktop clean
