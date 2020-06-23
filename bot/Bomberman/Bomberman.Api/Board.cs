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
using System;
using System.Collections.Generic;
using System.Linq;
using Bomberman.Api.Infrastructure;

namespace Bomberman.Api
{
    public class Board
    {

        private String BoardString { get; }
        private LengthToXY LengthXY;

        public Board(String boardString, bool useFile)
        {
            var pattern = useFile ? "\r\n" : "\n";
            BoardString = boardString.Replace(pattern, "");
            LengthXY = new LengthToXY(Size);
        }

        /// <summary>
        /// GameBoard size (actual board size is Size x Size cells)
        /// </summary>
        public int Size
        {
            get
            {
                return (int)Math.Sqrt(BoardString.Length);
            }
        }

        public Point GetBomberman()
        {
            return Get(Element.BOMBERMAN)
                    .Concat(Get(Element.BOMB_BOMBERMAN))
                    .Concat(Get(Element.DEAD_BOMBERMAN))
                    .Single();
        }

        public List<Point> GetOtherBombermans()
        {
            return Get(Element.OTHER_BOMBERMAN)
                .Concat(Get(Element.OTHER_BOMB_BOMBERMAN))
                .Concat(Get(Element.OTHER_DEAD_BOMBERMAN))
                .OrderBy(a => a.X)
                .ThenBy(a => a.Y)
                .ToList();
        }

        public bool isMyBombermanDead
        {
            get
            {
                return BoardString.Contains((char)Element.DEAD_BOMBERMAN);
            }
        }

        public Element GetAt(Point point)
        {
            if (point.IsOutOf(Size))
            {
                return Element.WALL;
            }
            return (Element)BoardString[LengthXY.GetLength(point.X, point.Y)];
        }

        public bool IsAt(Point point, Element element)
        {
            if (point.IsOutOf(Size))
            {
                return false;
            }

            return GetAt(point) == element;
        }

        public string BoardAsString()
        {
            string result = "";
            for (int i = 0; i < Size; i++)
            {
                result += BoardString.Substring(i * Size, Size);
                result += "\n";
            }
            return result;
        }

        /// <summary>
        /// gets board view as string
        /// </summary>
        public string ToString()
        {
            return string.Format("{0}\n" +
                     //"Bomberman at: {1}\n" +
                     //"Other bombermans at: {2}\n" +
                     //"Meat choppers at: {3}\n" +
                     //"Destroy walls at: {4}\n" +
                     //"Bombs at: {5}\n" +
                     //"Blasts: {6}\n" +
                     //"Expected blasts at: {7}\n" +
                     //"Perks at: {8}",
                     BoardAsString(),
                     GetBomberman(),
                     ListToString(GetOtherBombermans()),
                     ListToString(GetMeatChoppers()),
                     ListToString(GetDestroyableWalls()),
                     ListToString(GetBombs()),
                     ListToString(GetBlasts()),
                     ListToString(GetFutureBlasts()),
                     ListToString(GetPerks()));
        }

        private string ListToString(List<Point> list)
        {
            return string.Join(",", list.ToArray());
        }

        public List<Point> GetBarrier()
        {
            return GetMeatChoppers()
                .Concat(GetWalls())
                .Concat(GetBombs())
                .Concat(GetDestroyableWalls())
                .Concat(GetOtherBombermans())
                .Distinct()
                .OrderBy(a => a.X)
                .ThenBy(a => a.Y)
                .ToList();
        }

        public List<Point> GetMeatChoppers()
        {
            return Get(Element.MEAT_CHOPPER)
                .OrderBy(a => a.X)
                .ThenBy(a => a.Y)
                .ToList();
        }

        public List<Point> GetMeatChoppersWithNextMoves()
        {
            var choppers = Get(Element.MEAT_CHOPPER);
            var result = new List<Point>();

            foreach (var chopper in choppers)
            {
                result.Add(chopper);
                result.Add(chopper.ShiftTop());
                result.Add(chopper.ShiftRight());
                result.Add(chopper.ShiftBottom());
                result.Add(chopper.ShiftLeft());
            }

            return result.Where(x => !x.IsOutOf(Size) && !GetWalls().Contains(x)).Distinct()
                .OrderBy(a => a.X)
                .ThenBy(a => a.Y)
                .ToList();
        }

        public bool IsNearChopper(Point point)
        {
            return GetMeatChoppersWithNextMoves().Any(b => b.Equals(point));
        }

        public List<Point> Get(Element element)
        {
            List<Point> result = new List<Point>();

            for (int i = 0; i < Size * Size; i++)
            {
                Point pt = LengthXY.GetXY(i);

                if (IsAt(pt, element))
                {
                    result.Add(pt);
                }
            }

            return result;
        }

        public List<Point> Get(Point point, int size, params Element[] elements)
        {
            var topLeft = point.ShiftLeft(size).ShiftTop(size);
            var bottomRight = point.ShiftRight(size).ShiftBottom(size);

            return Get(topLeft, bottomRight, elements);
        }

        public List<Point> Get(Point topLeft, Point bottomRight, params Element[] elements)
        {
            List<Point> result = new List<Point>();

            foreach (var element in elements)
            {
                var foundElements = Get(element);
                result.AddRange(foundElements);

            }

            result = result.Where(a => a.X >= topLeft.X && a.Y <= topLeft.Y).ToList();

            result = result.Where(a => a.X <= bottomRight.X && a.Y >= bottomRight.Y).ToList();

            return result
                .OrderBy(a => a.X)
                .ThenBy(a => a.Y)
                .ToList();
        }

        public List<Point> GetWalls()
        {
            return Get(Element.WALL)
                .OrderBy(a => a.X)
                .ThenBy(a => a.Y)
                .ToList();
        }

        public List<Point> GetDestroyableWalls()
        {
            return Get(Element.DESTROYABLE_WALL)
                .OrderBy(a => a.X)
                .ThenBy(a => a.Y)
                .ToList();
        }

        public List<Point> GetBombs(bool onlyNextStep = false)
        {
            var result = Get(Element.BOMB_TIMER_1);

            if (!onlyNextStep)
            {
                result = result
                    .Concat(Get(Element.BOMB_BOMBERMAN))
                    .Concat(Get(Element.OTHER_BOMB_BOMBERMAN))
                    .Concat(Get(Element.BOMB_TIMER_2))
                    .Concat(Get(Element.BOMB_TIMER_3))
                    .Concat(Get(Element.BOMB_TIMER_4))
                    .Concat(Get(Element.BOMB_TIMER_5))
                    .ToList();
            }

            if (Global.HasPrevBoard)
            {
                var meatChoppers = Get(Element.MEAT_CHOPPER);
                var bombMeatChoppers = new List<Point>();

                if (onlyNextStep)
                {
                    bombMeatChoppers = meatChoppers
                        .Where(x => Global.PrevBoard.IsAt(x, Element.BOMB_TIMER_2)).ToList();
                }
                else
                {
                    bombMeatChoppers = meatChoppers
                        .Where(x => Global.PrevBoard.IsAnyOfAt(x, Constants.BOMB_ELEMENTS)).ToList();
                }

                result.AddRange(bombMeatChoppers);
            }

            return result
                .OrderBy(a => a.X)
                .ThenBy(a => a.Y)
                .ToList();
        }

        public List<Point> GetPerks()
        {
            return Get(Element.BOMB_BLAST_RADIUS_INCREASE)
                .Concat(Get(Element.BOMB_COUNT_INCREASE))
                .Concat(Get(Element.BOMB_IMMUNE))
                .Concat(Get(Element.BOMB_REMOTE_CONTROL))
                .OrderBy(a => a.X)
                .ThenBy(a => a.Y)
                .ToList();
        }

        public List<Point> GetBlasts()
        {
            return Get(Element.BOOM)
                .OrderBy(a => a.X)
                .ThenBy(a => a.Y)
                .ToList();
        }

        public bool IsAtFutureBlast(Point point, int blastSize = 3)
        {
            return GetFutureBlasts().Any(b => b.Equals(point));
        }

        public List<Point> GetFutureBlasts(bool onlyNextStep = false, int? bombPower = null)
        {
            var points = GetBombs(onlyNextStep);

            return GetFutureBlastsForBombs(points, bombPower);
        }

        public List<Point> GetFutureBlastsForBombs(Bomb bomb, int? bombPower = null)
        {
            return GetFutureBlastsForBombs(new List<Bomb> { bomb }, bombPower);
        }

        public List<Point> GetFutureBlastsForBombs(Point point, int? bombPower = null)
        {
            return GetFutureBlastsForBombs(new List<Bomb> { new Bomb(point) }, bombPower);
        }

        public List<Point> GetFutureBlastsForBombs(List<Point> points, int? bombPower = null) 
        {
            return GetFutureBlastsForBombs(points.Select(p => new Bomb(p)).ToList(), bombPower);
        }

        public List<Point> GetFutureBlastsForBombs(List<Bomb> bombs, int? bombPower = null)
        {
            var result = new List<Point>();
            foreach (var bomb in bombs)
            {
                result.Add(bomb.Point);

                //var currentBombBlastSize = Global.Me.MyBombs.GetPoints().Contains(bomb) ? Global.Me.MyBombsPower : blastSize;
                //var currentBombBlastSize = Global.Me.MyBombs.GetPoints().Contains(bomb.Point) ? bomb.Power : blastSize;
                CalculateFutureBlasts(result, bomb, b => b.ShiftTop(), bombPower);
                CalculateFutureBlasts(result, bomb, b => b.ShiftRight(), bombPower);
                CalculateFutureBlasts(result, bomb, b => b.ShiftBottom(), bombPower);
                CalculateFutureBlasts(result, bomb, b => b.ShiftLeft(), bombPower);
            }

            return result.Where(blast => !blast.IsOutOf(Size) && !GetWalls().Contains(blast)).Distinct()
                .OrderBy(a => a.X)
                .ThenBy(a => a.Y)
                .ToList();
        }

        private void CalculateFutureBlasts(List<Point> result, Bomb bomb, Func<Point, Point> func, int? bombPower = null)
        {
            var newBlast = func(bomb.Point);
            if (IsAnyOfAt(newBlast, Element.WALL, Element.DESTROYABLE_WALL))
                return;

            result.Add(newBlast);
            for (var i = 0; i < (bombPower ?? bomb.Power - 1); i++)
            {
                newBlast = func(newBlast);
                if (IsAnyOfAt(newBlast, Element.WALL, Element.DESTROYABLE_WALL))
                    return;

                result.Add(newBlast);
            }
        }

        public bool IsAnyOfAt(Point point, params Element[] elements)
        {
            return elements.Any(elem => IsAt(point, elem));
        }

        public bool IsNear(Point point, Element element)
        {
            if (point.IsOutOf(Size))
                return false;

            return IsAt(point.ShiftLeft(), element) ||
                   IsAt(point.ShiftRight(), element) ||
                   IsAt(point.ShiftTop(), element) ||
                   IsAt(point.ShiftBottom(), element);
        }

        public bool IsBarrierAt(Point point)
        {
            return GetBarrier().Contains(point);
        }

        public int CountNear(Point point, Element element)
        {
            if (point.IsOutOf(Size))
                return 0;

            int count = 0;
            if (IsAt(point.ShiftLeft(), element)) count++;
            if (IsAt(point.ShiftRight(), element)) count++;
            if (IsAt(point.ShiftTop(), element)) count++;
            if (IsAt(point.ShiftBottom(), element)) count++;
            return count;
        }
    }
}
