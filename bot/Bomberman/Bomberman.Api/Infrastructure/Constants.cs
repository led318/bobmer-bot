namespace Bomberman.Api
{
    public static class Constants
    {
        public static readonly Point EMPTY_POINT = new Point();

        public static readonly Element RC_BOMB_ELEMENT = Element.BOMB_TIMER_5;

        public static readonly Element[] BOMB_ELEMENTS = new[] {
            Element.BOMB_TIMER_1,
            Element.BOMB_TIMER_2,
            Element.BOMB_TIMER_3,
            Element.BOMB_TIMER_4,
            Element.BOMB_TIMER_5,
            //Element.BOOM
        };

        public static readonly Element[] ZOMBI_ELEMENTS = new[] {
            Element.DeadMeatChopper,
            Element.DestroyedWall
        };

        public static readonly Element[] ENEMIES_ELEMENTS = new[] {
            Element.OTHER_BOMBERMAN,
            Element.OTHER_BOMB_BOMBERMAN,
            Element.MEAT_CHOPPER
        };

        public static readonly Element[] OTHER_BOMBERMANS_ELEMENTS = new[] {
            Element.OTHER_BOMBERMAN,
            Element.OTHER_BOMB_BOMBERMAN
        };

        public static readonly Element[] MOVABLE_ELEMENTS = new[] {
            Element.Space,
            Element.BOMB_BLAST_RADIUS_INCREASE,
            Element.BOMB_COUNT_INCREASE,
            Element.BOMB_IMMUNE,
            Element.BOMB_REMOTE_CONTROL,
            Element.BOOM
        };

        public static readonly Element[] BONUS_ELEMENTS = new[] {
            Element.BOMB_BLAST_RADIUS_INCREASE,
            //Element.BOMB_COUNT_INCREASE,
            Element.BOMB_IMMUNE,
            Element.BOMB_REMOTE_CONTROL
        };

        public static readonly Element[] WALL_ELEMENTS = new[] {
            Element.WALL,
            Element.DESTROYABLE_WALL
        };
    }
}
