using SFPackager.Models.Xml;
using SFPackager.Services.Manifest;
using System.IO;
using System.Linq;
using Xunit;

namespace SFPackager.Tests.DescribeCsproj
{
    public class ParseCsprojFile
    {

        [Fact]
        public void Test()
        {
            var loader = new ManifestLoader<CoreProjectFile>(false);

            var basePath = new DirectoryInfo(System.AppContext.BaseDirectory);
            var combined = Path.GetFullPath(Path.Combine(basePath.FullName, @"..\..\..\..\..\src\SFPackager\SFPackager.csproj"));
            var projModel = loader.Load(combined);

            Assert.NotEmpty(projModel.PropertyGroup);
            Assert.NotNull(projModel.PropertyGroup.First().TargetFramework);
            Assert.NotNull(projModel.PropertyGroup.First().RuntimeIdentifierRaw);
            Assert.NotEmpty(projModel.PropertyGroup.First().RuntimeIdentifiers);
        }
    }
}
