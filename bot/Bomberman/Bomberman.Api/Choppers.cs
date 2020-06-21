using Bomberman.Api.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bomberman.Api.Enums;

namespace Bomberman.Api
{
    public class Choppers
    {
        private string _chopperLogFile = $"c:/temp/bomberman/choppers/choppers_log.txt";

        public List<Chopper> AllChoppers { get; set; } = new List<Chopper>();
        public List<Chopper> NonConflictChoppers => AllChoppers.Where(x => !x.IsConflict).ToList();

        public Chopper Get(Point point)
        {
            return AllChoppers.FirstOrDefault(x => x.Point.Equals(point));
        }

        public void Init()
        {
            //AllBombs.Tick();

            ProcessDeadChoppers();

            var liveMeatChopperPoints = Global.Board.GetMeatChoppers();

            foreach (var liveMeatChopperPoint in liveMeatChopperPoints)
            {
                var nearPrevStepChoppers = NonConflictChoppers
                    .Where(x => x.IsNear(liveMeatChopperPoint))
                    .ToList();

                if (nearPrevStepChoppers.Any())
                {
                    if (nearPrevStepChoppers.Count() == 1)
                    {
                        nearPrevStepChoppers.First().CalculateAndSetDirection(liveMeatChopperPoint);
                    }
                    else
                    {
                        nearPrevStepChoppers.ForEach(x => x.SetConflict());
                    }
                }
                else
                {
                    AllChoppers.Add(new Chopper(liveMeatChopperPoint, true));
                }
            }

            AllChoppers.RemoveAll(x => !x.IsOnBoard);
            AllChoppers.ForEach(x => x.ResetStateFlags());

            AllChoppers.ForEach(x => x.InitBoardPoints());
            //Console.WriteLine("choppers at board: " + AllChoppers.Count);
        }

        private void ProcessDeadChoppers()
        {
            var deadMeatChopperPoints = Global.Board.Get(Element.DEAD_MEAT_CHOPPER);
            foreach (var deadMeatChopperPoint in deadMeatChopperPoints) {
                AllChoppers.RemovePoint(deadMeatChopperPoint);
            }
        }

        // temp for collecting choppers behavior
        /*
        public void WriteChopperLogs()
        {
            foreach (var chopper in AllChoppers)
            {
                using (var stream = new StreamWriter(_chopperLogFile, true))
                {
                    stream.WriteLine(string.Join(",", chopper.MoveHistory.Select(x => x.ToString())));
                    chopper.MoveHistory.Clear();
                }
            }
        }
        */
    }
}
