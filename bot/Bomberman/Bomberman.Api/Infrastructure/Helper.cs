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
    }
}
