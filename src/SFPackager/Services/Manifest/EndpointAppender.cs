using System.Linq;
using System.Xml;
using SFPackager.Helpers;
using SFPackager.Models;

namespace SFPackager.Services.Manifest
{
    public class EndpointAppender : BaseManifestHandler
    {
        private readonly PackageConfig _packageConfig;

        public EndpointAppender(PackageConfig packageConfig)
        {
            _packageConfig = packageConfig;
        }

        public void SetEndpoints(XmlDocument document, string applicationTypeName, string serviceManifestName)
        {
            var nsManager = GetNsManager(document);

            var endpoints = _packageConfig
                .Endpoints
                .Where(x => x.ApplicationTypeName.Equals(applicationTypeName))
                .Where(x => x.ServiceManifestName.Equals(serviceManifestName))
                .ToList();

            if(!endpoints.Any())
                return;

            document.RemoveNodes("//x:ServiceManifest/x:Resources/x:Endpoints", "/x:Endpoint", nsManager);

            var endpointsNode = document.GetNode("//x:ServiceManifest/x:Resources/x:Endpoints", nsManager);

            foreach (var endpoint in endpoints)
            {
                var endpointElement = document.CreateElement("Endpoint", NamespaceString);
                endpointElement.SetAttribute("Protocol", endpoint.Protocol);
                endpointElement.SetAttribute("Name", endpoint.EndpointName);
                endpointElement.SetAttribute("Type", endpoint.Type);
                endpointElement.SetAttribute("Port", endpoint.Port.ToString());
                endpointsNode.AppendChild(endpointElement);
            }
        }
    }
}