using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace nir2.Serialization
{
    [Serializable]
    public class ConnectionData
    {
        public string Name { get; set; }
        public int ConnectionTypeId { get; set; }
        public string Class1Name { get; set; }
        public string Class2Name { get; set; }
        public string Role1 { get; set; }
        public string Role2 { get; set; }

        [XmlArray("Roles")]
        [XmlArrayItem("Role")]
        public List<RoleData> Roles { get; set; } = new List<RoleData>();
    }
}
