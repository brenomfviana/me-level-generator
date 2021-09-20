namespace LevelGenerator
{
    /// This class represents the grid of rooms of levels.
    public class RoomGrid
    {
        /// The level grid offset.
        public static readonly int LEVEL_GRID_OFFSET = 150;

        /// This matrix holds the grids of rooms corresponding to a Dungeon.
        private Room[,] grid;

        /// Define the `[]` operator for the grid.
        ///
        /// `get`: Return the room of the given position (coordinate x and y).
        /// `set`: Assign a room to the given position (coordinate x and y).
        public Room this[int x, int y]
        {
            get => grid[x + LEVEL_GRID_OFFSET, y + LEVEL_GRID_OFFSET];
            set => grid[x + LEVEL_GRID_OFFSET, y + LEVEL_GRID_OFFSET] = value;
        }

        /// Room Grid constructor.
        ///
        /// Initialize the room grid with the maximum matrix offset.
        public RoomGrid()
        {
            grid = new Room[LEVEL_GRID_OFFSET * 2, LEVEL_GRID_OFFSET * 2];
        }
    }
}
