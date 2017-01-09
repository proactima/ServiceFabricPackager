using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SFPackager.Helpers;
using SFPackager.Models;

namespace SFPackager.Services
{
    public class VersionService
    {
        public void SetVersionIfNoneIsDeployed(
            VersionMap versions,
            VersionNumber newVersion)
        {
            foreach (var v in versions.PackageVersions)
            {
                v.Value.Version = newVersion;
                v.Value.IncludeInPackage = true;

                if (v.Value.VersionType != VersionType.Service)
                    continue;

                versions.PackageVersions[v.Value.ParentRef].IncludeInPackage = true;
                versions.PackageVersions[v.Value.ParentRef].Version = newVersion;
            }

            versions.PackageVersions[Constants.GlobalIdentifier].Version = newVersion;
        }

        public void SetVersionsIfVersionIsDeployed(
            VersionMap currentVersionMap,
            VersionMap packagedVersionMap,
            VersionNumber newVersion)
        {
            //var currentVersionMap = JsonConvert.DeserializeObject<Dictionary<string, GlobalVersion>>
            //    (currentHashMapResponse.ResponseContent);
            var versionMap = packagedVersionMap.PackageVersions;

            versionMap
                .Where(x => x.Value.VersionType == VersionType.ServicePackage)
                .Where(x => !x.Value.Hash.Equals(GetGlobalVersion(currentVersionMap.PackageVersions, x.Key).Hash))
                .ForEach(s =>
                {
                    SetVersionOnPackage(s, newVersion, versionMap);
                });

            versionMap
                .Where(x => x.Value.VersionType == VersionType.Service)
                .Where(x => !x.Value.Hash.Equals(GetGlobalVersion(currentVersionMap.PackageVersions, x.Key).Hash))
                .ForEach(s =>
            {
                s.Value.Version = newVersion;
                s.Value.IncludeInPackage = true;
                versionMap[s.Value.ParentRef].Version = newVersion;
                versionMap[s.Value.ParentRef].IncludeInPackage = true;
            });

            versionMap
                .Where(x => x.Value.VersionType == VersionType.Application)
                .Where(x => !x.Value.Hash.Equals(GetGlobalVersion(currentVersionMap.PackageVersions, x.Key).Hash))
                .ForEach(s =>
                {
                    s.Value.Version = newVersion;
                    s.Value.IncludeInPackage = true;
                });

            foreach (var globalVersion in versionMap.Where(x => !x.Value.IncludeInPackage))
            {
                if (currentVersionMap.PackageVersions.ContainsKey(globalVersion.Key))
                {
                    globalVersion.Value.Version = currentVersionMap.PackageVersions[globalVersion.Key].Version;
                }
                else
                {
                    if (globalVersion.Value.VersionType != VersionType.ServicePackage)
                        continue;

                    SetVersionOnPackage(globalVersion, newVersion, versionMap);
                }
            }

            if (versionMap.Any(x => x.Value.IncludeInPackage))
                versionMap[Constants.GlobalIdentifier].Version = newVersion;
        }

        private static void SetVersionOnPackage(
            KeyValuePair<string, GlobalVersion> version,
            VersionNumber newVersion,
            IReadOnlyDictionary<string, GlobalVersion> versionMap)
        {
            version.Value.Version = newVersion;
            version.Value.IncludeInPackage = true;
            versionMap[version.Value.ParentRef].Version = newVersion;
            versionMap[version.Value.ParentRef].IncludeInPackage = true;

            var appRef = versionMap[version.Value.ParentRef].ParentRef;
            versionMap[appRef].Version = newVersion;
            versionMap[appRef].IncludeInPackage = true;
        }

        private static GlobalVersion GetGlobalVersion(IReadOnlyDictionary<string, GlobalVersion> currentVersionMap,
            string key)
        {
            return currentVersionMap.ContainsKey(key)
                ? currentVersionMap[key]
                : new GlobalVersion();
        }
    }
}