using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Api
{
    public class OtherBomberman : IHasPoint
    {
        //public int Id { get; set; }
        public Point Point { get; set; }
        public int AfkPoints { get; set; } = 0;
    }
}
