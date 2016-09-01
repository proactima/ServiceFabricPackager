using System.Collections.Generic;
using System.Xml;

namespace SFPackager.Services.Manifest
{
    public class FakeManifestCreator : BaseManifestHandler
    {
        public XmlDocument GetFakeServiceManifest()
        {
            var document = new XmlDocument(new NameTable());

            var manifestElement = document.CreateElement("ServiceManifest", NamespaceString);
            var resourcesElement = document.CreateElement("Resources", NamespaceString);
            var endpointsElement = document.CreateElement("Endpoints", NamespaceString);

            resourcesElement.AppendChild(endpointsElement);
            manifestElement.AppendChild(resourcesElement);
            document.AppendChild(manifestElement);

            return document;
        }

        public XmlDocument GetFakeApplicationManifest(List<string> serviceManifestNames)
        {
            var document = new XmlDocument(new NameTable());

            var manifestElement = document.CreateElement("ApplicationManifest", NamespaceString);

            foreach (var serviceManifestName in serviceManifestNames)
            {
                var importElement = document.CreateElement("ServiceManifestImport", NamespaceString);
                var serviceElement = document.CreateElement("ServiceManifestRef", NamespaceString);
                serviceElement.SetAttribute("ServiceManifestName", serviceManifestName);
                importElement.AppendChild(serviceElement);
                manifestElement.AppendChild(importElement);
            }

            document.AppendChild(manifestElement);

            return document;
        }
    }
}