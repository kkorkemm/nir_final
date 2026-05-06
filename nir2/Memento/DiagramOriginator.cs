using nir2.Entities;
using nir2.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace nir2.Memento
{
    public class DiagramOriginator
    {
        private List<UMLElement> model;
        private Canvas MainCanvas;

        public DiagramOriginator(Canvas canvas)
        {
            MainCanvas = canvas;
            model = new List<UMLElement>();
        }

        public DiagramMemento CreateMemento(List<UMLElement> elements)
        {
            return new DiagramMemento(elements);
        }

        public void RestoreMemento(DiagramMemento memento)
        {
            MainCanvas.Children.Clear();
            model.Clear();

            foreach (Class cls in memento.Classes)
            {
                cls.Draw(MainCanvas, cls.X, cls.Y);
                model.Add(cls);
            }

            foreach (Connection con in memento.Connections)
            {
                con.Draw(MainCanvas);
                model.Add(con);
            }
        }
    }
}
