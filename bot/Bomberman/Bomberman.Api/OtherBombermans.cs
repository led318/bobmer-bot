using System.Collections.Generic;
using System.Linq;
using System;
using Bomberman.Api.Infrastructure;

namespace Bomberman.Api
{
    public class OtherBombermans
    {
        private List<OtherBomberman> _afkOtherBombermansStatus = new List<OtherBomberman>();
        private List<Point> _afkOtherBombermans =>
            _afkOtherBombermansStatus.Where(b => b.AfkPoints >= Config.AfkBreakpoint).Select(b => b.Point).ToList();

        public Point Target { get; set; }
        public bool IsTargetAfk => Config.IsLocal || _afkOtherBombermans.Any();
        public List<Point> Bombermans { get; set; }

        public void Clear()
        {
            _afkOtherBombermansStatus.Clear();
        }

        public bool InitAndCheckSuicide()
        {
            Bombermans = Global.Board.GetOtherBombermans();

            CalculateAfk();

            if (CalculateSuicide())
                return true;

            if (_afkOtherBombermans.Any())
            {
                CalculateTargetBomberman(_afkOtherBombermans);
                Console.WriteLine("target: " + Target + " AFK");
            }
            else if (Bombermans.Any())
            {
                CalculateTargetBomberman(Bombermans);
                Console.WriteLine("target: " + Target);
            }

            return false;
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

            Console.WriteLine("afk: " + _afkOtherBombermans.Count);
        }

        private bool CalculateSuicide()
        {
            Console.WriteLine("suicide points: " + Global.Me.SuicidePoints);

            if (Bombermans.Count == 1 || Global.Me.SuicidePoints > 0)
            {
                Global.Me.SuicidePoints++;

                if (Global.Me.IsForceSuicide || (Global.Me.IsSuicide && !_afkOtherBombermans.Any()))
                {
                    Console.WriteLine(Global.Me.SuicideMessage);                    
                    return true;
                }
            }

            return false;
        }

        private void CalculateTargetBomberman(List<Point> bombermans)
        {
            Target = Helper.GetNearest(Global.Me.Point, bombermans).FirstOrDefault();
        }
    }
}
