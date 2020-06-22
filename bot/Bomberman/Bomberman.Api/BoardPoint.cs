using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bomberman.Api.Enums;
using Bomberman.Api.Infrastructure;

namespace Bomberman.Api
{
    public class BoardPoint: HasPoint, IHasElement
    {
        public Element Element { get; set; }

        public int Rating => ChopperPossibility;

        public int ChopperPossibility { get; set; }

        public BoardPoint(): base()
        {
                
        }

        public BoardPoint(Point point): base(point)
        {
            
        }

        public void Init()
        {
            Element = Global.Board.GetAt(Point);
        }

        public void PrintToConsole()
        {
            Console.Write((char)Element);
        }
    }
}
