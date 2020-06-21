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

        public Direction Direction { get; set; }
        public Point Point { get; set; }
        public Element Element { get; set; }

        public NearPoint NextNearPoint { get; set; }
        public bool IsActCurrentMove { get; set; }
        public bool IsEmpty => Constants.MOVABLE_ELEMENTS.Contains(Element);
        public bool IsFutureBlast { get; set; }
        public bool IsFutureBlastNextStep { get; set; }
        public bool IsBomb { get; set; }
        public bool IsNearChopper { get; set; }

        public bool IsBlast => Element == Element.BOOM;
        public bool IsWall => Element == Element.WALL;
        public bool IsDestroyableWall => Element == Element.DESTROYABLE_WALL;
        public bool IsChopper => Element == Element.MEAT_CHOPPER;
        public bool IsOtherBomberman => Constants.OTHER_BOMBERMANS_ELEMENTS.Contains(Element);
        public bool IsOtherBombBomberman => Element == Element.OTHER_BOMB_BOMBERMAN;

        #region Bonus
        public bool IsBonus => Constants.BONUS_ELEMENTS.Contains(Element);
        public bool IsBonusRC => Element == Element.BOMB_REMOTE_CONTROL;
        public bool IsBonusBlast => Element == Element.BOMB_BLAST_RADIUS_INCREASE;
        public bool IsBonusImmune => Element == Element.BOMB_IMMUNE;
        public bool IsBonusCount => Element == Element.BOMB_COUNT_INCREASE;
        #endregion

        public bool IsDanger => Rating > Config.DangerBreakpoint;

        public int Rating 
        {
            get
            {
                var result = 0;

                if (IsWall)
                    result += Config.DangerRatingCritical;

                if (IsDestroyableWall)
                    result += Config.DangerRatingCritical;

                if (!IsEmpty)
                    result += Config.DangerRatingLow;

                if (IsBomb)
                    result += Config.DangerRatingCritical;

                if (!Global.Me.BonusImmune.IsActive && IsFutureBlast)
                    result += Config.DangerRatingMiddle;

                if (!Global.Me.BonusImmune.IsActive && IsFutureBlastNextStep)
                    result += Config.DangerRatingCritical;

                if (IsNearChopper)
                    result += Config.DangerRatingHigh;

                if (IsChopper)
                    result += Config.DangerRatingCritical;

                if (IsOtherBomberman)
                    result += Config.DangerRatingCritical;

                if (IsBonus)
                    result -= Config.DangerRatingMiddle;


                if (NextNearPoint != null)
                {
                    if (!NextNearPoint.IsEmpty)
                        result += Config.DangerRatingLow;

                    if (NextNearPoint.IsBomb)
                        result += Config.DangerRatingMiddle;

                    if (NextNearPoint.IsNearChopper)
                        result += Config.DangerRatingLow;

                    if (NextNearPoint.IsChopper)
                        result += Config.DangerRatingMiddle;

                    if (!Global.Me.BonusImmune.IsActive && NextNearPoint.IsFutureBlastNextStep)
                        result += Config.DangerRatingMiddle;

                    if (NextNearPoint.IsOtherBombBomberman)
                        result += Config.DangerRatingCritical;

                    if (NextNearPoint.IsBonus)
                        result -= Config.DangerRatingLow;

                    if (IsActCurrentMove && (NextNearPoint.IsWall || NextNearPoint.IsDestroyableWall || NextNearPoint.IsBomb))
                        result += Config.DangerRatingCritical;

                    if (NextNearPoint.NextNearPoint != null)
                    {
                        if (IsActCurrentMove && (NextNearPoint.NextNearPoint.IsWall || NextNearPoint.NextNearPoint.IsDestroyableWall || NextNearPoint.NextNearPoint.IsBomb))
                            result += Config.DangerRatingHigh;
                    }
                }

                return result;
            }
        }

        public NearPoint(Direction direction)
        {
            Direction = direction;
        }

        public void Init(Point point, int nestLevel = 0)
        {
            Point = point.GetNewPosition(Direction);
            Element = Global.Board.GetAt(Point);

            IsFutureBlast = Global.Board.GetFutureBlasts().Any(b => b.Equals(Point));
            IsFutureBlastNextStep = Global.Board.GetFutureBlasts(true).Any(b => b.Equals(Point));
            IsBomb = Global.Board.IsAnyOfAt(Point, Constants.BOMB_ELEMENTS);
            IsNearChopper = Global.Board.IsNearChopper(Point);

            if (nestLevel < Config.NearNestLevel)
            {
                NextNearPoint = new NearPoint(Direction);
                NextNearPoint.Init(Point, nestLevel + 1);
            }
        }

        public void InitAct(bool isActCurrentMove)
        {
            IsActCurrentMove = isActCurrentMove;
            if (NextNearPoint != null)
                NextNearPoint.InitAct(isActCurrentMove);
        }

        public override string ToString()
        {
            return $"{Point} {Direction,5}";
        }
    }
}
