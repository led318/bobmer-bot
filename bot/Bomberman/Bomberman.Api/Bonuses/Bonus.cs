using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Api
{
    public abstract class Bonus
    {
        public Element Element { get; protected set; }

        public abstract bool IsActive { get; }
        public abstract bool IsActiveNextStep { get; }

        //public abstract void Activate();
        public virtual void Tick() { }
        public virtual void Use() { }
    }
}
