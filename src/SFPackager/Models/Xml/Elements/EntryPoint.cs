using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class EntryPoint
    {
        [XmlElement("ExeHost")]
        public ExeHost ExeHost { get; set; }
    }
}