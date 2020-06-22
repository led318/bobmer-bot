using Bomberman.Api.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bomberman.Api.Enums;

namespace Bomberman.Api
{
    public class Bombs
    {
        public List<Bomb> AllBombs { get; set; } = new List<Bomb>();

        public void Init()
        {
            
            //AllBombs.Tick();

            //blasted bombs
            var blastedBombs = AllBombs.Where(x => Global.Board.IsAt(x.Point, Element.BOOM)).ToList();
            AllBombs.RemoveAll(x => blastedBombs.ContainsPoint(x));
            //end

            //new bombs
            var bombPoints = Global.Board.GetBombs();
            var newBombPoints = bombPoints.Where(b => !AllBombs.ContainsPoint(b)).ToList();
            var newBombs = new List<Bomb>();

            foreach (var newBombPoint in newBombPoints)
            {
                var bomb = new Bomb(newBombPoint);
                newBombs.Add(bomb);
            }

            AllBombs.AddRange(newBombs);
            //end

            AllBombs.ForEach(x => x.Init());

            Console.WriteLine("bombs: " + AllBombs.Count);
        }

    }
}
