using nir2.Pages;
using nir2.Visitor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Path = System.Windows.Shapes.Path;

namespace nir2.Entities
{
    public class Connection : UMLElement
    {
        public string Name { get; set; }
        public ConnectionType ConnectionType { get; set; }

        public Class Class1 { get; set; }
        public Class Class2 { get; set; }

        public ConnectionRole Role1 { get; set; }
        public ConnectionRole Role2 { get; set; }

        private Path line;
        private Path diamondPath; // ромб для композиии/ агрегации
        private Path arrowPath; // для наследования / реализации
        private double x1, x2, y1, y2;

        public Connection() { }

        public TextBlock role1Text;
        public TextBlock role2Text;
        public TextBlock mult1Text;
        public TextBlock mult2Text;

        public void Draw(Canvas canvas)
        {
            MainCanvas = canvas;
            mainPage = (((canvas.Parent as ScrollViewer).Parent as Grid).Parent as Grid).Parent as MainPage;
            uIElements = new List<UIElement>();

            line = new Path
            {
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Tag = this // сохранение ссылки
            };
            
            UpdateLinePosition();

            MainCanvas.Children.Add(line);
            uIElements.Add(line);

            if (role1Text!= null)
            {
                MainCanvas.Children.Add(role1Text);
                MainCanvas.Children.Add(mult1Text);
                uIElements.Add(role1Text);
                uIElements.Add(mult1Text);
            }

            if (role2Text != null)
            {
                MainCanvas.Children.Add(role2Text);
                MainCanvas.Children.Add(mult2Text);
                uIElements.Add(role2Text);
                uIElements.Add(mult2Text);
            }
        }

        /// <summary>
        /// Перерисовка стрелки при перемщении классов
        /// </summary>
        public void UpdateLinePosition()
        {
            if (Class1 == null || Class2 == null || line == null) return;

            // Удаляем старый ромб если есть
            if (diamondPath != null && MainCanvas.Children.Contains(diamondPath))
            {
                MainCanvas.Children.Remove(diamondPath);
                diamondPath = null;
            }
            // Удаляем старый треугольник если есть
            if (arrowPath != null && MainCanvas.Children.Contains(arrowPath))
            {
                MainCanvas.Children.Remove(arrowPath);
                arrowPath = null;
            }

            if (Math.Abs(Class1.X - Class2.X) < 100)
            {
                x1 = Class1.X + Class1.Width / 2;
                x2 = Class2.X + Class2.Width / 2;

                if (Class1.Y < Class2.Y)
                {
                    y1 = Class1.Y + Class1.Height;
                    y2 = Class2.Y;
                }
                else
                {
                    y1 = Class1.Y;
                    y2 = Class2.Y + Class2.Height;
                }
            }
            else
            {
                y1 = Class1.Y + Class1.Height / 2;
                y2 = Class2.Y + Class2.Height / 2;

                if (Class1.X < Class2.X)
                {
                    x1 = Class1.X + Class1.Width;
                    x2 = Class2.X;
                }
                else
                {
                    x1 = Class1.X;
                    x2 = Class2.X + Class2.Width;
                }
            }

            Point startPoint = new Point(x1, y1);
            Point endPoint = new Point(x2, y2);

            // Обновляем позиции текстов ролей и кратностей
            UpdateRoleText();
            UpdateRoleTextPositions(startPoint, endPoint);

           
            switch (ConnectionType.Id)
            {
                case 1:

                    if (Class1 == Class2)
                        line.Data = CreateSelfConnectionWithArrow(startPoint);
                    else
                        line.Data = CreateDependencyArrow(startPoint, endPoint);
                    break;
                case 2:
                    line.StrokeDashArray = new DoubleCollection { 3, 3 };
                    line.Data = CreateDependencyArrow(startPoint, endPoint);
                    arrowPath = null;
                    break;
                case 4:
                    line.Data = CreateLineWithArrow(endPoint, startPoint, 0.07);
                    break;
                case 7:
                    line.StrokeDashArray = new DoubleCollection { 3, 3 };
                    line.Data = CreateLineWithArrow(endPoint, startPoint, 0.07);
                    break;
                case 3: line.Data = CreateLineWithDiamond(startPoint, endPoint, Brushes.White); break;
                case 5: line.Data = CreateLineWithDiamond(startPoint, endPoint, Brushes.Black); break;
                case 6:
                    line.Data = new LineGeometry(startPoint, endPoint);
                    line.Stroke = Brushes.Black; break;
                default:
                    line.Data = new LineGeometry(startPoint, endPoint);
                    line.Stroke = Brushes.Black; break;
            }
        }

        /// <summary>
        /// Паттерн Посетитель
        /// </summary>
        public override void Accept(IVisitor visitor)
        {
            visitor.VisitConnection(this);
        }

        /// <summary>
        /// Создает линию для самоассоциации
        /// </summary>
        private Geometry CreateSelfConnectionWithArrow(Point startPoint)
        {
            var pathGeometry = new PathGeometry();

            double loopWidth = 50;
            double loopHeight = 30;
            double arrowSize = 8;

            startPoint = new Point(startPoint.X + 100, startPoint.Y);

            var pathFigure = new PathFigure { StartPoint = startPoint };

            Point topLeft = new Point(startPoint.X, startPoint.Y - loopHeight);
            Point topRight = new Point(startPoint.X + loopWidth, startPoint.Y - loopHeight);
            Point bottomRight = new Point(startPoint.X + loopWidth, startPoint.Y);
            Point arrowBase = new Point(startPoint.X + loopWidth - arrowSize, startPoint.Y);

            // Петля
            pathFigure.Segments.Add(new LineSegment(topLeft, true));
            pathFigure.Segments.Add(new LineSegment(topRight, true));
            pathFigure.Segments.Add(new LineSegment(bottomRight, true));
            pathFigure.Segments.Add(new LineSegment(arrowBase, true));
            pathFigure.Segments.Add(new LineSegment(startPoint, true));

            pathGeometry.Figures.Add(pathFigure);

            Vector direction = new Vector(0, 1);

            double arrowLength = 12;
            double arrowWidth = 6;

            Point arrowTip = new Point(startPoint.X, startPoint.Y);

            Point leftPoint = new Point(
                arrowTip.X - arrowWidth,
                arrowTip.Y - arrowLength / 2);

            Point rightPoint = new Point(
                arrowTip.X + arrowWidth,
                arrowTip.Y - arrowLength / 2);

            // Левая сторона стрелки
            var leftArrow = new PathFigure { StartPoint = arrowTip };
            leftArrow.Segments.Add(new LineSegment(leftPoint, true));
            pathGeometry.Figures.Add(leftArrow);

            // Правая сторона стрелки
            var rightArrow = new PathFigure { StartPoint = arrowTip };
            rightArrow.Segments.Add(new LineSegment(rightPoint, true));
            pathGeometry.Figures.Add(rightArrow);

            return pathGeometry;
        }


        /// <summary>
        /// Создаёт линию для ассоциации и зависимости
        /// </summary>
        private Geometry CreateDependencyArrow(Point start, Point end)
        {
            var pathGeometry = new PathGeometry();

            Vector direction = new Vector(end.X - start.X, end.Y - start.Y);
            direction.Normalize();

            double arrowLength = 12;
            double arrowWidth = 6;

            // Боковые точки "птички"
            Point leftPoint = new Point(
                end.X - direction.X * arrowLength + (-direction.Y) * arrowWidth,
                end.Y - direction.Y * arrowLength + (direction.X) * arrowWidth);

            Point rightPoint = new Point(
                end.X - direction.X * arrowLength - (-direction.Y) * arrowWidth,
                end.Y - direction.Y * arrowLength - (direction.X) * arrowWidth);

            // Фигура 1: основная линия
            var mainLine = new PathFigure { StartPoint = start };
            mainLine.Segments.Add(new LineSegment(end, true));
            pathGeometry.Figures.Add(mainLine);

            // Фигура 2: левая сторона птички
            var leftArrow = new PathFigure { StartPoint = end };
            leftArrow.Segments.Add(new LineSegment(leftPoint, true));
            pathGeometry.Figures.Add(leftArrow);

            // Фигура 3: правая сторона птички
            var rightArrow = new PathFigure { StartPoint = end };
            rightArrow.Segments.Add(new LineSegment(rightPoint, true));
            pathGeometry.Figures.Add(rightArrow);

            return pathGeometry;

        }

        /// <summary>
        /// Создаёт линию для наследования и реализации
        /// </summary>
        /// 
        private PathGeometry CreateLineWithArrow(Point start, Point end, double arrowSize)
        {
            var lineGeometry = new PathGeometry();
            var arrowGeometry = new PathGeometry();

            Vector direction = new Vector(end.X - start.X, end.Y - start.Y);
            direction.Normalize();

            double arrowLength = 15;
            double arrowWidth = 10;
            Point lineEnd = new Point(
                end.X - direction.X * arrowLength,
                end.Y - direction.Y * arrowLength);

            // Линия
            var lineFigure = new PathFigure { StartPoint = start };
            lineFigure.Segments.Add(new LineSegment(lineEnd, true));
            lineGeometry.Figures.Add(lineFigure);

            // Стрелка
            Point arrowPoint1 = new Point(
                lineEnd.X + (-direction.Y) * arrowWidth / 2,
                lineEnd.Y + (direction.X) * arrowWidth / 2);

            Point arrowPoint2 = new Point(
                lineEnd.X - (-direction.Y) * arrowWidth / 2,
                lineEnd.Y - (direction.X) * arrowWidth / 2);

            var arrowFigure = new PathFigure { StartPoint = lineEnd };
            arrowFigure.Segments.Add(new LineSegment(arrowPoint1, true));
            arrowFigure.Segments.Add(new LineSegment(end, true));
            arrowFigure.Segments.Add(new LineSegment(arrowPoint2, true));
            arrowFigure.IsClosed = true;
            arrowGeometry.Figures.Add(arrowFigure);

            arrowPath = new Path();
            arrowPath.Data = arrowGeometry;
            arrowPath.Stroke = Brushes.Black;
            arrowPath.StrokeThickness = line.StrokeThickness;
            arrowPath.Fill = Brushes.White;
            MainCanvas.Children.Add(arrowPath);

            uIElements.Add(arrowPath);

            return lineGeometry;
        }

        /// <summary>
        /// Создаёт линию с ромбом
        /// </summary>
        private Geometry CreateLineWithDiamond(Point start, Point end, Brush fillBrush,  double size = 8)
        {
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure { StartPoint = end };

            Vector direction = new Vector(start.X - end.X, start.Y - end.Y);
            double length = direction.Length;
            direction.Normalize();

            Vector perpendicular = new Vector(-direction.Y, direction.X);

            // Размер ромба пропорционален длине линии
            double diamondSize = Math.Min(12, length * 0.15);

            // Вершины ромба
            Point tip = start;
            Point right = new Point(
                start.X - direction.X * diamondSize + perpendicular.X * diamondSize * 0.7,
                start.Y - direction.Y * diamondSize + perpendicular.Y * diamondSize * 0.7);
            Point baseCenter = new Point(
                start.X - direction.X * diamondSize * 2,
                start.Y - direction.Y * diamondSize * 2);
            Point left = new Point(
                start.X - direction.X * diamondSize - perpendicular.X * diamondSize * 0.7,
                start.Y - direction.Y * diamondSize - perpendicular.Y * diamondSize * 0.7);

            var diamondGeometry = new PathGeometry();
            diamondPath = new Path
            {
                Fill = fillBrush,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            var diamondFigure = new PathFigure { StartPoint = tip };
            diamondFigure.Segments.Add(new LineSegment(right, true));
            diamondFigure.Segments.Add(new LineSegment(baseCenter, true));
            diamondFigure.Segments.Add(new LineSegment(left, true));
            diamondFigure.IsClosed = true;
            diamondGeometry.Figures.Add(diamondFigure);
            diamondPath.Data = diamondGeometry;

            // Основная линия до центра основания ромба
            pathFigure.Segments.Add(new LineSegment(baseCenter, true));
            pathGeometry.Figures.Add(pathFigure);

            MainCanvas.Children.Add(diamondPath);
            uIElements.Add(diamondPath);
            return pathGeometry;
        }


        #region Роли

        private TextBlock CreateRoleText(string text)
        {
            return new TextBlock
            {
                Text = text ?? "",
                Foreground = Brushes.Black,
                Background = Brushes.Transparent,
                FontSize = 10,
                FontWeight = FontWeights.Normal,
                Padding = new Thickness(3, 1, 3, 1),
                Visibility = string.IsNullOrEmpty(text) ? Visibility.Collapsed : Visibility.Visible
            };
        }

        private TextBlock CreateMultiplicityText(string text)
        {
            return new TextBlock
            {
                Text = text ?? "",
                Foreground = Brushes.Black,
                Background = Brushes.Transparent,
                FontSize = 15,
                FontStyle = FontStyles.Normal,
                Padding = new Thickness(3, 1, 3, 1),
                Visibility = string.IsNullOrEmpty(text) ? Visibility.Collapsed : Visibility.Visible
            };
        }

        private void UpdateRoleText()
        {
            string r1Text = "";
            if (Role1 != null)
            {
                if (!string.IsNullOrWhiteSpace(Role1.Name))
                {
                    if (Role1.AccessType == AccessType.Public)
                        r1Text += "+";
                    else if (Role1.AccessType == AccessType.Protected)
                        r1Text += "#";
                    else
                        r1Text += "-";
                    r1Text += Role1.Name;
                }
            }

            string r2Text = "";
            if (Role2 != null)
            {
                if (!string.IsNullOrWhiteSpace(Role2.Name))
                {
                    if (Role2.AccessType == AccessType.Public)
                        r2Text += "+";
                    else if (Role2.AccessType == AccessType.Protected)
                        r2Text += "#";
                    else
                        r2Text += "-";
                    r2Text += Role2.Name;
                }
            }

            if (role1Text == null)
                role1Text = CreateRoleText(r1Text);
            else
                role1Text.Text = r1Text;

            if (role2Text == null)
                role2Text = CreateRoleText(r2Text);
            else
                role2Text.Text = r2Text;

            if (mult1Text == null)
                mult1Text = CreateMultiplicityText(Role1?.Multiplicity);
            else
                mult1Text.Text = Role1?.Multiplicity;
            
            if (mult2Text == null)
                mult2Text = CreateMultiplicityText(Role2?.Multiplicity);
            else
                mult2Text.Text = Role2?.Multiplicity;
        }

        /// <summary>
        /// Позиционирует текст для самосвязи
        /// </summary>
        private void PositionTextForSelfConnection(Point startPoint)
        {
            startPoint = new Point(startPoint.X + 100, startPoint.Y);

            // сбоку от петли
            double textX = startPoint.X - role2Text.ActualWidth - 10;
            double roleY = startPoint.Y - 20;
            double multiY = startPoint.Y - 35;

            Canvas.SetLeft(role2Text, textX);
            Canvas.SetTop(role2Text, roleY);
            Canvas.SetLeft(mult2Text, textX);
            Canvas.SetTop(mult2Text, multiY);

            // Скрываем первую роль
            role1Text.Visibility = Visibility.Collapsed;
            mult1Text.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Обновляет позиции текстов ролей и кратностей
        /// </summary>
        private void UpdateRoleTextPositions(Point startPoint, Point endPoint)
        {
            if (role1Text == null || role2Text == null ||
                mult1Text == null || mult2Text == null) return;

            bool isVertical = Math.Abs(startPoint.X - endPoint.X) < Math.Abs(startPoint.Y - endPoint.Y);

            if (Class1 == Class2)
                PositionTextForSelfConnection(startPoint);
            else
            {
                PositionTextElementsForClass(startPoint, endPoint, isVertical,
                    role1Text, mult1Text, true);

                PositionTextElementsForClass(endPoint, startPoint, isVertical,
                role2Text, mult2Text, false);
            }
        }

        /// <summary>
        /// Позиционирует текстовые элементы для одного класса
        /// </summary>
        private void PositionTextElementsForClass(Point classPoint, Point otherClassPoint, bool isVertical,
            TextBlock roleText, TextBlock multiplicityText, bool isFirstClass)
        {
            double dx = otherClassPoint.X - classPoint.X;
            double dy = otherClassPoint.Y - classPoint.Y;

            double offset = 10; // отступ 

            bool otherIsRight = otherClassPoint.X > classPoint.X;
            bool otherIsBelow = otherClassPoint.Y > classPoint.Y;

            bool isHorizontal = Math.Abs(dx) > Math.Abs(dy);

            if (isHorizontal)
            {
                // Горизонтальная линия
                if (otherIsRight)
                {
                    // класс СПРАВА 
                    // Текст СЛЕВА
                    // Роль СВЕРХУ, кратность СНИЗУ
                    Canvas.SetLeft(roleText, classPoint.X + offset);
                    Canvas.SetTop(roleText, classPoint.Y - offset - roleText.ActualHeight);

                    Canvas.SetLeft(multiplicityText, classPoint.X + offset);
                    Canvas.SetTop(multiplicityText, classPoint.Y + offset);
                }
                else
                {
                    // класс СЛЕВА
                    // Текст СПРАВА
                    // Роль СВЕРХУ, кратность СНИЗУ
                    Canvas.SetLeft(roleText, classPoint.X - offset - roleText.ActualWidth);
                    Canvas.SetTop(roleText, classPoint.Y - offset - roleText.ActualHeight);

                    Canvas.SetLeft(multiplicityText, classPoint.X - offset - multiplicityText.ActualWidth);
                    Canvas.SetTop(multiplicityText, classPoint.Y + offset);
                }
            }
            else
            {
                // Вертикальная линия
                if (otherIsBelow)
                {
                    // класс СНИЗУ
                    // Текст СВЕРХУ
                    // Роль СЛЕВА, кратность СПРАВА
                    Canvas.SetLeft(roleText, classPoint.X - offset - roleText.ActualWidth);
                    Canvas.SetTop(roleText, classPoint.Y + offset);

                    Canvas.SetLeft(multiplicityText, classPoint.X + offset);
                    Canvas.SetTop(multiplicityText, classPoint.Y + offset);
                }
                else
                {
                    // класс СВЕРХУ
                    // Текст СНИЗУ
                    // Роль СЛЕВА, кратность СПРАВА
                    Canvas.SetLeft(roleText, classPoint.X - offset - roleText.ActualWidth);
                    Canvas.SetTop(roleText, classPoint.Y - offset - roleText.ActualHeight);

                    Canvas.SetLeft(multiplicityText, classPoint.X + offset);
                    Canvas.SetTop(multiplicityText, classPoint.Y - offset - multiplicityText.ActualHeight);
                }
            }

            roleText.Visibility = string.IsNullOrEmpty(roleText.Text) ? Visibility.Collapsed : Visibility.Visible;
            multiplicityText.Visibility = string.IsNullOrEmpty(multiplicityText.Text) ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion

        #region Клонирование

        protected override UMLElement Clone()
        {
            return new Connection()
            {
                Name = this.Name,
                ConnectionType = this.ConnectionType,
                Class1 = this.Class1,
                Class2 = this.Class2,
                Role1 = this.Role1,
                Role2 = this.Role2
            };
        }

        protected override void CopyElement(UMLElement uMLElement)
        {
            throw new NotImplementedException();
        }

        protected override void PasteElement()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Методы для перемещения курсором
        protected override void Rect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void Rect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void Rect_MouseMove(object sender, MouseEventArgs e)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
