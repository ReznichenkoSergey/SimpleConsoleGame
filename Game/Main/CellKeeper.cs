using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Main
{
    public class CellKeeper
    {
        public Guid Id { get; set; }

        public int PositionX { get; set; }

        public int PositionY { get; set; }

        public CellKeeper(Guid id, int x, int y)
        {
            this.Id = id;
            this.PositionX = x;
            this.PositionY = y;
        }
    }
}
