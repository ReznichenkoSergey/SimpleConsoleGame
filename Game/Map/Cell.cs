using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Game.GameObjects;

namespace Game
{
    public enum CellState
    {
        HasShip,
        HadShip,
        IsEmpty, 
        IsShooted
    }
    public class Cell
    {
        public GameObject GameObject { get; set; }

        public Cell()
        {
        }

        public Cell(GameObject gameObject)
        {
            GameObject = gameObject;
        }

        public bool IsEmpty()
        {
            return GameObject == null;
        }
    }
}
