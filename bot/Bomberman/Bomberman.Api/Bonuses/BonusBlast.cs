using Bomberman.Api.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Api
{
    public class BonusBlast : Bonus
    {
        public override Element Element => Element.BOMB_BLAST_RADIUS_INCREASE;

        public override bool IsActive => DurationValue > 0;
        public override bool IsActiveNextTick => DurationValue > 1;

        public int NextTickEffect => IsActiveNextTick ? EffectValue : 0;

        public override void Add()
        {
            EffectValue += Config.BonusBlastEffect;
            DurationValue += Config.BonusBlastDuration;
        }

        public override void Utilize()
        {
            DurationValue--;
        }
    }
}
