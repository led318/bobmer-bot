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
                    result += 10;

                if (IsDestroyableWall)
                    result += 10;

                if (!IsEmpty)
                    result++;

                //if (!Global.Me.IsBonusImmune && IsBlast)
                //    result += 2;

                if (IsBomb)
                    result += 10;

                if (!Global.Me.IsBonusImmune && IsFutureBlast)
                    result += 2;

                if (!Global.Me.IsBonusImmune && IsFutureBlastNextStep)
                    result += 10;

                if (IsNearChopper)
                    result += 3;

                if (IsChopper)
                    result += 10;

                if (IsOtherBomberman)
                    result += 10;

                //if (IsBonusRC)
                //    result += 10; //temp

                if (IsBonus)
                    result -= 2;


                if (NextNearPoint != null)
                {
                    if (!NextNearPoint.IsEmpty)
                        result++;

                    if (NextNearPoint.IsBomb)
                        result += 2;

                    if (NextNearPoint.IsNearChopper)
                        result++;

                    if (NextNearPoint.IsChopper)
                        result += 2;

                    if (!Global.Me.IsBonusImmune && NextNearPoint.IsFutureBlastNextStep)
                        result += 2;

                    if (NextNearPoint.IsOtherBombBomberman)
                        result += 10;

                    if (NextNearPoint.IsBonus)
                        result--;

                    if (IsActCurrentMove && (NextNearPoint.IsWall || NextNearPoint.IsDestroyableWall || NextNearPoint.IsBomb))
                        result += 10;

                    if (NextNearPoint.NextNearPoint != null)
                    {
                        if (IsActCurrentMove && (NextNearPoint.NextNearPoint.IsWall || NextNearPoint.NextNearPoint.IsDestroyableWall || NextNearPoint.NextNearPoint.IsBomb))
                            result += 4;
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
