using Bomberman.Api.Enums;
using Bomberman.Api.Infrastructure;

namespace Bomberman.Api.Bonuses
{
    public class BonusCount: Bonus
    {
        public override Element Element => Element.BOMB_COUNT_INCREASE;

        public override bool IsActive => DurationValue > 0 && EffectValue > 0;
        public override bool IsActiveNextTick => DurationValue > 1 && EffectValue > 1;

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
            EffectValue--;
        }
    }
}
