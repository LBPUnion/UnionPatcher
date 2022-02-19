# UnionPatcher

A tool that will take the official EBOOT of a LittleBIGPlanet title and replace the server URLs with a custom one

## Prerequisites
* For running the console application, you will need the [.NET 6 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime?utm_source=getdotnetcore&utm_medium=referral)
* You will need to know the server URL you wish to use

## Getting the latest build
1. Access the [CI builds for UnionPatcher](https://github.com/LBPUnion/UnionPatcher/actions)
2. In the "workflow runs" grid, select the first in the list with a green check mark (✔️)
3. Scroll to the bottom of this build and find the correct zip for your platform

## Building manually (Required for MacOS)
You will need the [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

```bash
git clone https://github.com/LBPUnion/UnionPatcher
cd UnionPatcher
dotnet build UnionPatcher.sln
#Running
cd UnionPatcher/bin/Debug/net6.0/
./LBPUnion.UnionPatcher
```
