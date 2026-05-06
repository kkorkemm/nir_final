using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nir2.Entities
{
    public class ConnectionType
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ConnectionType(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
