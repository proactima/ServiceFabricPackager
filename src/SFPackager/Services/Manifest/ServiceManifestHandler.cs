using System;
using System.Collections.Generic;
using System.Linq;
using SFPackager.Models;
using SFPackager.Models.Xml;

namespace SFPackager.Services.Manifest
{
    public class ServiceManifestHandler
    {
        public void SetServiceManifestGeneral(
            ServiceManifest serviceManifest,
            GlobalVersion version)
        {
            serviceManifest.Version = version.Version.ToString();
        }

        public void SetServicePackagesData(
            ServiceManifest serviceManifest,
            Dictionary<string, GlobalVersion> versions,
            string parentRef)
        {
            var subPackages = versions
                .Where(x => x.Value.ParentRef.Equals(parentRef))
                .ToList();

            foreach (var package in subPackages)
            {
                var packageName = package.Key.Split('-')[1];

                switch (package.Value.PackageType)
                {
                    case PackageType.Code:
                        serviceManifest.CodePackage.Version = package.Value.Version.ToString();
                        break;
                    case PackageType.Config:
                        serviceManifest.ConfigPackage.Version = package.Value.Version.ToString();
                        break;
                    case PackageType.Data:
                        var dataPackages = serviceManifest
                            .DataPackages
                            .Where(x => x.Name.Equals(packageName, StringComparison.CurrentCultureIgnoreCase))
                            .ToList();
                        if (!dataPackages.Any())
                            return;

                        dataPackages.First().Version = package.Value.Version.ToString();
                        break;
                    case PackageType.None:
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}