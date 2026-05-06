using nir2.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace nir2.Serialization
{
    /// <summary>
    /// Сохранение и загрузка диаграмм
    /// </summary>
    public class DiagramService
    {
        public static string FILE_EXTENSION = ".numl";
        public static string FILE_FILTER = "NIR UML Files (*.numl) | *.numl";

        /// <summary>
        /// Сохраняет диаграмму в NUML файл
        /// </summary>
        public static bool SaveNUML(string filePath, List<UMLElement> elements)
        {
            try
            {
                var diagramData = ConvertToDiagramData(elements);

                var serializer = new XmlSerializer(typeof(DiagramData));
                using (var writer = new StreamWriter(filePath))
                {
                    serializer.Serialize(writer, diagramData);
                }

                MessageBox.Show($"Диаграмма успешно сохранена!\nФайл: {Path.GetFileName(filePath)}",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Загружает диаграмму из NUML файла
        /// </summary>
        public static List<UMLElement> LoadNUML(string filePath, Canvas canvas)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("Файл не найден", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return new List<UMLElement>();
                }

                var serializer = new XmlSerializer(typeof(DiagramData));
                using (var reader = new StreamReader(filePath))
                {
                    var diagramData = (DiagramData)serializer.Deserialize(reader);
                    return ConvertFromDiagramData(diagramData, canvas);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке файла: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<UMLElement>();
            }
        }

        /// <summary>
        /// Преобразует элементы диаграммы в данные для сериализации
        /// </summary>
        private static DiagramData ConvertToDiagramData(List<UMLElement> elements)
        {
            var diagramData = new DiagramData();
            var classDictionary = new Dictionary<Class, string>();

            // Собираем классы
            foreach (var element in elements.OfType<Class>())
            {
                var classData = new ClassData
                {
                    Name = element.Name ?? "UnnamedClass",
                    Constraint = element.Constraint.ToString(),
                    X = element.X,
                    Y = element.Y,
                    Width = element.Width,
                    Height = element.Height,
                    Attributes = new List<AttributeData>(),
                    Methods = new List<MethodData>()
                };

                if (element.Attributes != null)
                {
                    foreach (var attr in element.Attributes)
                    {
                        classData.Attributes.Add(new AttributeData
                        {
                            Name = attr.Name ?? "",
                            DataType = attr.dataType.ToString(),
                            AccessType = attr.accessType.ToString(),
                            Multiplicity = attr.Multiplicity ?? "",
                            Constraint = attr.Constraint.ToString(),
                            IsCollection = attr.IsCollection,
                            DefaultValue = attr.defaultValue ?? ""
                        });
                    }
                }

                if (element.Methods != null)
                {
                    foreach (var method in element.Methods)
                    {
                        var methodData = new MethodData
                        {
                            Name = method.Name ?? "",
                            DataType = method.dataType.ToString(),
                            AccessType = method.accessType.ToString(),
                            Constraint = method.Constraint.ToString(),
                            Arguments = new List<ArgumentData>(),
                            IsCollection = method.IsCollection
                        };

                        if (method.Arguments != null)
                        {
                            foreach (var arg in method.Arguments)
                            {
                                methodData.Arguments.Add(new ArgumentData
                                {
                                    Name = arg.Name ?? "",
                                    DataType = arg.dataType.ToString(),
                                    IsCollection = arg.IsCollection
                                });
                            }
                        }

                        classData.Methods.Add(methodData);
                    }
                }

                diagramData.Classes.Add(classData);
                classDictionary[element] = classData.Name;
            }

            // Собираем связи
            foreach (var element in elements.OfType<Connection>())
            {
                if (element.Class1 != null && element.Class2 != null &&
                    classDictionary.ContainsKey(element.Class1) &&
                    classDictionary.ContainsKey(element.Class2))
                {

                    ConnectionData connection = new ConnectionData()
                    {
                        Name = element.Name ?? "",
                        ConnectionTypeId = element.ConnectionType?.Id ?? 1,
                        Class1Name = classDictionary[element.Class1],
                        Class2Name = classDictionary[element.Class2],
                        Role1 = element.Role1?.Name ?? "",
                        Role2 = element.Role2?.Name ?? ""
                    };

                    if (element.Role1 != null)
                    {
                        RoleData role1 = new RoleData()
                        {
                            Name = element.Role1.Name,
                            Multiplicity = element.Role1.Multiplicity,
                            AccessType = element.Role1.AccessType.ToString(),
                            Direction = element.Role1.Direction
                        };

                        connection.Roles.Add(role1);
                    }

                    if (element.Role2 != null)
                    {
                        RoleData role2 = new RoleData()
                        {
                            Name = element.Role2.Name,
                            Multiplicity = element.Role2.Multiplicity,
                            AccessType = element.Role2.AccessType.ToString(),
                            Direction = element.Role2.Direction
                        };

                        connection.Roles.Add(role2);
                    }

                    diagramData.Connections.Add(connection);
                }
            }

            return diagramData;
        }

        /// <summary>
        /// Преобразует данные из файла в элементы диаграммы
        /// </summary>
        private static List<UMLElement> ConvertFromDiagramData(DiagramData diagramData, Canvas canvas)
        {
            var elements = new List<UMLElement>();
            var classDictionary = new Dictionary<string, Class>();

            foreach (var classData in diagramData.Classes)
            {
                var classElement = new Class
                {
                    Name = classData.Name,
                    Constraint = ParseEnum<ConstraintType>(classData.Constraint, ConstraintType.Null),
                    X = classData.X,
                    Y = classData.Y,
                    Width = classData.Width,
                    Height = classData.Height,
                    Attributes = new List<ClassAttribute>(),
                    Methods = new List<ClassMethod>()
                };

                foreach (var attrData in classData.Attributes)
                {
                    classElement.Attributes.Add(new ClassAttribute
                    {
                        Name = attrData.Name,
                        dataType = attrData.DataType,
                        accessType = ParseEnum<AccessType>(attrData.AccessType, AccessType.Public),
                        Multiplicity = attrData.Multiplicity,
                        Constraint = ParseEnum<ConstraintType>(attrData.Constraint, ConstraintType.Null),
                        IsCollection = attrData.IsCollection,
                        defaultValue = attrData.DefaultValue
                    });
                }

                foreach (var methodData in classData.Methods)
                {
                    var method = new ClassMethod
                    {
                        Name = methodData.Name,
                        dataType = methodData.DataType,
                        accessType = ParseEnum<AccessType>(methodData.AccessType, AccessType.Public),
                        Constraint = ParseEnum<ConstraintType>(methodData.Constraint, ConstraintType.Null),
                        Arguments = new List<MethodArgument>(),
                        IsCollection = methodData.IsCollection
                    };

                    foreach (var argData in methodData.Arguments)
                    {
                        method.Arguments.Add(new MethodArgument
                        {
                            Name = argData.Name,
                            dataType = argData.DataType,
                            IsCollection = argData.IsCollection
                        });
                    }

                    classElement.Methods.Add(method);
                }

                elements.Add(classElement);
                classDictionary[classData.Name] = classElement;

                classElement.Draw(canvas, classData.X, classData.Y);
            }

            foreach (var connData in diagramData.Connections)
            {
                if (classDictionary.TryGetValue(connData.Class1Name, out var class1) &&
                    classDictionary.TryGetValue(connData.Class2Name, out var class2))
                {
                    var connection = new Connection
                    {
                        Name = connData.Name,
                        ConnectionType = Model.connectionTypes.FirstOrDefault(ct => ct.Id == connData.ConnectionTypeId),
                        Class1 = class1,
                        Class2 = class2
                    };

                    foreach (var roleData in connData.Roles)
                    {
                        var role = new ConnectionRole
                        {
                            Name = roleData.Name,
                            Multiplicity = roleData.Multiplicity,
                            AccessType = ParseEnum<AccessType>(roleData.AccessType, AccessType.Public),
                            Direction = roleData.Direction
                        };

                        if (role.Direction == "1")
                            connection.Role1 = role;
                        else
                            connection.Role2 = role;
                    }

                    elements.Add(connection);
                    connection.Draw(canvas);
                }
            }

            return elements;
        }

        /// <summary>
        /// Парсит enum значение с fallback
        /// </summary>
        private static T ParseEnum<T>(string value, T defaultValue) where T : struct
        {
            return Enum.TryParse<T>(value, out T result) ? result : defaultValue;
        }

        /// <summary>
        /// Добавляет расширение .numl если нужно
        /// </summary>
        public static string EnsureFileExtension(string filePath)
        {
            if (string.IsNullOrEmpty(Path.GetExtension(filePath)))
            {
                return filePath + FILE_EXTENSION;
            }
            return filePath;
        }
    }
}
