using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SFPackager.Interfaces;
using SFPackager.Models;
using SFPackager.Models.Xml;
using SFPackager.Models.Xml.Elements;
using SFPackager.Services.Manifest;

namespace SFPackager.Services
{
    public class ServiceHashCalculator
    {
        private readonly ManifestLoader<ApplicationManifest> _appManifestLoader;
        private readonly IHandleFiles _fileHandler;
        private readonly ConsoleWriter _log;
        private readonly ManifestHandler _manifestHandler;
        private readonly PackageConfig _packageConfig;
        private readonly ManifestLoader<ServiceManifest> _serviceManifestLoader;
        private readonly HandleEnciphermentCert _handleEnciphermentCert;
        private readonly HandleEndpointCert _handleEndpointCert;

        public ServiceHashCalculator(
            ConsoleWriter log,
            PackageConfig packageConfig,
            IHandleFiles fileHandler,
            ManifestHandler manifestHandler,
            ManifestLoader<ApplicationManifest> appManifestLoader,
            ManifestLoader<ServiceManifest> serviceManifestLoader,
            HandleEnciphermentCert handleEnciphermentCert,
            HandleEndpointCert handleEndpointCert)
        {
            _log = log;
            _packageConfig = packageConfig;
            _fileHandler = fileHandler;
            _manifestHandler = manifestHandler;
            _appManifestLoader = appManifestLoader;
            _serviceManifestLoader = serviceManifestLoader;
            _handleEnciphermentCert = handleEnciphermentCert;
            _handleEndpointCert = handleEndpointCert;
        }

        public async Task<Dictionary<string, GlobalVersion>> Calculate(
            ServiceFabricApplicationProject project,
            VersionNumber currentVersion)
        {
            var projectHashes = new Dictionary<string, GlobalVersion>();

            foreach (var service in project.Services)
            {
                foreach (var subPackage in service.Value.SubPackages)
                {
                    _log.WriteLine($"Computing hash for Service: {service.Key} - Package: {subPackage.Name}");
                    var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
                    var directory = new DirectoryInfo(subPackage.Path);
                    IOrderedEnumerable<string> files;

                    if (subPackage.PackageType == PackageType.Code)
                    {
                        if (!service.Value.IsGuestExecutable)
                        {
                            files = directory
                                .GetFiles("*", SearchOption.AllDirectories)
                                .Where(
                                    x =>
                                        _packageConfig.HashIncludeExtensions.Any(
                                            include =>
                                                x.FullName.EndsWith(include,
                                                    StringComparison.CurrentCultureIgnoreCase)))
                                .Where(
                                    x =>
                                        _packageConfig.HashSpecificExludes.All(
                                            exclude => !x.FullName.ToLowerInvariant()
                                                .Contains(exclude.ToLowerInvariant())))
                                .Select(x => x.FullName)
                                .OrderBy(x => x);
                        }
                        else
                        {
                            files = directory
                                .GetFiles("*", SearchOption.AllDirectories)
                                .Where(
                                    x =>
                                        _packageConfig.HashSpecificExludes.All(
                                            exclude => !x.FullName.ToLowerInvariant()
                                                .Contains(exclude.ToLowerInvariant())))
                                .Select(x => x.FullName)
                                .OrderBy(x => x);
                        }
                    }
                    else
                    {
                        files = directory
                            .GetFiles("*", SearchOption.AllDirectories)
                            .Select(x => x.FullName)
                            .OrderBy(x => x);
                    }

                    foreach (var data in files.Select(File.ReadAllBytes))
                    {
                        hasher.AppendData(data);
                    }

                    var externalIncludes = _packageConfig
                        .ExternalIncludes
                        .Where(x => x
                            .ApplicationTypeName.Equals(project.ApplicationTypeName,
                                StringComparison.CurrentCultureIgnoreCase))
                        .Where(x => x
                            .ServiceManifestName.Equals(service.Value.ServiceName,
                                StringComparison.CurrentCultureIgnoreCase))
                        .Where(x => x
                            .PackageName.Equals(subPackage.Name,
                                StringComparison.CurrentCultureIgnoreCase))
                        .OrderBy(x => x.SourceFileName);

                    foreach (var externalFile in externalIncludes)
                    {
                        var file = await _fileHandler
                            .GetFileAsBytesAsync(externalFile.SourceFileName)
                            .ConfigureAwait(false);

                        if (!file.IsSuccessful)
                            throw new IOException("Failed to get external file from storage");

                        hasher.AppendData(file.ResponseContent);
                    }

                    var finalHash = hasher.GetHashAndReset();
                    var hash = BitConverter.ToString(finalHash).Replace("-", "").ToLowerInvariant();

                    var packageVersion = new GlobalVersion
                    {
                        Hash = hash,
                        VersionType = VersionType.ServicePackage,
                        ParentRef = $"{project.ApplicationTypeName}-{service.Key}",
                        PackageType = subPackage.PackageType
                    };

                    projectHashes.Add($"{project.ApplicationTypeName}-{service.Key}-{subPackage.Name}", packageVersion);
                }

                var serviceManifest = _serviceManifestLoader.Load(service.Value.SourceServiceManifestPath);
                _manifestHandler.SetServiceEndpoints(serviceManifest, project.ApplicationTypeName, service.Value.ServiceName);

                using (var serviceManifestStream = new MemoryStream())
                {
                    _serviceManifestLoader.Save(serviceManifest, serviceManifestStream);

                    var serviceVersion = new GlobalVersion
                    {
                        VersionType = VersionType.Service,
                        ParentRef = project.ApplicationTypeName,
                        Hash = HashStream(serviceManifestStream)
                    };

                    projectHashes.Add($"{project.ApplicationTypeName}-{service.Key}", serviceVersion);
                }
            }

            var appManifest = _appManifestLoader.Load(project.ApplicationManifestFileFullPath);
            _manifestHandler.CleanAppManifest(appManifest);
            _handleEndpointCert.SetEndpointCerts(_packageConfig, appManifest, project.ApplicationTypeName);
            _handleEnciphermentCert.SetEnciphermentCerts(_packageConfig, appManifest, project.ApplicationTypeName);

            var guests = _packageConfig.GuestExecutables.Where(x =>
                x.ApplicationTypeName.Equals(project.ApplicationTypeName, StringComparison.CurrentCultureIgnoreCase));
            
            foreach (var guest in guests)
            {
                var policies = new Policies();

                if (guest.GuestRunAs != null)
                {
                    var runAs = new RunAsPolicy
                    {
                        UserRef = guest.GuestRunAs.UserName,
                        CodePackageRef = "Code"
                    };

                    var runAsPolicies = new List<RunAsPolicy> {runAs};
                    policies.RunAsPolicy = runAsPolicies;

                    if(appManifest.Principals == null)
                        appManifest.Principals = new Principals();
                    if(appManifest.Principals.Users == null)
                        appManifest.Principals.Users = new Users();
                    if(appManifest.Principals.Users.User == null)
                        appManifest.Principals.Users.User = new List<User>();

                    if (!appManifest.Principals.Users.User.Any(x =>
                        x.Name.Equals(guest.GuestRunAs.UserName, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        var user = new User
                        {
                            Name = guest.GuestRunAs.UserName,
                            AccountType = guest.GuestRunAs.AccountType
                        };
                        appManifest.Principals.Users.User.Add(user);
                    }
                }

                var serviceManifestRef = new ServiceManifestRef
                {
                    ServiceManifestName = guest.PackageName,
                    ServiceManifestVersion = "1.0.0"
                };
                var serviceImport = new ServiceManifestImport
                {
                    ServiceManifestRef = serviceManifestRef,
                    ConfigOverrides = new ConfigOverrides(),
                    Policies = policies
                };

                appManifest.ServiceManifestImports.Add(serviceImport);
            }

            using (var appManifestStream = new MemoryStream())
            {
                _appManifestLoader.Save(appManifest, appManifestStream);
                projectHashes.Add(project.ApplicationTypeName, new GlobalVersion
                {
                    VersionType = VersionType.Application,
                    Version = currentVersion,
                    Hash = HashStream(appManifestStream)
                });
            }

            return projectHashes;
        }

        private static string HashStream(Stream stream)
        {
            stream.Position = 0;
            var streamLength = (int)stream.Length;
            var buffer = new byte[streamLength];
            stream.Read(buffer, 0, streamLength);

            using (var hasher = SHA256.Create())
            {
                var rawHash = hasher.ComputeHash(buffer);
                return BitConverter.ToString(rawHash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}