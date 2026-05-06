using nir2.Entities;
using nir2.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace nir2.Visitor
{
    public class PlantUMLGenerator : IVisitor
    {
        public string Output;

        public void VisitClass(Class userClass)
        {
            MainPage.InterfaceRealization(userClass);

            if (userClass.Constraint == ConstraintType.Interface)
                Output += $"interface {userClass.Name}\n";
            else if (userClass.Constraint == ConstraintType.Abstract)
                Output += $"abstract class {userClass.Name}\n";
            else if (userClass.Constraint == ConstraintType.Static)
                Output += $"static class {userClass.Name}\n";
            else
                Output += $"class {userClass.Name}\n";

            if (userClass.Attributes.Count > 0 || userClass.Methods.Count > 0)
            {
                Output += "{ \n";
                foreach (ClassAttribute attribute in userClass.Attributes)
                {
                    if (attribute.accessType == AccessType.Public)
                        Output += "+";
                    else if (attribute.accessType == AccessType.Protected)
                        Output += "#";
                    else
                        Output += "-";

                    Output += $"{attribute.dataType} {attribute.Name}\n";
                }

                foreach (ClassMethod method in userClass.Methods)
                {
                    if (method.accessType == AccessType.Public)
                        Output += "+";
                    else if (method.accessType == AccessType.Protected)
                        Output += "#";
                    else
                        Output += "-";

                    Output += $"{method.dataType} {method.Name}(";

                    if (method.Arguments != null && method.Arguments.Count >= 1)
                    {
                        foreach (var arg in method.Arguments)
                        {
                            string type = "";
                            switch (arg.dataType)
                            {
                                case "String": type = "string"; break;
                                case "Integer": type = "int"; break;
                                case "Boolean": type = "bool"; break;
                                case "Real": type = "double"; break;
                                case "UnlimitedNatural": type = "uint"; break;
                                default: type = arg.dataType; break;
                            }

                            Output += $"{type} {arg.Name}, ";
                        }

                        Output = Output.Remove(Output.Length - 2); // убрать последнюю запятую и пробел
                    }

                    Output += ")\n";
                }

                Output += "}\n";
            }
        }

        public void VisitConnection(Connection connection)
        {
            string type = "";
            switch (connection.ConnectionType.Id)
            {
                case 1: type = "-->"; break;
                case 2: type = "..>"; break;
                case 3: type = "o--"; break;
                case 4: type = "<|--"; break;
                case 5: type = "*--"; break;
                case 6: type = "--"; break;
                case 7: type = "<|.."; break;
                default: type = "--"; break;
            }

            string role1 = connection.Role1.Name;
            string role2 = connection.Role2.Name;
            string mult1 = connection.Role1.Multiplicity;
            string mult2 = connection.Role2.Multiplicity;

            string class1 = string.IsNullOrWhiteSpace(role1 + mult1) ? "" : $"\"{role1} {mult1}\"";
            string class2 = string.IsNullOrWhiteSpace(role2 + mult2) ? "" : $"\"{role2} {mult2}\"";

            Output += $"{connection.Class1.Name} {class1} {type} {class2} {connection.Class2.Name}\n";
        }
    }
}
