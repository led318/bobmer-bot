namespace Bomberman.Api.Infrastructure
{
    public static class Global
    {
        public static int RoundTick { get; set; }
        public static Board Board { get; set; }
        public static Bombs Bombs { get; set; }
        public static MyBomberman Me { get; set; }
        public static NearPoints NearPoints { get; set; }
        public static OtherBombermans OtherBombermans { get; set; }
    }
}
