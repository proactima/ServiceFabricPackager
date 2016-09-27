using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using SFPackager.Models;
using SFPackager.Services;
using Xunit;

namespace SFPackager.Tests.DescribeVersionService
{
    public class DescribeSetVersionsIfVersionIsDeployed
    {
        private readonly VersionService _versionService;

        public DescribeSetVersionsIfVersionIsDeployed()
        {
            _versionService = new VersionService();
        }

        [Fact]
        public void WhenAllHasChanged()
        {
            // g
            var newVersion = VersionNumber.Create(2, "testhash");
            var response = File.ReadAllText(@"DescribeVersionService\BasicVersionResponse.json");
            var currentHashMapResponse = new Response<string>
            {
                StatusCode = HttpStatusCode.OK,
                Operation = BlobOperation.GET,
                ResponseContent = response
            };
            var versions = new Dictionary<string, GlobalVersion>
            {
                [Constants.GlobalIdentifier] = new GlobalVersion
                {
                    VersionType = VersionType.Global,
                    Version = VersionNumber.Default()
                },
                ["App"] = new GlobalVersion
                {
                    VersionType = VersionType.Application,
                    Version = VersionNumber.Default()
                },
                ["Service"] = new GlobalVersion
                {
                    VersionType = VersionType.Service,
                    Version = VersionNumber.Default(),
                    ParentRef = "App",
                    Hash = "x"
                },
                ["Service-Code"] = new GlobalVersion
                {
                    VersionType = VersionType.ServicePackage,
                    Version = VersionNumber.Default(),
                    ParentRef = "Service",
                    Hash = "y"
                },
                ["Service-Config"] = new GlobalVersion
                {
                    VersionType = VersionType.ServicePackage,
                    Version = VersionNumber.Default(),
                    ParentRef = "Service",
                    Hash = "z"
                }
            };

            // w
            _versionService.SetVersionsIfVersionIsDeployed(currentHashMapResponse, versions, newVersion);

            // t
            foreach (var actual in versions.Where(x => x.Value.VersionType != VersionType.Global))
            {
                actual.Value.Version.RollingNumber.Should().Be(2);
                actual.Value.Version.CommitHash.Should().Be("testhash");
                actual.Value.IncludeInPackage.Should().BeTrue();
            }

            versions[Constants.GlobalIdentifier].Version.RollingNumber.Should().Be(2);
            versions[Constants.GlobalIdentifier].Version.CommitHash.Should().Be("testhash");
        }

        [Fact]
        public void WhenOnlySomeHasChanged()
        {
            // g
            var newVersion = VersionNumber.Create(2, "testhash");
            var response = File.ReadAllText(@"DescribeVersionService\BasicVersionResponse.json");
            var currentHashMapResponse = new Response<string>
            {
                StatusCode = HttpStatusCode.OK,
                Operation = BlobOperation.GET,
                ResponseContent = response
            };
            var versions = new Dictionary<string, GlobalVersion>
            {
                [Constants.GlobalIdentifier] = new GlobalVersion
                {
                    VersionType = VersionType.Global,
                    Version = VersionNumber.Default()
                },
                ["App"] = new GlobalVersion
                {
                    VersionType = VersionType.Application,
                    Version = VersionNumber.Default()
                },
                ["Service"] = new GlobalVersion
                {
                    VersionType = VersionType.Service,
                    Version = VersionNumber.Default(),
                    ParentRef = "App",
                    Hash = "x"
                },
                ["Service-Code"] = new GlobalVersion
                {
                    VersionType = VersionType.ServicePackage,
                    Version = VersionNumber.Default(),
                    ParentRef = "Service",
                    Hash = "a",
                    PackageType = PackageType.Code
                },
                ["Service-Config"] = new GlobalVersion
                {
                    VersionType = VersionType.ServicePackage,
                    Version = VersionNumber.Default(),
                    ParentRef = "Service",
                    Hash = "z",
                    PackageType = PackageType.Config
                }
            };

            // w
            _versionService.SetVersionsIfVersionIsDeployed(currentHashMapResponse, versions, newVersion);

            // t
            versions[Constants.GlobalIdentifier].Version.RollingNumber.Should().Be(2);
            versions[Constants.GlobalIdentifier].Version.CommitHash.Should().Be("testhash");
            versions["App"].IncludeInPackage.Should().BeTrue();
            versions["App"].Version.RollingNumber.Should().Be(2);
            versions["App"].Version.CommitHash.Should().Be("testhash");
            versions["Service"].IncludeInPackage.Should().BeTrue();
            versions["Service"].Version.RollingNumber.Should().Be(2);
            versions["Service"].Version.CommitHash.Should().Be("testhash");
            versions["Service-Code"].IncludeInPackage.Should().BeFalse();
            versions["Service-Code"].Version.RollingNumber.Should().Be(1);
            versions["Service-Code"].Version.CommitHash.Should().Be("orghash");
            versions["Service-Config"].IncludeInPackage.Should().BeTrue();
            versions["Service-Config"].Version.RollingNumber.Should().Be(2);
            versions["Service-Config"].Version.CommitHash.Should().Be("testhash");
        }

        [Fact]
        public void WhenAddingANewService()
        {
            // g
            var newVersion = VersionNumber.Create(2, "testhash");
            var response = File.ReadAllText(@"DescribeVersionService\BasicVersionResponse.json");
            var currentHashMapResponse = new Response<string>
            {
                StatusCode = HttpStatusCode.OK,
                Operation = BlobOperation.GET,
                ResponseContent = response
            };

            var versions = new Dictionary<string, GlobalVersion>
            {
                [Constants.GlobalIdentifier] = new GlobalVersion
                {
                    VersionType = VersionType.Global,
                    Version = VersionNumber.Default()
                },
                ["App"] = new GlobalVersion
                {
                    VersionType = VersionType.Application,
                    Version = VersionNumber.Default()
                },
                ["Service"] = new GlobalVersion
                {
                    VersionType = VersionType.Service,
                    Version = VersionNumber.Default(),
                    ParentRef = "App",
                    Hash = "x"
                },
                ["Service-Code"] = new GlobalVersion
                {
                    VersionType = VersionType.ServicePackage,
                    Version = VersionNumber.Default(),
                    ParentRef = "Service",
                    Hash = "a",
                    PackageType = PackageType.Code
                },
                ["Service-Config"] = new GlobalVersion
                {
                    VersionType = VersionType.ServicePackage,
                    Version = VersionNumber.Default(),
                    ParentRef = "Service",
                    Hash = "z",
                    PackageType = PackageType.Config
                },
                ["Service2"] = new GlobalVersion
                {
                    VersionType = VersionType.Service,
                    Version = VersionNumber.Default(),
                    ParentRef = "App",
                    Hash = "xx"
                },
                ["Service2-Code"] = new GlobalVersion
                {
                    VersionType = VersionType.ServicePackage,
                    Version = VersionNumber.Default(),
                    ParentRef = "Service",
                    Hash = "aa",
                    PackageType = PackageType.Code
                },
                ["Service2-Config"] = new GlobalVersion
                {
                    VersionType = VersionType.ServicePackage,
                    Version = VersionNumber.Default(),
                    ParentRef = "Service",
                    Hash = "zz",
                    PackageType = PackageType.Config
                }
            };

            // w
            _versionService.SetVersionsIfVersionIsDeployed(currentHashMapResponse, versions, newVersion);

            // t
            versions[Constants.GlobalIdentifier].Version.RollingNumber.Should().Be(2);
            versions[Constants.GlobalIdentifier].Version.CommitHash.Should().Be("testhash");
            versions["App"].IncludeInPackage.Should().BeTrue();
            versions["App"].Version.RollingNumber.Should().Be(2);
            versions["App"].Version.CommitHash.Should().Be("testhash");
            versions["Service"].IncludeInPackage.Should().BeTrue();
            versions["Service"].Version.RollingNumber.Should().Be(2);
            versions["Service"].Version.CommitHash.Should().Be("testhash");
            versions["Service-Code"].IncludeInPackage.Should().BeFalse();
            versions["Service-Code"].Version.RollingNumber.Should().Be(1);
            versions["Service-Code"].Version.CommitHash.Should().Be("orghash");
            versions["Service-Config"].IncludeInPackage.Should().BeTrue();
            versions["Service-Config"].Version.RollingNumber.Should().Be(2);
            versions["Service-Config"].Version.CommitHash.Should().Be("testhash");
            versions["Service2"].IncludeInPackage.Should().BeTrue("Service has been added");
            versions["Service2"].Version.RollingNumber.Should().Be(2);
            versions["Service2"].Version.CommitHash.Should().Be("testhash");
            versions["Service2-Code"].IncludeInPackage.Should().BeTrue();
            versions["Service2-Code"].Version.RollingNumber.Should().Be(2);
            versions["Service2-Code"].Version.CommitHash.Should().Be("testhash");
            versions["Service2-Config"].IncludeInPackage.Should().BeTrue();
            versions["Service2-Config"].Version.RollingNumber.Should().Be(2);
            versions["Service2-Config"].Version.CommitHash.Should().Be("testhash");
        }

        [Fact]
        public void WhenOnlySomeHasChangedComplex()
        {
            // g
            var newVersion = VersionNumber.Create(2, "testhash");
            var response = File.ReadAllText(@"DescribeVersionService\ComplexVersionResponse.json");
            var currentHashMapResponse = new Response<string>
            {
                StatusCode = HttpStatusCode.OK,
                Operation = BlobOperation.GET,
                ResponseContent = response
            };
            var versions = new Dictionary<string, GlobalVersion>
            {
                [Constants.GlobalIdentifier] = new GlobalVersion
                {
                    VersionType = VersionType.Global,
                    Version = VersionNumber.Default()
                },
                ["App"] = new GlobalVersion
                {
                    VersionType = VersionType.Application,
                    Version = VersionNumber.Default()
                },
                ["Service"] = new GlobalVersion
                {
                    VersionType = VersionType.Service,
                    Version = VersionNumber.Default(),
                    ParentRef = "App"
                },
                ["Service-Code"] = new GlobalVersion
                {
                    VersionType = VersionType.ServicePackage,
                    Version = VersionNumber.Default(),
                    ParentRef = "Service",
                    Hash = "a",
                    PackageType = PackageType.Code
                },
                ["Service-Config"] = new GlobalVersion
                {
                    VersionType = VersionType.ServicePackage,
                    Version = VersionNumber.Default(),
                    ParentRef = "Service",
                    Hash = "z",
                    PackageType = PackageType.Config
                },
                ["App2"] = new GlobalVersion
                {
                    VersionType = VersionType.Application,
                    Version = VersionNumber.Default()
                },
                ["Service2"] = new GlobalVersion
                {
                    VersionType = VersionType.Service,
                    Version = VersionNumber.Default(),
                    ParentRef = "App2"
                },
                ["Service2-Code"] = new GlobalVersion
                {
                    VersionType = VersionType.ServicePackage,
                    Version = VersionNumber.Default(),
                    ParentRef = "Service2",
                    Hash = "ww",
                    PackageType = PackageType.Code
                },
                ["Service2-Config"] = new GlobalVersion
                {
                    VersionType = VersionType.ServicePackage,
                    Version = VersionNumber.Default(),
                    ParentRef = "Service2",
                    Hash = "zz",
                    PackageType = PackageType.Config
                }
            };

            // w
            _versionService.SetVersionsIfVersionIsDeployed(currentHashMapResponse, versions, newVersion);

            // t
            versions[Constants.GlobalIdentifier].Version.RollingNumber.Should().Be(2);
            versions[Constants.GlobalIdentifier].Version.CommitHash.Should().Be("testhash");
            versions["App"].IncludeInPackage.Should().BeTrue();
            versions["App"].Version.RollingNumber.Should().Be(2);
            versions["App"].Version.CommitHash.Should().Be("testhash");
            versions["Service"].IncludeInPackage.Should().BeTrue();
            versions["Service"].Version.RollingNumber.Should().Be(2);
            versions["Service"].Version.CommitHash.Should().Be("testhash");
            versions["Service-Code"].IncludeInPackage.Should().BeFalse();
            versions["Service-Code"].Version.RollingNumber.Should().Be(1);
            versions["Service-Code"].Version.CommitHash.Should().Be("orghash");
            versions["Service-Config"].IncludeInPackage.Should().BeTrue();
            versions["Service-Config"].Version.RollingNumber.Should().Be(2);
            versions["Service-Config"].Version.CommitHash.Should().Be("testhash");
            versions["App2"].IncludeInPackage.Should().BeFalse();
            versions["App2"].Version.RollingNumber.Should().Be(1);
            versions["App2"].Version.CommitHash.Should().Be("orghash");
            versions["Service2"].IncludeInPackage.Should().BeFalse();
            versions["Service2"].Version.RollingNumber.Should().Be(1);
            versions["Service2"].Version.CommitHash.Should().Be("orghash");
            versions["Service2-Code"].IncludeInPackage.Should().BeFalse();
            versions["Service2-Code"].Version.RollingNumber.Should().Be(1);
            versions["Service2-Code"].Version.CommitHash.Should().Be("orghash");
            versions["Service2-Config"].IncludeInPackage.Should().BeFalse();
            versions["Service2-Config"].Version.RollingNumber.Should().Be(1);
            versions["Service2-Config"].Version.CommitHash.Should().Be("orghash");
        }
    }
}