using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Api.Infrastructure
{
    public static class Config
    {
        public static readonly int NearNestLevel = 3;

        public static bool ManualSuicide = false;
        public static readonly int SuicideBreakpoint = 50;
        public static readonly int ForceSuicideBreakpoint = ManualSuicide ? 0 : SuicideBreakpoint + 30;

        public static readonly int EnemiesDetectionBreakpoint = 2;

        public static readonly int BombsDefaultCount = 1;
        public static readonly int BombsDefaultPower = 3;

        public static readonly int BonusBlastIncrease = 2;
        public static readonly int BonusBlastTimeout = 15; // actually 10 but set to 15 to handle 10+ steps when bomb was placed near 10
        public static readonly int BonusBlastDisableTimeout = 5; // actually 10 but set to 15 to handle 10+ steps when bomb was placed near 10
        public static readonly int BonusCountIncrease = 3;
        public static readonly int BonusCountTimeout = 11; // actually 10 but set to 11 not to lose first step
        public static readonly int BonusImmuneTimeout = 9; // actually 10 but set to 9 to begin escaping earlier
        public static readonly int BonusRCCount = 5; // actually 3 but exist issues when last bomb is stack

        public static readonly int AfkBreakpoint = 5;

        public static readonly int DangerBreakpoint = 10;

        public static readonly int BombPlaceTimeout = 1;


    }
}
