using HrTetris.Enums;
using HrTetris.Model;
using HrTetris.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace HrTetris.Views
{
    /// <summary>
    /// Interaction logic for BoardView.xaml
    /// </summary>
    public partial class BoardView : UserControl
    {
        private const int _canvasLineThickness = 2;

        private double _playFieldLineWidth;
        private double _playFieldLineHeight;
        private double _playFieldWidth;
        private double _playFieldHeight;
        private double _nextShapeLineWidth;
        private double _nextShapeLineHeight;
        private Window _window;

        private DispatcherTimer _timer;
        private DateTime _lastTimerMove = DateTime.Now;
        private int _moveInterval;

        private bool _collided;

        private BoardViewModel _bvm;
        private int _boardWidth = 10;
        private int _boardHeight = 20;

        private Rectangle _cell = new Rectangle();
        private Model.Shape _moviengShape;
        private Model.Shape _nextShape;

        public BoardView()
        {
            InitializeComponent();
        }

        private void InitializeAll()
        {
            _bvm = new BoardViewModel(_boardWidth, _boardHeight);
            this.DataContext = _bvm;
            _playFieldWidth = playField.Width - _canvasLineThickness / 2;
            _playFieldHeight = playField.Height - _canvasLineThickness / 2;
            _playFieldLineWidth = _playFieldWidth / _boardWidth;
            _playFieldLineHeight = _playFieldHeight / _boardHeight;
            _cell.Width = _playFieldLineWidth - 2 * _canvasLineThickness;
            _cell.Height = _playFieldLineHeight - 2 * _canvasLineThickness;
            _bvm.SetRectangle(_cell);
            _bvm.GenerateShapes();

            _nextShapeLineWidth = _playFieldLineWidth * 4 - _canvasLineThickness / 2;
            _nextShapeLineHeight = _playFieldLineHeight * 4 - _canvasLineThickness / 2;
            canvasNextShape.Width = _nextShapeLineWidth + _canvasLineThickness / 2;
            canvasNextShape.Height = _nextShapeLineHeight + _canvasLineThickness / 2;

            _window = Window.GetWindow(this);
            _window.KeyDown += Window_KeyDown;
            _window.Closing += Window_Closing;
        }

        private void GenerateCleanBoard()
        {
            canvasPlayField.Children.Clear();
            canvasPlayField.Background = new SolidColorBrush(Colors.Black);
            GenerateVerticalLines(canvasPlayField, _playFieldWidth, _playFieldHeight, _playFieldLineWidth);
            GenerateHorizontalLines(canvasPlayField, _playFieldWidth, _playFieldHeight, _playFieldLineHeight);
        }
        private void GenerateCleanNextShape()
        {
            canvasNextShape.Children.Clear();
            canvasNextShape.Background = new SolidColorBrush(Colors.Black);
            GenerateVerticalLines(canvasNextShape, _nextShapeLineWidth, _nextShapeLineHeight, _playFieldLineWidth);
            GenerateHorizontalLines(canvasNextShape, _nextShapeLineWidth, _nextShapeLineHeight, _playFieldLineHeight);

        }

        private void GenerateVerticalLines(Canvas canvas, double fieldWidth, double fieldHeight, double lineWidth)
        {
            for (double i = 0; i <= fieldWidth; i += lineWidth)
            {
                Line line = new Line()
                {
                    Stroke = Brushes.Silver,
                    StrokeThickness = _canvasLineThickness,
                    X1 = i,
                    Y1 = 0,
                    X2 = i,
                    Y2 = fieldHeight
                };
                canvas.Children.Add(line);
            }
        }
        private void GenerateHorizontalLines(Canvas canvas, double fieldWidth, double fieldHeight, double lineHeight)
        {
            for (double i = 0; i <= fieldHeight; i += lineHeight)
            {
                Line line = new Line()
                {
                    Stroke = Brushes.Silver,
                    StrokeThickness = _canvasLineThickness,
                    X1 = 0,
                    Y1 = i,
                    X2 = fieldWidth,
                    Y2 = i
                };
                canvas.Children.Add(line);
            }
        }

        private void RedrawCells()
        {
            canvasPlayField.Children.Clear();
            GenerateCleanBoard();
            foreach (Cell c in _bvm.Board)
            {
                Canvas.SetLeft(c.Rectangle, _playFieldLineWidth * c.X + _canvasLineThickness);
                Canvas.SetTop(c.Rectangle, _playFieldLineHeight * c.Y + _canvasLineThickness);
                canvasPlayField.Children.Add(c.Rectangle);
            }
        }
        private void RedrawNextShape()
        {
            canvasNextShape.Children.Clear();
            GenerateCleanNextShape();
            if (_nextShape != null)
                foreach (Cell c in _nextShape.ShapeMembers)
                {
                    Canvas.SetLeft(c.Rectangle, _playFieldLineWidth * c.X + _canvasLineThickness);
                    Canvas.SetTop(c.Rectangle, _playFieldLineHeight * c.Y + _canvasLineThickness);
                    canvasNextShape.Children.Add(c.Rectangle);
                }
        }

        private void TryMove(KeyDirection keyDirection)
        {
            if (_moviengShape != null)
            {
                _moviengShape = _bvm.MoveShapeOnBoard(_moviengShape, keyDirection, out _collided);
                RedrawCells();
            }
        }

        private void StartGame()
        {
            GenerateBoard();
            _bvm.Restart();
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(10);
            _timer.Tick += _timer_Tick;
            GenerateNextShape();
            _collided = false;
            canvasPlayField.Focus();
            _lastTimerMove = DateTime.Now;
            _moveInterval = 1000;
            _timer.Start();
        }
        private void StopGame()
        {
            _timer.Stop();
            _collided = true;
            _nextShape = null;

            if (_bvm.NewHighScore())
            {
                EnterName dialog = new EnterName();
                if (dialog.ShowDialog() == true)
                {
                    Score score = new Score(_bvm.Level, _bvm.Score);
                    score.Name = dialog.EnteredName;
                    _bvm.AddHighScore(score);
                }

            }
        }

        private void CheckShapeColisionAfterMove()
        {
            if (_collided)
            {
                List<int> collapsRowIndex = _bvm.CheckRowsToCollapse();
                if (collapsRowIndex.Count > 0)
                {
                    _bvm.CollapseRows(collapsRowIndex);
                    TryLevelUp(collapsRowIndex.Count);
                }
                _collided = false;
                GenerateNextShape();
            }
        }

        private void TryLevelUp(int collapsedLines)
        {
            if (_bvm.TryLevelUp(collapsedLines))
            {
                _moveInterval = (int)(_moveInterval * 0.75);
            }
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if ((DateTime.Now - _lastTimerMove).TotalMilliseconds > _moveInterval)
            {
                TryMove(KeyDirection.Down);
                _lastTimerMove = DateTime.Now;
                CheckShapeColisionAfterMove();
            }
        }

        private void GenerateNextShape()
        {
            if (_nextShape == null)
                _nextShape = _bvm.GenerateRandomShape();
            _moviengShape = _nextShape;
            _nextShape = _bvm.GenerateRandomShape();
            if (_bvm.AddShapeToBoard(_moviengShape))
                StopGame();

            RedrawCells();
            RedrawNextShape();
        }

        private void GenerateBoard()
        {
            _bvm.Board.Clear();
            RedrawCells();
            RedrawNextShape();
        }

        #region Events
        private void GenerateBoard(object sender, RoutedEventArgs e)
        {
            GenerateBoard();
        }

        private void btnStartGame_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }
        private void BoardViewControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeAll();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _window.KeyDown -= Window_KeyDown;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_collided)
                switch (e.Key)
                {
                    case Key.Up:
                        TryMove(KeyDirection.Rotate);
                        break;
                    case Key.Left:
                        TryMove(KeyDirection.Left);
                        break;
                    case Key.Right:
                        TryMove(KeyDirection.Right);
                        break;
                    case Key.Down:
                        TryMove(KeyDirection.Down);
                        _lastTimerMove = DateTime.Now.AddSeconds(-5);
                        break;
                    case Key.Space:
                        TryMove(KeyDirection.Drop);
                        _lastTimerMove = DateTime.Now.AddSeconds(-5);
                        break;
                    default:
                        break;
                }
        }
        #endregion Events

    }
}
