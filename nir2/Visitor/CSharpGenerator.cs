using nir2.Entities;
using nir2.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nir2.Visitor
{
    public class CSharpGenerator : IVisitor
    {
        public string Output;

        public void VisitClass(Class userClass)
        {
            /// Ограничения
            if (userClass.Constraint == ConstraintType.Static)
            {
                Output += "public static class ";
            }
            else if (userClass.Constraint == ConstraintType.Abstract)
            {
                Output += "public abstract class ";
            }
            else if (userClass.Constraint == ConstraintType.Interface)
            {
                Output += "interface ";
            }
            else
            {
                Output += "public class ";
            }

            List<UMLElement> elements = Model.UserElements;

            MainPage.InterfaceRealization(userClass);

            /// Наследование
            var nasled = elements.Where(p => p is Connection && ((p as Connection).ConnectionType.Id == 4 || (p as Connection).ConnectionType.Id == 7)).ToList();
            var hasNasled = nasled.Where(p => p is Connection && (p as Connection).Class2 == userClass).ToList();
            if (hasNasled != null)
            {
                Output += $"{userClass.Name} :";

                // несколько наследников
                foreach (Connection con in hasNasled)
                {
                    Output += $" {con.Class1.Name},";
                }

                Output = Output.Remove(Output.Length - 1); // убрать последнюю запятую
                Output += " \n{\n";
            }
            else
            {
                Output += userClass.Name + "\n";
                Output += "{\n";
            }

            /// Connections
            foreach (UMLElement element in elements) 
            { 
                if (element is Connection) {
                    Connection connection = element as Connection;
                    string accessName = "public";
                    string roleName = connection.Class2.Name.ToLower();
                    string roleNameAgreg = connection.Class2.Name.ToLower() + "s";
                    if (connection.Role2 != null)
                    {
                        if (!string.IsNullOrWhiteSpace(connection.Role2.Name))
                        {
                            roleName = connection.Role2.Name;
                            roleNameAgreg = connection.Role2.Name;
                        }

                        accessName = connection.Role2.AccessType.ToString().ToLower();
                    }
                    
                    if (connection.Class1 == userClass)
                    {
                        switch (connection.ConnectionType.Id)
                        {
                            case 1:
                            case 6:
                                Output += $"\t{accessName} {connection.Class2.Name} {roleName};\n"; break;
                            case 2:
                                Output += "\t// Зависимость\n";
                                Output += $"\t{accessName} void ConnectionMethod()\n";
                                Output += "\t{\n";
                                Output += $"\t\t{connection.Class2.Name} {roleName} = new {connection.Class2.Name}();\n";
                                Output += "\t}\n";
                                break;
                            case 3:
                                Output += $"\tprivate {connection.Class2.Name}[] {roleNameAgreg};\n";
                                break;
                            case 5: Output += $"\t{accessName} {connection.Class2.Name}[] {roleNameAgreg};\n"; break;
                        }
                    }
                    else if (connection.Class2 == userClass)
                    {
                        if (connection.ConnectionType.Id == 6)
                        {
                            roleName = connection.Class1.Name.ToLower();
                            accessName = connection.Role1.AccessType.ToString().ToLower();
                            if (!string.IsNullOrWhiteSpace(connection.Role1.Name))
                            {
                                roleName = connection.Role1.Name;
                            }

                            Output += $"\t{accessName} {connection.Class1.Name} {roleName};\n"; break;
                        }
                    }
                }
            }

            foreach (var attribute in userClass.Attributes) {
                string dataType = "";
                switch (attribute.dataType)
                {
                    case "String":  dataType = "string"; break;
                    case "Integer": dataType = "int"; break;
                    case "Boolean": dataType = "bool"; break;
                    case "Real": dataType = "double"; break;
                    case "UnlimitedNatural": dataType = "uint"; break;
                    default: dataType = attribute.dataType; break;
                }

                string isCollection = "";
                if (attribute.IsCollection)
                    isCollection = "[]";

                string constraint = "";
                if (attribute.Constraint == ConstraintType.Static || attribute.Constraint == ConstraintType.Abstract || attribute.Constraint == ConstraintType.readOnly || attribute.Constraint == ConstraintType.Virtual)
                    constraint = attribute.Constraint.ToString().ToLower();

                string defaultValue = "";
                if (!string.IsNullOrEmpty(attribute.defaultValue))
                    defaultValue = $" = {attribute.defaultValue}";

                if (string.IsNullOrWhiteSpace(constraint))
                    Output += $"\t{attribute.accessType.ToString().ToLower()} {dataType}{isCollection} {attribute.Name}{defaultValue};\n";
                else
                    Output += $"\t{attribute.accessType.ToString().ToLower()} {constraint} {dataType}{isCollection} {attribute.Name}{defaultValue};\n";
            }

            foreach (var method in userClass.Methods)
            {
                Output += $"\t{method.accessType.ToString().ToLower()} ";

                string dataType = "";
                switch (method.dataType)
                {
                    case "String": dataType = "string"; break;
                    case "Integer": dataType = "int"; break;
                    case "Boolean": dataType = "bool"; break;
                    case "Real": dataType = "double"; break;
                    case "UnlimitedNatural": dataType = "uint"; break;
                    case "Void": dataType = "void"; break;
                    default: dataType = method.dataType; break;
                }

                string isCollection = "";
                if (method.IsCollection)
                    isCollection = "[]";

                string constraint = "";
                if (method.Constraint == ConstraintType.Static || method.Constraint == ConstraintType.Abstract || method.Constraint == ConstraintType.Virtual)
                    constraint = method.Constraint.ToString().ToLower();

                if (string.IsNullOrWhiteSpace(constraint))
                    Output += $"{dataType}{isCollection} {method.Name}(";
                else
                    Output += $"{constraint} {dataType}{isCollection} {method.Name}(";

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

                        string hasCollection = "";
                        if (arg.IsCollection)
                            hasCollection = "[] ";

                        Output += $"{type}{hasCollection} {arg.Name}, ";
                    }

                    Output = Output.Remove(Output.Length - 2); // убрать последнюю запятую и пробел
                }

                Output += ")";

                if (userClass.Constraint == ConstraintType.Interface)
                    Output += ";\n";
                else
                    Output += " {}\n";
            }

            Output += "}\n\n";
        }

        public void VisitConnection(Connection connection)
        {
            
        }
    }
}
        
