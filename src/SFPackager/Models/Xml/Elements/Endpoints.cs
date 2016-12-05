using System.Collections.Generic;
using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class Endpoints
    {
        [XmlElement("Endpoint")]
        public List<Endpoint> Endpoint { get; set; }
    }
}