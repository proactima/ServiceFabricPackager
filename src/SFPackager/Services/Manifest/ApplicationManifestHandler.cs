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
                if (!versions.ContainsKey(serviceName))
                    continue;

                serviceImport.ServiceManifestRef.ServiceManifestVersion = versions[serviceName].Version.ToString();
            }
        }
    }
}