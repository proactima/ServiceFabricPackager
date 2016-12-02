using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class EndpointBindingPolicy
    {
        [XmlAttribute("EndpointRef")]
        public string EndpointRef { get; set; }

        [XmlAttribute("CertificateRef")]
        public string CertificateRef { get; set; }
    }
}