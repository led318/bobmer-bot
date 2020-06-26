using Bomberman.Api.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Api
{
    public class BonusBlast : BonusByTick
    {
        public override bool IsDisabled => TicksLeft < Config.BonusBlastDisableTimeout;
        public override bool AreTicksConcatenated => true;

        public BonusBlast()
        {
            Element = Element.BOMB_BLAST_RADIUS_INCREASE;
            TicksLeft = Config.BonusBlastTimeout;
        }
    }
}
