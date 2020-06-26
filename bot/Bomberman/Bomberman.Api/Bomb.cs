using Bomberman.Api.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Api
{
    public class Bomb : IHasPoint
    {
        public Point Point { get; set; }
        public Element Element { get; set; }
        public int Power { get; set; }

        public bool IsRc
        {
            get
            {
                var result = Element == Constants.RC_BOMB_ELEMENT;

                if (!result && Global.HasPrevBoard)
                {
                    result = Global.PrevBoard.IsAnyOfAt(Point, Constants.RC_BOMB_ELEMENT);
                }

                return result;
            }
        }

        public bool IsNew => Element == Element.BOMB_BOMBERMAN;

        public Bomb(Point point, bool isMyBomb = false)
        {
            Point = point;
            Power = isMyBomb ? Global.Me.MyBombsPower : Config.BombsDefaultPower;
            Init();
        }

        public void Init()
        {
            if (Global.Board != null)
                Element = Global.Board.GetAt(Point);
        }
    }
}
