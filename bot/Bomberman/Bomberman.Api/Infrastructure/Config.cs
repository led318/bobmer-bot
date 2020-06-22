using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Configuration;

namespace Bomberman.Api.Infrastructure
{
    public static class Config
    {
        [AutoPopulateConfig]
        public static readonly string ConnectionString;
        [AutoPopulateConfig]
        public static readonly string ConnectionStringLocal;
        [AutoPopulateConfig]
        public static readonly bool IsLocal;
        [AutoPopulateConfig]
        public static readonly bool IsFile;
        [AutoPopulateConfig]
        public static readonly bool PrintBoard;

        [AutoPopulateConfig]
        public static readonly int BoardSize;

        [AutoPopulateConfig]
        public static readonly int NearNestLevel;
        [AutoPopulateConfig]
        public static bool ManualSuicide;
        [AutoPopulateConfig]
        public static bool IsSuicideEnabled;
        [AutoPopulateConfig]
        public static readonly int SuicideBreakpoint;
        [AutoPopulateConfig]
        public static readonly int ForceSuicideBonus;

        public static int ForceSuicideBreakpoint => ManualSuicide ? 0 : SuicideBreakpoint + ForceSuicideBonus;

        [AutoPopulateConfig]
        public static readonly int EnemiesDetectionBreakpoint;
        [AutoPopulateConfig]
        public static readonly int BombsDefaultCount;
        [AutoPopulateConfig]
        public static readonly int BombsDefaultPower;

        [AutoPopulateConfig]
        public static readonly int BonusBlastEffect;
        [AutoPopulateConfig]
        public static readonly int BonusBlastDuration; // actually 10 but set to 15 to handle 10+ steps when bomb was placed near 10
        [AutoPopulateConfig]
        public static readonly int BonusBlastDisableDuration; // actually 10 but set to 15 to handle 10+ steps when bomb was placed near 10
        [AutoPopulateConfig]
        public static readonly int BonusCountEffect;
        [AutoPopulateConfig]
        public static readonly int BonusCountDuration; // actually 10 but set to 11 not to lose first step
        [AutoPopulateConfig]
        public static readonly int BonusImmuneDuration; // actually 10 but set to 9 to begin escaping earlier
        [AutoPopulateConfig]
        public static readonly int BonusRCEffect; // actually 3 but exist issues when last bomb is stack
        [AutoPopulateConfig]
        public static readonly int AfkBreakpoint;
        [AutoPopulateConfig]
        public static readonly int DangerBreakpoint;
        [AutoPopulateConfig]
        public static readonly int BombPlaceTimeout;

        [AutoPopulateConfig]
        public static readonly int DangerRatingLow;
        [AutoPopulateConfig]
        public static readonly int DangerRatingMiddle;
        [AutoPopulateConfig]
        public static readonly int DangerRatingHigh;
        [AutoPopulateConfig]
        public static readonly int DangerRatingCritical;

        [AutoPopulateConfig]
        public static readonly int RoundLength;

        static Config()
        {
            var fields = typeof(Config).GetFields()
                .Where(p => p.GetCustomAttributes(typeof(AutoPopulateConfigAttribute), false).Any())
                .ToList();

            foreach (var field in fields)
            {
                var value = ConfigurationSettings.AppSettings[field.Name];
                if (string.IsNullOrWhiteSpace(value))
                {
                    Console.WriteLine("empty config: " + field.Name);

                }
                else 
                {
                    if (field.FieldType == typeof(string))
                        field.SetValue(null, value);

                    if (field.FieldType == typeof(int))
                        field.SetValue(null, int.Parse(value));

                    if (field.FieldType == typeof(bool))
                        field.SetValue(null, bool.Parse(value));

                }

            }
        }

    }
}