using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class StatefulServiceType
    {
        [XmlAttribute("ServiceTypeName")]
        public string ServiceTypeName { get; set; }

        [XmlElement("Extensions")]
        public Extensions Extensions { get; set; }
    }
}