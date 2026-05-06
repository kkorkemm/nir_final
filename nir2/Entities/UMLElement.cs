using nir2.Pages;
using nir2.Visitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace nir2.Entities
{
    public abstract class UMLElement : UIElement
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }  
        public double Height { get; set; }

        public bool isDragging { get; set;}
        public Point dragStartPoint { get; set; }

        public Canvas MainCanvas { get; set; }
        public MainPage mainPage { get; set; }

        public List<UIElement> uIElements { get; set; }

        protected ContextMenu contextMenu = new ContextMenu();

        public int RemoveId { get; set; }

        public UMLElement()
        {
            #region Контекстное меню
            contextMenu = new ContextMenu();
            var copyMenuItem = new MenuItem { Header = "Копировать" };
            copyMenuItem.Click += (s, e) => CopyElement(this);

            var pasteMenuItem = new MenuItem { Header = "Вставить" };
            pasteMenuItem.Click += (s, e) => PasteElement();

            contextMenu.Items.Add(copyMenuItem);
            contextMenu.Items.Add(pasteMenuItem);
            #endregion
        }

        public abstract void Accept(IVisitor visitor);

        #region Клонирование элементов
        protected abstract UMLElement Clone();

        protected abstract void CopyElement(UMLElement uMLElement);

        protected abstract void PasteElement();
        #endregion

        #region Методы для перемещения курсором
        protected abstract void Rect_MouseMove(object sender, MouseEventArgs e);
        protected abstract void Rect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e);
        protected abstract void Rect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e);
        #endregion
    }
}
