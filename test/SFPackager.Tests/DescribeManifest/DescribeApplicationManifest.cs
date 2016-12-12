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
    public class DescribeApplicationManifest
    {
        public DescribeApplicationManifest()
        {
            _appManifestHandler = new ApplicationManifestHandler();
        }

        private readonly ApplicationManifestHandler _appManifestHandler;

        [Fact]
        public void ItShouldSetAppTypeVersion()
        {
            // g
            var appManifest = new ApplicationManifest
            {
                ServiceManifestImports = new List<ServiceManifestImport>()
            };

            var version = new GlobalVersion
            {
                Hash = "hash",
                Version = VersionNumber.Create(2, "hash")
            };
            var versions = new Dictionary<string, GlobalVersion>();

            // w
            _appManifestHandler.SetGeneralInfo(appManifest, versions, version);

            // t
            appManifest.ApplicationTypeVersion.Should().Be("2-hash");
        }

        [Fact]
        public void ItShouldSetVersionOnServiceImport()
        {
            // g
            var appManifest = new ApplicationManifest
            {
                ServiceManifestImports = new List<ServiceManifestImport>
                {
                    new ServiceManifestImport
                    {
                        ServiceManifestRef = new ServiceManifestRef
                        {
                            ServiceManifestName = "Service1",
                            ServiceManifestVersion = "1-hash"
                        }
                    },
                    new ServiceManifestImport
                    {
                        ServiceManifestRef = new ServiceManifestRef
                        {
                            ServiceManifestName = "Service2",
                            ServiceManifestVersion = "1-hash"
                        }
                    }
                }
            };

            var newVersionNumber = VersionNumber.Create(2, "newhash");
            var version = new GlobalVersion
            {
                Hash = "hash",
                Version = newVersionNumber
            };
            var versions = new Dictionary<string, GlobalVersion>
            {
                ["Service1"] = new GlobalVersion { Version = newVersionNumber}
            };

            // w
            _appManifestHandler.SetGeneralInfo(appManifest, versions, version);

            // t
            var service1 = appManifest.ServiceManifestImports.Single(
                    x => x.ServiceManifestRef.ServiceManifestName.Equals("Service1"));

            service1.ServiceManifestRef.ServiceManifestVersion.Should().Be("2-newhash");

            var service2 = appManifest.ServiceManifestImports.Single(
                    x => x.ServiceManifestRef.ServiceManifestName.Equals("Service2"));

            service2.ServiceManifestRef.ServiceManifestVersion.Should().Be("1-hash");
        }
    }
}