using System.Collections.Generic;

namespace Bomberman.Api.Infrastructure
{
    public static class Global
    {
        public static int RoundTick { get; set; }
        public static List<Board> RoundBoards { get; set; }

        public static Board Board => RoundBoards[RoundTick];
        public static Board PrevTickBoard => RoundTick > 0 ? RoundBoards[RoundTick - 1] : null;
        //public static bool IsBoardValid => RoundTick == RoundBoards.Count - 1;
        public static BoardState BoardState { get; set; }

        public static Bombs Bombs { get; set; }
        public static MyBomberman Me { get; set; }
        public static NearPoints NearPoints { get; set; }
        public static OtherBombermans OtherBombermans { get; set; }
        public static Choppers Choppers { get; set; }
    }
}
