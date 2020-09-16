using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HrTetris.Enums;

namespace HrTetris.Model
{
    public class Shape
    {
        public ShapeID ID;
        public int Index;
        public int X;
        public int Y;
        public List<Cell> ShapeMembers = new List<Cell>();
        public Shape() { }
        public Shape(ShapeID id, int index)
        {
            ID = id;
            Index = index;
        }

        public Shape Clone()
        {
            Shape shape = new Shape();
            shape.X = X;
            shape.Y = Y;
            shape.ID = ID;
            shape.Index = Index;
            foreach (Cell c in ShapeMembers)
            {
                shape.ShapeMembers.Add(c.Clone());
            }
            return shape;
        }
    }
}
