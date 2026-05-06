using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nir2.Entities;

namespace nir2.Memento
{
    public class DiagramMemento
    {
        public List<Class> Classes { get; set; }
        public List<Connection> Connections { get; set; }

        public DiagramMemento(List<UMLElement> elements)
        {
            Classes = elements.OfType<Class>().Select(p => CloneClass(p)).ToList();
            Connections = elements.OfType<Connection>().Select(p => CloneConnection(p)).ToList();
        }

        public Class CloneClass(Class original)
        {
            return new Class{
                Name = original.Name,
                Constraint = original.Constraint,
                AccessType = original.AccessType,
                X = original.X,
                Y = original.Y,
                Width = original.Width,
                Height = original.Height,
                Attributes = original.Attributes?.Select(p => CloneAttribute(p)).ToList(),
                Methods = original.Methods?.Select(p => CloneMethod(p)).ToList(),
            };
        }

        public Connection CloneConnection(Connection original)
        {
            return new Connection
            {
                Name = original.Name,
                ConnectionType = original.ConnectionType,
                Class1 = CloneClass(original.Class1),
                Class2 = CloneClass(original.Class2),
                Role1 = original.Role1,
                Role2 = original.Role2
            };
        }

        public ClassAttribute CloneAttribute(ClassAttribute original)
        {
            return new ClassAttribute 
            {
                Name = original.Name,
                Constraint = original.Constraint,
                Multiplicity = original.Multiplicity,
                dataType = original.dataType,
                defaultValue = original.defaultValue,
                accessType = original.accessType,
                IsCollection = original.IsCollection
            };
        }

        public ClassMethod CloneMethod(ClassMethod original)
        {
            return new ClassMethod 
            { 
                Name = original.Name,
                dataType = original.dataType,
                accessType = original.accessType,
                Arguments = original.Arguments?.Select(p => CloneArgument(p)).ToList(),
                Constraint = original.Constraint,
                IsCollection = original.IsCollection
            };
        }

        public MethodArgument CloneArgument(MethodArgument original)
        {
            return new MethodArgument 
            { 
                Name = original.Name,
                dataType = original.dataType,
                IsCollection = original.IsCollection
            };
        }
    }
}
