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
using Bomberman.Api.Enums;
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
            Global.Bombs = new Bombs();
            Global.Choppers = new Choppers();
            Global.RoundBoards = new List<Board>();
            Global.BoardState = new BoardState();
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
            sw.Stop();
            Console.WriteLine($"elapsed: {sw.ElapsedMilliseconds}ms");

            return string.Join(",", _currentMoves);
        }

        private void Process(Board board)
        {
            _currentMoves.Clear();
            Global.RoundBoards.Add(board);
            Global.RoundTick = Global.RoundBoards.Count - 1;

            if (board.isMyBombermanDead)
            {
                //Global.RoundTick = 0;
                Global.RoundBoards.Clear();

                Global.Me.Clear();
                _currentMoves.Add(Direction.Stop);
                Global.OtherBombermans.Clear();
                Config.ManualSuicide = false;
            }
            else
            {
                /*
                if (Global.RoundTick % 20 == 0)
                    Global.Choppers.WriteChopperLogs();
                */

                Global.BoardState.Init();
               // Global.BoardState.PrintToConsole();
                Global.Bombs.Init();

                Console.WriteLine("round tick: " + Global.RoundTick);
                Global.Me.Tick();
                Global.Me.Point = Global.Board.GetBomberman();

                Global.Choppers.Init();
                Global.OtherBombermans.Init();

                if (Global.Me.CheckSuicide())
                {
                    _currentMoves.Add(Direction.Stop);
                    _currentMoves.Add(Direction.Act);
                    //Global.RoundTick++;
                    return;
                }

                Global.Me.InitNearEnemies();
                Global.Me.CheckMyBombs();
                Global.NearPoints.Init();
                Global.Me.PrintStatus();

                CalculateActCurrentMove();

                Console.WriteLine(Global.NearPoints.GetPrintStr());

                CalculateAvailableDirection();
                CheckUseRC();

                if (_isActCurrentMove)
                {
                    if (!Global.Me.NearEnemies.Any() && Global.Me.HaveMoreDestroyableWallsNextStep)
                    {
                        Console.WriteLine("skip act current move, next move more walls");
                    }
                    else if (!Global.Me.NearMeatChoppers.Any() && Global.Me.HaveDirectAfkTargetNextStep && !Global.Me.HaveDirectAfkTargetCurrentStep)
                    {
                        Console.WriteLine("skip act current move, next move afk target");
                    }
                    else
                    {
                        _currentMoves.Add(Direction.Act);
                        Global.Me.SetMyBomb();
                        Console.WriteLine("ACT CURRENT MOVE!!!!!!");
                    }
                }

                _currentMoves.Add(_currentDirection);
                // Global.RoundTick++;
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


            if (Global.Me.CanPlaceBombs)
            {
                if (Global.Me.NearEnemies.Any() || Global.Me.HaveDestroyableWallsNear)
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
            if (Global.NearPoints.OnlyDangerPoints)
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
            Global.Me.PreviousStep = nearPoint;
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
