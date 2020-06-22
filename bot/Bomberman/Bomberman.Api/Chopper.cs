using Bomberman.Api.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.WebSockets;

namespace Bomberman.Api
{
    public class Chopper : HasPoint
    { 
        public MovePosition MovePosition { get; private set; }
        public List<MovePosition> MoveHistory { get; private set; } = new List<MovePosition>();

        private List<Point> _nextTickPoints { get; set; } = new List<Point>();
        public List<BoardPoint> BoardPoints { get; private set; } = new List<BoardPoint>();

        private readonly Element[] CanNotMoveThrought = new Element[]
        {
            Element.WALL,
            Element.DESTROYABLE_WALL
        };

        private readonly MoveDirection[] _nextTickMoveDirectionMap = new[]
        {
            MoveDirection.Forward,
            MoveDirection.Backward,
            MoveDirection.Right,
            MoveDirection.Left
        };

        private static readonly int _nextTickMaxPossibility = 100;

        private readonly Dictionary<MoveDirection, int> _nextTickPointMovePossibilities = new Dictionary<MoveDirection, int>
        {
            { MoveDirection.Forward, 69 },
            { MoveDirection.Backward, 22},
            { MoveDirection.Right, 4},
            { MoveDirection.Left, 4},
            { MoveDirection.Unknown, _nextTickMaxPossibility }
        };

        public bool IsConflict { get; set; }
        public bool IsOnBoard { get; set; }
        public bool IsUnknownPossibleDirection => MovePosition == MovePosition.Unknown;

        public Chopper()
        {

        }

        public Chopper(Point point, bool isOnBoard = false)
        {
            Point = point;
            SetPosition(MovePosition.Unknown);

            if (isOnBoard)
            {
                IsOnBoard = true;
                InitUnknownNextTickPoints();
            }
        }

        public void CalculateAndSetDirection(Point point)
        {
            var position = GetNearPosition(point);
            SetPosition(position);
            Point = point;
            IsOnBoard = true;

            InitNextTickPoints();
        }

        private void InitNextTickPoints()
        {
            _nextTickPoints.Clear();

            var forward = GetNearPosition(MovePosition, MoveDirection.Forward);
            _nextTickPoints.Add(forward);

            var backward = GetNearPosition(MovePosition, MoveDirection.Backward);
            _nextTickPoints.Add(backward);

            var right = GetNearPosition(MovePosition, MoveDirection.Right);
            _nextTickPoints.Add(right);

            var left = GetNearPosition(MovePosition, MoveDirection.Left);
            _nextTickPoints.Add(left);
        }

        private void InitUnknownNextTickPoints()
        {
            _nextTickPoints.Clear();

            var top = Point.ShiftTop();
            _nextTickPoints.Add(top);

            var bottom = Point.ShiftBottom();
            _nextTickPoints.Add(bottom);

            var right = Point.ShiftRight();
            _nextTickPoints.Add(right);

            var left = Point.ShiftLeft();
            _nextTickPoints.Add(left);
        }

        public void InitBoardPoints()
        {
            BoardPoints.Clear();

            var coefficients = new List<Tuple<int, int>>();
            var result = new List<BoardPoint>();

            if (IsUnknownPossibleDirection)
            {
                for (var i = 0; i < _nextTickPoints.Count; i++)
                {
                    var point = _nextTickPoints[i];

                    if (Global.Board.IsAnyOfAt(point, CanNotMoveThrought))
                    {
                        continue;
                    }

                    coefficients.Add(new Tuple<int, int>(i, _nextTickMaxPossibility));
                }
            }
            else
            {
                for (int i = 0; i < _nextTickPoints.Count; i++)
                {
                    var point = _nextTickPoints[i];

                    if (Global.Board.IsAnyOfAt(point, CanNotMoveThrought))
                    {
                        continue;
                    }

                    var coefficient = _nextTickPointMovePossibilities[_nextTickMoveDirectionMap[i]];
                    coefficients.Add(new Tuple<int, int>(i, coefficient));
                }
            }

            var coefficientsSum = coefficients.Select(x => x.Item2).Sum();
            foreach (var coefficient in coefficients)
            {
                var point = _nextTickPoints[coefficient.Item1];

                var boardPoint = new BoardPoint(point)
                {
                    ChopperPossibility = coefficient.Item2 * 100 / coefficientsSum
                };

                result.Add(boardPoint);
            }

            BoardPoints.AddRange(result);

            //Console.WriteLine(string.Join(" | ", BoardPoints.Select(x => x.Point + " " + x.ChopperPossibility)));
            //return result;
        }

        public void SetPosition(MovePosition movePosition)
        {
            MovePosition = movePosition;
            MoveHistory.Add(MovePosition);
        }

        public void SetConflict()
        {
            IsConflict = true;
            SetPosition(MovePosition.Unknown);

            InitUnknownNextTickPoints();
        }

        public void ResetStateFlags()
        {
            IsConflict = false;
            IsOnBoard = false;
        }

        public int GetPossibility(Point point)
        {
            var boardPoint = BoardPoints.FirstOrDefault(x => x.Point.Equals(point));
            return boardPoint?.ChopperPossibility ?? 0;
        }
    }
}