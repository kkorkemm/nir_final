using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nir2.Entities
{
    public class ClassMethod
    {
        public string Name { get; set; }
        public string dataType {  get; set; } 
        public AccessType accessType { get; set; }
        public bool IsCollection { get; set; }

        /// <summary>
        /// Ограничения
        /// </summary>
        public ConstraintType Constraint { get; set; }

        /// <summary>
        /// Аргументы метода
        /// </summary>
        public List<MethodArgument> Arguments { get; set; }

        public override string ToString()
        {
            string access = "";
            if (accessType == AccessType.Public) access = "+";
            else if (accessType == AccessType.Protected) access = "#";
            else access = "-";

            string argString = "";
            if (Arguments != null && Arguments.Count >= 1)
            {
                foreach (var arg in Arguments)
                {
                    string isCollection = "";
                    if (arg.IsCollection)
                        isCollection = "[]";
                    argString += $"{arg.Name}: {arg.dataType}{isCollection}, ";
                }
                
                argString = argString.Remove(argString.Length - 2); // убрать последнюю запятую и пробел
            }

            string result = "";
            string hasCollection = "";
            if (IsCollection)
                hasCollection = "[]";

            if (dataType == "Void")
                result = $"{access} {Name}({argString})";
            else
                result = $"{access} {Name}({argString}): {dataType}{hasCollection}";

            return result;
        }
    }
}
