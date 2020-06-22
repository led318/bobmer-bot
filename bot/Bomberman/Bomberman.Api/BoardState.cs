using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bomberman.Api.Infrastructure;

namespace Bomberman.Api
{
    public class BoardState
    {
        public BoardPoint[] BoardMap { get; private set; }
        private LengthToXY _lengthToXy = new LengthToXY(Config.BoardSize);

        public BoardState()
        {
            BoardMap = new BoardPoint[Config.BoardSize * Config.BoardSize];

            Init(true);
        }

        private void SetBoardPoint(BoardPoint boardPoint)
        {
            var length = _lengthToXy.GetLength(boardPoint.Point.X, boardPoint.Point.Y);

            BoardMap[length] = boardPoint;
        }

        public BoardPoint GetBoardPoint(int x, int y)
        {
            var length = _lengthToXy.GetLength(x, y);

            var boardPoint = BoardMap[length];

            return boardPoint;
        }

        public BoardPoint GetBoardPoint(Point point)
        {
            return GetBoardPoint(point.X, point.Y);
        }

        public BoardPoint GetBoardPoint(IHasPoint hasPoint)
        {
            return GetBoardPoint(hasPoint.Point);
        }

        public void Init(bool empty = false)
        {
            for (var i = 0; i < Config.BoardSize; i++)
            {
                for (var j = 0; j < Config.BoardSize; j++)
                {
                    var point = new Point(i, j);
                    var boardPoint = new BoardPoint(point);

                    if (!empty)
                    {
                        boardPoint.Init();
                    }

                    SetBoardPoint(boardPoint);
                }
            }
        }



        /*
        public BoardPoint GetBoardPoint(Point point)
        {
            return BoardMap[point.X, point.Y];
        }
        */

        public void PrintToConsole()
        {
            Console.WriteLine("=== board state ===");

            for (var i = 0; i < Config.BoardSize; i++)
            {
                for (var j = 0; j < Config.BoardSize; j++)
                {
                    GetBoardPoint(i, j).PrintToConsole();
                }

                Console.WriteLine();
            }

            Console.WriteLine("=== ===== ===== ===");
        }
    }
}
