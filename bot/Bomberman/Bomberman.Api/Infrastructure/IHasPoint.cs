using Bomberman.Api.Enums;

namespace Bomberman.Api.Infrastructure
{
    public interface IHasPoint
    {
        Point Point { get; set; }
        bool IsNear(Point point);
        //int IsNear(Point point);
        MovePosition GetNearPosition(Point point);
    }
}
