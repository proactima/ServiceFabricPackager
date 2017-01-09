using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using SFPackager.Models;
using SFPackager.Services;
using Xunit;

namespace SFPackager.Tests.DescribeVersionService
{
    public class DescribeSetVersionsIfVersionIsDeployed
    {
        public DescribeSetVersionsIfVersionIsDeployed()
        {
            _versionService = new VersionService();
        }

        private readonly VersionService _versionService;

        [Fact]
        public void WhenAddingANewService()
        {
            // g
            var newVersion = VersionNumber.Create(2, "testhash");
            var response = File.ReadAllText(@"DescribeVersionService\BasicVersionResponse.json");
            var currentVersionMap = new VersionMap
            {
                PackageVersions = JsonConvert.DeserializeObject<Dictionary<string, GlobalVersion>>(response)
            };

            var versions = new VersionMap
            {
                PackageVersions = new Dictionary<string, GlobalVersion>
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
                    ["Service-Data"] = new GlobalVersion
                    {
                        VersionType = VersionType.ServicePackage,
                        Version = VersionNumber.Default(),
                        ParentRef = "Service",
                        Hash = "z",
                        PackageType = PackageType.Data
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
                }
            };

            // w
            _versionService.SetVersionsIfVersionIsDeployed(currentVersionMap, versions, newVersion);

            // t
            var versionMap = versions.PackageVersions;
            versionMap[Constants.GlobalIdentifier].Version.RollingNumber.Should().Be(2);
            versionMap[Constants.GlobalIdentifier].Version.CommitHash.Should().Be("testhash");
            versionMap["App"].IncludeInPackage.Should().BeTrue();
            versionMap["App"].Version.RollingNumber.Should().Be(2);
            versionMap["App"].Version.CommitHash.Should().Be("testhash");
            versionMap["Service"].IncludeInPackage.Should().BeTrue();
            versionMap["Service"].Version.RollingNumber.Should().Be(2);
            versionMap["Service"].Version.CommitHash.Should().Be("testhash");
            versionMap["Service-Code"].IncludeInPackage.Should().BeFalse();
            versionMap["Service-Code"].Version.RollingNumber.Should().Be(1);
            versionMap["Service-Code"].Version.CommitHash.Should().Be("orghash");
            versionMap["Service-Config"].IncludeInPackage.Should().BeTrue();
            versionMap["Service-Config"].Version.RollingNumber.Should().Be(2);
            versionMap["Service-Config"].Version.CommitHash.Should().Be("testhash");
            versionMap["Service-Data"].IncludeInPackage.Should().BeTrue();
            versionMap["Service-Data"].Version.RollingNumber.Should().Be(2);
            versionMap["Service-Data"].Version.CommitHash.Should().Be("testhash");
            versionMap["Service2"].IncludeInPackage.Should().BeTrue("Service has been added");
            versionMap["Service2"].Version.RollingNumber.Should().Be(2);
            versionMap["Service2"].Version.CommitHash.Should().Be("testhash");
            versionMap["Service2-Code"].IncludeInPackage.Should().BeTrue();
            versionMap["Service2-Code"].Version.RollingNumber.Should().Be(2);
            versionMap["Service2-Code"].Version.CommitHash.Should().Be("testhash");
            versionMap["Service2-Config"].IncludeInPackage.Should().BeTrue();
            versionMap["Service2-Config"].Version.RollingNumber.Should().Be(2);
            versionMap["Service2-Config"].Version.CommitHash.Should().Be("testhash");
        }
        
        [Fact]
        public void WhenAllHasChanged()
        {
            // g
            var newVersion = VersionNumber.Create(2, "testhash");
            var response = File.ReadAllText(@"DescribeVersionService\BasicVersionResponse.json");
            var currentVersionMap = new VersionMap
            {
                PackageVersions = JsonConvert.DeserializeObject<Dictionary<string, GlobalVersion>>(response)
            };

            var versions = new VersionMap
            {
                PackageVersions = new Dictionary<string, GlobalVersion>
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
                }
            };

            // w
            _versionService.SetVersionsIfVersionIsDeployed(currentVersionMap, versions, newVersion);

            // t
            foreach (var actual in versions.PackageVersions.Where(x => x.Value.VersionType != VersionType.Global))
            {
                actual.Value.Version.RollingNumber.Should().Be(2);
                actual.Value.Version.CommitHash.Should().Be("testhash");
                actual.Value.IncludeInPackage.Should().BeTrue();
            }

            versions.PackageVersions[Constants.GlobalIdentifier].Version.RollingNumber.Should().Be(2);
            versions.PackageVersions[Constants.GlobalIdentifier].Version.CommitHash.Should().Be("testhash");
        }

        [Fact]
        public void WhenOnlySomeHasChanged()
        {
            // g
            var newVersion = VersionNumber.Create(2, "testhash");
            var response = File.ReadAllText(@"DescribeVersionService\BasicVersionResponse.json");
            var currentVersionMap = new VersionMap
            {
                PackageVersions = JsonConvert.DeserializeObject<Dictionary<string, GlobalVersion>>(response)
            };

            var newVersions = new VersionMap
            {
                PackageVersions = new Dictionary<string, GlobalVersion>
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
                }
            };

            // w
            _versionService.SetVersionsIfVersionIsDeployed(currentVersionMap, newVersions, newVersion);

            // t
            var versionMap = newVersions.PackageVersions;
            versionMap[Constants.GlobalIdentifier].Version.RollingNumber.Should().Be(2);
            versionMap[Constants.GlobalIdentifier].Version.CommitHash.Should().Be("testhash");
            versionMap["App"].IncludeInPackage.Should().BeTrue();
            versionMap["App"].Version.RollingNumber.Should().Be(2);
            versionMap["App"].Version.CommitHash.Should().Be("testhash");
            versionMap["Service"].IncludeInPackage.Should().BeTrue();
            versionMap["Service"].Version.RollingNumber.Should().Be(2);
            versionMap["Service"].Version.CommitHash.Should().Be("testhash");
            versionMap["Service-Code"].IncludeInPackage.Should().BeFalse();
            versionMap["Service-Code"].Version.RollingNumber.Should().Be(1);
            versionMap["Service-Code"].Version.CommitHash.Should().Be("orghash");
            versionMap["Service-Config"].IncludeInPackage.Should().BeTrue();
            versionMap["Service-Config"].Version.RollingNumber.Should().Be(2);
            versionMap["Service-Config"].Version.CommitHash.Should().Be("testhash");
        }

        [Fact]
        public void WhenOnlySomeHasChangedComplex()
        {
            // g
            var newVersion = VersionNumber.Create(2, "testhash");
            var response = File.ReadAllText(@"DescribeVersionService\ComplexVersionResponse.json");
            var currentVersionMap = new VersionMap
            {
                PackageVersions = JsonConvert.DeserializeObject<Dictionary<string, GlobalVersion>>(response)
            };

            var versions = new VersionMap
            {
                PackageVersions = new Dictionary<string, GlobalVersion>
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
                }
            };

            // w
            _versionService.SetVersionsIfVersionIsDeployed(currentVersionMap, versions, newVersion);

            // t
            var versionMap = versions.PackageVersions;
            versionMap[Constants.GlobalIdentifier].Version.RollingNumber.Should().Be(2);
            versionMap[Constants.GlobalIdentifier].Version.CommitHash.Should().Be("testhash");
            versionMap["App"].IncludeInPackage.Should().BeTrue();
            versionMap["App"].Version.RollingNumber.Should().Be(2);
            versionMap["App"].Version.CommitHash.Should().Be("testhash");
            versionMap["Service"].IncludeInPackage.Should().BeTrue();
            versionMap["Service"].Version.RollingNumber.Should().Be(2);
            versionMap["Service"].Version.CommitHash.Should().Be("testhash");
            versionMap["Service-Code"].IncludeInPackage.Should().BeFalse();
            versionMap["Service-Code"].Version.RollingNumber.Should().Be(1);
            versionMap["Service-Code"].Version.CommitHash.Should().Be("orghash");
            versionMap["Service-Config"].IncludeInPackage.Should().BeTrue();
            versionMap["Service-Config"].Version.RollingNumber.Should().Be(2);
            versionMap["Service-Config"].Version.CommitHash.Should().Be("testhash");
            versionMap["App2"].IncludeInPackage.Should().BeFalse();
            versionMap["App2"].Version.RollingNumber.Should().Be(1);
            versionMap["App2"].Version.CommitHash.Should().Be("orghash");
            versionMap["Service2"].IncludeInPackage.Should().BeFalse();
            versionMap["Service2"].Version.RollingNumber.Should().Be(1);
            versionMap["Service2"].Version.CommitHash.Should().Be("orghash");
            versionMap["Service2-Code"].IncludeInPackage.Should().BeFalse();
            versionMap["Service2-Code"].Version.RollingNumber.Should().Be(1);
            versionMap["Service2-Code"].Version.CommitHash.Should().Be("orghash");
            versionMap["Service2-Config"].IncludeInPackage.Should().BeFalse();
            versionMap["Service2-Config"].Version.RollingNumber.Should().Be(1);
            versionMap["Service2-Config"].Version.CommitHash.Should().Be("orghash");
        }
    }
}