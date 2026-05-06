using nir2.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nir2.Visitor
{
    public class UMLValidator : IVisitor
    {
        public string Errors;

        public void VisitClass(Class userClass)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(userClass.Name)) 
                errors.AppendLine("Укажите название класса");
            else 
                if (userClass.Name.Contains(" "))
                    errors.AppendLine("Название класса содержит пробелы");

            foreach (var attribute in userClass.Attributes)
            {
                if (string.IsNullOrWhiteSpace(attribute.Name))
                    errors.AppendLine("Укажите название атрибута");
                else
                {
                    if (attribute.Name.Contains(" "))
                        errors.AppendLine($"Название атрибута {attribute.Name} содержит пробелы");
                    if (string.IsNullOrWhiteSpace(attribute.dataType))
                        errors.AppendLine($"Укажите тип данных атрибута {attribute.Name}");
                }    
            }

            foreach (var method in userClass.Methods)
            {
                if (string.IsNullOrWhiteSpace(method.Name))
                    errors.AppendLine("Укажите название операции");
                else
                {
                    if (method.Name.Contains(" "))
                        errors.AppendLine($"Название операции {method.Name} содержит пробелы");
                    if (string.IsNullOrWhiteSpace(method.dataType))
                        errors.AppendLine($"Укажите тип возвращаемого значения операции {method.Name}");
                }
            }

            Errors += errors.ToString();
        }

        public void VisitConnection(Connection connection)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(connection.Name))
            {
                errors.AppendLine("Укажите название связи");
            }

            Errors += errors.ToString();
        }
    }
}
