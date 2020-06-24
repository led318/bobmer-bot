namespace Bomberman.Api.Infrastructure
{
    public enum MovePosition
    {
        Stop, //position not changed
        Top,
        Right,
        Bottom,
        Left,
        Unknown, //position not found
    }

    public enum MoveDirection
    {
        Forward,
        Backward,
        Right,
        Left,
        Unknown
    }

    public enum ActStrategy
    {
        DoNotAct,
        ActThenMove,
        MoveThenAct,
        RCThenMove,
        MoveThenRC,
        ActThenStop,
    }
}