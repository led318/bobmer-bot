using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Api
{
    public class Bomberman : IHasPoint
    {
        public Point Point { get; set; }

        public BonusBlast BonusBlast { get; private set; } = new BonusBlast();
        public BonusCount BonusCount { get; private set; } = new BonusCount();
        public BonusImmune BonusImmune { get; private set; } = new BonusImmune();
        public BonusRC BonusRC { get; private set; } = new BonusRC();        

        public void ClearBonuses()
        {
            BonusBlast.Clear();
            BonusCount.Clear();
            BonusImmune.Clear();
            BonusRC.Clear();
        }

        public void TickBonuses()
        {
            BonusBlast.Tick();
            BonusCount.Tick();
            BonusImmune.Tick();
            BonusRC.Tick();
        }

        public void ProcessBonuses(NearPoint nearPoint)
        {
            if (nearPoint != null)
            {
                if (nearPoint.IsBonusRC)
                    BonusRC.Add();

                if (nearPoint.IsBonusBlast)
                    BonusBlast.Add();

                if (nearPoint.IsBonusImmune)
                    BonusImmune.Add();

                if (nearPoint.IsBonusCount)
                    BonusCount.Add();
            }
        }
    }
}
