using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class StatelessServiceType
    {
        [XmlAttribute("ServiceTypeName")]
        public string ServiceTypeName { get; set; }

        [XmlAttribute("HasPersistedState")]
        public string HasPersistedState { get; set; }
    }
}