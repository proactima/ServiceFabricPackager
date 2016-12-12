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
    public class DescribeServiceManifest
    {
        public DescribeServiceManifest()
        {
            _serviceManifestHandler = new ServiceManifestHandler();
        }

        private readonly ServiceManifestHandler _serviceManifestHandler;

        [Fact]
        public void ItShouldSetVersionOnServiceManifest()
        {
            // g
            var oldVersion = VersionNumber.Create(1, "hash");
            var newVersion = new GlobalVersion
            {
                Version = VersionNumber.Create(2, "newhash")
            };
            var serviceManifest = new ServiceManifest
            {
                Version = oldVersion.ToString()
            };

            // w
            _serviceManifestHandler.SetServiceManifestGeneral(serviceManifest, newVersion);

            // t
            serviceManifest.Version.Should().Be("2-newhash");
        }

        [Fact]
        public void ItShouldSetVersionOnPackages()
        {
            // g
            var currentVersion = VersionNumber.Create(1, "hash");
            var newVersion = VersionNumber.Create(2, "newhash");

            var serviceManifest = new ServiceManifest
            {
                CodePackage = new CodePackage
                {
                    Name = "Code",
                    Version = currentVersion.ToString()
                },
                ConfigPackage = new ConfigPackage
                {
                    Name = "Config",
                    Version = currentVersion.ToString()
                },
                DataPackages = new List<DataPackage>
                {
                    new DataPackage
                    {
                        Name = "MyData",
                        Version = currentVersion.ToString()
                    }
                }
            };

            var parentRef = "Service1";

            var globalVersions = new Dictionary<string, GlobalVersion>
            {
                ["thing-Code"] = new GlobalVersion
                {
                    ParentRef = parentRef,
                    PackageType = PackageType.Code,
                    VersionType = VersionType.ServicePackage,
                    Version = newVersion
                },
                ["thing-Config"] = new GlobalVersion
                {
                    ParentRef = parentRef,
                    PackageType = PackageType.Config,
                    VersionType = VersionType.ServicePackage,
                    Version = newVersion
                },
                ["thing-MyData"] = new GlobalVersion
                {
                    ParentRef = parentRef,
                    PackageType = PackageType.Data,
                    VersionType = VersionType.ServicePackage,
                    Version = newVersion
                }
            };

            // w
            _serviceManifestHandler.SetServicePackagesData(serviceManifest, globalVersions, parentRef);

            // t
            serviceManifest.CodePackage.Version.Should().Be("2-newhash");
            serviceManifest.ConfigPackage.Version.Should().Be("2-newhash");
            serviceManifest.DataPackages.Count.Should().Be(1);
            serviceManifest.DataPackages.First().Version.Should().Be("2-newhash");
        }
    }
}