using nir2.Pages;
using nir2.Visitor;
using nir2.Windows;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace nir2.Entities
{
    using static Model;

    public class Class : UMLElement
    {
        public string Name { get; set; }

        public Class()
        {
            Attributes = new List<ClassAttribute>();
            Methods = new List<ClassMethod>();
        }

        /// <summary>
        /// Ограничения
        /// </summary>
        public ConstraintType Constraint { get; set; }
        public AccessType AccessType { get; set; }

        public List<ClassAttribute> Attributes { get; set; }
        public List<ClassMethod> Methods { get; set; }

        public ListBox methodsListbox { get; set; }
        public ListBox attributesListBox { get; set; }

        public StackPanel classStackPanel { get; set; }

        private static Random random = new Random();

        /// <summary>
        /// Паттерн Посетитель
        /// </summary>
        public override void Accept(IVisitor visitor)
        {
            visitor.VisitClass(this);
        }

        /// <summary>
        /// Графический образ
        /// </summary>
        public void Draw(Canvas canvas, double x = 0, double y = 0)
        {
            Width = 250;

            double baseHeight = 40;
            double attributesHeight = Attributes?.Count * 21 ?? 0;
            double methodsHeight = Methods?.Count * 21 ?? 0;

            Height = Math.Max(baseHeight, 40 + attributesHeight + methodsHeight);

            uIElements = new List<UIElement>();

            MainCanvas = canvas;
            mainPage = (((canvas.Parent as ScrollViewer).Parent as Grid).Parent as Grid).Parent as MainPage;

            if (x == 0)
                X = random.Next(50, (int)(canvas.ActualWidth - Width));
            else X = x;
            if (y == 0)
                Y = random.Next(100, (int)(canvas.ActualHeight - Height));
            else Y = y;

            classStackPanel = new StackPanel()
            {
                Width = Width,
                Height = Height,
                Cursor = Cursors.Cross,
                Background = Brushes.White,
                ContextMenu = contextMenu  
            };

            TextBlock className = new TextBlock
            {
                Text = Name,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                FontSize = 15,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Width = classStackPanel.Width,
                Height = 40
            };

            if (Constraint == ConstraintType.Static)
            {
                className.TextDecorations = TextDecorations.Underline;
            }
            else if (Constraint == ConstraintType.Abstract)
            {
                className.FontStyle = FontStyles.Italic;
            }
            else if (Constraint == ConstraintType.Interface)
            {
                className.Text = "«interface»\n" + className.Text;
            }

            ListBox attributes = new ListBox() { AllowDrop = true };
            
            if (Attributes != null)
            {
                foreach (var attr in Attributes)
                {
                    attributes.Items.Add(attr);
                }
            }

            var converter = new ToStringConverter();
            DataTemplate attributeTemplate = new DataTemplate();
            FrameworkElementFactory attributeFactory = new FrameworkElementFactory(typeof(TextBlock));

            var binding = new Binding();
            binding.Converter = converter;
            attributeFactory.SetBinding(TextBlock.TextProperty, binding);

            Style attributeStyle = new Style(typeof(TextBlock));

            // DataTrigger для статических атрибутов
            DataTrigger staticTrigger = new DataTrigger()
            {
                Binding = new Binding("Constraint"),
                Value = ConstraintType.Static
            };
            staticTrigger.Setters.Add(new Setter(TextBlock.TextDecorationsProperty, TextDecorations.Underline));

            // DataTrigger для абстрактных атрибутов
            DataTrigger abstractTrigger = new DataTrigger()
            {
                Binding = new Binding("Constraint"),
                Value = ConstraintType.Abstract
            };
            abstractTrigger.Setters.Add(new Setter(TextBlock.FontStyleProperty, FontStyles.Italic));

            attributeStyle.Triggers.Add(staticTrigger);
            attributeStyle.Triggers.Add(abstractTrigger);

            attributeFactory.SetValue(TextBlock.StyleProperty, attributeStyle);
            attributeTemplate.VisualTree = attributeFactory;
            attributes.ItemTemplate = attributeTemplate;

            ListBox methods = new ListBox() { AllowDrop = true };
            if (Methods != null)
            {
                foreach (var met in Methods)
                {
                    methods.Items.Add(met);
                }
            }

            Style listBoxStyle = new Style(typeof(ListBox));
            listBoxStyle.Setters.Add(new Setter(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled));
            listBoxStyle.Setters.Add(new Setter(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled));
            attributes.Style = listBoxStyle;
            methods.Style = listBoxStyle;

            DataTemplate methodTemplate = new DataTemplate();
            FrameworkElementFactory methodFactory = new FrameworkElementFactory(typeof(TextBlock));
            methodFactory.SetBinding(TextBlock.TextProperty, binding);

            Style methodStyle = new Style(typeof(TextBlock));

            DataTrigger methodStaticTrigger = new DataTrigger()
            {
                Binding = new Binding("Constraint"),
                Value = ConstraintType.Static
            };
            methodStaticTrigger.Setters.Add(new Setter(TextBlock.TextDecorationsProperty, TextDecorations.Underline));

            DataTrigger methodAbstractTrigger = new DataTrigger()
            {
                Binding = new Binding("Constraint"),
                Value = ConstraintType.Abstract
            };
            methodAbstractTrigger.Setters.Add(new Setter(TextBlock.FontStyleProperty, FontStyles.Italic));

            methodStyle.Triggers.Add(methodStaticTrigger);
            methodStyle.Triggers.Add(methodAbstractTrigger);

            methodFactory.SetValue(TextBlock.StyleProperty, methodStyle);
            methodTemplate.VisualTree = methodFactory;
            methods.ItemTemplate = methodTemplate;

            Border classBorder = new Border();
            classBorder.Background = Brushes.White;
            classBorder.BorderBrush = Brushes.DarkGray;
            classBorder.BorderThickness = new Thickness(1);
            classBorder.Padding = new Thickness(1);
            classBorder.CornerRadius = new CornerRadius(3);
            classBorder.Child = classStackPanel;

            methodsListbox = methods;
            attributesListBox = attributes;

            classStackPanel.MouseLeftButtonDown += Rect_MouseLeftButtonDown;
            classStackPanel.MouseMove += Rect_MouseMove;
            classStackPanel.MouseLeftButtonUp += Rect_MouseLeftButtonUp;
            classStackPanel.MouseDown += Rect_MouseDown;

            attributes.Drop += ListBox_Drop;
            methods.Drop += ListBox_Drop;
            attributes.PreviewMouseMove += ListBox_PreviewMouseMove;
            methods.PreviewMouseMove += ListBox_PreviewMouseMove;

            classStackPanel.Children.Add(className);
            classStackPanel.Children.Add(attributes);
            classStackPanel.Children.Add(methods);

            Canvas.SetLeft(classBorder, X);
            Canvas.SetTop(classBorder, Y);
            MainCanvas.Children.Add(classBorder);

            uIElements.Add(attributes);
            uIElements.Add(methods);
            uIElements.Add(classBorder);
        }

        /// <summary>
        /// Обновляет все связи, связанные с этим классом
        /// </summary>
        public void UpdateConnections()
        {
            foreach (var element in Model.UserElements)
            {
                if (element is Connection connection)
                {
                    if (connection.Class1 == this || connection.Class2 == this)
                    {
                        connection.UpdateLinePosition();
                    }
                }
            }
        }

        // Создаем конвертер для вызова ToString()
        public class ToStringConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value?.ToString();
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        #region Клонирование класса
        protected override UMLElement Clone()
        {
            Class newClass = new Class()
            {
                Name = this.Name + "Copy",
                Attributes = new List<ClassAttribute>(),
                Methods = new List<ClassMethod>(),
                Constraint = this.Constraint
            };

            List<Class> sameName = Model.UserElements.OfType<Class>().Where(p => p.Name.StartsWith(newClass.Name)).ToList();
            if (sameName.Count > 0)
            {
                newClass.Name += sameName.Count + 1;
            }

            var atList = this.Attributes.ToList();
            var metList = this.Methods.ToList();

            foreach (var attribute in atList)
            {
                newClass.Attributes.Add(
                    new ClassAttribute { 
                        Name = attribute.Name, 
                        dataType = attribute.dataType,
                        Constraint = attribute.Constraint,
                        IsCollection = attribute.IsCollection,
                        defaultValue = attribute.defaultValue,
                        accessType = attribute.accessType,
                        Multiplicity = attribute.Multiplicity
                    });
            }

            foreach (var method in metList)
            {
                newClass.Methods.Add(
                    new ClassMethod
                    {
                        Name = method.Name,
                        accessType = method.accessType,
                        dataType = method.dataType,
                        Arguments = method.Arguments,
                        Constraint = method.Constraint,
                        IsCollection = method.IsCollection,
                    });
            }

            return newClass;
        }

        protected override void CopyElement(UMLElement uMLElement)
        {
            Model.copiedElements.Clear(); // Очищаем предыдущие скопированные элементы
            Model.copiedElements.Add(uMLElement);
        }

        protected override void PasteElement()
        {
            if (Model.copiedElements.LastOrDefault() is Class copiedElement)
            {
                Class newElement = copiedElement.Clone() as Class;
                newElement.Draw(MainCanvas);
                Model.UserElements.Add(newElement);
            }

            mainPage.ListClasses.ItemsSource = Model.UserElements.Where(p => p.GetType() == typeof(Class)).ToList();
        }
        #endregion

        #region Методы для перемещения курсором
        protected override void Rect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            dragStartPoint = e.GetPosition(MainCanvas);
            (sender as StackPanel).CaptureMouse();
        }
        protected override void Rect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            (sender as StackPanel).ReleaseMouseCapture();
        }

        protected override void Rect_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is StackPanel)
            {
                if (isDragging)
                {
                    Point currentPoint = e.GetPosition(MainCanvas);
                    double offsetX = currentPoint.X - dragStartPoint.X;
                    double offsetY = currentPoint.Y - dragStartPoint.Y;

                    double newX = X + offsetX;
                    double newY = Y + offsetY;

                    // Проверка на выход за пределы Canvas
                    if (newX < 0) newX = 0;
                    if (newY < 0) newY = 0;
                    if (newX + Width > MainCanvas.ActualWidth) newX = MainCanvas.ActualWidth - Width;
                    if (newY + Height > MainCanvas.ActualHeight) newY = MainCanvas.ActualHeight - Height;

                    offsetX = newX - X;
                    offsetY = newY - Y;

                    X = newX;
                    Y = newY;

                    foreach (var element in uIElements)
                    {
                        Canvas.SetLeft(element, Canvas.GetLeft(element) + offsetX);
                        Canvas.SetTop(element, Canvas.GetTop(element) + offsetY);
                    }

                    // ОБНОВЛЯЕМ СВЯЗИ ПРИ ПЕРЕМЕЩЕНИИ
                    UpdateConnections();

                    dragStartPoint = currentPoint;

                }
            }
        }

        /// <summary>
        /// Обновляет высоту класса на основе содержимого (при перемещении атрибутов и методов)
        /// </summary>
        public void UpdateHeight()
        {
            double baseHeight = 40;
            double attributesHeight = Attributes?.Count * 22 ?? 0;
            double methodsHeight = Methods?.Count * 21 ?? 0;

            double newHeight = Math.Max(40, baseHeight + attributesHeight + methodsHeight);

            Height = newHeight;
            classStackPanel.Height = newHeight; 
        }

        private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (sender is ListBox listBox && listBox.SelectedItem != null)
                {
                    DataObject data = new DataObject(typeof(object), listBox.SelectedItem);
                    DragDrop.DoDragDrop(listBox, data, DragDropEffects.Move);
                }
            }
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (sender is ListBox targetListBox)
            {
                object data = e.Data.GetData(typeof(object));
                if (data != null)
                {
                    ListBox sourceListBox = FindSourceListBox(data);
                    if (sourceListBox != null)
                    {
                        var sourceClass = FindClassByListBox(sourceListBox);

                        if (sourceClass != null)
                        {
                            if (data is ClassAttribute attribute)
                            {
                                sourceClass.Attributes.Remove(attribute);
                                sourceClass.UpdateHeight();
                            }
                            else if (data is ClassMethod method)
                            {
                                sourceClass.Methods.Remove(method);
                                sourceClass.UpdateHeight();
                            }

                            sourceListBox.Items.Remove(data);
                        }
                    }

                    if (!targetListBox.Items.Contains(data))
                    {
                        var targetClass = FindClassByListBox(targetListBox);

                        if (targetClass != null)
                        {
                            if (data is ClassAttribute attr)
                            {
                                targetClass.Attributes.Add(attr);
                                targetClass.UpdateHeight();
                            }
                            else if (data is ClassMethod method)
                            {
                                targetClass.Methods.Add(method);
                                targetClass.UpdateHeight();
                            }

                            targetListBox.Items.Add(data);
                        }
                    }
                }
            }
        }

        private ListBox FindSourceListBox(object data)
        {
            foreach (var element in MainCanvas.Children)
            {
                if (element is Border border)
                {
                    StackPanel stack = (StackPanel)border.Child;
                    foreach (var child in stack.Children)
                    {
                        if (child is ListBox listBox && listBox.Items.Contains(data))
                        {
                            return listBox;
                        }
                    }
                }
            }
            return null;
        }

        private Class FindClassByListBox(ListBox listBox)
        {
            foreach (var element in Model.UserElements)
            {
                if (element is Class userClass)
                {
                    if (userClass.methodsListbox == listBox || userClass.attributesListBox == listBox )
                    {
                        return userClass;
                    }
                }
            }
           
            return null;
        }
        #endregion

        /// <summary>
        /// Двойной клик
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Rect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                isDragging = false;
                string className = "";
                foreach(var element in (sender as StackPanel).Children)
                {
                    if (element is TextBlock)
                        className = (element as TextBlock).Text;
                }

                Class userClass = Model.UserElements.OfType<Class>().FirstOrDefault(p => p.Name == className);
                if (userClass == null)
                {
                    string last = className.Split()[1];
                    userClass = Model.UserElements.OfType<Class>().FirstOrDefault(p => p.Name == last);
                }
                e.Handled = true;

                // Двойной клик!
                
                try
                {
                    ClassWindow classWindow = new ClassWindow(userClass, MainCanvas);
                    classWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
