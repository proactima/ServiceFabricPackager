using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class Resources
    {
        [XmlElement("Endpoints")]
        public Endpoints Endpoints { get; set; }
    }
}