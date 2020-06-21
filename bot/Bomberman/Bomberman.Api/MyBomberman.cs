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



        #region Destroyable Walls
        public int DestroyableWallsNearCount => GetPointDestroyableWalls(Point).Count();
        public bool HaveDestroyableWallsNear => DestroyableWallsNearCount > 0;

        public int DestroyableWallsNextStepCount => PreviousMove != null ? GetPointDestroyableWalls(PreviousMove.Point).Count() : 0;
        public bool HaveDestroyableWallsNextStep => DestroyableWallsNextStepCount > 0;

        public bool HaveMoreDestroyableWallsNextStep => DestroyableWallsNearCount == 1
            && DestroyableWallsNearCount < DestroyableWallsNextStepCount;

        public List<Point> GetPointDestroyableWalls(Point point, bool currentStep = true) 
        {
            var result = new List<Point>();

            var blastSize = currentStep ? MyBombsForSearchPower : MyBombsForSearchPower;

            CheckIfIsDestroyableWall(result, point, blastSize, p => p.ShiftTop());
            CheckIfIsDestroyableWall(result, point, blastSize, p => p.ShiftRight());
            CheckIfIsDestroyableWall(result, point, blastSize, p => p.ShiftBottom());
            CheckIfIsDestroyableWall(result, point, blastSize, p => p.ShiftLeft());

            return result;
        }

        private void CheckIfIsDestroyableWall(List<Point> result, Point point, int blastSize, Func<Point, Point> func)
        {
            var pointToCheck = point;

            for (var i = 0; i < blastSize; i++)
            {
                pointToCheck = func(pointToCheck);
                if (Global.Board.IsAt(pointToCheck, Element.WALL))
                    return;

                if (Global.Board.IsAt(pointToCheck, Element.DESTROYABLE_WALL))
                    result.Add(pointToCheck);
            }
        }


        #endregion

        #region Near Enemies
        public IEnumerable<Point> NearEnemies { get; private set; } = new List<Point>();
        public IEnumerable<Point> NearMeatChoppers { get; private set; } = new List<Point>();

        public bool HaveDirectAfkTargetCurrentStep => Global.OtherBombermans.HaveAfkBombermans && HaveDirectAfkTarget(true);
        public bool HaveDirectAfkTargetNextStep => Global.OtherBombermans.HaveAfkBombermans && HaveDirectAfkTarget(false);

        private bool HaveDirectAfkTarget(bool currentStep = true)
        {
            var result = new List<Point>();
            var searchPoint = Global.OtherBombermans.Target;
            var blastSize = currentStep ? MyBombsForSearchPower : MyBombsForSearchPower;

            if (!currentStep && PreviousMove == null)
                return false;

            var startPoint = currentStep ? Point : PreviousMove.Point;

            CheckDirectAfkTarget(result, startPoint, searchPoint, blastSize, p => p.ShiftTop());
            CheckDirectAfkTarget(result, startPoint, searchPoint, blastSize, p => p.ShiftRight());
            CheckDirectAfkTarget(result, startPoint, searchPoint, blastSize, p => p.ShiftBottom());
            CheckDirectAfkTarget(result, startPoint, searchPoint, blastSize, p => p.ShiftLeft());

            return result.Any();
        }

        private void CheckDirectAfkTarget(List<Point> result, Point startPoint, Point searchPoint, int blastSize, Func<Point, Point> func)
        {
            var pointToCheck = startPoint;

            for (var i = 0; i < blastSize; i++)
            {
                pointToCheck = func(pointToCheck);

                if (Global.Board.IsAnyOfAt(pointToCheck, Constants.WALL_ELEMENTS))
                    return;

                if (pointToCheck.Equals(searchPoint))
                    result.Add(pointToCheck);
            }
        }

        public void InitNearEnemies()
        {
            NearEnemies = Global.Board.Get(Point, Config.EnemiesDetectionBreakpoint, Constants.ENEMIES_ELEMENTS);
            NearMeatChoppers = Global.Board.Get(Point, Config.EnemiesDetectionBreakpoint, Element.MEAT_CHOPPER);
        }

        #endregion

        #region Move
        private NearPoint _previousMove;
        public NearPoint PreviousMove
        {
            get { return _previousMove; }
            set
            {
                _previousMove = value;

                if (_previousMove != null)
                {
                    if (_previousMove.IsBonusRC)
                        Bonuses.Add(new BonusRC());

                    if (_previousMove.IsBonusBlast)
                        Bonuses.Add(new BonusBlast());

                    if (_previousMove.IsBonusImmune)
                        Bonuses.Add(new BonusImmune());

                    if (_previousMove.IsBonusCount)
                        Bonuses.Add(new BonusCount());
                }
            }
        }
        #endregion 

        #region MyBombs
        public List<Bomb> MyBombs { get; private set; } = new List<Bomb>();

        public List<Point> MyFutureBlasts { get; private set; } = new List<Point>();
        public List<Point> MyFirstRCBombFutureBlasts { get; private set; } = new List<Point>();

        public bool IsMyBombLive => MyBombs.Any();
        public bool IsMyBombRC => IsMyBombLive && MyBombs.Any(b => b.IsRc);
        public bool CanPlaceBombs => MyBombs.Count() < MaxBombsCount && _bombPlaceTimeout == 0;
        private int _bombPlaceTimeout = 0;
        public void SetMyBomb()
        {
            _bombPlaceTimeout = Config.BombPlaceTimeout + 1;
            MyBombs.Add(new Bomb(Point, true));
        }

        public void CheckMyBombs()
        {
            var allBombs = Global.Board.GetBombs();
            MyBombs = MyBombs.Where(b => allBombs.Contains(b.Point)).ToList();
            foreach (var myBomb in MyBombs) {
                myBomb.Init();
            }

            MyFutureBlasts = Global.Board.GetFutureBlastsForBombs(MyBombs.GetPoints());

            var myFirstRCBomb = MyBombs.FirstOrDefault(b => b.IsRc);
            MyFirstRCBombFutureBlasts = myFirstRCBomb != null
                ? Global.Board.GetFutureBlastsForBombs(myFirstRCBomb.Point)
                : new List<Point>();
        }        

        private bool IsMeOnMyFirstRCBombBlast => PreviousMove != null && MyFirstRCBombFutureBlasts.Contains(PreviousMove.Point);

        #endregion

        #region Suicide

        private int _suicidePoints = 0;
        private bool _isSuicide => _suicidePoints > Config.SuicideBreakpoint;
        private bool _isForceSuicide => _suicidePoints > Config.ForceSuicideBreakpoint;
        private string _suicideForcePart => _isForceSuicide ? "FORCE " : string.Empty;
        private string _suicideMessage => $"!!!!!{_suicideForcePart}SUICIDE!!!!!";

        public bool CheckSuicide()
        {
            Console.WriteLine("suicide points: " + _suicidePoints);

            if (Global.OtherBombermans.Bombermans.Count == 1 || _suicidePoints > 0)
            {
                _suicidePoints++;

                if (_isForceSuicide || (_isSuicide && !Global.OtherBombermans.HaveAfkBombermans))
                {
                    Console.WriteLine(_suicideMessage);
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Bonus
        //public bool IsBonusRC => IsBonusType(Element.BOMB_REMOTE_CONTROL); //wrong logic
        public bool CanUseRC => IsMyBombRC && !IsMeOnMyFirstRCBombBlast;
        private List<BonusRC> BonusesRC => Bonuses.Select(b => b as BonusRC).Where(b => b != null).OrderBy(b => b.UsesLeft).ToList();
        public void UseRC()
        {
            Console.WriteLine("ACHTUNG!!! USE RC!!!!!!");

            var rcBonusToUse = BonusesRC.FirstOrDefault();
            if (rcBonusToUse != null)
                rcBonusToUse.Use();
        }

        private bool IsBonusBlast => IsBonusType(Element.BOMB_BLAST_RADIUS_INCREASE);
        private int BonusBlastMultiplier => GetBonusTypeCount(Element.BOMB_BLAST_RADIUS_INCREASE);
        private int BonusBlastForSearchMultiplier => GetBonuses(Element.BOMB_BLAST_RADIUS_INCREASE).Count(b => !((BonusByTick)b).IsDisabled);
        private int BonusBlastNextStepMultiplier => GetBonuses(Element.BOMB_BLAST_RADIUS_INCREASE).Count(b => b.IsActiveNextStep);

        public bool IsBonusImmune => IsBonusType(Element.BOMB_IMMUNE);

        private bool IsBonusCount => IsBonusType(Element.BOMB_COUNT_INCREASE);
        private int BonusCountMultiplier => GetBonusTypeCount(Element.BOMB_COUNT_INCREASE);

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

        private List<Bonus> GetBonuses(Element element)
        {
            return Bonuses.Where(b => b.Element == element).ToList();
        }



        #endregion

        public void Clear()
        {
            _suicidePoints = 0;
            Bonuses.Clear();
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

            Bonuses = Bonuses.Where(b => b.IsActive).ToList();

            if (_bombPlaceTimeout > 0)
                _bombPlaceTimeout--;
        }

        public void PrintStatus()
        {
            Console.WriteLine("bomb place timeout: " + _bombPlaceTimeout);
            Console.WriteLine("bomb power: " + MyBombsPower);
            Console.WriteLine("bombs max count: " + MaxBombsCount);
            Console.WriteLine("immune: " + IsBonusImmune);
            Console.WriteLine("rc: " + IsMyBombRC);
            Console.WriteLine("direct afk: " + HaveDirectAfkTarget(false));
            //Console.WriteLine("my bomb rc: " + IsMyBombRC);
            //Console.WriteLine("my bombs" + Newtonsoft.Json.JsonConvert.SerializeObject(MyBombs));
            //Console.WriteLine("destroyable walls: " + DestroyableWallsNearCount);
            //Console.WriteLine("destroyable walls next step: " + DestroyableWallsNextStepCount);
        }
    }
}
