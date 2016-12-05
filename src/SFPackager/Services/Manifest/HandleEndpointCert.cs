using System.Collections.Generic;
using System.Linq;
using SFPackager.Helpers;
using SFPackager.Models;
using SFPackager.Models.Xml;
using SFPackager.Models.Xml.Elements;

namespace SFPackager.Services.Manifest
{
    public class HandleEndpointCert
    {
        public void SetEndpointCerts(
            PackageConfig packageConfig,
            ApplicationManifest appManifest,
            string appTypeName)
        {
            var httpsCerts = packageConfig
                .Https
                .Where(x => x.ApplicationTypeName.Equals(appTypeName))
                .OrderBy(x => x.CertThumbprint)
                .ToList();

            if (!httpsCerts.Any())
                return;

            if (appManifest.Certificates == null)
                appManifest.Certificates = new Certificates();
            if (appManifest.Certificates.EndpointCertificates == null)
                appManifest.Certificates.EndpointCertificates = new List<EndpointCertificate>();

            var certList = appManifest.Certificates.EndpointCertificates;

            var distinctThumbprints = httpsCerts
                .GroupBy(x => x.CertThumbprint);

            distinctThumbprints.ForEach((certGroup, i) =>
            {
                var certificate = certGroup.First();
                var certName = $"Certificate{i}";

                certList.Add(new EndpointCertificate
                {
                    Name = certName,
                    X509FindValue = certificate.CertThumbprint
                });

                certGroup.ForEach(cert =>
                {
                    var importNodeList = appManifest
                        .ServiceManifestImports
                        .Where(x => x.ServiceManifestRef.ServiceManifestName.Equals(cert.ServiceManifestName))
                        .ToList();

                    if (!importNodeList.Any())
                        return;

                    var importNode = importNodeList.First();

                    if (importNode.Policies == null)
                        importNode.Policies = new Policies();
                    if (importNode.Policies.EndpointBindingPolicy == null)
                        importNode.Policies.EndpointBindingPolicy = new List<EndpointBindingPolicy>();

                    var binding = new EndpointBindingPolicy
                    {
                        CertificateRef = certName,
                        EndpointRef = cert.EndpointName
                    };

                    importNode.Policies.EndpointBindingPolicy.Add(binding);
                });
            });

            appManifest.Certificates.EndpointCertificates = certList;
        }
    }
}