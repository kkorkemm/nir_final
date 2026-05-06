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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace nir2.Pages
{
    using Entities;
    using Microsoft.Win32;
    using nir2.Parser;
    using nir2.Visitor;
    using System.IO;
    using System.Reflection.Emit;
    using System.Xml.Linq;
    using Windows;
    using Serialization;
    using nir2.Memento;

    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private string currentFilePath = null;

        //private DiagramOriginator originator;
        //private DiagramHistory history;

        public MainPage()
        {
            InitializeComponent();
            Loaded += (s, e) => { this.Focus(); }; // Даем фокус странице

            //originator = new DiagramOriginator(MainCanvas);
            //history = new DiagramHistory(originator);

            //// Начальное состояние
            //history.SaveState();

            UpdateWindowTitle();
        }

        #region База
        private void UpdateLists()
        {
            List<Class> classes = Model.UserElements.OfType<Class>().ToList();
            List<Connection> connections = Model.UserElements.OfType<Connection>().ToList();

            ListClasses.ItemsSource = classes;
            ListConnections.ItemsSource = connections;
        }

        /// <summary>
        /// Для схлопываний и наложений связей
        /// </summary>
        private void UpdateLists(List<UMLElement> elements)
        {
            List<Class> classes = elements.OfType<Class>().ToList();
            List<Connection> connections = elements.OfType<Connection>().ToList();

            List<Connection> assosiations = connections.Where(p => p.ConnectionType.Id == 1 && p.Class1.Name != p.Class2.Name).ToList();

            HashSet<Connection> processedConnections = new HashSet<Connection>();

            foreach (Connection con in assosiations)
            {
                // Пропускаем, если связь уже обработана
                if (processedConnections.Contains(con))
                    continue;

                Connection sameCon = connections.FirstOrDefault(p =>
                    p.Class1.Name == con.Class2.Name &&
                    p.Class2.Name == con.Class1.Name &&
                    p.ConnectionType.Id == 1);

                if (sameCon != null && sameCon != con)
                {
                    con.ConnectionType = Model.connectionTypes.FirstOrDefault(p => p.Id == 6);
                    if (sameCon.Role2 != null)
                        con.Role1 = sameCon.Role2;
                    con.Name = $"{con.Class1.Name} - {con.Class2.Name}: Двунаправленная ассоциация";

                    connections.Remove(sameCon);

                    if (sameCon.uIElements != null)
                    {
                        foreach (var ui in sameCon.uIElements)
                            MainCanvas.Children.Remove(ui);

                        sameCon.uIElements.Clear();
                    }

                    MainCanvas.Children.Remove(sameCon);

                    processedConnections.Add(con);
                    processedConnections.Add(sameCon);
                }
            }

            Model.UserElements.Clear();
            Model.UserElements.AddRange(connections);
            Model.UserElements.AddRange(classes);

            ListClasses.ItemsSource = classes;
            ListConnections.ItemsSource = connections;

            processedConnections.Clear();
        }

        private void BtnAddClass_Click(object sender, RoutedEventArgs e)
        {
            SaveBeforeAction();

            ClassWindow classWindow = new ClassWindow(null, MainCanvas);
            classWindow.ShowDialog();

            UpdateLists();
        }

        private void BtnAddConnection_Click(object sender, RoutedEventArgs e)
        {
            SaveBeforeAction();

            List<Class> classes = Model.UserElements.OfType<Class>().ToList();

            if (classes.Count < 2)
            {
                MessageBox.Show("В диаграмме должно быть минимум два класса", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ConnectionWindow connectionWindow = new ConnectionWindow(null, MainCanvas);
            connectionWindow.ShowDialog();

            UpdateLists(Model.UserElements);
        }

        private void ListClasses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListClasses.SelectedItem != null)
            {
                SaveBeforeAction();
                Class selectedClass = ListClasses.SelectedItem as Class;
                ClassWindow classWindow = new ClassWindow(selectedClass, MainCanvas);
                classWindow.ShowDialog();
                
                UpdateLists();

                ListClasses.SelectedItem = null;
            }
        }

        private void ListConnections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListConnections.SelectedItem != null)
            {
                SaveBeforeAction();
                Connection connection = ListConnections.SelectedItem as Connection;
                ConnectionWindow connectionWindow = new ConnectionWindow(connection, MainCanvas);
                connectionWindow.ShowDialog();
                UpdateLists(Model.UserElements);

                ListConnections.SelectedItem = null;
            }
        }
        #endregion

        public void SaveBeforeAction()
        {
         //   history.SaveState();
        }

        /// <summary>
        /// Валидация UML (проверка на пустые поля в названиях)
        /// </summary>
        private bool CheckBefore()
        {
            UMLValidator validator = new UMLValidator();
            foreach (var element in Model.UserElements)
            {
                element.Accept(validator);
            }

            if (!string.IsNullOrWhiteSpace(validator.Errors))
            {
                MessageBox.Show(validator.Errors, "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Сохранение диаграммы
        /// </summary>
        private bool SaveToFile(string filePath)
        {
            try
            {
                bool success = DiagramService.SaveNUML(filePath, Model.UserElements);

                if (success)
                {
                    currentFilePath = filePath;
                    UpdateWindowTitle();
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }
        }

        /// <summary>
        /// Заголовок окна
        /// </summary>
        private void UpdateWindowTitle()
        {
            string appName = "Редактор диаграмм классов UML";

            var mainWindow = Application.Current.MainWindow;
            if (mainWindow != null)
            {
                if (!string.IsNullOrEmpty(currentFilePath))
                {
                    string fileName = Path.GetFileName(currentFilePath);
                    mainWindow.Title = $"{fileName} - {appName}";
                }
                else
                {
                    mainWindow.Title = $"Новая диаграмма - {appName}";
                }
            }
        }

        /// <summary>
        /// Проверка на несохраненные изменения
        /// </summary>
        /// <returns> true - пользователю пофиг на несохраненные </returns>
        private bool CheckUnsavedChanges()
        {
            /// !!!! ПОКА ЧТО ПРОСТО ТУПАЯ ПРОВЕРКА НА ТО, ЕСТЬ ЛИ ЭЛЕМЕНТЫ - ПОДУМАТЬ
            if (Model.UserElements.Any())
            {
                var result = MessageBox.Show("Есть несохраннные изменения. Продолжить?", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question);

                return result == MessageBoxResult.Yes;
            }
            return true;
        }

        /// <summary>
        /// Создать новую диаграмму
        /// </summary>
        private void CreateNewDiagram()
        {
            if (!string.IsNullOrEmpty(currentFilePath)) {
                bool result = CheckUnsavedChanges();

                if (result)
                {
                    currentFilePath = null;
                    ClearAll();
                }
            }
        }

        private void ClearAll()
        {
            MainCanvas.Children.Clear();
            ListConnections.ItemsSource = null;
            ListConnections.Items.Clear();
            ListClasses.ItemsSource = null;
            ListClasses.Items.Clear();
            Model.UserElements.Clear();
            Model.copiedElements.Clear();
            Model.userDataTypes.Clear();
            UpdateWindowTitle();

            Model.userDataTypes.Add(DataType.String.ToString());
            Model.userDataTypes.Add(DataType.Integer.ToString());
            Model.userDataTypes.Add(DataType.Boolean.ToString());
            Model.userDataTypes.Add(DataType.Real.ToString());
            Model.userDataTypes.Add(DataType.Void.ToString());
        }

        private void MenuNew_Click(object sender, RoutedEventArgs e)
        {
            CreateNewDiagram();
        }

        /// <summary>
        /// Сохранение диаграммы
        /// </summary>
        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {
            bool result = CheckBefore();
            if (!result)
                return;
            
            try
            {
                if (!string.IsNullOrEmpty(currentFilePath) && currentFilePath.EndsWith(".numl"))
                {
                    SaveToFile(currentFilePath);
                }
                else
                {
                    var saveDialog = new SaveFileDialog
                    {
                        Filter = DiagramService.FILE_FILTER,
                        DefaultExt = DiagramService.FILE_EXTENSION,
                        Title = "Сохранить диаграмму"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        string filePath = DiagramService.EnsureFileExtension(saveDialog.FileName);
                        DiagramService.SaveNUML(filePath, Model.UserElements);

                        currentFilePath = filePath;
                        UpdateWindowTitle();
                    }
                }     
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckUnsavedChanges())
            {
                return;
            }

            try
            {
                var openDialog = new OpenFileDialog
                {
                    Filter = DiagramService.FILE_FILTER,
                    Title = "Открыть диаграмму"
                };

                if (openDialog.ShowDialog() == true)
                {
                    CreateNewDiagram();
                    var elements = DiagramService.LoadNUML(openDialog.FileName, MainCanvas);
                    Model.UserElements.AddRange(elements);

                    currentFilePath = openDialog.FileName;

                    UpdateLists(Model.UserElements);
                    UpdateWindowTitle();

                    foreach (var el in Model.UserElements)
                    {
                        if (el is Class)
                            Model.userDataTypes.Add((el as Class).Name);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            if (CheckUnsavedChanges())
            {
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Горячие клавиши
        /// </summary>
        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.S:
                        MenuSave_Click(null, null);
                        e.Handled = true;
                        break;
                    case Key.O:
                        MenuOpen_Click(null, null);
                        e.Handled = true;
                        break;
                    case Key.N:
                        MenuNew_Click(null, null); 
                        e.Handled = true;
                        break;
                    case Key.Z:
                        MenuZ_Click(null, null);
                        e.Handled = true;
                        break;
                    case Key.Add:
                    case Key.OemPlus:
                        zoomFactor = Math.Min(ZOOM_MAX, zoomFactor + ZOOM_STEP);
                        ApplyZoom(new Point(MainCanvas.ActualWidth / 2, MainCanvas.ActualHeight / 2));
                        e.Handled = true;
                        break;

                    case Key.Subtract:
                    case Key.OemMinus:
                        zoomFactor = Math.Max(ZOOM_MIN, zoomFactor - ZOOM_STEP);
                        ApplyZoom(new Point(MainCanvas.ActualWidth / 2, MainCanvas.ActualHeight / 2));
                        e.Handled = true;
                        break;

                    case Key.D0:
                    case Key.NumPad0:
                        zoomFactor = 1.0;
                        ApplyZoom(new Point(MainCanvas.ActualWidth / 2, MainCanvas.ActualHeight / 2));
                        e.Handled = true;
                        break;

                }
            }

            // Прокрутка стрелками
            switch (e.Key)
            {
                case Key.Left:
                    CanvasScrollViewer.ScrollToHorizontalOffset(CanvasScrollViewer.HorizontalOffset - 20);
                    e.Handled = true;
                    break;
                case Key.Right:
                    CanvasScrollViewer.ScrollToHorizontalOffset(CanvasScrollViewer.HorizontalOffset + 20);
                    e.Handled = true;
                    break;
                case Key.Up:
                    CanvasScrollViewer.ScrollToVerticalOffset(CanvasScrollViewer.VerticalOffset - 20);
                    e.Handled = true;
                    break;
                case Key.Down:
                    CanvasScrollViewer.ScrollToVerticalOffset(CanvasScrollViewer.VerticalOffset + 20);
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// Сохранение диаграммы в виде рисунка
        /// </summary>
        private void MenuSaveImg_Click(object sender, RoutedEventArgs e)
        {
            bool result = CheckBefore();
            if (!result)
                return;

            MainCanvas.UpdateLayout();

            // Рассчитываем реальные границы содержимого (включая элементы за пределами Canvas)
            Rect bounds = VisualTreeHelper.GetDescendantBounds(MainCanvas);
            if (bounds.IsEmpty)
            {
                MessageBox.Show("Диаграмма пуста!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                DrawingVisual dv = new DrawingVisual();
                using (DrawingContext dc = dv.RenderOpen())
                {
                    dc.DrawRectangle(Brushes.White, null, bounds);
                    VisualBrush vb = new VisualBrush(MainCanvas);
                    dc.DrawRectangle(vb, null, bounds);
                }
                double dpi = 96;
                RenderTargetBitmap rtb = new RenderTargetBitmap(
                    (int)bounds.Width,
                    (int)bounds.Height,
                    dpi, dpi, PixelFormats.Pbgra32);

                rtb.Render(dv);

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PNG Image (*.png)|*.png",
                    FileName = "ClassDiagram"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (FileStream fs = File.Create(saveFileDialog.FileName))
                    {
                        encoder.Save(fs);
                    }
                    MessageBox.Show("Диаграмма сохранена в PNG", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        #region Генерации там всякие

        /// Реализация интерфейса в коде
        public static void InterfaceRealization(Class userClass)
        {
            List<UMLElement> elements = Model.UserElements;
            if (userClass.Constraint != ConstraintType.Interface)
            {
                var hasInter = elements.Where(p => p is Class && (p as Class).Constraint == ConstraintType.Interface).ToList();

                var cons = elements.Where(p => p is Connection && (p as Connection).ConnectionType.Id == 7).ToList();

                List<ClassMethod> newMethods = new List<ClassMethod>();
                foreach (var con in cons)
                {
                    if ((con as Connection).Class1 == userClass)
                    {
                        if ((con as Connection).Class2.Constraint == ConstraintType.Interface)
                        {
                            newMethods = ((con as Connection).Class2.Methods);
                        }
                    }
                    if ((con as Connection).Class2 == userClass)
                    {
                        newMethods = ((con as Connection).Class1.Methods);
                    }
                }

                foreach (var method in newMethods)
                {
                    if (!userClass.Methods.Contains(method))
                        userClass.Methods.Add(method);
                }
            }
        }

        /// <summary>
        /// Генерация PlantUML
        /// </summary>
        private void PlantUMLGenerate_Click(object sender, RoutedEventArgs e)
        {
            bool result = CheckBefore();
            if (!result)
                return;

            PlantUMLGenerator generator = new PlantUMLGenerator();
            generator.Output = "@startuml\n";
            foreach (var element in Model.UserElements)
            {
                element.Accept(generator);
            }
            generator.Output += "@enduml";

            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                FileName = "PlantUML.txt",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = ".txt",
                Title = "Сохранить код PlantUML"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, generator.Output);

                    MessageBox.Show("Генерация в PlantUML прошла успешно!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Генерация C#
        /// </summary>
        private void CSharpGenerate_Click(object sender, RoutedEventArgs e)
        {
            bool result = CheckBefore();
            if (!result)
                return;

            CSharpGenerator generator = new CSharpGenerator();
            foreach (var element in Model.UserElements)
            {
                element.Accept(generator);
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                FileName = "Code.cs",
                Filter = "C# Files (*.cs)|*.cs",
                DefaultExt = ".cs",
                Title = "Сохранить код диаграммы"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, generator.Output);

                    MessageBox.Show("Генерация в C# прошла успешно!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Загрузка кода для генерации диаграмм
        /// </summary>
        private void GetFromCSharp_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckUnsavedChanges())
            {
                return;
            }

            var openDialog = new OpenFileDialog
            {
                Filter = "С# Files (*.cs) | *.cs",
                Title = "Загрузить код"
            };

            StringBuilder results = new StringBuilder();

            if (openDialog.ShowDialog() == true)
            {
                currentFilePath = openDialog.FileName;
                ClearAll();

                using (StreamReader reader = new StreamReader(openDialog.FileName))
                {
                    string content = reader.ReadToEnd();

                    LexemeAnalyzer lexer = new LexemeAnalyzer(content);
                    try
                    {
                        lexer.Run();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }

                    results = new StringBuilder();
                    results.AppendLine("\nРезультаты лексического анализа:");
                    results.AppendLine($"Позиция \t|Строка \t|Тип лексемы \t\t|Значение \t|");
                    foreach (var lexeme in lexer.GetData())
                    {
                        results.AppendLine($"{lexeme.Position}\t\t{lexeme.Line}\t\t {lexeme.lexemeType}\t\t\t {lexeme.Value}");
                    }

                    results.AppendLine("\n\n");

                    SyntaxAnalyzer generator = new SyntaxAnalyzer(lexer.GetData());

                    try
                    {
                        generator.Run();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }

                    results.AppendLine("\nРезультаты синтаксического анализа:");
                    List<UMLElement> result = generator.GetResult();
                    List<Class> classes = result.OfType<Class>().Where(p => p is Class).ToList();
                    foreach (Class cl in classes)
                    {
                        results.AppendLine($"{cl.Name}\t\t{cl.Attributes.Count}\t\t {cl.Methods.Count}\t\t\t{cl.AccessType}\t\t{cl.Constraint}");

                        foreach (var attribute in cl.Attributes)
                            results.AppendLine($"\t\t{attribute.Name}\t\t{attribute.dataType}\t\t {attribute.accessType}\t\t\t{attribute.Constraint}");

                        foreach (var method in cl.Methods)
                            results.AppendLine($"\t\t{method.Name}\t\t{method.dataType}\t\t {method.accessType}\t\t\t{method.Constraint}");
                    }
                    List<Connection> connections = result.OfType<Connection>().Where(p => p is Connection).ToList();
                    foreach (Connection con in connections)
                    {
                        results.AppendLine($"{con.Name}\t\t{con.Class1.Name}\t\t {con.Class2.Name}\t\t\t{con.ConnectionType.Name}\t\t");
                        if (con.Role2 != null)
                            results.AppendLine($"{con.Role2.Name}");
                    }

                    var realizations = connections.Where(p => p.ConnectionType.Id == 4).ToList();
                    foreach (Connection rel in realizations)
                    {
                        if (rel.Class1.Constraint == ConstraintType.Interface)
                            rel.ConnectionType = new ConnectionType(7, "Реализация");
                    }

                    if (result != null)
                    {
                        UpdateLists(result);

                        classes = Model.UserElements.OfType<Class>().ToList();
                        connections = Model.UserElements.OfType<Connection>().ToList();

                        foreach (var cl in classes)
                        {
                            cl.Draw(MainCanvas);
                            Model.userDataTypes.Add(cl.Name);
                        }
                        foreach (var con in connections)
                        {
                            con.ConnectionType = Model.connectionTypes.FirstOrDefault(ct => ct.Id == con.ConnectionType.Id);
                            con.Name = $"{con.Class1.Name} - {con.Class2.Name}: {con.ConnectionType.Name}";
                            con.Class1 = classes.FirstOrDefault(p => p.Name == con.Class1.Name);
                            con.Class2 = classes.FirstOrDefault(p => p.Name == con.Class2.Name);
                            con.Draw(MainCanvas);
                        }
                    }
                }
            }

            if (results.Length > 0)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog()
                {
                    FileName = "Parser Results.txt",
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    DefaultExt = ".txt",
                    Title = "Сохранить результаты работы парсера"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        File.WriteAllText(saveFileDialog.FileName, results.ToString());

                        MessageBox.Show("Результаты анализаторов успешно сохранены!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Вернуть удаленные классы
        /// </summary>
        private void MenuZ_Click(object sender, RoutedEventArgs e)
        {
            if (Model.removedElements.Count > 0)
            {
                int id = Model.removedElements.LastOrDefault().RemoveId;

                var removedElements = Model.removedElements.Where(p => p.RemoveId == id).ToList();

                foreach (var element in removedElements)
                {
                    if (element is Connection)
                        (element as Connection).Draw(MainCanvas);
                    else if (element is Class)
                        (element as Class).Draw(MainCanvas, element.X, element.Y);

                    Model.removedElements.Remove(element);
                    Model.UserElements.Add(element);
                }

                UpdateLists(Model.UserElements);
            }
        }

        #region Масштабирование
        // текущий масштаб 1.0 = 100%
        private double zoomFactor = 1.0;
        private const double ZOOM_MIN = 0.2; // нельзя уменьшать масштаб меньше 20%
        private const double ZOOM_MAX = 3.0; // не больше 300%
        private const double ZOOM_STEP = 0.1; // 10%

        private Point mouseDownPosition; // мышь в момент нажатия
        private bool isPanning = false; // перетаскивание

        private void ApplyZoom(Point zoomCenter)
        {
            // для плавности
            double scrollX = CanvasScrollViewer.HorizontalOffset;
            double scrollY = CanvasScrollViewer.VerticalOffset;

            Point mousePosInDocument = new Point(
            (scrollX + zoomCenter.X) / zoomFactor,
            (scrollY + zoomCenter.Y) / zoomFactor);

            // изменение масштаба
            var scaleTransform = new ScaleTransform(zoomFactor, zoomFactor);
            MainCanvas.LayoutTransform = scaleTransform;


            double maxX = 100;
            double maxY = 100;

            // Находим самый правый и самый нижний элемент
            foreach (var element in Model.UserElements)
            {
                if (element is Class classElement)
                {
                    maxX = Math.Max(maxX, classElement.X + classElement.Width + 200);
                    maxY = Math.Max(maxY, classElement.Y + classElement.Height + 300);
                }
            }

            // Добавляем запас
            maxX += 500;
            maxY += 500;

            // Применяем с учетом масштаба
            MainCanvas.Width = maxX * zoomFactor;
            MainCanvas.Height = maxY * zoomFactor;

            double newScrollX = (mousePosInDocument.X * zoomFactor) - zoomCenter.X;
            double newScrollY = (mousePosInDocument.Y * zoomFactor) - zoomCenter.Y;

            CanvasScrollViewer.ScrollToHorizontalOffset(newScrollX);
            CanvasScrollViewer.ScrollToVerticalOffset(newScrollY);

        }

        private void MainCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point mousePos = e.GetPosition(MainCanvas);

            // Изменяем масштаб (мин и макс не дает выйти за пределы)
            if (e.Delta > 0) // крутим от себя - увеличение
                zoomFactor = Math.Min(ZOOM_MAX, zoomFactor + ZOOM_STEP);
            else // delta < 0 - уменьшение
                zoomFactor = Math.Max(ZOOM_MIN, zoomFactor - ZOOM_STEP);

            // Применяем масштабирование
            ApplyZoom(mousePos);

            ZoomText.Text = $"Масштаб: {zoomFactor:P0}";

            e.Handled = true; // событие обработано
        }

        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed || Keyboard.Modifiers == ModifierKeys.Control)
            {
                isPanning = true;
                mouseDownPosition = e.GetPosition(CanvasScrollViewer);
                MainCanvas.Cursor = Cursors.SizeAll;
                e.Handled = true;
            }

            ZoomText.Text = $"Масштаб: {zoomFactor:P0}";
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPanning)
            {
                Point currentPos = e.GetPosition(CanvasScrollViewer);
                double offsetX = mouseDownPosition.X - currentPos.X;
                double offsetY = mouseDownPosition.Y - currentPos.Y;

                CanvasScrollViewer.ScrollToHorizontalOffset(CanvasScrollViewer.HorizontalOffset + offsetX);
                CanvasScrollViewer.ScrollToVerticalOffset(CanvasScrollViewer.VerticalOffset + offsetY);

                mouseDownPosition = currentPos;
                e.Handled = true;
            }

            ZoomText.Text = $"Масштаб: {zoomFactor:P0}";
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // сброс перехвата
            if (Mouse.Captured == MainCanvas)
            {
                Mouse.Capture(null);
            }

            if (isPanning)
            {
                isPanning = false;
                MainCanvas.Cursor = Cursors.Arrow;
                e.Handled = true;
            }
        }

        private void MenuHelp_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow helpWindow = new HelpWindow();
            helpWindow.ShowDialog();
        }
        #endregion    
    }
}
