using System.Collections.Generic;
using System.Xml;
using SFPackager.Helpers;
using SFPackager.Models;

namespace SFPackager.Services.Manifest
{
    public class ServiceImportMutator : BaseManifestHandler
    {
        public void Execute(XmlDocument document, Dictionary<string, GlobalVersion> versions)
        {
            var nsManager = GetNsManager(document);

            var serviceNodes = document.GetNodes(
                "//x:ApplicationManifest/x:ServiceManifestImport/x:ServiceManifestRef", nsManager);

            foreach (var service in serviceNodes)
            {
                var serviceElement = service as XmlElement;
                if (serviceElement == null)
                    continue;

                var serviceName = serviceElement.GetAttribute("ServiceManifestName");
                if (!versions.ContainsKey(serviceName))
                    continue;

                var serviceVersion = versions[serviceName].Version;
                serviceElement.SetAttribute("ServiceManifestVersion", serviceVersion.ToString());
            }
        }
    }
}