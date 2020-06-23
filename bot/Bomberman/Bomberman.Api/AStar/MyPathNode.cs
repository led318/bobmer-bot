using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Api.AStar
{
    public class MyPathNode : IPathNode<Object>
    {
        public Element Element { get; set; }
        public Point Point { get; set; }

        public Int32 X { get; set; }
        public Int32 Y { get; set; }
        public Boolean IsWall { get; set; }

        public bool IsWalkable(Object unused)
        {
            return !IsWall;
        }
    }
}
