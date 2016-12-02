using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using SFPackager.Helpers;
using SFPackager.Models;
using SFPackager.Services.Manifest;
using Xunit;
using Xunit.Abstractions;

namespace SFPackager.Tests.DescribeManifest
{
    public class DescribePrincipalAppender
    {
        private readonly ITestOutputHelper _output;

        public DescribePrincipalAppender(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Test()
        {
            var packageConfig = new PackageConfig
            {
                Encipherment = new List<Encipherment>
                {
                    new Encipherment()
                    {
                        ApplicationTypeName = "MyApp",
                        CertName = "MyCert",
                        CertThumbprint = "MyThumb",
                        Name = "MyCustomName"
                    }
                }
            };
            var principalAppender = new PrincipalAppender(packageConfig);

            var document = new XmlDocument();
            using (var fileStream = new FileStream(@"DescribeManifest\OriginalApplicationManifest.xml", FileMode.Open))
            using (var reader = XmlReader.Create(fileStream))
            {
                document.Load(reader);
                var manager = new XmlNamespaceManager(document.NameTable);
                manager.AddNamespace("x", "http://schemas.microsoft.com/developer/msbuild/2003");
            }
            
            principalAppender.SetPrincipals(document, "MyApp");
        }
    }
}
