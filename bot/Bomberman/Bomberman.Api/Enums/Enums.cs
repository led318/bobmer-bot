using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Api.Enums
{
    public enum MovePosition
    {
        Stop, //position not changed
        Top,
        Right,
        Bottom,
        Left,
        Unknown, //position not found
    }

    public enum MoveDirection
    {
        Forward,
        Backward,
        Right,
        Left,
        Unknown
    }
}
