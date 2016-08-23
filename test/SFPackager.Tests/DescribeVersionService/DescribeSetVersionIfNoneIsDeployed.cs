using System.Collections.Generic;
using FluentAssertions;
using SFPackager.Models;
using SFPackager.Services;
using Xunit;

namespace SFPackager.Tests.DescribeVersionService
{
    public class DescribeSetVersionIfNoneIsDeployed
    {
        private readonly VersionService _versionService;

        public DescribeSetVersionIfNoneIsDeployed()
        {
            _versionService = new VersionService();
        }

        [Fact]
        public void ItShouldSetVersionOnAllItems()
        {
            // g
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
                    ParentRef = "Service"
                },
                ["Service-Config"] = new GlobalVersion
                {
                    VersionType = VersionType.ServicePackage,
                    Version = VersionNumber.Default(),
                    ParentRef = "Service"
                }
            };
            var newVersion = VersionNumber.Create(2, "testhash");

            // w
            _versionService.SetVersionIfNoneIsDeployed(versions, newVersion);

            // t
            foreach (var actual in versions)
            {
                actual.Value.Version.RollingNumber.Should().Be(2);
                actual.Value.Version.CommitHash.Should().Be("testhash");
                actual.Value.IncludeInPackage.Should().BeTrue();
            }
        }
    }
}