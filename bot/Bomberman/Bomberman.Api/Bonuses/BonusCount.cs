using Bomberman.Api.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Api
{
    public class BonusCount: Bonus
    {
        public override Element Element => Element.BOMB_COUNT_INCREASE;

        public override bool IsActive => DurationValue > 0;
        public override bool IsActiveNextTick => DurationValue > 1;

        public BonusCount()
        {
            Add();
        }

        public override void Add()
        {
            EffectValue = Config.BonusCountEffect;
            DurationValue = Config.BonusCountDuration;
        }

        public override void Utilize()
        {
            DurationValue--;
        }
    }
}
