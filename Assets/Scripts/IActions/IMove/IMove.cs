﻿using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Interfaces
{
    public interface IMove
    {
        public UniTask Move(BaseUnit unit, Tile targetTile);
    }
}
