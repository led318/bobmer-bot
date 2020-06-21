using Bomberman.Api.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Api
{
    public class BonusImmune: BonusByTick
    {
        public override bool AreTicksConcatenated => true; 

        public BonusImmune()
        {
            Element = Element.BOMB_IMMUNE;
            TicksLeft = Config.BonusImmuneTimeout;
        }
    }
}
