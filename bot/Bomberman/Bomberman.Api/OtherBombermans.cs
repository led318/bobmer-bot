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
            _afkOtherBombermansStatus
                .Where(b => b.AfkPoints >= Config.AfkBreakpoint)
                .Select(b => b.Point).ToList();

        public Point Target { get; set; }
        public Element TargetElement => Global.Board.GetAt(Target);
        public bool IsTargetLive => Constants.OTHER_BOMBERMANS_ELEMENTS.Contains(TargetElement);

        public bool IsTargetAfk => AfkOtherBombermans.Any();
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

            return false;
        }

        private void CalculateAfk()
        {
            if (Bombermans.Any() && Global.HasPrevBoard)
            {
                foreach (var bomberman in Bombermans)
                {
                    if (Global.PrevBoard.IsAnyOfAt(bomberman, Constants.OTHER_BOMBERMANS_ELEMENTS))
                    {
                        var afkBombermanStatus = _afkOtherBombermansStatus.FirstOrDefault(x => x.Point.Equals(bomberman));
                        if (afkBombermanStatus == null)
                        {
                            afkBombermanStatus = new OtherBomberman
                            {
                                Point = bomberman,
                                IsProcessed = true
                            };

                            _afkOtherBombermansStatus.Add(afkBombermanStatus);
                        }
                        else
                        {
                            afkBombermanStatus.AfkPoints++;
                            afkBombermanStatus.IsProcessed = true;
                        }
                    }
                }
            }

            _afkOtherBombermansStatus = _afkOtherBombermansStatus.Where(x => x.IsProcessed).ToList();
            _afkOtherBombermansStatus.ForEach(x => x.IsProcessed = false);

            Console.WriteLine("afk: " + AfkOtherBombermans.Count);
        }

        private bool CalculateSuicide()
        {
            //Console.WriteLine("suicide points: " + Global.Me.SuicidePoints);

            if (Bombermans.Count == 1 || Global.Me.SuicidePoints > 0)
            {
                Global.Me.SuicidePoints++;

                if (Global.Me.IsForceSuicide || (Global.Me.IsSuicide && !AfkOtherBombermans.Any()))
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
