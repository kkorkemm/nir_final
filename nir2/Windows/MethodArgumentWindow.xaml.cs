using nir2.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// <summary>
    /// Логика взаимодействия для MethodArgumentWindow.xaml
    /// </summary>
    public partial class MethodArgumentWindow : Window
    {
        ObservableCollection<MethodArgument> arguments;
        ClassMethod currentMethod = new ClassMethod();

        public MethodArgumentWindow(ClassMethod classMethod)
        {
            InitializeComponent();
            TextName.Text += classMethod.Name;
            arguments = new ObservableCollection<MethodArgument>();
            ComboTypes.ItemsSource = Model.userDataTypes.Where(p => p != "Void").ToList();


            if (classMethod.Arguments != null)
            {
                foreach (var arg in classMethod.Arguments)
                {
                    arguments.Add(arg);
                }
            }
            else
            {
                classMethod.Arguments = new List<MethodArgument>();
            }

            GridArguments.ItemsSource = arguments;
            currentMethod = classMethod;
            DataContext = currentMethod;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            foreach (MethodArgument argument in arguments)
            {
                if (string.IsNullOrWhiteSpace(argument.Name))
                    errors.AppendLine("Пустое название аргумента");
                if (string.IsNullOrWhiteSpace(argument.dataType))
                    errors.AppendLine("Пустой тип данных");
            }

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            currentMethod.Arguments = arguments.ToList();
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
