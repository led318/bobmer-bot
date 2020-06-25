using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bomberman.Api.Infrastructure;

namespace Bomberman.Api
{
    public class NearPoint : IHasPoint
    {
        private Dictionary<Direction, List<Direction>> _sideDirections = new Dictionary<Direction, List<Direction>>()
        {
            {Direction.Up, new List<Direction> {Direction.Left, Direction.Right}},
            {Direction.Down, new List<Direction> {Direction.Left, Direction.Right}},
            {Direction.Right, new List<Direction> {Direction.Up, Direction.Down}},
            {Direction.Left, new List<Direction> {Direction.Up, Direction.Down}}
        };

        private List<Point> _sidePoints = new List<Point>();

        public Direction Direction { get; set; }
        public Point Point { get; set; }
        public Element Element { get; set; }

        public NearPoint NextNearPoint { get; set; }
        //public bool IsActCurrentMove { get; set; }
        public bool IsEmpty => Constants.MOVABLE_ELEMENTS.Contains(Element);
        public bool IsFutureBlast { get; set; }
        public bool IsFutureBlastNextStep { get; set; }
        public bool IsMyFutureBlastToIgnore { get; set; }

        private int _bonusBombPower => Config.BombsDefaultPower + Config.BonusBlastIncrease;
        public bool IsBonusFutureBlastNextStep { get; set; }
        public bool IsBonusRCBlastNextStep { get; set; }
        public bool IsBomb { get; set; }

        #region Near Chopper
        public Chopper NearChopper { get; set; }
        public int NearChopperPossibility => !IsNearChopper ? 0 : NearChopper.GetPossibility(Point);
        public bool IsNearChopper => NearChopper != null;

        public int GetNearChopperDangerPoints(bool isNextNearPoint = false, bool isNextNextNearPoint = false)
        {
            if (NearChopperPossibility == 0)
                return 0;

            if (NearChopperPossibility > 60)
                return isNextNearPoint 
                    ? isNextNextNearPoint
                        ? Config.DangerRatingMedium
                        : Config.DangerRatingHigh 
                    : Config.DangerRatingCritical;

            if (NearChopperPossibility > 20)
                return isNextNearPoint 
                    ? isNextNextNearPoint
                        ? Config.DangerRatingLow
                        : Config.DangerRatingMedium 
                    : Config.DangerRatingHigh;

            return isNextNearPoint ? Config.DangerRatingLow : Config.DangerRatingMedium;
        }

        #endregion

        public bool IsBlast => Element == Element.BOOM;
        public bool IsWall => Element == Element.WALL;
        public bool IsDestroyableWall => Element == Element.DESTROYABLE_WALL;
        public bool IsDestroyedWall => Element == Element.DestroyedWall;

        public bool IsChopper => Element == Element.MEAT_CHOPPER;
        public bool IsBombChopper => IsChopper && Global.HasPrevBoard && Global.PrevBoard.IsAnyOfAt(Point, Constants.BOMB_ELEMENTS);

        public bool IsZombieChopper => Element == Element.DeadMeatChopper;
        public bool IsNearZombieChopper => Global.Board.IsNear(Point, Element.DeadMeatChopper);

        public bool IsOtherBomberman => Constants.OTHER_BOMBERMANS_ELEMENTS.Contains(Element);
        public bool IsOtherBombBomberman => Element == Element.OTHER_BOMB_BOMBERMAN;

        #region Bonus
        public bool IsBonus => Constants.BONUS_ELEMENTS.Contains(Element);
        public bool IsBonusRC => Element == Element.BOMB_REMOTE_CONTROL;
        public bool IsBonusBlast => Element == Element.BOMB_BLAST_RADIUS_INCREASE;
        public bool IsBonusImmune => Element == Element.BOMB_IMMUNE;
        public bool IsBonusCount => Element == Element.BOMB_COUNT_INCREASE;
        #endregion

        public int DestroyableWallsCount => Helper.GetDestroyableWallsCount(Point);
        public bool HaveMoreDestroyableWalls => DestroyableWallsCount > Global.Me.DestroyableWallsCount;

        public bool IsDanger => Rating > Config.DangerBreakpoint;

        public bool IsDangerForActThenMove
        {
            get
            {
                var nextPoint = NextNearPoint != null
                                                     && (NextNearPoint.IsWall
                                                         || NextNearPoint.IsDestroyableWall
                                                         || NextNearPoint.IsBomb
                                                         || NextNearPoint.IsOtherBomberman
                                                         || NextNearPoint.IsOtherBombBomberman
                                                         || NextNearPoint.IsBombChopper);

                return nextPoint;
            }
        }

        public bool IsLessDangerForActThenMove
        {
            get
            {
                var nextNextPoint = NextNearPoint != null
                                    && NextNearPoint.NextNearPoint != null
                                    && (NextNearPoint.NextNearPoint.IsWall
                                        || NextNearPoint.NextNearPoint.IsDestroyableWall
                                        || NextNearPoint.NextNearPoint.IsBomb
                                        || NextNearPoint.NextNearPoint.IsOtherBomberman
                                        || NextNearPoint.NextNearPoint.IsOtherBombBomberman
                                        || NextNearPoint.NextNearPoint.IsBombChopper);

                return nextNextPoint;
            }
        }

        public bool IsSafeForActThenMove
        {
            get
            {
                var thisPoint = HaveSideToEscape();
                var nextPoint = NextNearPoint != null && NextNearPoint.HaveSideToEscape();
                var nextNextPoint = NextNearPoint != null
                                    && NextNearPoint.NextNearPoint != null
                                    && NextNearPoint.NextNearPoint.HaveSideToEscape();

                return thisPoint || nextPoint; //|| nextNextPoint;
            }
        }

        public bool IsMoreSafeForActThenMove
        {
            get
            {
                var nextNextPoint = NextNearPoint != null
                                    && NextNearPoint.NextNearPoint != null
                                    && NextNearPoint.NextNearPoint.HaveSideToEscape();

                return IsSafeForActThenMove && nextNextPoint;
            }
        }

        #region Target

        public bool HaveDirectAfkTarget => Global.Me.HaveDirectAfkTarget(Point);

        #endregion

        public bool IsCriticalDanger
        {
            get
            {
                if (IsWall)
                    return true;

                if (IsDestroyableWall)
                    return true;

                if (IsBomb)
                    return true;

                if (IsZombieChopper)
                    return true;

                if (IsNearZombieChopper)
                    return true;

                if (!Global.Me.IsBonusImmune && IsFutureBlastNextStep)
                    return true;

                if (Global.Me.IsOnBomb && IsDangerForActThenMove && !IsSafeForActThenMove)
                    return true;

                if (Global.Me.IsOnBomb && IsLessDangerForActThenMove && !IsSafeForActThenMove)
                    return true;

                if (IsChopper)
                    return true;

                //if (IsOtherBomberman)
                //    return true;

                if (NearChopperPossibility > 60)
                    return true;

                if (NextNearPoint != null)
                {
                    if (NextNearPoint.IsOtherBombBomberman)
                        return true;

                    if (NextNearPoint.IsNearZombieChopper)
                        return true;
                }

                return false;
            }
        }

        public int Rating
        {
            get
            {
                var result = 0;

                if (IsWall)
                {
                    result += Config.DangerRatingCritical;
                }

                if (IsDestroyableWall)
                {
                    result += Config.DangerRatingCritical;
                }

                if (!IsEmpty)
                    result += Config.DangerRatingLow;

                if (IsBomb)
                {
                    result += Config.DangerRatingCritical;
                }

                if (IsZombieChopper)
                {
                    result += Config.DangerRatingCritical;
                    //Console.WriteLine("ZOMBIE!!! next");
                }

                if (IsNearZombieChopper)
                {
                    result += Config.DangerRatingCritical;
                    // Console.WriteLine("NEAR ZOMBIE!!! next");
                }

                if (IsDestroyedWall)
                {
                    result += Config.DangerRatingMedium;
                }


                if (!Global.Me.IsBonusImmune && IsFutureBlast && !IsMyFutureBlastToIgnore)
                    result += Config.DangerRatingMedium;

                if (!Global.Me.IsBonusImmune && IsFutureBlastNextStep)
                {
                    result += Config.DangerRatingCritical;
                }

                if (!Global.Me.IsBonusImmune && IsBonusFutureBlastNextStep)
                    result += Config.DangerRatingMedium;

                if (!Global.Me.IsBonusImmune && IsBonusRCBlastNextStep)
                    result += (Config.DangerRatingHigh + Config.DangerRatingMedium);

                if (IsNearChopper)
                    result += GetNearChopperDangerPoints();

                if (IsChopper)
                {
                    result += Config.DangerRatingCritical;
                }

                if (IsOtherBomberman)
                {
                    result += Config.DangerRatingCritical;
                }

                if (IsBonus)
                    result -= Config.DangerRatingHigh;

                /*
                if (IsActCurrentMove && HasSideToEscape())
                    result -= (Config.DangerRatingHigh + Config.DangerRatingMedium);
                */

                //if (!Global.Me.HaveDirectAfkTargetCurrentStep
                //    && Global.Me.HaveDirectAfkTarget(Point))
                //{
                //    result -= Config.DangerRatingHigh;
                //}



                //if (Global.Me.IsOnBomb && IsDangerForActThenMove && !IsSafeForActThenMove)
                //    result += Config.DangerRatingCritical;

                //todo: maybe enable this if have issues with dead ends
                if (IsDangerForActThenMove)
                    result += Config.DangerRatingHigh;

                if (IsLessDangerForActThenMove)
                    result += Config.DangerRatingMedium;

                if (IsDangerForActThenMove && IsSafeForActThenMove)
                    result -= (Config.DangerRatingMedium + Config.DangerRatingLow);

                //if (IsLessDangerForActThenMove && IsSafeForActThenMove)
                //    result -= Config.DangerRatingLow;

                //if (Helper.HaveDirectAfkTarget(Point))
                //    result -= Config.DangerRatingMedium;

                //if (Helper.HaveNearAfkTarget(Point))
                //    result -= Config.DangerRatingMedium;

                if (NextNearPoint != null)
                {
                    if (!NextNearPoint.IsEmpty)
                        result += Config.DangerRatingLow;

                    if (NextNearPoint.IsBomb)
                        result += Config.DangerRatingMedium;

                    if (NextNearPoint.IsNearChopper)
                        result += GetNearChopperDangerPoints(true);

                    if (NextNearPoint.IsChopper)
                        result += Config.DangerRatingHigh;

                    if (!Global.Me.IsBonusImmune && NextNearPoint.IsFutureBlast)
                        result += Config.DangerRatingMedium;

                    if (!Global.Me.IsBonusImmune && NextNearPoint.IsFutureBlastNextStep)
                        result += Config.DangerRatingMedium;

                    if (!Global.Me.IsBonusImmune && NextNearPoint.IsBonusRCBlastNextStep)
                        result += Config.DangerRatingMedium;

                    if (NextNearPoint.IsOtherBombBomberman)
                    {
                        result += Config.DangerRatingCritical;
                    }

                    if (NextNearPoint.IsZombieChopper)
                    {
                        result += Config.DangerRatingHigh;
                        // Console.WriteLine("ZOMBIE!!! +1");
                    }

                    if (NextNearPoint.IsNearZombieChopper)
                    {
                        result += Config.DangerRatingCritical;
                        // Console.WriteLine("NEAR ZOMBIE!!! +1");
                    }

                    if (NextNearPoint.IsDestroyedWall)
                    {
                        result += Config.DangerRatingLow;
                    }

                    if (NextNearPoint.IsBonus)
                        result -= Config.DangerRatingMedium;

                    if (NextNearPoint.IsBomb)
                        result += Config.DangerRatingMedium;

                    //if (Helper.HaveDirectAfkTarget(NextNearPoint.Point))
                    //    result -= Config.DangerRatingLow;

                    //if (Helper.HaveNearAfkTarget(NextNearPoint.Point))
                    //    result -= Config.DangerRatingLow;

                    //if (IsActCurrentMove && NextNearPoint.HasSideToEscape())
                    //    result -= Config.DangerRatingHigh;

                    /*
                    if (IsActCurrentMove && (NextNearPoint.IsWall
                                             || NextNearPoint.IsDestroyableWall
                                             || NextNearPoint.IsBomb
                                             || NextNearPoint.IsOtherBomberman
                                             || NextNearPoint.IsOtherBombBomberman
                                             || NextNearPoint.IsBombChopper))
                    {
                        result += Config.DangerRatingCritical;
                    }
                    */

                    if (NextNearPoint.NextNearPoint != null)
                    {
                        if (NextNearPoint.NextNearPoint.IsBomb)
                            result += Config.DangerRatingLow;

                        /*
                        if (IsActCurrentMove && (NextNearPoint.NextNearPoint.IsWall
                                                 || NextNearPoint.NextNearPoint.IsDestroyableWall
                                                 || NextNearPoint.NextNearPoint.IsBomb
                                                 || NextNearPoint.NextNearPoint.IsOtherBomberman
                                                 || NextNearPoint.NextNearPoint.IsOtherBombBomberman
                                                 || NextNearPoint.NextNearPoint.IsBombChopper))
                            result += Config.DangerRatingHigh;
                        */

                        /*
                        if (IsActCurrentMove && NextNearPoint.NextNearPoint.HasSideToEscape())
                            result -= Config.DangerRatingHigh;
                        */

                        //if (NextNearPoint.NextNearPoint.IsBonus)
                        //    result -= Config.DangerRatingLow;

                        if (NextNearPoint.NextNearPoint.IsZombieChopper)
                        {
                            result += Config.DangerRatingHigh;
                            // Console.WriteLine("ZOMBIE!!! +2");
                        }

                        if (NextNearPoint.NextNearPoint.IsNearZombieChopper)
                        {
                            result += Config.DangerRatingHigh;
                            //Console.WriteLine("NEAR ZOMBIE!!! +2");
                        }

                        if (NextNearPoint.NextNearPoint.IsNearChopper)
                        {
                            result += GetNearChopperDangerPoints(true, true);
                        }

                        if (NextNearPoint.NextNearPoint.IsChopper)
                            result += Config.DangerRatingMedium;

                        if (NextNearPoint.NextNearPoint.IsDestroyedWall)
                        {
                            result += Config.DangerRatingLow;
                        }

                        if (!Global.Me.IsBonusImmune && NextNearPoint.NextNearPoint.IsFutureBlast)
                        {
                            result += Config.DangerRatingMedium;
                        }
                    }
                }

                return result;
            }
        }

        private bool HaveSideToEscape()
        {
            return IsEmpty && _sidePoints.Any(x => Global.Board.IsAnyOfAt(x, Constants.MOVABLE_ELEMENTS));
        }

        public NearPoint(Direction direction)
        {
            Direction = direction;
        }

        public void Init(Point point, int nestLevel = 0)
        {
            Point = point.GetNewPosition(Direction);
            Element = Global.Board.GetAt(Point);

            IsFutureBlast = Global.Blasts.IsFutureBlast(Point);
            IsFutureBlastNextStep = Global.Blasts.IsFutureBlastNextStep(Point);
            IsBonusFutureBlastNextStep = Global.Blasts.IsBonusFutureBlastNextStep(Point);
            IsMyFutureBlastToIgnore = Global.Blasts.IsMyFutureBlastToIgnore(Point);

            //IsFutureBlast = Global.Board.GetFutureBlasts().Any(b => b.Equals(Point));
            //IsFutureBlastNextStep = Global.Board.GetFutureBlasts(true).Any(b => b.Equals(Point));
            //IsBonusFutureBlastNextStep = Global.Board.GetFutureBlasts(true, _bonusBombPower).Any(b => b.Equals(Point));
            /*
            var rcBombs = Global.Board.Get(Element.BOMB_TIMER_5);
            if (Global.HasPrevBoard)
            {
                var prevRCBombs = Global.PrevBoard.Get(Element.BOMB_TIMER_5);
                var notPresentPrevRCBombs = prevRCBombs.Where(x => !rcBombs.Contains(x)).ToList();

                foreach (var notPresentPrevRCBomb in notPresentPrevRCBombs)
                {
                    var currentElement = Global.Board.GetAt(notPresentPrevRCBomb);
                    if (currentElement == Element.MEAT_CHOPPER)
                    {
                        rcBombs.Add(notPresentPrevRCBomb);
                    }
                }
            }

            rcBombs = rcBombs.Where(x => !Global.Me.MyRCBombs.GetPoints().Contains(x)).ToList();
*/
            //IsBonusRCBlastNextStep = Global.Board.GetFutureBlastsForBombs(rcBombs, _bonusBombPower).Any(b => b.Equals(Point));
            IsBonusRCBlastNextStep = Global.Blasts.IsBonusRCNextStep(Point);


            IsBomb = Global.Board.IsAnyOfAt(Point, Constants.BOMB_ELEMENTS) || IsBombChopper;
            //IsNearChopper = Global.Board.IsNearChopper(Point);
            NearChopper = Global.Choppers.Get(Point);

            InitSidePoints();



            if (nestLevel < Config.NearNestLevel)
            {
                NextNearPoint = new NearPoint(Direction);
                NextNearPoint.Init(Point, nestLevel + 1);
            }
        }

        private void InitSidePoints()
        {
            _sidePoints.Clear();
            var sideDirections = _sideDirections[Direction];
            foreach (var sideDirection in sideDirections)
            {
                var sidePoint = new Point();

                switch (sideDirection)
                {
                    case Direction.Up:
                        sidePoint = Point.ShiftTop();
                        break;

                    case Direction.Right:
                        sidePoint = Point.ShiftRight();
                        break;

                    case Direction.Down:
                        sidePoint = Point.ShiftBottom();
                        break;

                    case Direction.Left:
                        sidePoint = Point.ShiftLeft();
                        break;
                }

                _sidePoints.Add(sidePoint);
            }
        }



        /*
        public void InitAct(bool isActCurrentMove)
        {
            IsActCurrentMove = isActCurrentMove;
            if (NextNearPoint != null)
                NextNearPoint.InitAct(isActCurrentMove);
        }
        */

        public override string ToString()
        {
            return $"{Point} {Direction,5}";
        }
    }
}
