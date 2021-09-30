using System;
using System.Collections.Generic;
using System.Linq;

namespace LevelGenerator
{
    class PathFinding
    {
        //protected int auxoffset = 0;

        private int nVisitedRooms = 0;
        private int neededLocks = 0;
        protected Dungeon dun;
        protected List<Location> path = new List<Location>();

        protected Location current = null;
        protected Location start = null;
        protected Location target = null;

        protected List<Location> locksLocation = new List<Location>();
        protected List<Location> allLocksLocation = new List<Location>();

        protected Room actualRoom, parent;
        protected RoomGrid grid;

        protected RoomType type;

        protected int x, y, iPositive, jPositive;

        protected List<int> lockedRooms = new List<int>();
        protected List<int> keys = new List<int>();

        protected List<Location> openList = new List<Location>();
        private List<Location> closedList = new List<Location>();

        protected int minX, minY, maxX, maxY;

        protected int sizeX;
        protected int sizeY;

        protected int[,] map;

        public List<Location> ClosedList { get => closedList; set => closedList = value; }
        public int NVisitedRooms { get => nVisitedRooms; set => nVisitedRooms = value; }
        public int NeededLocks { get => neededLocks; set => neededLocks = value; }

        // Constructor
        public PathFinding (Dungeon dun)
        {
            this.dun = dun;
            neededLocks= 0;

            grid = dun.grid;
            minX = RoomGrid.LEVEL_GRID_OFFSET;
            minY = RoomGrid.LEVEL_GRID_OFFSET;
            maxX = -RoomGrid.LEVEL_GRID_OFFSET;
            maxY = -RoomGrid.LEVEL_GRID_OFFSET;

            initiatePathFinding();
        }

        // Check what adjacent rooms exits and can be visited and return the valid ones
        public static List<Location> GetWalkableAdjacentSquares(int x, int y, int sizeX, int sizeY, int[,] map)
        {
            var proposedLocations = new List<Location>();
            if (y > 0)
                proposedLocations.Add(new Location { x = x, y = y - 1 });
            if (y < (2 * sizeY) - 1)
                proposedLocations.Add(new Location { x = x, y = y + 1 });
            if (x > 0)
                proposedLocations.Add(new Location { x = x - 1, y = y });
            if (x < (2 * sizeX) - 1)
                proposedLocations.Add(new Location { x = x + 1, y = y });

            return proposedLocations.Where(l => (map[l.x, l.y] >= (int) Common.RoomCode.N && map[l.x, l.y] != (int) Common.RoomCode.E)).ToList();
        }

        // Check if current location is a key room and...
        public void validateKeyRoom(Location current)
        {
            if (map[current.x, current.y] > 0 && map[current.x, current.y] < (int) Common.RoomCode.C)
            {
                //If there is still a lock to be open (there may be more keys than locks in the level, so the verification is necessary) find its location and check if the key to open it is the one found
                if (locksLocation.Count > 0)
                {
                    foreach (var room in locksLocation)
                    {
                        //If the key we found is the one that open the room we are checking now, change the lock to an open corridor and update the algorithm's knowledge
                        if (map[room.x, room.y] == -map[current.x, current.y])
                        {
                            map[room.x, room.y] = (int) Common.RoomCode.C;
                            //remove the lock from the unopenned locks location list
                            locksLocation.Remove(room);
                            //Check if the parent room of the locked room was already closed by the algorithm (if it was in the closed list)
                            foreach (var closedRoom in ClosedList)
                            {
                                //If it was already closed, reopen it. Remove from the closed list and add to the open list
                                if (closedRoom.x == room.Parent.x && closedRoom.y == room.Parent.y)
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
                                        if (locked.x == closedRoom.x && locked.y == closedRoom.y)
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
        public static void PrintPathFinding(List<Location> path, int time = 10)
        {
            int i = 1;
            foreach (var p in path)
            {
                Console.SetCursorPosition(58, i++);
                Console.WriteLine(p.y.ToString() + ", " + p.x.ToString());

                // show current square on the map
                Console.SetCursorPosition(p.y, p.x + 20);
                Console.Write('.');
                Console.SetCursorPosition(p.y, p.x + 20);
                System.Threading.Thread.Sleep(time);
            }
            Console.SetCursorPosition(0, 40);
        }

        // NEED FIX -> run A* in random.seed(13)
        public static void PrintPathFound(Location current, int time = 10)
        {
            while (current != null)
            {
                Console.SetCursorPosition(current.y + 20, current.x + 20);
                Console.Write('_');
                Console.SetCursorPosition(current.y + 20, current.x + 20);
                current = current.Parent;
                System.Threading.Thread.Sleep(time);
            }
            Console.SetCursorPosition(0, 40);
        }

        // Initiate the path finding setting map, sizes, rooms and filling the grid
        private void initiatePathFinding()
        {
            foreach (Room room in dun.Rooms)
            {
                if (room.type == RoomType.Key)
                    keys.Add(room.key);
                if (room.type == RoomType.Locked)
                    lockedRooms.Add(room.key);

                if (room.x < minX)
                    minX = room.x;
                if (room.y < minY)
                    minY = room.y;
                if (room.x > maxX)
                    maxX = room.x;
                if (room.y > maxY)
                    maxY = room.y;
            }

            //The starting location is room (0,0)
            start = new Location { x = -2 * minX, y = -2 * minY };
            //Size of the new grid
            sizeX = maxX - minX + 1;
            sizeY = maxY - minY + 1;
            map = new int[2 * sizeX, 2 * sizeY];

            //101 is EMPTY
            for (int i = 0; i < 2 * sizeX; ++i)
            {
                for (int j = 0; j < 2 * sizeY; ++j)
                {
                    map[i, j] = (int) Common.RoomCode.E;
                }
            }
            //Fill the new grid
            for (int i = minX; i < maxX + 1; ++i)
            {
                for (int j = minY; j < maxY + 1; ++j)
                {
                    //Converts the original coordinates (may be negative) to positive
                    iPositive = i - minX;
                    jPositive = j - minY;
                    actualRoom = grid[i, j];
                    //If the position has a room, check its type and fill the grid accordingly
                    if (actualRoom != null)
                    {
                        type = actualRoom.type;
                        //0 is a NORMAL ROOM
                        if (type == RoomType.Normal)
                        {
                            map[iPositive * 2, jPositive * 2] = (int) Common.RoomCode.N;
                        }
                        //The sequential, positivie index of the key is its representation
                        else if (type == RoomType.Key)
                        {
                            map[iPositive * 2, jPositive * 2] = keys.IndexOf(actualRoom.key) + 1;
                        }
                        //If the room is locked, the room is a normal room, only the corridor is locked. But is the lock is the last one in the sequential order, than the room is the objective
                        else if (type == RoomType.Locked)
                        {
                            if (lockedRooms.IndexOf(actualRoom.key) == lockedRooms.Count - 1)
                            {
                                map[iPositive * 2, jPositive * 2] = (int) Common.RoomCode.B;
                                target = new Location { x = iPositive * 2, y = jPositive * 2 };
                            }
                            else
                                map[iPositive * 2, jPositive * 2] = (int) Common.RoomCode.N;
                        }
                        else
                        {
                            Console.WriteLine("Something went wrong printing the tree!\n");
                            Console.WriteLine("This Room type does not exist!\n\n");
                        }
                        parent = actualRoom.parent;
                        //If the actual room is a locked room and has a parent, then the connection between then is locked and is represented by the negative value of the index of the key that opens the lock
                        if (parent != null)
                        {

                            x = parent.x - actualRoom.x + 2 * iPositive;
                            y = parent.y - actualRoom.y + 2 * jPositive;
                            if (type == RoomType.Locked)
                            {
                                locksLocation.Add(new Location { x = x, y = y, Parent = new Location { x = 2 * (parent.x - actualRoom.x) + 2 * iPositive, y = 2 * (parent.y - actualRoom.y) + 2 * jPositive } });
                                int test = keys.IndexOf(actualRoom.key);
                                if (test == -1)
                                {
                                    System.Console.WriteLine("There's a missing key here! What????");
                                    Console.ReadKey();
                                    map[x, y] = (int) Common.RoomCode.C;
                                }
                                else
                                    map[x, y] = -(keys.IndexOf(actualRoom.key) + 1);
                            }
                            //If the connection is open, 100 represents a normal corridor
                            else
                                map[x, y] = (int) Common.RoomCode.C;
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
    }
}
