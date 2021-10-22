# Sels.Core
Base solution with code shared across my other solutions.

Contains various generic code components/mini frameworks that act as building blocks for my bigger projects.

Projects are published as NuGet packages.

## Local setup
The projects published as NuGet packages can also be installed locally.

If folder "C:\NuGet" exists and contains the nuget.exe (Can be downloaded here: https://www.nuget.org/downloads) all you have to do is rebuild the solution/project(s) and the project(s) will install themselves as local NuGet packages.

To reference them you need to add "C:\NuGet" as a NuGet package source in Visual Studio.