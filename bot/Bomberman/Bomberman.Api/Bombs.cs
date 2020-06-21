using Bomberman.Api.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Api
{
    public class Bombs
    {
        public List<Bomb> AllBombs { get; set; } = new List<Bomb>();

        public void Init()
        {
            
            AllBombs.Tick();

            var bombPoints = Global.Board.GetBombs();

            var newBombPoints = bombPoints.Where(b => !AllBombs.ContainsPoint(b));
            var newBombs = new List<Bomb>();

            foreach (var newBombPoint in newBombPoints)
            {
                
            }

            AllBombs.AddRange(newBombs);
            
        }

    }
}
