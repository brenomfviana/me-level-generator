using System;
using System.Linq;

namespace LevelGenerator
{
    class DFS : PathFinding
    {
        public DFS(Dungeon _dungeon)
            : base(_dungeon) {}

        //The DFS Algorithm
        public int FindRoute(
            Dungeon _dungeon,
            ref Random _rand
        ) {
            if (target == null)
            {
                return 0;
            }

            //
            openList.Add(start);
            path.Add(start);
            //
            while (openList.Count > 0)
            {
                // get the first
                Location current = openList.First();
                ValidateKeyRoom(current);

                // add the current square to the closed list
                ClosedList.Add(current);

                if (((map[current.X, current.Y] >= (int) Util.RoomCode.N) &&
                    (map[current.X, current.Y] < (int) Util.RoomCode.C)) ||
                    (map[current.X, current.Y] == (int) Util.RoomCode.B)
                ) {
                    NVisitedRooms++;
                }
                //Check if the actual room is a locked one. If it is, add 1 to the number of locks needed to reach the goal
                foreach (var locked in allLocksLocation)
                {
                    if (locked.X == current.X && locked.Y == current.Y)
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
                    if(ClosedList.FirstOrDefault(l => l.X == target.X && l.Y == target.Y) != null)
                    {
                        break;
                    }
                }

                var adjacentSquares = GetWalkableAdjacentSquares(
                    current.X, current.Y, sizeX, sizeY, map
                );

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
                    if (ClosedList.FirstOrDefault(l => l.X == adjacentSquare.X
                            && l.Y == adjacentSquare.Y) != null)
                    {
                        continue;
                    }

                    // if it's not in the open list...
                    if (openList.FirstOrDefault(l => l.X == adjacentSquare.X
                            && l.Y == adjacentSquare.Y) == null)
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
