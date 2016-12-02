using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using SFPackager.Models;
using SFPackager.Models.Xml;
using SFPackager.Services.Manifest;
using Xunit;

namespace SFPackager.Tests.DescribeManifest
{
    public class DescribePrincipalAppender
    {
        [Fact]
        public void Test()
        {
            // g
            var packageConfig = new PackageConfig
            {
                Encipherment = new List<Encipherment>
                {
                    new Encipherment
                    {
                        ApplicationTypeName = "MyApp",
                        CertName = "MyCert",
                        CertThumbprint = "MyThumb",
                        Name = "MyCustomName"
                    }
                }
            };

            var appManifest = new ApplicationManifest();
            var enciphermentHandler = new HandleEnciphermentCert();

            // w
            enciphermentHandler.SetEnciphermentCerts(packageConfig, appManifest, "MyApp");

            // t
            var securityAccessPolicy = appManifest.Policies.SecurityAccessPolicies.SecurityAccessPolicy.First();
            securityAccessPolicy.ResourceRef.Should().Be("MyCert");
            securityAccessPolicy.GrantRights.Should().Be("Read");
            securityAccessPolicy.PrincipalRef.Should().Be("MyCustomName");
            securityAccessPolicy.ResourceType.Should().Be("Certificate");

            var user = appManifest.Principals.Users.User.First();
            user.AccountType.Should().Be("NetworkService");
            user.Name.Should().Be("MyCustomName");

            var certificate = appManifest.Certificates.SecretsCertificate.First();
            certificate.Name.Should().Be("MyCert");
            certificate.X509FindType.Should().Be("FindByThumbprint");
            certificate.X509FindValue.Should().Be("MyThumb");
        }
    }
}