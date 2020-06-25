using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bomberman.Api.Infrastructure;

namespace Bomberman.Api
{
    public class Blasts
    {
        public List<Blast> AllBlasts { get; set; } = new List<Blast>();
        public List<Blast> NotBonusBlasts => AllBlasts.Where(x => !x.IsBonus).ToList();

        public bool IsFutureBlast(Point point)
        {
            return AllBlasts.Any(x => x.Point.Equals(point));
        }

        public bool IsFutureBlastNextStep(Point point)
        {
            return AllBlasts.Any(x => !x.IsBonus && x.IsNextStep && x.Point.Equals(point));
        }

        public bool IsBonusFutureBlastNextStep(Point point)
        {
            return AllBlasts.Any(x => x.IsEnemyBonusNextStep && x.Point.Equals(point));
        }

        public bool IsBonusRCNextStep(Point point)
        {
            return AllBlasts.Any(x => x.IsRC && x.IsBonus && x.IsEnemy && x.Point.Equals(point));
        }

        public bool IsMyFutureBlastToIgnore(Point point)
        {
            var pointBlasts = AllBlasts.Where(x => x.Point.Equals(point)).ToList();

            return pointBlasts.Count() == 1 && pointBlasts[0].IsMyToIgnore;
        }

        public List<Blast> GetMyBlasts()
        {
            return AllBlasts.Where(x => x.IsMy).ToList();
        }

        public int GetPointMinBlastTicks(Point point)
        {
            var pointBlastsWithTicks = AllBlasts
                .Where(x => x.Point.X == point.X && x.Point.Y == point.Y && x.TicksLeft.HasValue).ToList();

            if (!pointBlastsWithTicks.Any())
                return -1;

            return pointBlastsWithTicks.Min(x => x.TicksLeft.Value);
        }

        public void Init()
        {
            AllBlasts.Clear();

            var myBombs = Global.Me.MyBombs;
            var myBombPoints = myBombs.GetPoints();

            foreach (var myBomb in myBombs)
            {
                var blastPoints = Global.Board.GetFutureBlastsForBombs(myBomb);

                foreach (var blastPoint in blastPoints)
                {
                    var blast = new Blast(myBomb, blastPoint, true, false);

                    AllBlasts.Add(blast);
                }
            }

            var allBombPoints = Global.Board.GetBombs();

            var enemyBombPoints = allBombPoints.Where(x => !myBombPoints.Contains(x)).ToList();

            foreach (var enemyBombPoint in enemyBombPoints)
            {
                var enemyBomb = new Bomb(enemyBombPoint);

                var blastPoints = Global.Board.GetFutureBlastsForBombs(enemyBombPoint);

                foreach (var blastPoint in blastPoints)
                {
                    var blast = new Blast(enemyBomb, blastPoint, false, false);

                    AllBlasts.Add(blast);
                }

                var bonusBombPower = Config.BombsDefaultPower + Config.BonusBlastIncrease;
                var bonusBlastPointsAll = Global.Board.GetFutureBlastsForBombs(enemyBombPoint, bonusBombPower);
                var bonusBlastPoints = bonusBlastPointsAll.Where(x => !blastPoints.Contains(x)).ToList();

                foreach (var bonusBlastPoint in bonusBlastPoints)
                {
                    var blast = new Blast(enemyBomb, bonusBlastPoint, false, true);

                    AllBlasts.Add(blast);
                }
            }
        }
    }
}
