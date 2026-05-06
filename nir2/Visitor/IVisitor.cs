using nir2.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nir2.Visitor
{
    public interface IVisitor
    {
        void VisitClass(Class userClass);
        void VisitConnection(Connection connection);
    }
}
