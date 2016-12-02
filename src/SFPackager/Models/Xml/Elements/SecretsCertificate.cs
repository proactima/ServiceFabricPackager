using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class SecretsCertificate
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("X509FindType")]
        public string X509FindType { get; set; }

        [XmlAttribute("X509FindValue")]
        public string X509FindValue { get; set; }
    }
}