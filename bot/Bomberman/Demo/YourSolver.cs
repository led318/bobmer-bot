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
        private bool _isActCurrentMove;

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

                CalculateActCurrentMove();

                Console.WriteLine(Global.NearPoints.GetPrintStr());
                CalculateAvailableDirection();
                CheckUseRC();

                ProcessActCurrentMove();

                if (!Global.HasManualMove)
                {
                    _currentMoves.Add(_currentDirection);
                }
            }

            if (Global.HasManualMove)
                Global.ManualMove = string.Empty;
        }

        private void ProcessActCurrentMove()
        {
            if (_isActCurrentMove)
            {
                if (!Global.Me.HaveDirectAfkTargetCurrentStep
                    && !Global.Me.NearEnemies.Any()
                    && Global.Me.HaveMoreDestroyableWallsNextStep)
                {
                    Console.WriteLine("skip act current move, next move more walls");
                    return;
                }

                if (!Global.Me.HaveDirectAfkTargetCurrentStep
                         && !Global.Me.NearMeatChoppers.Any()
                         && Global.Me.HaveDirectAfkTargetNextStep)
                {
                    Console.WriteLine("skip act current move, next move afk target");
                    return;
                }

                if (Global.Me.HaveDirectBonus() && !Global.Me.HaveDirectAfkTargetCurrentStep)
                {
                    Console.WriteLine("skip act current move, to not produce zombie");
                    return;
                }

                if (_currentDirection == Direction.Stop)
                {
                    Console.WriteLine("skip act current move, is blocked");
                    return;
                }

                if (!Global.HasManualMove)
                {
                    _currentMoves.Add(Direction.Act);
                    Global.Me.SetMyBomb();
                    Console.WriteLine("ACT CURRENT MOVE!!!!!!");
                }
            }
        }

        private void WriteToLog(string str)
        {
            using (var log = new StreamWriter(_logPath, true))
            {
                log.WriteLine(str);
            }
        }

        private void CheckUseRC()
        {
            if (!_isActCurrentMove && Global.Me.CanUseRC)
            {
                Global.Me.UseRC();
                ActCurrentMove();
            }
        }

        private void CalculateActCurrentMove()
        {
            _isActCurrentMove = false;


            if (Global.Me.CanPlaceBombs && !Global.Me.IsMyBombRC)
            {
                if (Global.Me.HaveDirectAfkTargetCurrentStep 
                    || Global.Me.NearEnemies.Any() 
                    || Global.Me.HaveDestroyableWallsNear)
                {
                    ActCurrentMove();
                }
            }

            Global.NearPoints.InitActNearPoints(_isActCurrentMove);
        }

        private void ActCurrentMove()
        {
            _isActCurrentMove = true;
        }

        private void CalculateAvailableDirection()
        {
            if (Global.NearPoints.OnlyCriticalDangerPoints)
            {
                SetCurrentDirection();
                WriteStopLog();

                return;
            }

            var minRatingPoints = Global.NearPoints.GetMinRatingPoints();
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
            Global.Me.PreviousMove = nearPoint;
        }

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
    }
}
