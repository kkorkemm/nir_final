using nir2.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace nir2.Serialization
{
    [Serializable]
    [XmlRoot("ClassDiagram")]
    public class DiagramData
    {
        public string Version { get; set; } = "1.0";
        public DateTime Created { get; set; } = DateTime.Now;

        [XmlArray("Classes")]
        [XmlArrayItem("Class")]
        public List<ClassData> Classes { get; set; } = new List<ClassData>();

        [XmlArray("Connections")]
        [XmlArrayItem("Connection")]
        public List<ConnectionData> Connections { get; set; } = new List<ConnectionData>();
    }
}
