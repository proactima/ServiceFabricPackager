using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class Principals
    {
        [XmlElement("Users")]
        public Users Users { get; set; }
    }
}