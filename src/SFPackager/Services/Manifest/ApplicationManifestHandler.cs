using System.Collections.Generic;
using SFPackager.Models;
using SFPackager.Models.Xml;

namespace SFPackager.Services.Manifest
{
    public class ApplicationManifestHandler
    {
        public void SetGeneralInfo(
            ApplicationManifest appManifest,
            IReadOnlyDictionary<string, GlobalVersion> versions,
            GlobalVersion appTypeVersion)
        {
            appManifest.ApplicationTypeVersion = appTypeVersion.Version.ToString();

            foreach (var serviceImport in appManifest.ServiceManifestImports)
            {
                var serviceName = serviceImport.ServiceManifestRef.ServiceManifestName;
                var properServiceKey = $"{appManifest.ApplicationTypeName}-{serviceName}";
                if (!versions.ContainsKey(properServiceKey))
                    continue;

                serviceImport.ServiceManifestRef.ServiceManifestVersion = versions[properServiceKey].Version.ToString();
            }
        }
    }
}