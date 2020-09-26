using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HrTetris.Model
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

        public Cell Clone(bool shouldFill = false)
        {
            SolidColorBrush stroke = (SolidColorBrush)Rectangle.Stroke;
            SolidColorBrush fill = (SolidColorBrush)Rectangle.Fill;
            if (shouldFill)
            {
                stroke = new SolidColorBrush(fill.Color);
                fill = new SolidColorBrush(Colors.Gray);
            }

            return new Cell(Helper.HelperMethods.CloneRectangle(Rectangle, stroke, fill), X, Y);
        }
    }
}
