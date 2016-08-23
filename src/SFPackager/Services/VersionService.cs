using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
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
            AzureResponse<string> currentHashMapResponse,
            Dictionary<string, GlobalVersion> versions,
            VersionNumber newVersion)
        {
            var versionMap = JsonConvert.DeserializeObject<Dictionary<string, GlobalVersion>>
                (currentHashMapResponse.ResponseContent);

            var things = versions
                .Where(x => x.Value.VersionType == VersionType.ServicePackage)
                .Where(v => !v.Value.Hash.Equals(versionMap[v.Key].Hash));

            foreach (var v in things)
            {
                SetVersionOnPackage(v, newVersion, versions);
            }

            foreach (var globalVersion in versions.Where(x => !x.Value.IncludeInPackage))
            {
                if (versionMap.ContainsKey(globalVersion.Key))
                {
                    globalVersion.Value.Version = versionMap[globalVersion.Key].Version;
                }
                else
                {
                    if (globalVersion.Value.VersionType != VersionType.ServicePackage)
                        continue;

                    SetVersionOnPackage(globalVersion, newVersion, versions);
                }
            }

            if (versions.Any(x => x.Value.IncludeInPackage))
                versions[Constants.GlobalIdentifier].Version = newVersion;
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