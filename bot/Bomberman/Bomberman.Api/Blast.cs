using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bomberman.Api.Infrastructure;

namespace Bomberman.Api
{
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
            IsBonus = isBonus;
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
}
