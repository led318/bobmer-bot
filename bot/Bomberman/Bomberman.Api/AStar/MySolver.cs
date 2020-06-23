using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Api.AStar
{
    public class MySolver<TPathNode, TUserContext> : SpatialAStar<TPathNode, TUserContext> where TPathNode : IPathNode<TUserContext>
    {
        protected override Double Heuristic(PathNode inStart, PathNode inEnd)
        {
            return Math.Abs(inStart.X - inEnd.X) + Math.Abs(inStart.Y - inEnd.Y);
        }

        //protected override Double NeighborDistance(PathNode inStart, PathNode inEnd)
        //{
        //    return Heuristic(inStart, inEnd);
        //}

        protected override Double NeighborDistance(PathNode inStart, PathNode inEnd)
        {
            int diffX = Math.Abs(inStart.X - inEnd.X);
            int diffY = Math.Abs(inStart.Y - inEnd.Y);

            switch (diffX + diffY)
            {
                case 1: return 1;
                case 2: return 999;
                case 0: return 0;
                default:
                    throw new ApplicationException();
            }
        }

        public MySolver(TPathNode[,] inGrid)
            : base(inGrid)
        {
        }
    }
}
