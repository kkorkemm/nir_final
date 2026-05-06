using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace nir2.Entities
{
    public class ClassAttribute
    {
        public string Name { get; set; }
        public string dataType { get; set; }
        public AccessType accessType { get; set; }
        public bool IsCollection { get; set; }

        public ClassAttribute() { }

        /// <summary>
        /// Кратность
        /// </summary>
        public string Multiplicity { get; set; }

        /// <summary>
        /// Ограничение
        /// </summary>
        public ConstraintType Constraint { get; set; }

        /// <summary>
        /// Значение по умолчанию
        /// </summary>
        public string defaultValue { get; set; }

        public override string ToString()
        {
            string access = "";
            if (accessType == AccessType.Public) access = "+";
            else if (accessType == AccessType.Protected) access = "#";
            else access = "-";

            string hasCollection = "";
            if (IsCollection)
                hasCollection = "[]";
            if (!string.IsNullOrWhiteSpace(Multiplicity))
                hasCollection = $"[{Multiplicity}]";

            string hasDefault = "";
            if (!string.IsNullOrWhiteSpace(defaultValue))
                hasDefault = $" = {defaultValue}";

            string result = $"{access} {Name}: {dataType}{hasCollection}{hasDefault}";

            if  (Constraint != ConstraintType.Null)
            {
                if (Constraint != ConstraintType.Static && Constraint != ConstraintType.Abstract)
                    result = result + " {" + Constraint + "}";
            }

            return result;
        }
    }
}
