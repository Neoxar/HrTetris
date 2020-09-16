using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Shapes;
using HrTetris.Enums;
using HrTetris.Helper;
using HrTetris.Model;
using HrTetris.Properties;

namespace HrTetris.ViewModel
{
    public class BoardViewModel : INotifyPropertyChanged
    {
        #region Local Variables
        private Rectangle _cellRectangle;
        private List<Model.Shape> _shapes;
        private int _width;
        private int _height;
        private int _collapsedLines;

        private List<int> _linePoints = new List<int> { 40, 100, 300, 1200 };

        private readonly Random _random;
        #endregion Local Variables

        public List<Cell> Board;

        private List<Score> _topScores;
        public List<Score> TopScores
        {
            get
            {
                return _topScores.OrderBy(ts => ts.Points).ToList();
            }
            set
            {
                if (value != _topScores)
                {
                    _topScores = value;
                    RaisePropertyChanged(nameof(TopScores));
                }
            }
        }

        private int _score;
        public int Score
        {
            get { return _score; }
            set
            {
                if (value != _score)
                {
                    _score = value;
                    RaisePropertyChanged(nameof(Score));
                }
            }
        }

        public int _level;
        public int Level
        {
            get { return _level; }
            set
            {
                if (value != _level)
                {
                    _level = value;
                    RaisePropertyChanged(nameof(Level));
                }
            }
        }

        #region Constructors

        public BoardViewModel()
        {
            Restart();
            Board = new List<Cell>();
            LoadHighScore();
            _random = new Random();
        }
        public BoardViewModel(int width, int height)
           : this()
        {
            _width = width;
            _height = height;
        }

        #endregion Constructors

        public void SetRectangle(Rectangle cellRectangle)
        {
            _cellRectangle = cellRectangle;
        }

        public void Restart()
        {
            Score = 0;
            Level = 1;
            _collapsedLines = 0;
        }
        public bool TryLevelUp(int collapsedLines)
        {
            bool leveledUp = false;
            _collapsedLines += collapsedLines;
            int newLevel = (_collapsedLines / 10) % 10 + 1;
            if (newLevel != _level)
            {
                Level = newLevel;
                leveledUp = true;
            }
            return leveledUp;
        }

        public bool NewHighScore()
        {
            bool newHighScore = false;
            if (TopScores.Count < 10 || TopScores.FirstOrDefault(ts => ts.Points < _score) != null)
            {
                newHighScore = true;
            }
            return newHighScore;
        }
        public void AddHighScore(Score score)
        {
            if (_topScores.Count > 10)
            {
                _topScores.Remove(_topScores.FirstOrDefault(ms => ms.Points == TopScores.Min(ts => ts.Points)));
            }
            _topScores.Add(score);
            RaisePropertyChanged(nameof(TopScores));
            SaveHighScore();
        }
        private void LoadHighScore()
        {
            if (string.IsNullOrWhiteSpace(Settings.Default.TopScores))
                TopScores = new List<Score>();
            else
                TopScores = JSON.DeserializeObject<List<Score>>(Settings.Default.TopScores);
        }
        private void SaveHighScore()
        {
            Settings.Default.TopScores = JSON.SerializeToJsonString<List<Score>>(_topScores);
            Settings.Default.Save();
        }

        #region Row Collapse

        public List<int> CheckRowsToCollapse()
        {
            List<int> rowsToCollapse = new List<int>();
            for (int i = _height - 1; i > 0; i--)
            {
                if (Board.Where(c => c.Y == i).Count() == _width + 2)
                {
                    rowsToCollapse.Add(i);
                }
            }
            return rowsToCollapse;
        }
        public void CollapseRows(List<int> rowIndex)
        {
            rowIndex.Sort();
            foreach (int i in rowIndex)
                CollapseRow(i);
            Score += _linePoints[rowIndex.Count - 1] * (_level);
        }
        private void CollapseRow(int index)
        {
            List<Cell> toRemove = Board.Where(c => c.Y == index && c.X != 0 && c.X != _width + 1).ToList();
            foreach (Cell c in toRemove)
                Board.Remove(c);
            List<Cell> toDrop = Board.Where(c => c.Y < index && c.X != 0 && c.X != _width + 1).ToList();
            foreach (Cell c in toDrop)
                c.Y += 1;
        }

        #endregion Row Collapse

        #region Shape Generation

        public void GenerateBoardWalls()
        {
            for (int i = 0; i < _height; i++)
            {
                Board.Add(new Cell(HelperMethods.CloneRectangle(_cellRectangle, new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.Gray)), 0, i));
                Board.Add(new Cell(HelperMethods.CloneRectangle(_cellRectangle, new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.Gray)), _width + 1, i));
            }
        }

        public void GenerateShapes()
        {
            _shapes = new List<Model.Shape>();
            _shapes.Add(GenerateShape(ShapeID.O, 0, Colors.Yellow, new List<Point> { new Point(1, 1), new Point(2, 1), new Point(1, 2), new Point(2, 2) }));

            _shapes.Add(GenerateShape(ShapeID.I, 0, Colors.Teal, new List<Point> { new Point(0, 1), new Point(1, 1), new Point(2, 1), new Point(3, 1) }));
            _shapes.Add(GenerateShape(ShapeID.I, 1, Colors.Teal, new List<Point> { new Point(2, 0), new Point(2, 1), new Point(2, 2), new Point(2, 3) }));

            _shapes.Add(GenerateShape(ShapeID.S, 0, Colors.Pink, new List<Point> { new Point(1, 2), new Point(2, 2), new Point(2, 1), new Point(3, 1) }));
            _shapes.Add(GenerateShape(ShapeID.S, 1, Colors.Pink, new List<Point> { new Point(2, 0), new Point(2, 1), new Point(3, 1), new Point(3, 2) }));

            _shapes.Add(GenerateShape(ShapeID.Z, 0, Colors.Green, new List<Point> { new Point(1, 1), new Point(2, 1), new Point(2, 2), new Point(3, 2) }));
            _shapes.Add(GenerateShape(ShapeID.Z, 1, Colors.Green, new List<Point> { new Point(3, 0), new Point(2, 1), new Point(3, 1), new Point(2, 2) }));

            _shapes.Add(GenerateShape(ShapeID.L, 0, Colors.Orange, new List<Point> { new Point(1, 1), new Point(2, 1), new Point(3, 1), new Point(1, 2) }));
            _shapes.Add(GenerateShape(ShapeID.L, 1, Colors.Orange, new List<Point> { new Point(2, 0), new Point(2, 1), new Point(2, 2), new Point(3, 2) }));
            _shapes.Add(GenerateShape(ShapeID.L, 2, Colors.Orange, new List<Point> { new Point(1, 1), new Point(2, 1), new Point(3, 1), new Point(3, 0) }));
            _shapes.Add(GenerateShape(ShapeID.L, 3, Colors.Orange, new List<Point> { new Point(2, 0), new Point(2, 1), new Point(2, 2), new Point(1, 0) }));

            _shapes.Add(GenerateShape(ShapeID.J, 0, Colors.Blue, new List<Point> { new Point(1, 1), new Point(2, 1), new Point(3, 1), new Point(3, 2) }));
            _shapes.Add(GenerateShape(ShapeID.J, 1, Colors.Blue, new List<Point> { new Point(2, 0), new Point(2, 1), new Point(2, 2), new Point(3, 0) }));
            _shapes.Add(GenerateShape(ShapeID.J, 2, Colors.Blue, new List<Point> { new Point(1, 1), new Point(2, 1), new Point(3, 1), new Point(1, 0) }));
            _shapes.Add(GenerateShape(ShapeID.J, 3, Colors.Blue, new List<Point> { new Point(2, 0), new Point(2, 1), new Point(2, 2), new Point(1, 2) }));

            _shapes.Add(GenerateShape(ShapeID.T, 0, Colors.Red, new List<Point> { new Point(1, 1), new Point(2, 1), new Point(3, 1), new Point(2, 2) }));
            _shapes.Add(GenerateShape(ShapeID.T, 1, Colors.Red, new List<Point> { new Point(2, 0), new Point(2, 1), new Point(2, 2), new Point(3, 1) }));
            _shapes.Add(GenerateShape(ShapeID.T, 2, Colors.Red, new List<Point> { new Point(1, 1), new Point(2, 1), new Point(3, 1), new Point(2, 0) }));
            _shapes.Add(GenerateShape(ShapeID.T, 3, Colors.Red, new List<Point> { new Point(2, 0), new Point(2, 1), new Point(2, 2), new Point(1, 1) }));
        }

        private Model.Shape GenerateShape(ShapeID id, int index, Color color, List<Point> points)
        {
            Model.Shape shape = new Model.Shape(id, index);
            foreach (Point p in points)
            {
                shape.ShapeMembers.Add(new Cell(HelperMethods.CloneRectangle(_cellRectangle, new SolidColorBrush(Colors.Black), new SolidColorBrush(color)), (int)p.X, (int)p.Y));
            }
            return shape;
        }

        public Model.Shape GenerateRandomShape()
        {
            List<Model.Shape> tempShapes = _shapes.FindAll(s => s.Index == 0);
            Model.Shape tmp = tempShapes[_random.Next(0, tempShapes.Count)].Clone();
            tmp.X = (_width - 1) / 2;
            tmp.Y -= CalculateMaxVerticalMove(tmp, -1);
            return tmp;
        }

        #endregion Shape Generation

        #region Shape Manipulation

        public bool AddShapeToBoard(Model.Shape shape)
        {
            bool collision = false;
            foreach (Cell c in shape.ShapeMembers)
            {
                int x = c.X + shape.X;
                int y = c.Y + shape.Y;

                Cell colision = Board.FirstOrDefault(bc => bc.X == x && bc.Y == y);
                if (colision != null)
                {
                    collision = true;
                    Board.Remove(colision);
                }

                Cell newCell = c.Clone();
                newCell.X = x;
                newCell.Y = y;
                Board.Add(newCell);
            }
            return collision;
        }

        private void RemoveShapeFromBoard(Model.Shape shape)
        {
            foreach (Cell c in shape.ShapeMembers)
            {
                int x = c.X + shape.X;
                int y = c.Y + shape.Y;

                Cell existing = Board.FirstOrDefault(bc => bc.X == x && bc.Y == y);
                if (existing != null)
                {
                    Board.Remove(existing);
                }
            }
        }

        public Model.Shape MoveShapeOnBoard(Model.Shape shape, KeyDirection direction, out bool collided)
        {
            Model.Shape newShape = TryMoveShape(shape, direction, out collided);
            AddShapeToBoard(newShape);
            return newShape;
        }

        private Model.Shape TryMoveShape(Model.Shape shape, KeyDirection direction, out bool collided)
        {
            collided = false;
            RemoveShapeFromBoard(shape);
            Model.Shape shapeMoved = shape.Clone();

            if (direction == KeyDirection.Left || direction == KeyDirection.Right)
            {
                shapeMoved.X += direction == KeyDirection.Left ? -1 : 1;
            }
            else if (direction == KeyDirection.Down)
            {
                shapeMoved.Y += 1;
            }
            else if (direction == KeyDirection.Drop)
            {
                shapeMoved.Y += CalculateMaxVerticalMove(shape, 1);
            }
            else if (direction == KeyDirection.Rotate)
            {
                shapeMoved = RotateShape(shapeMoved);
            }
            if (!CanShapeBeAddedToBoard(shapeMoved))
            {
                if (direction == KeyDirection.Drop || direction == KeyDirection.Down)
                    collided = true;
                shapeMoved = shape;
            }
            return shapeMoved;
        }

        public Model.Shape RotateShape(Model.Shape shape)
        {
            List<Model.Shape> tempShapes = _shapes.FindAll(s => s.ID == shape.ID);
            int nextIndex = shape.Index + 1;
            if (nextIndex >= tempShapes.Count)
                nextIndex = 0;
            Model.Shape newShape = tempShapes.FirstOrDefault(s => s.Index == nextIndex);
            newShape.X = shape.X;
            newShape.Y = shape.Y;
            return newShape;
        }

        #endregion Shape Manipulation

        private bool CanShapeBeAddedToBoard(Model.Shape shape, bool goingUp = false)
        {
            bool validShapePosition = true;
            foreach (Cell c in shape.ShapeMembers)
            {
                int x = c.X + shape.X;
                int y = c.Y + shape.Y;
                if (y >= _height || y < 0 || !goingUp && Board.FirstOrDefault(bc => bc.X == x && bc.Y == y) != null)
                {
                    validShapePosition = false;
                    break;
                }
            }
            return validShapePosition;
        }

        private int CalculateMaxVerticalMove(Model.Shape shape, int direction)
        {
            int maxMove = 0;
            Model.Shape shapeMoved = shape.Clone();
            while (CanShapeBeAddedToBoard(shapeMoved, direction == -1))
            {
                maxMove++;
                shapeMoved = shape.Clone();
                shapeMoved.Y += direction * maxMove;
            }
            maxMove--;
            return maxMove;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
