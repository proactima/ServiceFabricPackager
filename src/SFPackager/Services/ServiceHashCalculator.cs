﻿using System;
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

                projectHashes.Add($"{service.Key}", new GlobalVersion
                {
                    VersionType = VersionType.Service,
                    ParentRef = project.ApplicationTypeName
                });
            }

            return projectHashes;
        }
    }
}