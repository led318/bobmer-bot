using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters;

namespace Bomberman.Api.Infrastructure
{
    public static class Global
    {
        public static int RoundTickIndex { get; set; }

        public static Board Board { get; set; }
        public static Board PrevBoard { get; set; }
        public static bool HasPrevBoard => PrevBoard != null;

        public static MyBomberman Me { get; set; }
        public static NearPoints NearPoints { get; set; }
        public static OtherBombermans OtherBombermans { get; set; }
        public static Choppers Choppers { get; set; }

        public static bool HasManualMove => !string.IsNullOrEmpty(ManualMove);
        public static string ManualMove { get; set; }

        public static Blasts Blasts { get; set; }
    }


    public class Blast : IHasPoint
    {
        public Point Point { get; set; }
        public Bomb Bomb { get; set; }
        public Element Element => Bomb.Element;
        public Element PrevElement { get; set; }

        public bool IsMy { get; set; }

        public bool IsMyRC => IsRC && IsMy;
        public bool IsMyNextStep => IsNextStep && IsMy;
        public bool IsMyToIgnore => IsMy && !IsRC && TicksLeft >= 2;


        public bool IsEnemy => !IsMy;
        public bool IsEnemyRC => IsRC && IsEnemy;
        public bool IsEnemyNextStep => IsNextStep && IsEnemy;
        public bool IsEnemyBonusNextStep => IsEnemyNextStep && IsBonus;

        public bool IsBonus { get; set; }
        public bool IsRC { get; set; }
        public bool IsNextStep { get; set; }

        public int? TicksLeft { get; set; }

        public Blast(Bomb bomb, Point point, bool isMy, bool isBonus)
        {
            Bomb = bomb;
            Point = point;
            IsMy = isMy;
            IsBonus= isBonus;
            Init();
        }

        public void Init()
        {
            PrevElement = Global.HasPrevBoard ? Global.PrevBoard.GetAt(Point) : Element.DUMMY;

            TicksLeft = GetTicksLeftForElement(Element);
            IsRC = Element == Constants.RC_BOMB_ELEMENT || PrevElement == Constants.RC_BOMB_ELEMENT;
            IsNextStep = Element == Element.BOMB_TIMER_1 || PrevElement == Element.BOMB_TIMER_2;

            if (!TicksLeft.HasValue && Global.HasPrevBoard)
            {
                var prevElement = Global.PrevBoard.GetAt(Point);
                TicksLeft = GetTicksLeftForElement(prevElement);
                if (TicksLeft.HasValue || TicksLeft != 4)
                    TicksLeft -= 1;
            }
        }

        private int? GetTicksLeftForElement(Element element)
        {
            switch (element)
            {
                case Element.BOMB_TIMER_1:
                    return 0;

                case Element.BOMB_TIMER_2:
                    return 1;

                case Element.BOMB_TIMER_3:
                    return 2;

                case Element.BOMB_TIMER_4:
                    return 3;

                case Element.BOMB_TIMER_5:
                    return 4;

            }

            return null;
        }
    }

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
