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
            Dictionary<string, GlobalVersion> versions,
            VersionNumber newVersion)
        {
            foreach (var v in versions)
            {
                v.Value.Version = newVersion;
                v.Value.IncludeInPackage = true;

                if (v.Value.VersionType != VersionType.Service)
                    continue;

                versions[v.Value.ParentRef].IncludeInPackage = true;
                versions[v.Value.ParentRef].Version = newVersion;
            }

            versions[Constants.GlobalIdentifier].Version = newVersion;
        }

        public void SetVersionsIfVersionIsDeployed(
            Response<string> currentHashMapResponse,
            Dictionary<string, GlobalVersion> packagedVersionMap,
            VersionNumber newVersion)
        {
            var currentVersionMap = JsonConvert.DeserializeObject<Dictionary<string, GlobalVersion>>
                (currentHashMapResponse.ResponseContent);

            packagedVersionMap
                .Where(x => x.Value.VersionType == VersionType.ServicePackage)
                .Where(v => !v.Value.Hash.Equals(currentVersionMap[v.Key].Hash))
                .ForEach(s =>
                {
                    SetVersionOnPackage(s, newVersion, packagedVersionMap);
                });
            
            packagedVersionMap
                .Where(x => x.Value.VersionType == VersionType.Service)
                .Where(x => !x.Value.Hash.Equals(currentVersionMap[x.Key].Hash))
                .ForEach(s =>
            {
                s.Value.Version = newVersion;
                s.Value.IncludeInPackage = true;
                packagedVersionMap[s.Value.ParentRef].Version = newVersion;
                packagedVersionMap[s.Value.ParentRef].IncludeInPackage = true;
            });

            packagedVersionMap
                .Where(x => x.Value.VersionType == VersionType.Application)
                .Where(x => !x.Value.Hash.Equals(currentVersionMap[x.Key].Hash))
                .ForEach(s =>
                {
                    s.Value.Version = newVersion;
                    s.Value.IncludeInPackage = true;
                });

            foreach (var globalVersion in packagedVersionMap.Where(x => !x.Value.IncludeInPackage))
            {
                if (currentVersionMap.ContainsKey(globalVersion.Key))
                {
                    globalVersion.Value.Version = currentVersionMap[globalVersion.Key].Version;
                }
                else
                {
                    if (globalVersion.Value.VersionType != VersionType.ServicePackage)
                        continue;

                    SetVersionOnPackage(globalVersion, newVersion, packagedVersionMap);
                }
            }

            if (packagedVersionMap.Any(x => x.Value.IncludeInPackage))
                packagedVersionMap[Constants.GlobalIdentifier].Version = newVersion;
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
    }
}