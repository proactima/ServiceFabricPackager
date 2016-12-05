using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class ExeHost
    {
        [XmlElement("Program")]
        public string Program { get; set; }

        [XmlElement("WorkingFolder")]
        public string WorkingFolder { get; set; }
    }
}