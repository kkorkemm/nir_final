using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nir2.Entities
{
    public class MethodArgument
    {
        public string Name { get; set; }
        public string dataType { get; set; }
        public bool IsCollection { get; set; }
    }
}
