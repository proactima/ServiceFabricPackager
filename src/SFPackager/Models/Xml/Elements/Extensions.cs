using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class Extensions
    {
        [XmlElement("Extension")]
        public Extension Extension { get; set; }
    }
}