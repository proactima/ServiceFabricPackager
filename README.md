# ServiceFabricPackager

## TLDR
A tool that manages complex Service Fabric application packaging process.

## Description
Service Fabric Packager is a tool that helps you with more advanced SF Packaging scenarions. Current list of features includes:
* Auto-discovers SF Application projects in a folder
* Tracks changes in a package (code/config/data) for each of the Service Fabric Services found.
* Does partial packaging and updates all the manifests with correct versions
* Keeps an external history of what packages changed (Azure Blob)
* Supports external configuration of manifests (Azure Blob).

## How to use
Coming soon

## How to build
Download and build. More detailed info coming soon.

## Actual useful information
### Tracking changes
For the change tracking to work properly, all your Service Fabric projects must be built with the deterministic flag enabled in Roslyn (see https://blogs.msdn.microsoft.com/dotnet/2016/04/02/whats-new-for-c-and-vb-in-visual-studio/)
When the packager is run, it will compute a hash of all files in each package folder (Data, Config & Code). It will use these hashes to determine if a package has changed.

### Packaging and version updates
Service Fabric packager will connect to your cluster and get the versions of the currently deployed applications (this is a bit simplistic today).
Based on what it finds, it will then select the next version number.
When it starts packaging, it will load the version map of the currently running versions. Based on that, it'll compare the local package hashes against the
version map for the current version and determine what to package.
The version map also contains the current version of all the things, so it knows what version to assign to the different Applications, Services and Packages.
