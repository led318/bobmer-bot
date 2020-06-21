using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Api
{
    public class BoardPoint: HasPoint
    {
        public int ChopperPossibility { get; set; }

        public BoardPoint(): base()
        {
                
        }

        public BoardPoint(Point point): base(point)
        {
            
        }
    }
}
