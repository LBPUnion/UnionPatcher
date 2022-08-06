mkdir -p builds;
#dotnet clean;

dotnet publish -c Windows -r win-x64 --self-contained

dotnet publish -c Linux -r linux-x64 --self-contained
dotnet publish -c Linux -r linux-arm --self-contained
dotnet publish -c Linux -r linux-arm64 --self-contained

dotnet publish -c MacOS -r osx-x64 --self-contained
dotnet publish -c MacOS -r osx-arm64 --self-contained

# $1: Name.zip
# $2: Path to zip
function createBuild() {
    currentDirectory=$(pwd)
    cd $2 || return 1;
    
    zip -r "$1" *;
    cd $currentDirectory || return 1;
    mv "$2/$1" builds/
}

createBuild "UnionPatcher-Windows-x64.zip" "UnionPatcher.Gui.Windows/bin/Release/net6.0-windows/win-x64/publish/"

createBuild "UnionPatcher-Linux-x64.zip" "UnionPatcher.Gui.Linux/bin/Release/net6.0/linux-x64/publish/"
createBuild "UnionPatcher-Linux-arm.zip" "UnionPatcher.Gui.Linux/bin/Release/net6.0/linux-arm/publish/"
createBuild "UnionPatcher-Linux-arm64.zip" "UnionPatcher.Gui.Linux/bin/Release/net6.0/linux-arm64/publish/"

createBuild "UnionPatcher-macOS-x64.zip" "UnionPatcher.Gui.MacOS/bin/Release/net6.0/osx-x64/publish/"
createBuild "UnionPatcher-macOS-arm64.zip" "UnionPatcher.Gui.MacOS/bin/Release/net6.0/osx-arm64/publish/"