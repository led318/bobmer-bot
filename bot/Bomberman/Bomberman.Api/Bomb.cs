using Bomberman.Api.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Api
{
    public class Bomb : IHasPoint, IHasTick, IHasElement
    {
        public Point Point { get; set; }
        public Element Element { get; set; }
        public int Power { get; set; }
        public int Timer { get; set; }
        public bool IsKamikadze { get; set; } //todo
        public bool IsRC { get; set; } //todo
        public bool IsBlastNextTick => Timer == 1;
        
        [Obsolete]
        public bool IsRc => Element == Constants.RC_BOMB_ELEMENT;
        public bool IsNew => Element == Element.BOMB_BOMBERMAN;

        public Bomb(Point point, bool isMyBomb = false)
        {
            Point = point;
            Power = isMyBomb ? Global.Me.MyBombsPower : Config.BombsDefaultPower;
            Init();
        }

        public void Init()
        {
            Element = Global.Board.GetAt(Point);
        }

        public void Tick()
        {
            Timer--;
        }
    }
}
