using System;
using System.Collections.Generic;
using System.Linq;

namespace LevelGenerator
{
    class PathFinding
    {
        protected Dungeon dungeon;
        protected List<Location> path = new List<Location>();
        protected List<Location> openList = new List<Location>();
        private List<Location> closedList = new List<Location>();

        private int nVisitedRooms = 0;
        private int neededLocks = 0;

        protected Location start = null;
        protected Location target = null;

        protected List<Location> locksLocation = new List<Location>();
        protected List<Location> allLocksLocation = new List<Location>();

        protected Room parent;
        protected RoomGrid grid;

        protected List<int> locks = new List<int>();
        protected List<int> keys = new List<int>();

        protected int minX;
        protected int minY;
        protected int maxX;
        protected int maxY;

        protected int sizeX;
        protected int sizeY;

        protected int[,] map;

        public List<Location> ClosedList { get => closedList; set => closedList = value; }
        public int NVisitedRooms { get => nVisitedRooms; set => nVisitedRooms = value; }
        public int NeededLocks { get => neededLocks; set => neededLocks = value; }

        /// Path Finding constructor.
        public PathFinding (
            Dungeon _dungeon
        ) {
            dungeon = _dungeon;
            neededLocks = 0;
            grid = dungeon.grid;
            minX = RoomGrid.LEVEL_GRID_OFFSET;
            minY = RoomGrid.LEVEL_GRID_OFFSET;
            maxX = -RoomGrid.LEVEL_GRID_OFFSET;
            maxY = -RoomGrid.LEVEL_GRID_OFFSET;
            InitiatePathFinding();
        }

        /// Initialize the path finding setting map, sizes, and rooms, and
        /// filling the grid.
        private void InitiatePathFinding()
        {
            foreach (Room room in dungeon.Rooms)
            {
                if (room == null) { continue; }
                // Update grid bounds
                minX = minX > room.X ? room.X : minX;
                minY = minY > room.Y ? room.Y : minY;
                maxX = room.X > maxX ? room.X : maxX;
                maxY = room.Y > maxY ? room.Y : maxY;
                // Add the keys and locked doors in the level
                if (room.RoomType == RoomType.key)
                {
                    keys.Add(room.KeyToOpen);
                }
                if (room.RoomType == RoomType.locked)
                {
                    locks.Add(room.KeyToOpen);
                }
            }

            // The starting location is room (0,0)
            start = new Location { X = -2 * minX, Y = -2 * minY };
            // Size of the new grid
            sizeX = maxX - minX + 1;
            sizeY = maxY - minY + 1;
            map = new int[2 * sizeX, 2 * sizeY];

            //101 is EMPTY
            for (int i = 0; i < 2 * sizeX; ++i)
            {
                for (int j = 0; j < 2 * sizeY; ++j)
                {
                    map[i, j] = (int) Util.RoomCode.E;
                }
            }

            // Fill the new grid
            for (int i = minX; i < maxX + 1; i++)
            {
                for (int j = minY; j < maxY + 1; j++)
                {
                    //Converts the original coordinates (may be negative) to positive
                    // Get the even positions
                    int iep = (i - minX) * 2;
                    int jep = (j - minY) * 2;
                    // Get the respective room
                    Room current = grid[i, j];
                    //If the position has a room, check its type and fill the grid accordingly
                    if (current != null)
                    {
                        //0 is a NORMAL ROOM
                        if (current.RoomType == RoomType.normal)
                        {
                            map[iep, jep] = (int) Util.RoomCode.N;
                        }
                        //The sequential, positivie index of the key is its representation
                        else if (current.RoomType == RoomType.key)
                        {
                            int _key = keys.IndexOf(current.KeyToOpen);
                            map[iep, jep] = _key + 1;
                        }
                        //If the room is locked, the room is a normal room, only the corridor is locked. But is the lock is the last one in the sequential order, than the room is the objective
                        else if (current.RoomType == RoomType.locked)
                        {
                            int _lock = locks.IndexOf(current.KeyToOpen);
                            map[iep, jep] = _lock == locks.Count - 1 ?
                                (int) Util.RoomCode.B :
                                (int) Util.RoomCode.N;
                            if (_lock == locks.Count - 1)
                            {
                                target = new Location { X = iep * 2, Y = jep * 2 };
                            }
                        }
                        Room parent = dungeon.GetParent(current.index);
                        //If the actual room is a locked room and has a parent, then the connection between then is locked and is represented by the negative value of the index of the key that opens the lock
                        if (parent != null)
                        {
                            // Get the corridor between both rooms
                            int x = parent.X - current.X + iep;
                            int y = parent.Y - current.Y + jep;
                            if (current.RoomType == RoomType.locked)
                            {
                                locksLocation.Add(new Location { X = x, Y = y, Parent = new Location { X = 2 * (parent.X - current.X) + 2 * iep, Y = 2 * (parent.Y - current.Y) + 2 * jep } });
                                int test = keys.IndexOf(current.KeyToOpen);
                                if (test == -1)
                                {
                                    System.Console.WriteLine("There's a missing key here! What????");
                                    Console.ReadKey();
                                    map[x, y] = (int) Util.RoomCode.C;
                                }
                                else
                                    map[x, y] = -(keys.IndexOf(current.KeyToOpen) + 1);
                            }
                            //If the connection is open, 100 represents a normal corridor
                            else
                            {
                                map[x, y] = (int) Util.RoomCode.C;
                            }
                        }
                    }
                }
            }
            //Add all the locks location to the list that will hold their values through the execution of the algorithm
            foreach (var locked in locksLocation)
            {
                allLocksLocation.Add(locked);
            }
        }

        // Check what adjacent rooms exits and can be visited and return the valid ones
        public static List<Location> GetWalkableAdjacentSquares(
            int x,
            int y,
            int sizeX,
            int sizeY,
            int[,] map
        ) {
            List<Location> proposedLocations = new List<Location>();
            if (y > 0)
            {
                proposedLocations.Add(new Location { X = x, Y = y - 1 });
            }
            if (y < (2 * sizeY) - 1)
            {
                proposedLocations.Add(new Location { X = x, Y = y + 1 });
            }
            if (x > 0)
            {
                proposedLocations.Add(new Location { X = x - 1, Y = y });
            }
            if (x < (2 * sizeX) - 1)
            {
                proposedLocations.Add(new Location { X = x + 1, Y = y });
            }
            return proposedLocations.Where(
                l => (map[l.X, l.Y] >= 0 && map[l.X, l.Y] != 101)
            ).ToList();
        }

        // Check if current location is a key room and...
        public void ValidateKeyRoom(Location current)
        {
            if (map[current.X, current.Y] > 0 && map[current.X, current.Y] < 100)
            {
                //If there is still a lock to be open (there may be more keys than locks in the level, so the verification is necessary) find its location and check if the key to open it is the one found
                if (locksLocation.Count > 0)
                {
                    foreach (var room in locksLocation)
                    {
                        //If the key we found is the one that open the room we are checking now, change the lock to an open corridor and update the algorithm's knowledge
                        if (map[room.X, room.Y] == -map[current.X, current.Y])
                        {
                            map[room.X, room.Y] = 100;
                            //remove the lock from the unopenned locks location list
                            locksLocation.Remove(room);
                            //Check if the parent room of the locked room was already closed by the algorithm (if it was in the closed list)
                            foreach (var closedRoom in ClosedList)
                            {
                                //If it was already closed, reopen it. Remove from the closed list and add to the open list
                                if (closedRoom.X == room.Parent.X && closedRoom.Y == room.Parent.Y)
                                {
                                    ClosedList.Remove(closedRoom);
                                    nVisitedRooms--;
                                    //Console.SetCursorPosition(0, 15 + auxoffset);
                                    //auxoffset += 1;
                                    //Console.WriteLine("Removed!");
                                    openList.Add(closedRoom);
                                    //If the closed room was a locked one, also remove one of the needed locks, as it is now reopen and will be revisited
                                    foreach (var locked in allLocksLocation)
                                    {
                                        if (locked.X == closedRoom.X && locked.Y == closedRoom.Y)
                                        {
                                            neededLocks--;
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }

        // Print all the path used by the algorithm
        public static void PrintPathFinding(
            List<Location> path,
            int time = 10
        ) {
            int i = 1;
            foreach (var p in path)
            {
                Console.SetCursorPosition(58, i++);
                Console.WriteLine(p.Y.ToString() + ", " + p.X.ToString());

                // show current square on the map
                Console.SetCursorPosition(p.Y, p.X + 20);
                Console.Write('.');
                Console.SetCursorPosition(p.Y, p.X + 20);
                System.Threading.Thread.Sleep(time);
            }
            Console.SetCursorPosition(0, 40);
        }

        // NEED FIX -> run A* in random.seed(13)
        public static void PrintPathFound(
            Location _current,
            int _time = 10
        ) {
            while (_current != null)
            {
                Console.SetCursorPosition(_current.Y + 20, _current.X + 20);
                Console.Write('_');
                Console.SetCursorPosition(_current.Y + 20, _current.X + 20);
                _current = _current.Parent;
                System.Threading.Thread.Sleep(_time);
            }
            Console.SetCursorPosition(0, 40);
        }
    }
}
