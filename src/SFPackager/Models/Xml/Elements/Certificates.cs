using System.Collections.Generic;
using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class Certificates
    {
        [XmlElement("EndpointCertificate")]
        public List<EndpointCertificate> EndpointCertificates { get; set; }
    }
}