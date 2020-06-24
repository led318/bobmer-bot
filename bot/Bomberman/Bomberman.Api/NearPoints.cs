using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public bool OnlyCriticalDangerPoints => Points.All(x => x.IsCriticalDanger);

        public void InitActNearPoints(bool isActCurrentMove)
        {
            foreach (var nearPoint in Points)
                nearPoint.InitAct(isActCurrentMove);
        }

        public IEnumerable<NearPoint> GetMinRatingPoints()
        {
            var minRating = Points.Min(x => x.Rating);
            return Points.Where(x => x.Rating == minRating).ToList();
        }

        public void Init()
        {
            foreach (var nearPoint in Points)
                nearPoint.Init(Infrastructure.Global.Me.Point);
        }

        private string RatingStr(NearPoint nearPoint)
        {
            if (nearPoint.IsCriticalDanger)
                return $"*{nearPoint.Rating,2}*";

            if (nearPoint.IsMyFutureBlastToIgnore)
                return $"-{nearPoint.Rating,2}-";

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
