/*-
 * #%L
 * Codenjoy - it's a dojo-like platform from developers to developers.
 * %%
 * Copyright (C) 2018 Codenjoy
 * %%
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public
 * License along with this program.  If not, see
 * <http://www.gnu.org/licenses/gpl-3.0.html>.
 * #L%
 */
using Bomberman.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Bomberman.Api.Infrastructure;

namespace Demo
{
    /// <summary>
    /// This is BombermanAI client demo.
    /// </summary>
    internal class YourSolver : AbstractSolver
    {
        public YourSolver(string server, bool useFile)
            : base(server, useFile)
        {
            Global.Me = new MyBomberman();
            Global.NearPoints = new NearPoints();
            Global.OtherBombermans = new OtherBombermans();
            Global.Choppers = new Choppers();
            Global.Blasts = new Blasts();
        }

        private string _logPath = $"C:/temp/bomberman/log_{DateTime.Now.ToShortDateString().Replace('/', '_')}-{DateTime.Now.ToShortTimeString().Replace(':', '_')}.txt";
        private string _stopLogPath = $"C:/temp/bomberman/stop/stop_{DateTime.Now.ToShortDateString().Replace('/', '_')}-{DateTime.Now.ToShortTimeString().Replace(':', '_')}.txt";

        static Random _rnd = new Random();

        private Direction _currentDirection = Direction.Up;
        private List<Direction> _currentMoves = new List<Direction>();
        private ActStrategy? _actStrategy = null;
        private bool _threadSleepOnAct = false;

        /// <summary>
        /// Calls each move to make decision what to do (next move)
        /// </summary>
        protected override string Get(Board board)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Process(board);
            Global.RoundTickIndex++;
            sw.Stop();
            Console.WriteLine($"elapsed: {sw.ElapsedMilliseconds}ms");

            return string.Join(",", _currentMoves);
        }

        private Direction GetManualMove()
        {
            switch (Global.ManualMove)
            {
                case "w":
                    return Direction.Up;
                case "d":
                    return Direction.Right;
                case "s":
                    return Direction.Down;
                case "a":
                    return Direction.Left;
                case "f":
                    return Direction.Act;
            }

            return Direction.Stop;
        }

        private void Process(Board board)
        {
            Console.WriteLine($"tick index: {Global.RoundTickIndex}");

            _currentMoves.Clear();
            Global.PrevBoard = Global.Board;
            Global.Board = board;

            if (Global.HasManualMove)
            {
                Console.WriteLine($"manual move: '{Global.ManualMove}'");
                _currentMoves.Add(GetManualMove());
            }

            if (Global.Board.isMyBombermanDead)
            {
                Global.Me.Clear();
                _currentMoves.Add(Direction.Stop);
                Global.OtherBombermans.Clear();
                Config.ManualSuicide = false;

                Global.RoundTickIndex = 0;
            }
            else
            {
                Global.Me.Tick();
                Global.Me.Point = Global.Board.GetBomberman();
                Global.Me.Element = Global.Board.GetAt(Global.Me.Point);

                Global.Choppers.Init();

                if (Global.OtherBombermans.InitAndCheckSuicide() && !Global.HasManualMove)
                {
                    _currentMoves.Add(Direction.Stop);
                    _currentMoves.Add(Direction.Act);
                    return;
                }

                Global.Me.InitNearEnemies();
                Global.Me.InitMyBombs();
                Global.Blasts.Init();
                Global.Me.InitMyBlasts();
                //Console.WriteLine("next step blasts: " + Global.Blasts.AllBlasts.Where(x => x.IsNextStep).Count());

                Global.NearPoints.Init();
                Global.Me.PrintStatus();



                Console.WriteLine(Global.NearPoints.GetPrintStr());
                CalculateAvailableDirection();

                CalculateActStrategy();

                if (!Global.HasManualMove)
                {
                    ProcessActStrategy();
                }
            }

            if (Global.HasManualMove)
            {
                Global.ManualMove = string.Empty;
            }
        }


        private void ProcessActStrategy()
        {
            switch (_actStrategy)
            {
                case ActStrategy.ActThenMove:
                    Global.Me.SetMyBomb();
                    _currentMoves.Add(Direction.Act);
                    _currentMoves.Add(_currentDirection);
                    ThreadSleep();
                    break;

                case ActStrategy.RCThenMove:
                    _currentMoves.Add(Direction.Act);
                    _currentMoves.Add(_currentDirection);
                    ThreadSleep();
                    break;

                case ActStrategy.MoveThenAct:
                    Global.Me.SetMyBombNextStep();
                    _currentMoves.Add(_currentDirection);
                    _currentMoves.Add(Direction.Act);
                    ThreadSleep();
                    break;

                case ActStrategy.MoveThenRC:
                    _currentMoves.Add(_currentDirection);
                    _currentMoves.Add(Direction.Act);
                    ThreadSleep();
                    break;

                case ActStrategy.ActThenStop:
                    _currentMoves.Add(Direction.Act);
                    _currentMoves.Add(Direction.Stop);
                    ThreadSleep();
                    break;

                default:
                    _currentMoves.Add(_currentDirection);
                    break;
            }
        }

        private void ThreadSleep()
        {
            Console.WriteLine(string.Join(",", _currentMoves.Select(x => x.ToString())));
            if (_threadSleepOnAct)
                Thread.Sleep(5000);
        }

        private void WriteToLog(string str)
        {
            using (var log = new StreamWriter(_logPath, true))
            {
                log.WriteLine(str);
            }
        }

        private void CalculateActStrategy()
        {
            _actStrategy = null;

            if (Global.Me.CanPlaceBombs && !Global.Me.IsMyBombRC)
            {
                if (Global.Me.HaveDirectAfkTargetCurrentStep)
                {
                    if (!Global.Me.HaveNextStep || 
                        (Global.Me.NextStep.IsDangerForActThenMove && !Global.Me.NextStep.IsSafeForActThenMove))
                    {
                        _actStrategy = ActStrategy.ActThenStop;
                        Console.WriteLine("ActThenStop, current step afk target, danger for move");
                        return;
                    }
                    else
                    {
                        _actStrategy = ActStrategy.ActThenMove;
                        Console.WriteLine("ActThenMove, current step afk target");
                        return;
                    }
                }

               // if (Global.Me.HaveDirectAfkTargetNextStep && Global.Me.HaveNextStep && !Global.Me.NextStep.IsDangerForActThenMove)
                if (Global.Me.HaveDirectAfkTargetNextStep)
                {
                    _actStrategy = ActStrategy.MoveThenAct;
                    Console.WriteLine("MoveThenAct, next step afk target");
                    return;
                }

                //if (Global.Me.HaveNextStep && Global.Me.NextStep.HaveMoreDestroyableWalls && !Global.Me.NextStep.IsDangerForActThenMove && !Global.Me.HaveDirectBonusNextStep())
                if (Global.Me.HaveNextStep 
                    && Global.Me.NextStep.HaveMoreDestroyableWalls 
                    && !Global.Me.NextStep.IsDangerForActThenMove 
                    && !Global.Me.HaveDirectBonusNextStep())
                {
                    _actStrategy = ActStrategy.MoveThenAct;
                    Console.WriteLine("MoveThenAct, next step more walls");
                    return;
                }

                if (Global.Me.HaveDestroyableWallsNear && !Global.Me.HaveDirectBonus())
                {
                    _actStrategy = ActStrategy.ActThenMove;
                    Console.WriteLine("ActThenMove, have walls");
                    return;
                }

                if (Global.Me.NearEnemies.Any() && !Global.Me.HaveDirectBonus())
                {
                    _actStrategy = ActStrategy.ActThenMove;
                    Console.WriteLine("ActThenMove, have enemies");
                    return;
                }

                if (Global.Me.NearZombies.Any())
                {
                    _actStrategy = ActStrategy.ActThenMove;
                    Console.WriteLine("ActThenMove, have zombies");
                    return;
                }
            }

            if (Global.Me.IsMyBombRC)
            {
                if (Global.Me.IsBonusImmune)
                {
                    _actStrategy = ActStrategy.RCThenMove;
                    Console.WriteLine("RCThenMove, have immune");
                    return;
                }

                if (!Global.Me.IsOnMyRC)
                {
                    _actStrategy = ActStrategy.RCThenMove;
                    Console.WriteLine("RCThenMove, is not on my rc");
                    return;
                }

                if (!Global.Me.IsOnMyRCNextStep)
                {
                    _actStrategy = ActStrategy.MoveThenRC;
                    Console.WriteLine("MoveThenRC, is not on my rc next move");
                    return;
                }
            }
        }

        private void CalculateAvailableDirection()
        {
            if (Global.NearPoints.OnlyCriticalDangerPoints)
            {
                SetCurrentDirection();
                //WriteStopLog();

                return;
            }

            var minRatingPoints = Global.NearPoints.GetMinRatingNonCriticalPoints();
            if (minRatingPoints.Count() > 1 && Global.OtherBombermans.Target != null)
                minRatingPoints = Helper.GetNearest(Global.OtherBombermans.Target, minRatingPoints);

            SetRandomNearPoint(minRatingPoints);
            return;
        }

        private void SetRandomNearPoint(IEnumerable<NearPoint> nearPoints)
        {
            int randomIndex = _rnd.Next(nearPoints.Count());
            var nearPoint = nearPoints.ElementAt(randomIndex);
            SetCurrentDirection(nearPoint);
        }

        private void SetCurrentDirection(NearPoint nearPoint = null)
        {
            _currentDirection = nearPoint?.Direction ?? Direction.Stop;
            Global.Me.NextStep = nearPoint;
        }

        /*
        private void WriteStopLog()
        {
            try
            {
                new System.Threading.Thread(delegate ()
                {
                    using (var log = new StreamWriter(_stopLogPath, true))
                    {
                        log.WriteLine("############################################");
                        log.WriteLine(DateTime.Now.ToLongTimeString());
                        log.WriteLine(Global.Board.ToString());
                        log.WriteLine(Global.NearPoints.GetPrintStr());

                        log.WriteLine();
                        log.WriteLine("### me:");
                        log.WriteLine(JsonConvert.SerializeObject(Global.Me));

                        log.WriteLine();
                        log.WriteLine("### near points:");
                        log.WriteLine(JsonConvert.SerializeObject(Global.NearPoints));

                        log.WriteLine();
                        log.WriteLine("### other bombermans:");
                        log.WriteLine(JsonConvert.SerializeObject(Global.OtherBombermans));

                        log.WriteLine("############################################");
                    }
                }).Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("something wrong with stop log: " + ex.Message);
            }
        }
        */
    }
}
