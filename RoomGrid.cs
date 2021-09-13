using System;

namespace LevelGenerator
{
    /// This class represents the grid of rooms of levels.
    public class RoomGrid
    {
        /// This matrix holds the grids of rooms corresponding to a Dungeon.
        private Room[,] grid;

        /// Define the `[]` operator for the grid.
        ///
        /// `get`: Return the room of the given position (coordinate x and y).
        /// `set`: Assign a room to the given position (coordinate x and y).
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

        /// Room Grid constructor.
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
