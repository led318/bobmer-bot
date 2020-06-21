using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Api
{
    public abstract class BonusByUse : Bonus
    {
        public override bool IsActive => UsesLeft > 0;
        public override bool IsActiveNextStep => true;
        public int UsesLeft { get; set; }

        public override void Use()
        {
            if (UsesLeft > 0)
                UsesLeft--;
        }
    }
}
