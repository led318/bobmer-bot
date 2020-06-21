using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Api
{
    public abstract class BonusByTick : Bonus
    {
        public override bool IsActive => TicksLeft > 0;
        public virtual bool IsDisabled => false;
        public override bool IsActiveNextStep => TicksLeft > 1;
        public virtual bool AreTicksConcatenated => false;

        public int TicksLeft { get; set; }


        public override void Tick()
        {
            if (IsActive)
                TicksLeft--;
        }
    }
}
