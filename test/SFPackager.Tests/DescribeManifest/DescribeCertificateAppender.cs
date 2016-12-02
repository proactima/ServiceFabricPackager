using System.Collections.Generic;
using System.IO;
using System.Xml;
using SFPackager.Models;
using SFPackager.Services.Manifest;
using Xunit;

namespace SFPackager.Tests.DescribeManifest
{
    public class DescribeCertificateAppender
    {
        [Fact]
        public void Balls()
        {
            var packageConfig = new PackageConfig
            {
                
            };

            var certificateAppender = new CertificateAppender(packageConfig);

            var document = new XmlDocument();
            using (var fileStream = new FileStream(@"DescribeManifest\OriginalApplicationManifest.xml", FileMode.Open))
            using (var reader = XmlReader.Create(fileStream))
            {
                document.Load(reader);
                var manager = new XmlNamespaceManager(document.NameTable);
                manager.AddNamespace("x", "http://schemas.microsoft.com/developer/msbuild/2003");
            }

            certificateAppender.SetCertificates(document, "", new List<string>());
        }
    }
}