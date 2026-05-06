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
    using System.Collections.ObjectModel;

    /// <summary>
    /// Логика взаимодействия для ClassWindow.xaml
    /// </summary>
    public partial class ClassWindow : Window
    {
        private Class userClass = new Class();
        private bool isNew = true;
        public Canvas MainCanvas;

        private ObservableCollection<ClassAttribute> attributesCollection;
        private ObservableCollection<ClassMethod> methodsCollection;

        public ClassWindow(Class newClass, Canvas canvas)
        {
            InitializeComponent();

            attributesCollection = new ObservableCollection<ClassAttribute>();
            methodsCollection = new ObservableCollection<ClassMethod>();

            if (newClass != null)
            {
                userClass = newClass;

                if (userClass.Attributes != null)
                {
                    foreach (var attr in userClass.Attributes)
                    {
                        attributesCollection.Add(attr);
                    }
                }
                if (userClass.Methods != null)
                {
                    foreach (var method in userClass.Methods)
                    {
                        methodsCollection.Add(method);
                    }
                }

                if (userClass.Constraint == ConstraintType.Static)
                {
                    radioStatic.IsChecked = true;
                }
                else if (userClass.Constraint == ConstraintType.Abstract)
                {
                    radioAbstract.IsChecked = true;
                }
                else if (userClass.Constraint == ConstraintType.Interface)
                {
                    radioInterface.IsChecked = true;
                }

                isNew = false;
                BtnDelete.IsEnabled = true;

                Title = "Редактирование класса";
            }
            else
            {
                userClass.Attributes = new List<ClassAttribute>();
                userClass.Methods = new List<ClassMethod>();
            }

            DataContext = userClass;

            GridAttributes.ItemsSource = attributesCollection;
            GridMethods.ItemsSource = methodsCollection;

            MainCanvas = canvas;

            comboAttrTypes.ItemsSource = Model.userDataTypes.Where(p => p != "Void").ToList();
            comboMethodTypes.ItemsSource = Model.userDataTypes.ToList();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (radioStatic.IsChecked == true)
            {
                userClass.Constraint = ConstraintType.Static;
            }
            else if (radioAbstract.IsChecked == true)
            {
                userClass.Constraint = ConstraintType.Abstract;
            }
            else if (radioInterface.IsChecked == true) 
            {
                userClass.Constraint = ConstraintType.Interface;
            }

            userClass.Attributes = attributesCollection.ToList();
            userClass.Methods = methodsCollection.ToList();

            if (isNew)
            {
                Class sameName = Model.UserElements.OfType<Class>().FirstOrDefault(p => p.Name == userClass.Name);
                if (sameName != null)
                {
                    MessageBox.Show("Уже существует класс с таким именем", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Model.UserElements.Add(userClass);
                userClass.Draw(MainCanvas);
                Model.userDataTypes.Add(userClass.Name);
            }
            else
            {
                try
                {
                    List<Class> sameName = Model.UserElements.OfType<Class>().Where(p => p.Name == userClass.Name).ToList();
                    if (sameName.Count > 1)
                    {
                        MessageBox.Show("Уже существует класс с таким именем", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    
                    // перерисовка класса после редактирования (до этого название не менялось в прямоугольнике)
                    Class oldClass = Model.UserElements.OfType<Class>().FirstOrDefault(p => p.Name == userClass.Name);
                    if (oldClass != null)
                    {
                        oldClass.Name = userClass.Name;
                        oldClass.Constraint = userClass.Constraint;
                        oldClass.Attributes = userClass.Attributes;
                        oldClass.Methods = userClass.Methods;

                        var elementsToRemove = oldClass.uIElements?.ToList();
                        if (elementsToRemove != null)
                        {
                            foreach (var element in elementsToRemove)
                            {
                                MainCanvas.Children.Remove(element);
                            }
                        }
                        oldClass.Draw(MainCanvas, oldClass.X, oldClass.Y);
                    }
                }
                catch
                {
                    Model.UserElements.Add(userClass);
                    
                    userClass.Draw(MainCanvas);
                }
                
            }

            Close();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить класс? Удаление класса также приводит к удалению всех его связей", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var connectionsToRemove = Model.UserElements?
                        .OfType<Connection>()
                        .Where(c => c?.Class1 == userClass || c?.Class2 == userClass)
                        .ToList() ?? new List<Connection>();

                    int count = Model.removedElements.Count + 1;

                    foreach (var connection in connectionsToRemove)
                    {
                        RemoveVisualElements(connection);
                        Model.UserElements?.Remove(connection);
                        connection.RemoveId = count;
                        Model.removedElements.Add(connection);
                    }

                    userClass.RemoveId = count;
                    Model.removedElements.Add(userClass);
                    Model.userDataTypes.Remove(userClass.Name);
                    RemoveVisualElements(userClass);
                    Model.UserElements?.Remove(userClass);

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении класса: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            Close();
        }

        private void RemoveVisualElements(UMLElement element)
        {
            if (element?.uIElements == null) return;

            // Создаем копию коллекции, т.к. мы будем изменять оригинальную коллекцию
            var uiElementsToRemove = element.uIElements.ToList();

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

        private void radioInterface_Checked(object sender, RoutedEventArgs e)
        {
            if (radioInterface.IsChecked == true || radioAbstract.IsChecked == true || radioStatic.IsChecked == true)
                BtnClear.Visibility = Visibility.Visible;
            else
                BtnClear.Visibility = Visibility.Hidden;
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            radioInterface.IsChecked = false;
            radioAbstract.IsChecked = false;
            radioStatic.IsChecked = false;
            userClass.Constraint = ConstraintType.Null;
        }

        private void BtnAddArgument_Click(object sender, RoutedEventArgs e)
        {
            ClassMethod classMethod = (sender as Button).DataContext as ClassMethod;
            if (classMethod != null)
            {
                MethodArgumentWindow argumentWindow = new MethodArgumentWindow(classMethod);
                argumentWindow.ShowDialog();
            }
        }
    }
}
