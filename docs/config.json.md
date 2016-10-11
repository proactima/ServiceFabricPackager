# Config file

This file specifies the configuration the packager needs to work properly.

## Where to put it?

It should be either in the folder specified by --localStoragePath or in the Azure Blob Container specified by --storageAccountContainer.
The name of the file must match the parameter --configFileName.

## Example config.json

There is an example config file you can look at: [config.example.json](config.example.json)

# Description of Config Json

In this section we'll describe each part of the config schema and what they are used for.

## Cluster

```
"Cluster": {
  "Endpoint": "localhost",
  "Port": 19080,
  "PfxFile": "mycert.pfx",
  "PfxKey": "mysupersecretkey"
}
```

This defines how to connect to the cluster to read the currently deployed version.
If ```--secureCluster``` is used, the fields ```PfXFile``` and ```PfxKey``` is required, since you are then connecting to a secured cluster.

In the future we plan on having a VSTS Task that can read this from the VSTS Connection Management in stead of putting it in the config file.

## HashIncludeExtensions

```
"HashIncludeExtensions": [
  ".dll",
  ".exe",
  ".config"
]
```

This section defines what file extensions to include when hashing and packaging. This only applies to the Code folder (i.e. the bin folder).
In the example above all ```.dll```, ```.exe``` and ```.config``` files will be included.

## HashSpecificExludes

```
"HashSpecificExludes": [
  "nondeterministic.dll"
]
```

This is specific files to ignore while hashing (tho they will be included when packaging).
The reason for this is that there are things that isn't deterministically created. For example, Resource files are not.
This would mean that every time you build, the package would look like it had changed since the resource file was regenerated.

## ExternalIncludes

```
"ExternalIncludes": [
  {
    "ApplicationTypeName": "AppTypeName",
    "ServiceManifestName": "ServiceName",
    "PackageName": "Config",
    "SourceFileName": "prodconfig.xml",
    "TargetFileName": "Settings.xml"
  }
]
```

This is external files to copy into the package during packaging.
You specify where you want the file to end up by setting ```ApplicationTypeName```, ```ServiceManifestName``` and ```PackageName```.
The ```SourceFileName``` is the file to copy, and ```TargetFileName``` is the filename of the target.
The files need to exist in the same location as the config file.

This might be extended to support globbing of some kind in the future if there is a need for that.

## Endpoints

```
"Endpoints": [
  {
    "ApplicationTypeName": "ApiType",
    "ServiceManifestName": "ServiceName",
    "EndpointName": "ServiceEndpointHttp",
    "Port": 80,
    "Protocol": "http",
    "Type": "Input"
  },
  {
    "ApplicationTypeName": "ApiType",
    "ServiceManifestName": "ServiceName",
    "EndpointName": "ServiceEndpointHttps",
    "Port": 443,
    "Protocol": "https",
    "Type": "Input"
  }
]
```

Specify endpoints for a service. If you specify an endpoint for a service, all existing endpoints in the ServiceManifest will be removed first.
The ```EndpointName```, ```Port```, ```Protocol``` and ```Type``` maps directly through to the ```Endpoint``` tag in the ServiceManifest.
If you want an endpoint with an auto-assigned port (for internal use), just remove the ```Port``` field entierly.

## Https

```
"Https": [
  {
    "ApplicationTypeName": "AppTypeName",
    "ServiceManifestName": "ServiceName",
    "EndpointName": "ServiceEndpointHttps",
    "CertThumbprint": "CERTTHUMBPRINT"
  }
]
```

Specify Certificate bindings that should be applied to the ApplicationManifest.
Note that the ```EndpointName``` here must match an endpoint in the ServiceManifest for the service.

