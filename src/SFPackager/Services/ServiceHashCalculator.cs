using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml;
using SFPackager.Interfaces;
using SFPackager.Models;
using SFPackager.Services.Manifest;

namespace SFPackager.Services
{
    public class ServiceHashCalculator
    {
        private readonly FakeManifestCreator _manifestCreator;
        private readonly EndpointAppender _endpointAppender;
        private readonly CertificateAppender _certificateAppender;
        private readonly ConsoleWriter _log;
        private readonly PackageConfig _packageConfig;
        private readonly IHandleFiles _fileHandler;

        public ServiceHashCalculator(
            EndpointAppender endpointAppender,
            FakeManifestCreator manifestCreator,
            CertificateAppender certificateAppender,
            ConsoleWriter log,
            PackageConfig packageConfig,
            IHandleFiles fileHandler)
        {
            _endpointAppender = endpointAppender;
            _manifestCreator = manifestCreator;
            _certificateAppender = certificateAppender;
            _log = log;
            _packageConfig = packageConfig;
            _fileHandler = fileHandler;
        }

        public async Task<Dictionary<string, GlobalVersion>> Calculate(ServiceFabricApplicationProject project, VersionNumber currentVersion)
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
                        files = directory
                            .GetFiles("*", SearchOption.AllDirectories)
                            .Where(x => _packageConfig.HashIncludeExtensions.Any(include => x.FullName.EndsWith(include, StringComparison.CurrentCultureIgnoreCase)))
                            .Where(x => _packageConfig.HashSpecificExludes.All(exclude => !x.FullName.ToLowerInvariant().Contains(exclude.ToLowerInvariant())))
                            .Select(x => x.FullName)
                            .OrderBy(x => x);
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

                    var serviceVersion = new GlobalVersion
                    {
                        Hash = hash,
                        VersionType = VersionType.ServicePackage,
                        ParentRef = service.Key,
                        PackageType = subPackage.PackageType
                    };

                    projectHashes.Add($"{service.Key}-{subPackage.Name}", serviceVersion);
                }

                var fakeServiceManifest = _manifestCreator.GetFakeServiceManifest();
                _endpointAppender.SetEndpoints(fakeServiceManifest, project.ApplicationTypeName, service.Key);

                projectHashes.Add($"{service.Key}", new GlobalVersion
                {
                    VersionType = VersionType.Service,
                    ParentRef = project.ApplicationTypeName,
                    Hash = HashXmlDocument(fakeServiceManifest)
                });
            }

            var serviceNames = project.Services.Select(x => x.Key).ToList();
            var fakeApplicationManifest = _manifestCreator.GetFakeApplicationManifest(serviceNames);
            _certificateAppender.SetCertificates(fakeApplicationManifest, project.ApplicationTypeName, serviceNames);

            projectHashes.Add(project.ApplicationTypeName, new GlobalVersion
            {
                VersionType = VersionType.Application,
                Version = currentVersion,
                Hash = HashXmlDocument(fakeApplicationManifest)
            });

            return projectHashes;
        }

        private static string HashXmlDocument(XmlDocument document)
        {
            using (var ms = new MemoryStream())
            {
                document.Save(ms);
                ms.Position = 0;
                var streamLength = (int)ms.Length;
                var buffer = new byte[streamLength];
                ms.Read(buffer, 0, streamLength);

                using (var hasher = SHA256.Create())
                {
                    var rawHash = hasher.ComputeHash(buffer);
                    return BitConverter.ToString(rawHash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}