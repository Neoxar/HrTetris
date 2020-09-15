using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Media;

namespace Tetris.Helper
{
    public class HelperMethods
    {
        public static Rectangle CloneRectangle(Rectangle cellRectangle, SolidColorBrush stroke, SolidColorBrush fill)
        {
            return new Rectangle()
            {
                Stroke = stroke,
                Fill = fill,
                Width = cellRectangle.Width,
                Height = cellRectangle.Height
            };
        }
    }
}
