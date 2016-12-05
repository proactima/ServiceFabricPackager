using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class EndpointCertificate
    {
        [XmlAttribute("X509FindValue")]
        public string X509FindValue { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }
    }
}