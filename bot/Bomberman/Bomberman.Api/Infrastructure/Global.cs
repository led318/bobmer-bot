using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters;

namespace Bomberman.Api.Infrastructure
{
    public static class Global
    {
        public static int RoundTickIndex { get; set; }

        public static Board Board { get; set; }
        public static Board PrevBoard { get; set; }
        public static bool HasPrevBoard => PrevBoard != null;

        public static MyBomberman Me { get; set; }
        public static NearPoints NearPoints { get; set; }
        public static OtherBombermans OtherBombermans { get; set; }
        public static Choppers Choppers { get; set; }

        public static bool HasManualMove => !string.IsNullOrEmpty(ManualMove);
        public static string ManualMove { get; set; }

        public static Blasts Blasts { get; set; }
    }
}
