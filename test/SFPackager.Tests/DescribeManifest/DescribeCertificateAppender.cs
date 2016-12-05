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
        public void ItHandlesASingleCertOnASingleEndpoint()
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

        [Fact]
        public void ItHandlesTwoCertsSpreadOnTwoEndpoints()
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
                    },
                    new HttpsConfig
                    {
                        ApplicationTypeName = "MyApp",
                        CertThumbprint = "MyOtherCert",
                        ServiceManifestName = "MyOtherService",
                        EndpointName = "MyOtherEndpoint"
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
                    },
                    new ServiceManifestImport
                    {
                        ServiceManifestRef = new ServiceManifestRef
                        {
                            ServiceManifestName = "MyOtherService"
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

            var otherEndpointBindingPolicy = appManifest
                .ServiceManifestImports.Last()
                .Policies
                .EndpointBindingPolicy.First();

            otherEndpointBindingPolicy.EndpointRef.Should().Be("MyOtherEndpoint");
            otherEndpointBindingPolicy.CertificateRef.Should().Be("Certificate1");

            var otherEndpointCertificate = appManifest.Certificates.EndpointCertificates.Last();

            otherEndpointCertificate.Name.Should().Be("Certificate1");
            otherEndpointCertificate.X509FindValue.Should().Be("MyOtherCert");
        }

        [Fact]
        public void ItHandlesSingleCertOnTwoEndpoints()
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
                    },
                    new HttpsConfig
                    {
                        ApplicationTypeName = "MyApp",
                        CertThumbprint = "MyCert",
                        ServiceManifestName = "MyService",
                        EndpointName = "MyOtherEndpoint"
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

            var otherEndpointBindingPolicy = appManifest
                .ServiceManifestImports.First()
                .Policies
                .EndpointBindingPolicy.Last();

            otherEndpointBindingPolicy.EndpointRef.Should().Be("MyOtherEndpoint");
            otherEndpointBindingPolicy.CertificateRef.Should().Be("Certificate0");

            var endpointCertificate = appManifest.Certificates.EndpointCertificates.First();
            endpointCertificate.Name.Should().Be("Certificate0");
            endpointCertificate.X509FindValue.Should().Be("MyCert");
        }
    }
}