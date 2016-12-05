using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class Parameter
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string DefaultValue { get; set; }
    }
}