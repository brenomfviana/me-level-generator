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

        protected Location target = null;

        protected List<Location> locksLocation = new List<Location>();
        protected List<Location> allLocksLocation = new List<Location>();

        protected RoomGrid grid;

        protected List<Location> openList = new List<Location>();
        private List<Location> closedList = new List<Location>();

        protected int sizeX;
        protected int sizeY;

        protected int[,] map;

        public List<Location> ClosedList { get => closedList; set => closedList = value; }
        public int NVisitedRooms { get => nVisitedRooms; set => nVisitedRooms = value; }
        public int NeededLocks { get => neededLocks; set => neededLocks = value; }

        // Constructor
        public PathFinding (
            Dungeon _dungeon
        ) {
            dun = _dungeon;
            grid = dun.grid;
            neededLocks= 0;
            InitiatePathFinding();
        }

        // Initiate the path finding setting map, sizes, rooms and filling the grid
        private void InitiatePathFinding()
        {
            //Size of the new grid
            sizeX = dun.maxX - dun.minX + 1;
            sizeY = dun.maxY - dun.minY + 1;
            map = new int[2 * sizeX, 2 * sizeY];

            //101 is EMPTY
            for (int i = 0; i < 2 * sizeX; i++)
            {
                for (int j = 0; j < 2 * sizeY; j++)
                {
                    map[i, j] = (int) Common.RoomCode.E;
                }
            }
            // Set the corridors, keys and locked rooms
            for (int i = dun.minX; i < dun.maxX + 1; i++)
            {
                for (int j = dun.minY; j < dun.maxY + 1; j++)
                {
                    int iep = (i - dun.minX) * 2;
                    int jep = (j - dun.minY) * 2;
                    Room current = grid[i, j];
                    if (current != null)
                    {
                        if (current.type == RoomType.Normal)
                        {
                            map[iep, jep] = (int) Common.RoomCode.N;
                        }
                        // The key ID is the sequential positive index
                        else if (current.type == RoomType.Key)
                        {
                            map[iep, jep] = dun.keyIds.IndexOf(current.key) + 1;
                        }
                        // If the room is locked, the room is a normal room,
                        // and the corridor is locked; but if the lock is the
                        // last one in the sequential order, then the room is
                        // the goal room
                        else if (current.type == RoomType.Locked)
                        {
                            if (dun.lockIds.IndexOf(current.key) == dun.lockIds.Count - 1)
                            {
                                map[iep, jep] = (int) Common.RoomCode.B;
                                target = new Location { x = iep, y = jep };
                            }
                            else
                                map[iep, jep] = (int) Common.RoomCode.N;
                        }
                        // If the current room is a locked room and has a
                        // parent, then the connection between then is locked
                        // and is represented by the negative value of the
                        // index of the key that opens the lock
                        Room parent = current.parent;
                        if (parent != null)
                        {
                            // Get the corridor between both rooms
                            int x = parent.x - current.x + iep;
                            int y = parent.y - current.y + jep;
                            // If the current room is locked
                            if (current.type == RoomType.Locked)
                            {
                                // Then, the corridor is locked
                                locksLocation.Add(new Location { x = x, y = y, Parent = new Location { x = 2 * (parent.x - current.x) + iep, y = 2 * (parent.y - current.y) + jep } });
                                int key = dun.keyIds.IndexOf(current.key);
                                if (key == -1)
                                {
                                    System.Console.WriteLine("There's a missing key here! What???? Its ID is " + current.key);
                                    map[x, y] = (int) Common.RoomCode.C;
                                }
                                else
                                {
                                    map[x, y] = -(key + 1);
                                }
                            }
                            else
                            {
                                // Otherwise it is an usual corridor
                                map[x, y] = (int) Common.RoomCode.C;
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
        public void ValidateKeyRoom(Location current)
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
    }
}
