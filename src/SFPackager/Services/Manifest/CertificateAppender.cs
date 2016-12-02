using System.Collections.Generic;
using System.Linq;
using System.Xml;
using SFPackager.Helpers;
using SFPackager.Models;

namespace SFPackager.Services.Manifest
{
    public class CertificateAppender : BaseManifestHandler
    {
        private readonly PackageConfig _packageConfig;

        public CertificateAppender(PackageConfig packageConfig)
        {
            _packageConfig = packageConfig;
        }

        public void SetCertificates(XmlDocument document, string applicationTypeName, List<string> serviceManifestNames)
        {
            var nsManager = GetNsManager(document);

            var httpsCerts = _packageConfig
                .Https
                .Where(x => x.ApplicationTypeName.Equals(applicationTypeName))
                .OrderBy(x => x.CertThumbprint)
                .ToList();

            var secretCerts = _packageConfig
                .Encipherment
                .Where(x => x.ApplicationTypeName.Equals(applicationTypeName))
                .ToList();

            if (!httpsCerts.Any() && !secretCerts.Any())
                return;

            var certificateElement = document.CreateElement("Certificates", NamespaceString);
            var distinctThumbprints = httpsCerts.GroupBy(x => x.CertThumbprint);

            distinctThumbprints.ForEach((certGroup, i) =>
            {
                var certificate = certGroup.First();
                var certName = $"Certificate{i}";

                var endpointElement = document.CreateElement("EndpointCertificate", NamespaceString);
                endpointElement.SetAttribute("X509FindValue", certificate.CertThumbprint);
                endpointElement.SetAttribute("Name", certName);
                certificateElement.AppendChild(endpointElement);

                certGroup.ForEach(cert =>
                {
                    var serviceManifestNode = document.GetNode($"//x:ApplicationManifest/x:ServiceManifestImport/x:ServiceManifestRef[@ServiceManifestName='{cert.ServiceManifestName}']", nsManager);
                    var policyNode = document.CreateElement("Policies", NamespaceString);
                    var bindingNode = document.CreateElement("EndpointBindingPolicy", NamespaceString);
                    bindingNode.SetAttribute("EndpointRef", cert.EndpointName);
                    bindingNode.SetAttribute("CertificateRef", certName);
                    policyNode.AppendChild(bindingNode);

                    var importNode = serviceManifestNode.ParentNode;
                    importNode.AppendChild(policyNode);
                });
            });

            foreach (var encipherment in secretCerts)
            {
                var secretElement = document.CreateElement("SecretsCertificate", NamespaceString);
                secretElement.SetAttribute("Name", encipherment.CertName);
                secretElement.SetAttribute("X509FindType", "FindByThumbprint");
                secretElement.SetAttribute("X509FindValue", encipherment.CertThumbprint);

                certificateElement.AppendChild(secretElement);
            }

            var root = document.GetNode("//x:ApplicationManifest", nsManager);
            root.AppendChild(certificateElement);
        }
    }
}