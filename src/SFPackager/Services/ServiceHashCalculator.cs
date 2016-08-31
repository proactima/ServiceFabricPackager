using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;
using SFPackager.Models;
using SFPackager.Services.Manifest;

namespace SFPackager.Services
{
    public class ServiceHashCalculator
    {
        private readonly PackageConfig _packageConfig;

        private readonly FakeManifestCreator _manifestCreator;
        private readonly EndpointAppender _endpointAppender;

        public ServiceHashCalculator(
            PackageConfig packageConfig,
            EndpointAppender endpointAppender,
            FakeManifestCreator manifestCreator)
        {
            _packageConfig = packageConfig;
            _endpointAppender = endpointAppender;
            _manifestCreator = manifestCreator;
        }

        public Dictionary<string, GlobalVersion> Calculate(ServiceFabricApplicationProject project)
        {
            var projectHashes = new Dictionary<string, GlobalVersion>();
            
            foreach (var service in project.Services)
            {
                foreach (var subPackage in service.Value.SubPackages)
                {
                    Console.WriteLine($"Computing hash for Service: {service.Key} - Package: {subPackage.Name}");
                    var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
                    var directory = new DirectoryInfo(subPackage.Path);
                    IOrderedEnumerable<string> files;

                    if (subPackage.PackageType == PackageType.Code)
                    {
                        files = directory
                            .GetFiles("*", SearchOption.AllDirectories)
                            .Where(Constants.IncludeFileFilter)
                            .Where(Constants.IgnoreList)
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

            return projectHashes;
        }

        private static string HashXmlDocument(XmlDocument document)
        {
            string hash;

            using (var ms = new MemoryStream())
            {
                document.Save(ms);
                ms.Position = 0;
                var streamLength = (int)ms.Length;
                var buffer = new byte[streamLength];
                ms.Read(buffer, 0, streamLength);

                var hasher = SHA256.Create();
                var rawHash = hasher.ComputeHash(buffer);
                hash = BitConverter.ToString(rawHash).Replace("-", "").ToLowerInvariant();
            }

            return hash;
        }

        public string HashServiceManifestChanges()
        {
            var a = SHA256.Create();
            

            return String.Empty;
        }
    }
}