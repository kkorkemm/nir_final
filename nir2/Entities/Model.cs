using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nir2.Entities
{
    public class Model
    {
        public string Name { get; set; }

        public static List<UMLElement> UserElements = new List<UMLElement>();
        public static List<UMLElement> copiedElements = new List<UMLElement>();
        public static List<UMLElement> removedElements = new List<UMLElement>();

        public static List<ConnectionType> connectionTypes = new List<ConnectionType>()
        {
            new ConnectionType(1, "Ассоциация"),
            new ConnectionType(6, "Двунаправленная ассоциация"),
            new ConnectionType(2, "Зависимость"),
            new ConnectionType(3, "Агрегация"),
            new ConnectionType(4, "Наследование"),
            new ConnectionType(5, "Композиция"),
            new ConnectionType(7, "Реализация")
        };

        public static List<string> userDataTypes = new List<string>();
    }

    /// <summary>
    /// Типы данных
    /// </summary>
    public enum DataType
    {
        String,
        Integer,
        Boolean,
        Real,
        Void
    }

    /// <summary>
    /// Модификаторы доступа
    /// </summary>
    public enum AccessType
    {
        Public,
        Protected,
        Private
    }

    /// <summary>
    /// Ограничения
    /// </summary>
    public enum ConstraintType
    {
        Null,
        Abstract,
        Static,
        Interface,
        readOnly,
        Virtual,
        required,
        notNull,

        Override,
        Const,
        unique,
        ordered,
        sequence
    }
}
