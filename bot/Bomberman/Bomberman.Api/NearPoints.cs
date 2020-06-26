using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bomberman.Api.Infrastructure;

namespace Bomberman.Api
{
    public class NearPoints
    {
        public readonly NearPoint[] Points = new NearPoint[4] {
            new NearPoint(Direction.Up),
            new NearPoint(Direction.Right),
            new NearPoint(Direction.Down),
            new NearPoint(Direction.Left)
        };

        public bool HaveMoreDestroyableWallsNear => Points.Any(x => x.HaveMoreDestroyableWalls);
        
        public bool OnlyCriticalDangerPoints => Points.All(x => x.IsCriticalDanger);

        public List<NearPoint> NotCriticalPoints => Points.Where(x => !x.IsCriticalDanger).ToList();

        public IEnumerable<NearPoint> GetMinRatingNonCriticalPoints()
        {
            //var minRating1 = Points.Min(x => x.Rating);
            //return Points.Where(x => x.Rating == minRating1).ToList();

            if (OnlyCriticalDangerPoints)
                return new List<NearPoint>();

            var minRating = NotCriticalPoints.Min(x => x.Rating);
            return NotCriticalPoints.Where(x => x.Rating == minRating).ToList();
        }

        public void Init()
        {
            foreach (var nearPoint in Points)
                nearPoint.Init(Global.Me.Point);
        }

        private string RatingStr(NearPoint nearPoint)
        {
            if (nearPoint.IsCriticalDanger)
                return $"*{nearPoint.Rating,2}*";

            if (nearPoint.IsMyFutureBlastToIgnore)
                return $"^{nearPoint.Rating,2}^";

            return $" {nearPoint.Rating,2} ";
        }

        public string GetPrintStr()
        {
            var str = $@"+----------------------+
|         {RatingStr(Points[0].NextNearPoint),2}         |
|         {RatingStr(Points[0]),2}         |
|{RatingStr(Points[3].NextNearPoint),2} {RatingStr(Points[3]),2}    {RatingStr(Points[1]),2} {RatingStr(Points[1].NextNearPoint),2}|
|         {RatingStr(Points[2]),2}         |
|         {RatingStr(Points[2].NextNearPoint),2}         |
+----------------------+";

            return str;
        }
    }
}
