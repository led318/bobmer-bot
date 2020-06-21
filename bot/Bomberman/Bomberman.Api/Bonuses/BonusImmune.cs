using Bomberman.Api.Enums;
using Bomberman.Api.Infrastructure;

namespace Bomberman.Api.Bonuses
{
    public class BonusImmune: Bonus
    {
        public override Element Element => Element.BOMB_IMMUNE;

        public override bool IsActive => DurationValue > 0;
        public override bool IsActiveNextTick => DurationValue > 1;

        public BonusImmune()
        {
            Add();
        }

        public override void Add()
        {
            DurationValue = Config.BonusImmuneDuration;
        }

        public override void Utilize()
        {
            DurationValue--;
        }
    }
}
