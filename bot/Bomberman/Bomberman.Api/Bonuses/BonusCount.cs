using Bomberman.Api.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Api
{
    public class BonusCount: BonusByTick
    {
        public BonusCount()
        {
            Element = Element.BOMB_COUNT_INCREASE;
            TicksLeft = Config.BonusCountTimeout;
        }
    }
}
