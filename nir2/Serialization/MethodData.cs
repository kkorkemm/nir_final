using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace nir2.Serialization
{
    [Serializable]
    public class MethodData
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public string AccessType { get; set; }
        public string Constraint { get; set; }
        public bool IsCollection { get; set; }

        [XmlArray("Arguments")]
        [XmlArrayItem("Argument")]
        public List<ArgumentData> Arguments { get; set; } = new List<ArgumentData>();
    }
}
