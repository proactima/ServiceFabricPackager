using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using FluentAssertions;
using SFPackager.Services;
using Xunit;

namespace SFPackager.Tests.DescribeSfProjectHandler
{
    public class DesribeExtractApplicationManifest
    {
        [Fact]
        public void ItShouldExtractApplicationPackageRootFromSfProj()
        {
            string actual;
            using (var fileStream = new FileStream(@"DescribeSfProjectHandler\Example.sfproj", FileMode.Open))
            using (var reader = XmlReader.Create(fileStream))
            {
                var document = new XmlDocument();
                document.Load(reader);
                var manager = new XmlNamespaceManager(document.NameTable);
                manager.AddNamespace("x", "http://schemas.microsoft.com/developer/msbuild/2003");

                actual = SfProjectHandler.ExtractApplicationManifest(@"C:\", document, manager);
            }

            actual.Should().Be("C:\\ApplicationPackageRoot");
        }
    }
}
