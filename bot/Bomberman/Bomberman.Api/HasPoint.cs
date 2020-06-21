using System;
using Bomberman.Api.Enums;
using Bomberman.Api.Infrastructure;

namespace Bomberman.Api
{
    public class HasPoint : IHasPoint
    {
        public Point Point { get; set; }

        private readonly MovePosition[] _positionsMap = new[]
        {
            MovePosition.Top,
            MovePosition.Right,
            MovePosition.Bottom,
            MovePosition.Left
        };

        public HasPoint()
        {

        }

        public HasPoint(Point point)
        {
            Point = point;
        }

        public bool IsNear(Point point)
        {
            return Point.ShiftLeft().Equals(point) ||
                   Point.ShiftRight().Equals(point) ||
                    Point.ShiftTop().Equals(point) ||
                    Point.ShiftBottom().Equals(point);
        }

        public MovePosition GetNearPosition(Point point)
        {
            if (Point.ShiftTop().Equals(point))
                return MovePosition.Top;

            if (Point.ShiftRight().Equals(point))
                return MovePosition.Right;

            if (Point.ShiftBottom().Equals(point))
                return MovePosition.Bottom;

            if (Point.ShiftLeft().Equals(point))
                return MovePosition.Left;

            if (Point.Equals(point))
                return MovePosition.Stop;

            return MovePosition.Unknown;
        }

        public Point GetNearPosition(MovePosition position, MoveDirection direction)
        {
            var movePosition = GetMovePositionForDirection(position, direction);

            switch (movePosition)
            {
                case MovePosition.Top:
                    return Point.ShiftTop();

                case MovePosition.Right:
                    return Point.ShiftRight();

                case MovePosition.Bottom:
                    return Point.ShiftBottom();

                case MovePosition.Left:
                    return Point.ShiftLeft();
            }

            throw new Exception("HasPoint.GetNearPosition: something went wrong");
        }

        private MovePosition GetMovePositionForDirection(MovePosition position, MoveDirection direction)
        {
            var positionIndex = _positionsMap.IndexOf(position);
            var nextPositionIndex = 0;

            switch (direction)
            {
                case MoveDirection.Forward:
                    nextPositionIndex = positionIndex;
                    break;

                case MoveDirection.Right:
                    nextPositionIndex = (positionIndex + 1) % 4;
                    break;

                case MoveDirection.Backward:
                    nextPositionIndex = (positionIndex + 2) % 4;
                    break;

                case MoveDirection.Left:
                    nextPositionIndex = (positionIndex + 3) % 4;
                    break;
            }

            return _positionsMap[nextPositionIndex];
        }
    }
}
