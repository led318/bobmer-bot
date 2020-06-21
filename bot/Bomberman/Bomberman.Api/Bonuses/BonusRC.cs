using Bomberman.Api.Enums;
using Bomberman.Api.Infrastructure;

namespace Bomberman.Api.Bonuses
{
    public class BonusRC: Bonus
    {
        public override Element Element => Element.BOMB_REMOTE_CONTROL;

        public override bool IsActive => EffectValue > 0;
        public override bool IsActiveNextTick => EffectValue > 1;

        public BonusRC()
        {
            Add();
        }

        public override void Add()
        {
            EffectValue = Config.BonusRCEffect;
        }

        public override void Utilize()
        {
            EffectValue--;
        }

        public override void Tick()
        {
            // do nothing, utilize by use action
        }
    }
}
