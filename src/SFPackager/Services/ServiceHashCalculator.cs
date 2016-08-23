using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using SFPackager.Models;

namespace SFPackager.Services
{
    public class ServiceHashCalculator
    {
        public Dictionary<string, GlobalVersion> Calculate(ServiceFabricApplicationProject project)
        {
            var projectHashes = new Dictionary<string, GlobalVersion>();
            
            foreach (var service in project.Services)
            {
                foreach (var subPackage in service.SubPackages)
                {
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
                        ParentRef = service.ServiceName,
                        PackageType = subPackage.PackageType
                    };

                    projectHashes.Add($"{service.ServiceName}-{subPackage.Name}", serviceVersion);
                }

                projectHashes.Add($"{service.ServiceName}", new GlobalVersion
                {
                    VersionType = VersionType.Service,
                    ParentRef = project.ApplicationTypeName
                });
            }
            

            //foreach (var includedProject in project.Services)
            //{
            //    var outputPath = $"{includedProject.ProjectFolder}{project.BuildOutputPathSuffix}";
            //    var directory = new DirectoryInfo(outputPath);
            //    var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            //    var files = directory
            //        .GetFiles("*", SearchOption.AllDirectories)
            //        .Where(Constants.IncludeFileFilter)
            //        .Where(Constants.IgnoreList)
            //        .Select(x => x.FullName)
            //        .OrderBy(x => x);

            //    foreach (var data in files.Select(File.ReadAllBytes))
            //    {
            //        hasher.AppendData(data);
            //    }

            //    var finalHash = hasher.GetHashAndReset();
            //    var hash = BitConverter.ToString(finalHash).Replace("-", "").ToLowerInvariant();

            //    var serviceVersion = new GlobalVersion
            //    {
            //        Hash = hash,
            //        VersionType = VersionType.Service,
            //        ParentRef = project.ApplicationTypeName
            //    };

            //    projectHashes.Add(includedProject.ServiceName, serviceVersion);
            //}

            return projectHashes;
        }
    }
}