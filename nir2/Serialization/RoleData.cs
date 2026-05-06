using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nir2.Serialization
{
    [Serializable]
    public class RoleData
    {
        public string Name { get; set; }
        public string Multiplicity { get; set; }
        public string AccessType { get; set; }
        public string Direction { get; set; }
    }
}
