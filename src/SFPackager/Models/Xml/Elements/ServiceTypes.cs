using System.Collections.Generic;
using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class ServiceTypes
    {
        [XmlElement("StatelessServiceType")]
        public List<StatelessServiceType> StatelessServiceType { get; set; }

        [XmlElement("StatefulServiceType")]
        public List<StatefulServiceType> StatefulServiceType { get; set; }
    }
}