using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nir2.Entities
{
    public class ConnectionRole
    {
        public string Name { get; set; }
        public string Multiplicity { get; set; }
        public AccessType AccessType { get; set; }
        public string Direction { get; set; }
    }
}
