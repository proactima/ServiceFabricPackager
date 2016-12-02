using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using SFPackager.Models;
using SFPackager.Models.Xml;
using SFPackager.Models.Xml.Elements;
using SFPackager.Services.Manifest;
using Xunit;

namespace SFPackager.Tests.DescribeManifest
{
    public class DescribeCertificateAppender
    {
        [Fact]
        public void ItHandlesASingleCertOnASignleEndpoint()
        {
            // g
            var packageConfig = new PackageConfig
            {
                Https = new List<HttpsConfig>
                {
                    new HttpsConfig
                    {
                        ApplicationTypeName = "MyApp",
                        CertThumbprint = "MyCert",
                        ServiceManifestName = "MyService",
                        EndpointName = "MyEndpoint"
                    }
                }
            };

            var appManifest = new ApplicationManifest
            {
                ServiceManifestImports = new List<ServiceManifestImport>
                {
                    new ServiceManifestImport
                    {
                        ServiceManifestRef = new ServiceManifestRef
                        {
                            ServiceManifestName = "MyService"
                        }
                    }
                }
            };

            var endpointHandler = new HandleEndpointCert();

            // w
            endpointHandler.SetEndpointCerts(packageConfig, appManifest, "MyApp");

            // t
            var endpointBindingPolicy = appManifest
                .ServiceManifestImports.First()
                .Policies
                .EndpointBindingPolicy.First();

            endpointBindingPolicy.EndpointRef.Should().Be("MyEndpoint");
            endpointBindingPolicy.CertificateRef.Should().Be("Certificate0");

            var endpointCertificate = appManifest.Certificates.EndpointCertificates.First();

            endpointCertificate.Name.Should().Be("Certificate0");
            endpointCertificate.X509FindValue.Should().Be("MyCert");
        }
    }
}