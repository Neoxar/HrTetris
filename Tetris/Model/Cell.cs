using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Tetris.Model
{
    public class Cell
    {
        public Rectangle Rectangle;
        public int X;
        public int Y;

        public Cell() { }
        public Cell(Rectangle rectangle, int x, int y)
            : this()
        {
            Rectangle = rectangle;
            X = x;
            Y = y;
        }

        public Cell Clone()
        {
            return new Cell(Helper.HelperMethods.CloneRectangle(Rectangle, (SolidColorBrush)Rectangle.Stroke, (SolidColorBrush)Rectangle.Fill), X, Y);
        }
    }
}
