using Bomberman.Api.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Api
{
    public static class Extensions
    {
        public static int IndexOf<T>(this Array array, T elem)
        {
            return Array.IndexOf(array, elem);
        }

        public static List<Point> GetPoints(this IEnumerable<IHasPoint> array)
        {
            return array.Select(x => x.Point).ToList();
        }

        public static bool ContainsPoint(this IEnumerable<IHasPoint> array, IHasPoint hasPoint)
        {
            return array.ContainsPoint(hasPoint.Point);
        }

        public static bool ContainsPoint(this IEnumerable<IHasPoint> array, Point point)
        {
            return array.Any(x => x.Equals(point));
        }

        public static void RemovePoint<T>(this ICollection<T> list, Point point)
            where T: IHasPoint
        {
            var itemToRemove = list.FirstOrDefault(x => x.Point.Equals(point));
            if (itemToRemove != null)
                list.Remove(itemToRemove);
        }

        public static void ForEach<T>(this IEnumerable<T> array, Action<T> func)
        {
            foreach (var item in array)
                func(item);
        }

        public static void Tick(this IEnumerable<IHasTick> array)
        {
            foreach (var item in array)
                item.Tick();
        }
    }
}
