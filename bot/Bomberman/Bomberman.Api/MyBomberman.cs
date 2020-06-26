using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Linq;
using Bomberman.Api.Infrastructure;

namespace Bomberman.Api
{
    public class MyBomberman : IHasPoint
    {
        public int MyBombsPower => Config.BombsDefaultPower + Config.BonusBlastIncrease * BonusBlastMultiplier;
        public int MyBombsForSearchPower => Config.BombsDefaultPower + Config.BonusBlastIncrease * BonusBlastForSearchMultiplier;
        public int MaxBombsCount => Config.BombsDefaultCount + Config.BonusCountIncrease * BonusCountMultiplier;


        public Point Point { get; set; }
        public Element Element { get; set; }

        public bool IsOnBomb => Element == Element.BOMB_BOMBERMAN;



        #region Destroyable Walls
        public int DestroyableWallsCount => Helper.GetDestroyableWallsCount(Point);
        public bool HaveDestroyableWallsNear => DestroyableWallsCount > 0;

        public int DestroyableWallsNextStepCount => HaveNextStep ? Helper.GetDestroyableWallsCount(NextStep.Point) : 0;

        public bool HaveMoreDestroyableWallsNextStep => DestroyableWallsCount == 1
                                                        && DestroyableWallsCount < DestroyableWallsNextStepCount;

        #endregion

        #region Near Enemies
        public IEnumerable<Point> NearEnemies { get; private set; } = new List<Point>();
        public IEnumerable<Point> NearMeatChoppers { get; private set; } = new List<Point>();

        public List<Point> NearZombies { get; private set; } = new List<Point>();

        public bool HaveDirectAfkTargetCurrentStep => Global.OtherBombermans.IsTargetAfk && Helper.HaveDirectAfkTarget(Point);
        public bool HaveDirectAfkTargetNextStep => Global.OtherBombermans.IsTargetAfk 
                                                   && Global.OtherBombermans.IsTargetLive
                                                   && HaveNextStep
                                                   && Helper.HaveDirectAfkTarget(NextStep.Point);

        public bool HaveDirectAfkTarget(Point point)
        {
            return Global.OtherBombermans.IsTargetAfk
                   && Helper.HaveDirectAfkTarget(point);
        }


        public void InitNearEnemies()
        {
            //NearEnemies = Global.Board.Get(Point, Config.EnemiesDetectionBreakpoint, Constants.ENEMIES_ELEMENTS);
            var tempNearEnemies = Global.Board.Get(Point, Config.EnemiesDetectionBreakpoint, Constants.ENEMIES_ELEMENTS);
            NearEnemies = tempNearEnemies.Where(x => !Global.OtherBombermans.AfkOtherBombermans.Contains(x)).ToList();

            NearMeatChoppers = Global.Board.Get(Point, Config.EnemiesDetectionBreakpoint, Element.MEAT_CHOPPER);

            NearZombies.Clear();

            if (Global.HasPrevBoard)
            {
                var tempNearZombies = Global.Board.Get(Point, Config.EnemiesDetectionBreakpoint, Constants.ZOMBI_ELEMENTS);

                foreach (var tempNearZombi in tempNearZombies)
                {
                    var prevStepCheck = Global.PrevBoard.CountNear(tempNearZombi, Constants.ZOMBI_ELEMENTS);

                    if (prevStepCheck > 0)
                        NearZombies.Add(tempNearZombi);
                }
            }

        }

        #endregion

        #region Move
        private NearPoint _nextStep;
        public NearPoint NextStep
        {
            get { return _nextStep; }
            set
            {
                _nextStep = value;

                if (_nextStep != null)
                {
                    if (_nextStep.IsBonusRC)
                        Bonuses.Add(new BonusRC());

                    if (_nextStep.IsBonusBlast)
                        Bonuses.Add(new BonusBlast());

                    if (_nextStep.IsBonusImmune)
                        Bonuses.Add(new BonusImmune());

                    if (_nextStep.IsBonusCount)
                        Bonuses.Add(new BonusCount());
                }
            }
        }

        public bool HaveNextStep => NextStep != null;

        public bool NextStepIsDangerForMove => !Global.Me.HaveNextStep
                                               || (Global.Me.NextStep.IsDangerForActThenMove
                                                   && !Global.Me.NextStep.IsSafeForActThenMove);
        #endregion

        #region MyBombs
        public List<Bomb> MyBombs { get; private set; } = new List<Bomb>();
        //public List<Bomb> MyRCBombs => MyBombs.Where(x => x.IsRc).ToList();

        public List<Blast> MyBlasts { get; private set; } = new List<Blast>();
        public List<Blast> MyRCBombBlasts { get; private set; } = new List<Blast>();

        public bool IsMyBombLive => MyBombs.Any();
        public bool IsMyBombRC => IsMyBombLive && MyBombs.Any(b => b.IsRc);
        public bool CanPlaceBombs => MyBombs.Count() < MaxBombsCount && BombPlaceTimeout == 0;
        public int BombPlaceTimeout = 1;
        public void SetMyBomb()
        {
            SetMyBombInternal(Point);
        }

        public void ResetBombPlaceTimeout()
        {
            BombPlaceTimeout = Config.BombPlaceTimeout + 1;
        }

        public void SetMyBombNextStep()
        {
            SetMyBombInternal(NextStep.Point);
        }

        private void SetMyBombInternal(Point point)
        {
            ResetBombPlaceTimeout();
            MyBombs.Add(new Bomb(point, true));
        }

        public void InitMyBombs()
        {
            var allBombs = Global.Board.GetBombs();
            MyBombs = MyBombs.Where(b => allBombs.Contains(b.Point)).ToList();
            foreach (var myBomb in MyBombs) {
                myBomb.Init();
            }
        }

        public void InitMyBlasts()
        {
            MyBlasts = Global.Blasts.GetMyBlasts();
            MyRCBombBlasts = MyBlasts.Where(x => x.IsRC).ToList();
        }

        public bool IsOnMyRC => MyRCBombBlasts.GetPoints().Contains(Point);
        public bool IsOnMyRCNextStep => HaveNextStep && MyRCBombBlasts.GetPoints().Contains(NextStep.Point);

        #endregion

        #region Suicide

        public int SuicidePoints = 0;
        public bool IsSuicide => SuicidePoints > Config.SuicideBreakpoint;
        public bool IsForceSuicide => SuicidePoints > Config.ForceSuicideBreakpoint;
        private string _suicideForcePart => IsForceSuicide ? "FORCE " : string.Empty;
        public string SuicideMessage => $"!!!!!{_suicideForcePart}SUICIDE!!!!!";
        #endregion

        #region Bonus
        private List<BonusRC> BonusesRC => Bonuses.Select(b => b as BonusRC).Where(b => b != null).OrderBy(b => b.UsesLeft).ToList();
        
        private bool IsBonusBlast => IsBonusType(Element.BOMB_BLAST_RADIUS_INCREASE);
        private int BonusBlastMultiplier => GetBonusTypeCount(Element.BOMB_BLAST_RADIUS_INCREASE);
        private int BonusBlastForSearchMultiplier
        {
            get
            {
                var bonuses = GetBonuses(Element.BOMB_BLAST_RADIUS_INCREASE);
                var allDisabled = bonuses.All(x => ((BonusByTick) x).IsDisabled);

                return allDisabled ? 0 : bonuses.Count;
                //GetBonuses(Element.BOMB_BLAST_RADIUS_INCREASE).Count(b => !((BonusByTick)b).IsDisabled);
            }
        }

        private int BonusBlastNextStepMultiplier => GetBonuses(Element.BOMB_BLAST_RADIUS_INCREASE).Count(b => b.IsActiveNextStep);
        private int BonusBlastDuration => GetBonusMaxDuration(Element.BOMB_BLAST_RADIUS_INCREASE);

        public bool IsBonusImmune => IsBonusType(Element.BOMB_IMMUNE);
        public int BonusImmuneDuration => GetBonusMaxDuration(Element.BOMB_IMMUNE);

        private bool IsBonusCount => IsBonusType(Element.BOMB_COUNT_INCREASE);
        private int BonusCountMultiplier => GetBonusTypeCount(Element.BOMB_COUNT_INCREASE);
        private int BonusCountDuration => GetBonusMaxDuration(Element.BOMB_COUNT_INCREASE);

        private List<Bonus> Bonuses = new List<Bonus>();
        private List<BonusByTick> BonusesByTick => Bonuses.Select(b => b as BonusByTick).Where(b => b != null).ToList();

        private bool IsBonusType(Element element)
        {
            return GetBonusTypeCount(element) > 0;
        }

        private int GetBonusTypeCount(Element element)
        {
            return Bonuses.Count(b => b.Element == element);
        }

        private int GetBonusMaxDuration(Element element)
        {
            var bonuses = Bonuses.Where(x => x.Element == element).ToList();
            return bonuses.Any() ? bonuses.Max(x => ((BonusByTick)x).TicksLeft) : 0;
        }

        private List<Bonus> GetBonuses(Element element)
        {
            return Bonuses.Where(b => b.Element == element).ToList();
        }

        public bool HaveDirectBonus()
        {
            return HaveDirectBonusOnPoint(Point);
        }

        public bool HaveDirectBonusNextStep()
        {
            return HaveNextStep && HaveDirectBonusOnPoint(NextStep.Point);
        }


        public bool HaveDirectBonusOnPoint(Point point)
        {
            var result = new List<Point>();
            var blastSize = MyBombsForSearchPower;

            var startPoint = point;

            CheckDirectBonus(result, startPoint, blastSize, p => p.ShiftTop());
            CheckDirectBonus(result, startPoint, blastSize, p => p.ShiftRight());
            CheckDirectBonus(result, startPoint, blastSize, p => p.ShiftBottom());
            CheckDirectBonus(result, startPoint, blastSize, p => p.ShiftLeft());

            return result.Any();
        }

        private void CheckDirectBonus(List<Point> result, Point startPoint, int blastSize, Func<Point, Point> func)
        {
            var pointToCheck = startPoint;

            for (var i = 0; i < blastSize; i++)
            {
                pointToCheck = func(pointToCheck);

                if (Global.Board.IsAnyOfAt(pointToCheck, Constants.WALL_ELEMENTS))
                    return;

                if (Global.Board.IsAnyOfAt(pointToCheck, Constants.BONUS_ELEMENTS))
                    result.Add(pointToCheck);
            }
        }

        #endregion

        public void Clear()
        {
            SuicidePoints = 0;
            Bonuses.Clear();
            ResetBombPlaceTimeout();
        }

        public void Tick()
        {
            var groupedTickBonuses = BonusesByTick.GroupBy(b => b.Element);
            foreach (var group in groupedTickBonuses)
            {
                var groupItems = group.OrderBy(b => b.TicksLeft).ToList();

                var index = 0;
                foreach (var bonusByTick in BonusesByTick)
                {
                    if (bonusByTick.AreTicksConcatenated && index > 0)
                        continue;

                    bonusByTick.Tick();
                    index++;
                }
            }

            var groups = Bonuses.GroupBy(x => x.Element);

            var activeGroups = groups.Where(x => x.Any(y => y.IsActive)).ToList();

            Bonuses = activeGroups.SelectMany(x => x).ToList();

            //Bonuses = Bonuses.Where(b => b.IsActive).ToList();

            if (BombPlaceTimeout > 0)
                BombPlaceTimeout--;
        }

        public void PrintStatus()
        {
            //Console.WriteLine($"bomb place timeout: {_bombPlaceTimeout}");
            Console.WriteLine($"bomb power: {MyBombsPower}, +{BonusBlastMultiplier}, {BonusBlastDuration}s");
            Console.WriteLine($"bombs max count: {MaxBombsCount}, {BonusCountDuration}s");
            Console.WriteLine($"immune: {IsBonusImmune}, {BonusImmuneDuration}s");
            Console.WriteLine($"rc: {IsMyBombRC}");
            Console.WriteLine($"is on my rc: {IsOnMyRC}");
            Console.WriteLine($"is on my rc next step: {IsOnMyRCNextStep}");
            Console.WriteLine("direct afk this step: " + HaveDirectAfkTargetCurrentStep);
            Console.WriteLine("direct afk next step: " + HaveDirectAfkTargetNextStep);
            //Console.WriteLine("my bomb rc: " + IsMyBombRC);
            //Console.WriteLine("my bombs" + Newtonsoft.Json.JsonConvert.SerializeObject(MyBombs));
            //Console.WriteLine("destroyable walls: " + DestroyableWallsNearCount);
            //Console.WriteLine("destroyable walls next step: " + DestroyableWallsNextStepCount);
        }
    }
}
