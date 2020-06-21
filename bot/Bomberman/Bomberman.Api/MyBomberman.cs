using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Linq;
using Bomberman.Api.Enums;
using Bomberman.Api.Infrastructure;

namespace Bomberman.Api
{
    public class MyBomberman : Bomberman
    {
        public int MyBombsPower => Config.BombsDefaultPower + BonusBlast.Effect;
        public int MyBombsForSearchPower => Config.BombsDefaultPower + BonusBlast.NextTickEffect;
        public int MaxBombsCount => Config.BombsDefaultCount + BonusCount.Effect;


        #region Destroyable Walls
        public int DestroyableWallsNearCount => GetPointDestroyableWalls(Point).Count();
        public bool HaveDestroyableWallsNear => DestroyableWallsNearCount > 0;

        public int DestroyableWallsNextStepCount => PreviousStep != null ? GetPointDestroyableWalls(PreviousStep.Point).Count() : 0;
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

            if (!currentStep && PreviousStep == null)
                return false;

            var startPoint = currentStep ? Point : PreviousStep.Point;

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
        private NearPoint _previousStep;
        public NearPoint PreviousStep
        {
            get { return _previousStep; }
            set
            {
                _previousStep = value;

                ProcessBonuses(_previousStep);
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

        private bool IsMeOnMyFirstRCBombBlast => PreviousStep != null && MyFirstRCBombFutureBlasts.Contains(PreviousStep.Point);

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

        public bool CanUseRC => IsMyBombRC && !IsMeOnMyFirstRCBombBlast;
        public void UseRC()
        {
            Console.WriteLine("ACHTUNG!!! USE RC!!!!!!");

            BonusRC.Utilize();
        }

        #endregion

        public void Clear()
        {
            _suicidePoints = 0;
            ClearBonuses();
        }

        public void Tick()
        {
            if (_bombPlaceTimeout > 0)
                _bombPlaceTimeout--;
        }

        public void PrintStatus()
        {
            Console.WriteLine("bomb place timeout: " + _bombPlaceTimeout);
            Console.WriteLine("bomb power: " + MyBombsPower);
            Console.WriteLine("bombs max count: " + MaxBombsCount);
            Console.WriteLine("immune: " + BonusImmune.IsActive);
            Console.WriteLine("rc: " + IsMyBombRC);
            Console.WriteLine("direct afk: " + HaveDirectAfkTarget(false));
            //Console.WriteLine("my bomb rc: " + IsMyBombRC);
            //Console.WriteLine("my bombs" + Newtonsoft.Json.JsonConvert.SerializeObject(MyBombs));
            //Console.WriteLine("destroyable walls: " + DestroyableWallsNearCount);
            //Console.WriteLine("destroyable walls next step: " + DestroyableWallsNextStepCount);
        }
    }
}
