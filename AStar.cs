using System;
using System.Collections.Generic;
using System.Linq;

/// Thanks to https://bitbucket.org/dandago/experimental/src/7adeb5f8cdfb054b540887d53cabf27e22a10059/AStarPathfinding/?at=master
namespace LevelGenerator
{
    //Class with location, heuristic and real distances of the room and the room that was used to go to the actual one (parent)
    class Location
    {
        public int x;
        public int y;
        public int F;
        public int G;
        public int H;
        public Location Parent;
    }

    class AStar
    {
        //Size of the grid
        static int sizeX;
        static int sizeY;

        //The A* algorithm
        public static int FindRoute(Dungeon dun)
        {

            List<Location> path = new List<Location>();


            Location current = null;
            Location start;
            Location target = null;
            //Location of the locks that were still not opened
            List<Location> locksLocation = new List<Location>();
            //Location of all the locks since the start of the algorithm
            List<Location> allLocksLocation = new List<Location>();
            //Counter for all the locks that were opened during the A* execution
            //TODO: Make the A* (or another algorithm) use only the really needed ones, the A* in the search phase opens some unecessary locked doors, but this could be prevented
            //By making partial A* from the start to the key of the first locked door found, then from the key to the door, from the door to the key to the next locked one, and so on
            int neededLocks = 0;
            Room actualRoom, parent;
            //The grid with the rooms
            RoomGrid grid = dun.grid;
            //Type of the room
            RoomType type;
            //X and Y coordinates
            int x, y, iPositive, jPositive;
            //List of all the locked rooms
            List<int> lockedRooms = new List<int>();
            //List of all the keys
            List<int> keys = new List<int>();
            //int nLockedRooms = 0;
            //Min and max boundaries of the grid based on the original grid
            int minX, minY, maxX, maxY;
            minX = RoomGrid.LEVEL_GRID_OFFSET;
            minY = RoomGrid.LEVEL_GRID_OFFSET;
            maxX = -RoomGrid.LEVEL_GRID_OFFSET;
            maxY = -RoomGrid.LEVEL_GRID_OFFSET;
            //Check all the rooms and add them to the keys and locks lists if they are one of them
            foreach (Room room in dun.Rooms)
            {
                if (room.type == RoomType.Key)
                {
                    keys.Add(room.key);
                }
                if (room.type == RoomType.Locked)
                {
                    lockedRooms.Add(room.key);
                }
                //Check the boundaries of the farthest rooms in the grid
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
            //List of visited rooms that are not closed yet
            var openList = new List<Location>();
            //List of closed rooms. They were visited and all neighboors explored.
            var closedList = new List<Location>();
            int g = 0;
            //Size of the new grid
            sizeX = maxX - minX + 1;
            sizeY = maxY - minY + 1;
            int[,] map = new int[2*sizeX, 2*sizeY];

            //101 is EMPTY
            for (int i = 0; i < 2*sizeX; ++i)
            {
                for (int j = 0; j < 2*sizeY; ++j)
                {
                    map[i, j] = 101;
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
                            map[iPositive * 2, jPositive * 2] = 0;
                        }
                        //The sequential, positivie index of the key is its representation
                        else if (type == RoomType.Key)
                        {
                            map[iPositive * 2, jPositive * 2] = keys.IndexOf(actualRoom.key)+1;
                        }
                        //If the room is locked, the room is a normal room, only the corridor is locked. But is the lock is the last one in the sequential order, than the room is the objective
                        else if (type == RoomType.Locked)
                        {
                            if (lockedRooms.IndexOf(actualRoom.key) == lockedRooms.Count -1)
                            {
                                map[iPositive * 2, jPositive * 2] = 102;
                                target = new Location { x = iPositive * 2, y = jPositive * 2 };
                            }
                            else
                                map[iPositive * 2, jPositive * 2] = 0;
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
                                locksLocation.Add(new Location { x = x, y = y, Parent = new Location { x = 2*(parent.x-actualRoom.x) +2*iPositive, y = 2 * (parent.y - actualRoom.y) + 2 * jPositive } });
                                map[x, y] = -(keys.IndexOf(actualRoom.key)+1);
                            }
                            //If the connection is open, 100 represents a normal corridor
                            else
                                map[x, y] = 100;
                        }
                    }
                }
            }
            //Add all the locks location to the list that will hold their values through the execution of the algorithm
            foreach(var locked in locksLocation)
            {
                allLocksLocation.Add(locked);
            }


            //start by adding the original position to the open list
            openList.Add(start);
            //While there are rooms to visit in the path
            while (openList.Count > 0)
            {
                // get the square with the lowest F score
                var lowest = openList.Min(l => l.F);
                current = openList.First(l => l.F == lowest);
                //if the current is a key, change the locked door status in the map
                if (map[current.x, current.y] > 0 && map[current.x, current.y] < 100)
                {
                    //If there is still a lock to be open (there may be more keys than locks in the level, so the verification is necessary) find its location and check if the key to open it is the one found
                    if (locksLocation.Count > 0)
                    {
                        foreach (var room in locksLocation)
                        {
                            //If the key we found is the one that open the room we are checking now, change the lock to an open corridor and update the algorithm's knowledge
                            if (map[room.x, room.y] == -map[current.x, current.y])
                            {
                                map[room.x, room.y] = 100;
                                //remove the lock from the unopenned locks location list
                                locksLocation.Remove(room);
                                //Check if the parent room of the locked room was already closed by the algorithm (if it was in the closed list)
                                foreach (var closedRoom in closedList)
                                {
                                    //If it was already closed, reopen it. Remove from the closed list and add to the open list
                                    if (closedRoom.x == room.Parent.x && closedRoom.y == room.Parent.y)
                                    {
                                        closedList.Remove(closedRoom);
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

                // add the current square to the closed list
                closedList.Add(current);
                //Check if the actual room is a locked one. If it is, add 1 to the number of locks needed to reach the goal
                foreach(var locked in allLocksLocation)
                {
                    if(locked.x == current.x && locked.y == current.y)
                    {
                        //Console.WriteLine("NEED A LOCK");
                        neededLocks++;
                        break;
                    }
                }
                // show current square on the map
                //Console.SetCursorPosition(current.y, current.x + 20);
                //Console.Write('.');
                //Console.SetCursorPosition(current.y, current.x + 20);
                //System.Threading.Thread.Sleep(100);

                // remove it from the open list
                openList.Remove(current);
                //Console.WriteLine("Check Path");
                // if we added the destination to the closed list, we've found a path
                if(closedList != null)
                    if (closedList.FirstOrDefault(l => l.x == target.x && l.y == target.y) != null)
                        break;

                var adjacentSquares = GetWalkableAdjacentSquares(current.x, current.y, map);
                g++;

                foreach (var adjacentSquare in adjacentSquares)
                {
                    // if this adjacent square is already in the closed list, ignore it
                    if (closedList.FirstOrDefault(l => l.x == adjacentSquare.x
                            && l.y == adjacentSquare.y) != null)
                        continue;

                    // if it's not in the open list...
                    if (openList.FirstOrDefault(l => l.x == adjacentSquare.x
                            && l.y == adjacentSquare.y) is null)
                    {
                        // compute its score, set the parent
                        adjacentSquare.G = g;
                        adjacentSquare.H = ComputeHScore(adjacentSquare.x, adjacentSquare.y, target.x, target.y);
                        adjacentSquare.F = adjacentSquare.G + adjacentSquare.H;
                        adjacentSquare.Parent = current;

                        // and add it to the open list
                        path.Add(adjacentSquare);
                        openList.Insert(0, adjacentSquare);
                    }
                    else
                    {
                        // test if using the current G score makes the adjacent square's F score
                        // lower, if yes update the parent because it means it's a better path
                        if (g + adjacentSquare.H < adjacentSquare.F)
                        {
                            adjacentSquare.G = g;
                            adjacentSquare.F = adjacentSquare.G + adjacentSquare.H;
                            adjacentSquare.Parent = current;
                        }
                    }
                }
            }
            // assume path was found; let's show it
            /*while (current != null)
            {
                Console.SetCursorPosition(current.y+20, current.x + 20);
                Console.Write('_');
                Console.SetCursorPosition(current.y+20, current.x + 20);
                current = current.Parent;
                System.Threading.Thread.Sleep(10);
            }*/
            // end
            //Console.WriteLine("NRooms: " + dun.Rooms.Count);
            //Console.WriteLine("OpenList: "+openList.Count + "  Closed List:"+ closedList.Count);
            return neededLocks;
        }

        //Check what adjacent rooms exits and can be visited and return the valid ones
        static List<Location> GetWalkableAdjacentSquares(int x, int y, int[,] map)
        {
            var proposedLocations = new List<Location>();
            if (y > 0)
                proposedLocations.Add(new Location { x = x, y = y - 1 });
            if (y < (2 * sizeY)-1)
                proposedLocations.Add(new Location { x = x, y = y + 1 });
            if (x > 0)
                proposedLocations.Add(new Location { x = x - 1, y = y });
            if (x < (2 * sizeX)-1)
                proposedLocations.Add(new Location { x = x + 1, y = y });

            return proposedLocations.Where(l => (map[l.x,l.y] >= 0 && map[l.x,l.y] != 101)).ToList();
        }

        //Compute the heuristic score, in this case, a Manhattan Distance
        static int ComputeHScore(int x, int y, int targetX, int targetY)
        {
            return Math.Abs(targetX - x) + Math.Abs(targetY - y);
        }
    }
}
