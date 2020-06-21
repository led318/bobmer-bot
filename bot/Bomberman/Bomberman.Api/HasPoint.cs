namespace Bomberman.Api
{
    public class HasPoint : IHasPoint
    {
        public Point Point { get; set; }

        public HasPoint()
        {

        }

        public HasPoint(Point point)
        {
            Point = point;
        }
    }
}
