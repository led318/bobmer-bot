using System.Collections.Generic;
using System.Linq;
using System;
using Bomberman.Api.Infrastructure;

namespace Bomberman.Api
{
    public class OtherBombermans
    {
        private List<OtherBomberman> _afkOtherBombermansStatus = new List<OtherBomberman>();
        public List<Point> AfkOtherBombermans =>
            _afkOtherBombermansStatus.Where(b => b.AfkPoints >= Config.AfkBreakpoint).Select(b => b.Point).ToList();

        public Point Target { get; set; }
        public bool HaveAfkBombermans => AfkOtherBombermans.Any();
        public List<Point> Bombermans { get; set; }

        public void Clear()
        {
            _afkOtherBombermansStatus.Clear();
        }

        public void Init()
        {
            Bombermans = Global.Board.GetOtherBombermans();

            CalculateAfk();

            //CalculateTargetBomberman(Global.Choppers.AllChoppers.GetPoints());
            //Console.WriteLine("target: " + Target + " CHOPPER");

            
            if (AfkOtherBombermans.Any())
            {
                CalculateTargetBomberman(AfkOtherBombermans);
                Console.WriteLine("target: " + Target + " AFK");
            }
            else if (Bombermans.Any())
            {
                CalculateTargetBomberman(Bombermans);
                Console.WriteLine("target: " + Target);
            }
        }

        private void CalculateAfk()
        {
            if (Bombermans.Any())
            {
                if (_afkOtherBombermansStatus.Any())
                {
                    foreach (var afkOtherBombermanStatus in _afkOtherBombermansStatus)
                    {
                        var otherBomberman = Bombermans.FirstOrDefault(b => b.Equals(afkOtherBombermanStatus.Point));
                        if (otherBomberman.Equals(Constants.EMPTY_POINT))
                            afkOtherBombermanStatus.AfkPoints = 0;
                        else
                            afkOtherBombermanStatus.AfkPoints += 1;
                    }
                }
                else
                    _afkOtherBombermansStatus = Bombermans.Select(b => new OtherBomberman { Point = b }).ToList();
            }

            Console.WriteLine("afk: " + AfkOtherBombermans.Count);
        }        

        private void CalculateTargetBomberman(List<Point> bombermans)
        {
            Target = Helper.GetNearest(Global.Me.Point, bombermans).FirstOrDefault();
        }
    }
}
