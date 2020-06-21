using Bomberman.Api.Enums;
using Bomberman.Api.Infrastructure;

namespace Bomberman.Api.Bonuses
{
    public abstract class Bonus : IHasTick
    {
        public abstract Element Element { get; }

        public abstract bool IsActive { get; }
        public abstract bool IsActiveNextTick { get; }

        public int Effect => IsActive ? EffectValue : 0;
        public int Duration => IsActive ? DurationValue : 0;

        protected int EffectValue { get; set; }
        protected int DurationValue { get; set; }

        public Bonus()
        {
        }

        public abstract void Add();
        public abstract void Utilize();

        public virtual void Clear()
        {
            EffectValue = 0;
            DurationValue = 0;
        }

        public virtual void Tick()
        {
            Utilize();
        }
    }
}
