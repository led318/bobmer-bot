using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bomberman.Api.Infrastructure;

namespace Bomberman.Api
{
    public static class Helper
    {
        public static List<Point> GetNearest(Point startPoint, IEnumerable<Point> targetPoints)
        {
            var convertedList = targetPoints.Select(p => new HasPoint(p));

            var result = GetNearest(startPoint, convertedList);
            return result.Select(p => p.Point).ToList();
        }

        public static List<T> GetNearest<T>(Point startPoint, IEnumerable<T> targetPoints)
            where T : IHasPoint
        {
            var minDistToTarget = targetPoints.Min(r => CalculateDistantion(r.Point, startPoint));
            var result = targetPoints.Where(r => CalculateDistantion(r.Point, startPoint) == minDistToTarget).ToList();

            return result;
        }

        private static double CalculateDistantion(Point point1, Point point2)
        {
            var deltaX = Math.Abs(point1.X - point2.X);
            var deltaY = Math.Abs(point1.Y - point2.Y);

            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        public static int GetDestroyableWallsCount(Point point)
        {
            var result = new List<Point>();

            var blastSize = Global.Me.MyBombsForSearchPower;

            CheckIfIsDestroyableWall(result, point, blastSize, p => p.ShiftTop());
            CheckIfIsDestroyableWall(result, point, blastSize, p => p.ShiftRight());
            CheckIfIsDestroyableWall(result, point, blastSize, p => p.ShiftBottom());
            CheckIfIsDestroyableWall(result, point, blastSize, p => p.ShiftLeft());

            return result.Count();
        }

        private static void CheckIfIsDestroyableWall(List<Point> result, Point point, int blastSize, Func<Point, Point> func)
        {
            var pointToCheck = point;

            for (var i = 0; i < blastSize; i++)
            {
                pointToCheck = func(pointToCheck);
                if (Global.Board.IsAt(pointToCheck, Element.WALL))
                    return;

                if (Global.Board.IsAt(pointToCheck, Element.DESTROYABLE_WALL))
                    result.Add(pointToCheck);
            }
        }

        public static bool HaveDirectAfkTarget(Point point)
        {
            var result = new List<Point>();
            var searchPoint = Global.OtherBombermans.Target;
            var blastSize = Global.Me.MyBombsForSearchPower;

            var startPoint = point;

            CheckDirectAfkTarget(result, startPoint, searchPoint, blastSize, p => p.ShiftTop());
            CheckDirectAfkTarget(result, startPoint, searchPoint, blastSize, p => p.ShiftRight());
            CheckDirectAfkTarget(result, startPoint, searchPoint, blastSize, p => p.ShiftBottom());
            CheckDirectAfkTarget(result, startPoint, searchPoint, blastSize, p => p.ShiftLeft());

            return result.Any();
        }

        public static bool HaveNearAfkTarget(Point startPoint)
        {
            var searchPoint = Global.OtherBombermans.Target;

            return startPoint.IsNear(searchPoint);
        }

        private static void CheckDirectAfkTarget(List<Point> result, Point startPoint, Point searchPoint, int blastSize, Func<Point, Point> func)
        {
            var pointToCheck = startPoint;

            for (var i = 0; i < blastSize; i++)
            {
                pointToCheck = func(pointToCheck);

                if (Global.Board.IsAnyOfAt(pointToCheck, Constants.WALL_ELEMENTS))
                    return;

                if (pointToCheck.Equals(searchPoint))
                    result.Add(pointToCheck);
            }
        }
    }
}
