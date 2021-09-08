using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LevelGenerator
{
    /// This class represents the grid of rooms of levels.
    public class RoomGrid
    {
        /// .
        private Room[,] grid;

        /// .
        public Room this[int X, int Y]
        {
            get => grid[
                    X + Constants.MATRIXOFFSET,
                    Y + Constants.MATRIXOFFSET
                ];
            set => grid[
                    X + Constants.MATRIXOFFSET,
                    Y + Constants.MATRIXOFFSET
                ] = value;
        }

        /// Room grid constructor.
        ///
        /// Initialize the room grid with the maximum matrix offset.
        public RoomGrid()
        {
            grid = new Room[
                Constants.MATRIXOFFSET * 2,
                Constants.MATRIXOFFSET * 2
            ];
        }
    }
}
