using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bomberman.Api.Enums;

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

        public bool OnlyDangerPoints => Points.All(x => x.IsDanger);

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

        public string GetPrintStr()
        {
            var str = $@"+--------------+
|      {Points[0].NextNearPoint.Rating,2}      |
|      {Points[0].Rating,2}      |
|{Points[3].NextNearPoint.Rating,2} {Points[3].Rating,2}    {Points[1].Rating,2} {Points[1].NextNearPoint.Rating,2}|
|      {Points[2].Rating,2}      |
|      {Points[2].NextNearPoint.Rating,2}      |
+--------------+
";

            return str;
        }
    }
}
