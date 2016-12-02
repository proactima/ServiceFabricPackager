using System.Collections.Generic;
using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class Users
    {
        [XmlElement("User")]
        public List<User> User { get; set; }
    }
}