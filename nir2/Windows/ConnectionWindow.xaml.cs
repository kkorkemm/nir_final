using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace nir2.Windows
{
    using Entities;

    /// <summary>
    /// Логика взаимодействия для ConnectionWindow.xaml
    /// </summary>
    public partial class ConnectionWindow : Window
    {
        Connection newConnection = new Connection();
        private bool isNew = true;

        Canvas MainCanvas;

        public ConnectionWindow(Connection connection, Canvas canvas)
        {
            InitializeComponent();

            if (connection != null) {
                newConnection = connection;
                isNew = false;

                BtnDelete.IsEnabled = true;
                Title = "Редактирование связи";

                if (newConnection.Role1 == null)
                {
                    newConnection.Role1 = new ConnectionRole();
                }

                if (newConnection.Role2 == null)
                {
                    newConnection.Role2 = new ConnectionRole();
                }
            }
            else
            {
                newConnection.Role1 = new ConnectionRole();
                newConnection.Role2 = new ConnectionRole();
            }

            List<UMLElement> classes = Model.UserElements.Where(p => p.GetType() == typeof(Class)).ToList();
            ComboClass1.ItemsSource = classes;
            ComboClass2.ItemsSource = classes;
            ComboLinkTypes.ItemsSource = Model.connectionTypes;

            MainCanvas = canvas;
            DataContext = newConnection;
        }

        private void BtnInfo1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Класс с указателем/ целый класс / зависимый класс / класс-предок / интерфейс", "Класс 1", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnInfo2_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Класс на которого указывают / часть целого / используемый класс / класс-потомок", "Класс 2", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ComboLinkTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var connectionType = ComboLinkTypes.SelectedItem as ConnectionType;
            var class1 = ComboClass1.SelectedItem as Class;
            var class2 = ComboClass2.SelectedItem as Class;

            if (class1 == class2 || connectionType.Id == 1)
            {
                TextRoleClass1.IsEnabled = false;
                TextMultClass1.IsEnabled = false;
                ComboAccessClass1.IsEnabled = false;
            }
            else
            {
                TextRoleClass1.IsEnabled = true;
                TextMultClass1.IsEnabled = true;
                ComboAccessClass1.IsEnabled = true;
            }

            if (isNew)
            {
                if (connectionType != null && class1 != null && class2 != null)
                {
                    string name = $"{class1.Name} - {class2.Name}:  {connectionType.Name}";
                    TextLinkName.Text = name;
                    newConnection.Name = name;
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var connectionType = ComboLinkTypes.SelectedItem as ConnectionType;
            var class1 = ComboClass1.SelectedItem as Class;
            var class2 = ComboClass2.SelectedItem as Class;

            StringBuilder errors = new StringBuilder();

            if (class1 == null)
                errors.AppendLine("Выберите первый класс");
            if (class2 == null)
                errors.AppendLine("Выберите второй класс");
            if (connectionType == null)
                errors.AppendLine("Выберите тип связи");
            if (class1 == class2)
                if (connectionType.Id != 1)
                    errors.AppendLine("Нельзя создать петлю с этим типом связи");
            if (connectionType.Id == 7)
                if (class1.Constraint != ConstraintType.Interface)
                    errors.AppendLine("Реализовать можно только интерфейсы");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                return; 
            }

            newConnection.Role1.Direction = "1";
            newConnection.Role2.Direction = "2";

            if (isNew)
            {
                Model.UserElements.Add(newConnection);
                newConnection.Draw(MainCanvas);
            }

            Close();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить связь между классами?", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                RemoveVisualElements(newConnection);
                Model.UserElements.Remove(newConnection);
                newConnection.RemoveId = Model.removedElements.Count + 1;
                Model.removedElements.Add(newConnection);
            }

            Close();
        }

        private void RemoveVisualElements(UMLElement element)
        {
            if (element?.uIElements == null) return;

            // Создаем копию коллекции, т.к. мы будем изменять оригинальную коллекцию
            var uiElementsToRemove = element.uIElements.ToList();

            int count = Model.removedElements.Count + 1;
            foreach (var uiElement in uiElementsToRemove)
            {
                if (uiElement != null && MainCanvas.Children.Contains(uiElement))
                {
                    MainCanvas.Children.Remove(uiElement);
                }
            }

            element.uIElements?.Clear();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
