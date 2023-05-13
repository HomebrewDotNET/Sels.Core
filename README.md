# Overview
This repository contains code libraries/frameworks all trying to solve a certain issue.
Intended to be used as building blocks for other libraries, frameworks, projects, ...

## Core vs Framework
This solution contains 2 types of projects, my 'Core' libraries and the frameworks.

Projects contained in the Frameworks folder are published with the intention that they can be used by other people.
They are fully blown frameworks.

The Core libraries on the other hand aren't intended to be used directly as they often change, are very specifically made for how I create my projects personally and contain partially made frameworks.
They evolve with my other libraries.

# Frameworks
The following frameworks are publically available on NuGet.

Will be added once latest stable versions are published.

# Local setup
The projects published as NuGet packages can also be installed locally.

If folder "C:\NuGet" exists and contains the nuget.exe (Can be downloaded here: https://www.nuget.org/downloads) all you have to do is rebuild the solution/project(s) and the project(s) will install themselves as local NuGet packages.

To reference them you need to add "C:\NuGet" as a NuGet package source in Visual Studio.