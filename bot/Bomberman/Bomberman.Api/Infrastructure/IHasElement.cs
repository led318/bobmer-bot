﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bomberman.Api.Enums;

namespace Bomberman.Api.Infrastructure
{
    public interface IHasElement
    {
        Element Element { get; set; }
    }
}
