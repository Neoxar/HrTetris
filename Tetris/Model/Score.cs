using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HrTetris.Model
{
    public class Score
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public int Points { get; set; }

        public Score(int level, int points)
        {
            Name = string.Empty;
            Level = level;
            Points = points;
        }
    }
}
