using System.Xml;

namespace SFPackager.Services.Manifest
{
    public class FakeManifestCreator : BaseManifestHandler
    {
        public XmlDocument GetFakeServiceManifest()
        {
            var document = new XmlDocument(new NameTable());
            var nsManager = GetNsManager(document);

            var manifestElement = document.CreateElement("ServiceManifest", NamespaceString);
            var resourcesElement = document.CreateElement("Resources", NamespaceString);
            var endpointsElement = document.CreateElement("Endpoints", NamespaceString);

            resourcesElement.AppendChild(endpointsElement);
            manifestElement.AppendChild(resourcesElement);
            document.AppendChild(manifestElement);

            return document;
        }
    }
}