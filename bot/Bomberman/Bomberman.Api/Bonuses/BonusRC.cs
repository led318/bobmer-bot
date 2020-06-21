using Bomberman.Api.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Api
{
    public class BonusRC: BonusByUse
    {
        public BonusRC()
        {
            Element = Element.BOMB_REMOTE_CONTROL;
            UsesLeft = Config.BonusRCCount;
        }
    }
}
