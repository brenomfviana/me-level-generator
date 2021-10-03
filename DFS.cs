using System;
using System.Linq;

namespace LevelGenerator
{
    class DFS : PathFinding
    {
        public DFS(Dungeon _dungeon)
            : base(_dungeon) {}

        public int FindRoute(
            Dungeon _dun,
            ref Random _rand
        ) {
            if (target == null)
            {
                return 0;
            }
            //The starting location is room (0,0)
            Location start = new Location { x = -2 * dun.minX, y = -2 * dun.minY };
            openList.Add(start);
            path.Add(start);
            while (openList.Count > 0)
            {
                Location current = openList.First();
                ValidateKeyRoom(current);
                ClosedList.Add(current);
                if (
                    ((map[current.x, current.y] >= (int) Common.RoomCode.N) &&
                    (map[current.x, current.y] < (int) Common.RoomCode.C)) ||
                    (map[current.x, current.y] == (int) Common.RoomCode.B)
                ) {
                    NVisitedRooms++;
                }
                // Check if the actual room is a locked one. If it is, add 1 to the number of locks needed to reach the goal
                foreach (var locked in allLocksLocation)
                {
                    if (locked.x == current.x && locked.y == current.y)
                    {
                        NeededLocks++;
                        break;
                    }
                }
                // remove it from the open list
                openList.Remove(current);
                // if we added the destination to the closed list, we've found a path
                if (ClosedList.Count > 0)
                {
                    if(ClosedList.FirstOrDefault(l => l.x == target.x && l.y == target.y) != null)
                    {
                        break;
                    }
                }

                var adjacentSquares = GetWalkableAdjacentSquares(current.x, current.y, sizeX, sizeY, map);

                int value = _rand.Next();
                adjacentSquares = adjacentSquares.OrderBy(X => value).ToList();

                foreach (var adjacentSquare in adjacentSquares)
                {
                    if (current.Parent == adjacentSquare)
                    {
                        adjacentSquares.Remove(adjacentSquare);
                        adjacentSquares.Add(adjacentSquare);
                        break;
                    }
                }

                foreach (var adjacentSquare in adjacentSquares)
                {
                    // if this adjacent square is already in the closed list, ignore it
                    if (ClosedList.FirstOrDefault(l => l.x == adjacentSquare.x
                            && l.y == adjacentSquare.y) != null)
                        continue;

                    // if it's not in the open list...
                    if (openList.FirstOrDefault(l => l.x == adjacentSquare.x
                            && l.y == adjacentSquare.y) is null)
                    {
                        adjacentSquare.Parent = current;

                        // and add it to the open list and add to your path
                        openList.Insert(0, adjacentSquare);
                        path.Add(adjacentSquare);
                    }
                    else
                    {
                        adjacentSquare.Parent = current;
                    }
                }
            }

            return NeededLocks;
        }
    }

}
