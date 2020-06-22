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

        public static void RemovePoint<T>(this ICollection<T> list, Point point)
            where T : IHasPoint
        {
            var itemToRemove = list.FirstOrDefault(x => x.Point.Equals(point));
            if (itemToRemove != null)
                list.Remove(itemToRemove);
        }
    }
}
