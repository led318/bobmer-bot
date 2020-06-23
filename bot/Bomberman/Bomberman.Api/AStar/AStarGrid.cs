using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bomberman.Api.Infrastructure;

namespace Bomberman.Api.AStar
{
    public class AStarGrid
    {
        public MyPathNode[,] Grid { get; private set; }
        public int GridSize = 23;

        public void Init()
        {
            Grid = new MyPathNode[23, 23];
            var allNearPoints = Global.NearPoints.GetAllNearPointsRecursively();

            for (var i = 0; i < GridSize; i++)
            {
                for (var j = 0; j < GridSize; j++)
                {
                    var point = new Point(i, j);
                    var element = Global.Board.GetAt(point);

                    Grid[i,j] = new MyPathNode
                    {
                        X = i,
                        Y = j,
                        Element = element,
                        Point = point,
                        IsWall = IsWall(allNearPoints, point, element)
                    };
                }
            }
        }

        private bool IsWall(List<NearPoint> allNearPoints, Point point, Element element)
        {
            var nearPoint = allNearPoints.FirstOrDefault(x => x.Point.Equals(point));
            if (nearPoint != null)
            {
                return nearPoint.IsCriticalDanger;
            }

            return Constants.WALL_ELEMENTS_ASTAR.Contains(element);
        }
    }
}
