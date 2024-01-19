# Script to build UnionPatcher for mac, builds a universal binary, and zips it up. also codesigns if $1 is specified

dotnet clean
dotnet publish UnionPatcher.Gui.MacOS --configuration Release /p:Platform="Any CPU" --self-contained -o macbuild
dotnet publish UnionPatcher.Gui.MacOS --configuration Release /p:Platform="Any CPU" --arch x64 --self-contained -o macbuildx86

rm -rf macbuilduniversal
mkdir macbuilduniversal
cp -r macbuild/UnionPatcher.Gui.MacOS.app macbuilduniversal/UnionPatcher.app
cp UnionPatcher.Gui.MacOS/Info.plist macbuilduniversal/UnionPatcher.app/Contents/Info.plist
rm -rf macbuilduniversal/UnionPatcher.app/Contents/MacOS/scetool/linux*
rm -rf macbuilduniversal/UnionPatcher.app/Contents/MacOS/scetool/win*

lipo -create -output macbuilduniversal/UnionPatcher.app/Contents/MacOS/LBPUnion.UnionPatcher.Gui.MacOS macbuildx86/LBPUnion.UnionPatcher.Gui.MacOS macbuild/LBPUnion.UnionPatcher.Gui.MacOS
touch macbuilduniversal/UnionPatcher.app

if [ -z ${1+x} ]; then
	codesign -f --deep -s "$1" macbuilduniversal/UnionPatcher.app
fi
cd macbuilduniversal
zip -r UnionPatcher-macOS-universal.zip UnionPatcher.app
