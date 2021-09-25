using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace LevelGenerator
{
    /// This class holds the crossover operator.
    public class Crossover
    {
        /// Choose a random room to switch between the parents and arrange
        /// every aspect of the room needed after the change. Including the
        /// grid, and also the exceptions where the new nodes overlap the old
        /// ones.
        public static Individual[] Apply(
            Individual _parent1,
            Individual _parent2,
            ref Random _rand
        ) {
            // Initialize the two new individuals
            Individual[] individuals = new Individual[2];

            // Initialize the offspring
            Dungeon dungeon1;
            Dungeon dungeon2;
            // Console.WriteLine("=========================================");
            // LevelDebug.PrintMap(_parent1.dungeon, LevelDebug.INDENT);
            // LevelDebug.PrintTree(_parent1.dungeon, LevelDebug.INDENT);
            // Console.WriteLine(" >>>>");
            // LevelDebug.PrintMap(_parent2.dungeon, LevelDebug.INDENT);
            // LevelDebug.PrintTree(_parent2.dungeon, LevelDebug.INDENT);
            // Console.WriteLine("-----------------------------------------");

            // Cut points
            Room rCut1 = null;
            Room rCut2 = null;
            // Tabu List: list of rooms that were the root of the branch and
            // led to an impossible crossover
            List<Room> failedRooms = new List<Room>();
            // List of mission rooms (rooms with keys and locked doors) in the
            // branch to be traded of each parent
            List<int> pMissionRooms1 = new List<int>();
            List<int> pMissionRooms2 = new List<int>();
            // List of mission rooms (rooms with keys and locked doors) in the
            // traded branch after the crossover (i.e., in the new individuals)
            List<int> oMissionRooms1 = new List<int>();
            List<int> oMissionRooms2 = new List<int>();
            // Total number of rooms in each branch that will be traded
            int nRooms1 = 0;
            int nRooms2 = 0;
            // Check if the trade is possible or not
            bool isImpossible = false;

            do {
                // Clone the dungeons of both parents
                dungeon1 = _parent1.dungeon.Clone();
                dungeon2 = _parent2.dungeon.Clone();
                // The number of non-null rooms of the dungeons
                List<Room> nonNullRooms1 = dungeon1.GetNonNullRooms();
                List<Room> nonNullRooms2 = dungeon2.GetNonNullRooms();

                // Get a random node from the parent as the root of the branch
                // that will be traded
                do {
                    int index = _rand.Next(1, nonNullRooms1.Count);
                    rCut1 = nonNullRooms1[index];
                } while (rCut1 == null);
                // Calculate the number of keys, locks and rooms in the branch
                // of the cut point in dungeon 1
                CalculateBranchRooms(
                    dungeon1,
                    rCut1.index,
                    ref nRooms1,
                    ref pMissionRooms1
                );

                // While the number of keys and locks from a branch is greater
                // than the number of rooms of the other branch, redraw the cut
                // point (the root of the branch)
                do {
                    do {
                        int index = _rand.Next(1, nonNullRooms2.Count);
                        rCut2 = nonNullRooms2[index];
                    } while (rCut2 == null && failedRooms.Contains(rCut2));
                    // Calculate the number of keys, locks and rooms in the
                    // branch of the cut point in dungeon 2
                    CalculateBranchRooms(
                        dungeon2,
                        rCut2.index,
                        ref nRooms2,
                        ref pMissionRooms2
                    );
                    // Add the cut room in the list of failed rooms
                    failedRooms.Add(rCut2);
                    // If no room can be the cut point, then the crossover
                    // operation is impossible
                    if (failedRooms.Count == nonNullRooms2.Count - 1)
                    {
                        isImpossible = true;
                    }
                } while ((pMissionRooms2.Count > nRooms1 ||
                          pMissionRooms1.Count > nRooms2) &&
                          !isImpossible);

                // If the crossover is possible, then perform it
                if (!isImpossible)
                {
                    // Swap the two branches
                    bool swap1 = SwapBranches(
                        ref dungeon1,
                        _parent2.dungeon,
                        rCut1.index,
                        rCut2.index
                    );
                    // Console.WriteLine("   ");
                    bool swap2 = SwapBranches(
                        ref dungeon2,
                        _parent1.dungeon,
                        rCut2.index,
                        rCut1.index
                    );

                    // LevelDebug.PrintMap(dungeon1, LevelDebug.INDENT);
                    // LevelDebug.PrintTree(dungeon1, LevelDebug.INDENT);
                    // Console.WriteLine(" >>>>");
                    // LevelDebug.PrintMap(dungeon2, LevelDebug.INDENT);
                    // Console.WriteLine("-----------------------------------------");

                    // If one of the dungeons could not finalize the crossover,
                    // then try another tuple of cut points
                    if (!swap1 || !swap2)
                    {
                        isImpossible = true;
                        continue;
                    }

                    // Update the grid of the two new dungeons
                    // If any conflicts arise here, they will be handled in the
                    // creation of child nodes; that is, any overlap will make
                    // the node and its children cease to exist
                    dungeon1.RefreshGrid(rCut1.index, rCut1.ParentDirection);
                    dungeon2.RefreshGrid(rCut2.index, rCut2.ParentDirection);

                    // Calculate the number of keys, locks and rooms in the
                    // newly switched branchs
                    oMissionRooms1 = new List<int>();
                    CalculateBranchRooms(
                        dungeon1,
                        rCut1.index,
                        ref nRooms1,
                        ref oMissionRooms1
                    );
                    oMissionRooms2 = new List<int>();
                    CalculateBranchRooms(
                        dungeon2,
                        rCut2.index,
                        ref nRooms2,
                        ref oMissionRooms2
                    );
                }
            // If mission rooms are missing in the new branches or the number
            // of rooms in the branches is greater than the number of total
            // rooms, retry apply the crossover
            } while ((oMissionRooms1.Count != pMissionRooms1.Count ||
                      oMissionRooms2.Count != pMissionRooms2.Count ||
                      pMissionRooms1.Count > nRooms2               ||
                      pMissionRooms2.Count > nRooms1) &&
                      !isImpossible);

            // If the crossover did not generate impossible dungeons, then
            // fix the created dungeons
            if (!isImpossible)
            {
                // Console.WriteLine("New");
                // Replace locks and keys in the new branchs
                dungeon1.FixBranch(rCut1.index, pMissionRooms1, ref _rand);
                dungeon2.FixBranch(rCut2.index, pMissionRooms2, ref _rand);
            }
            else
            {
                // Console.WriteLine("Keep");
                // Clone the dungeons of both parents
                dungeon1 = _parent1.dungeon.Clone();
                dungeon2 = _parent2.dungeon.Clone();
            }

            // LevelDebug.PrintMap(dungeon1, LevelDebug.INDENT);
            // LevelDebug.PrintTree(dungeon1, LevelDebug.INDENT);
            // Console.WriteLine(" >>>>");
            // LevelDebug.PrintMap(dungeon2, LevelDebug.INDENT);
            // LevelDebug.PrintTree(dungeon2, LevelDebug.INDENT);
            // Console.WriteLine("=========================================");
            // Console.WriteLine();

            individuals[0] = new Individual(dungeon1);
            individuals[1] = new Individual(dungeon2);

            return individuals;
        }

        /// Find the number of rooms of a branch and the mission rooms (rooms
        /// with keys and locked doors) in the branch. The key rooms are saved
        /// in the list with its positive ID, while the locked rooms with its
        /// negative value of the ID.
        private static void CalculateBranchRooms(
            Dungeon _dungeon,
            int _root,
            ref int _nRooms,
            ref List<int> _missionRooms
        ) {
            // Initialize the referenced variables
            _nRooms = 0;
            _missionRooms = new List<int>();
            // Search for the mission rooms in the dungeon
            Queue<int> toVisit = new Queue<int>();
            toVisit.Enqueue(_root);
            while (toVisit.Count > 0)
            {
                _nRooms++;
                int current = toVisit.Dequeue();
                // Check if the current room is a mission room, if so, add it in
                // the list of mission rooms
                RoomType type = _dungeon.Rooms[current].RoomType;
                if (type == RoomType.key)
                {
                    _missionRooms.Add(_dungeon.Rooms[current].KeyToOpen);
                }
                else if (type == RoomType.locked)
                {
                    _missionRooms.Add(-_dungeon.Rooms[current].KeyToOpen);
                }
                // Add the next rooms in the queue
                int[] nexts = new int[] {
                    _dungeon.GetChildIndexByDirection(
                        current, Util.Direction.Left
                    ),
                    _dungeon.GetChildIndexByDirection(
                        current, Util.Direction.Down
                    ),
                    _dungeon.GetChildIndexByDirection(
                        current, Util.Direction.Right
                    ),
                };
                foreach (int next in nexts)
                {
                    if (next >= 0 &&
                        next < Dungeon.CAPACITY &&
                        _dungeon.Rooms[next] != null
                    ) {
                        toVisit.Enqueue(next);
                    }
                }
            }
        }

        /// Replace a branch of the dungeon `_to` with a branch of the dungeon
        /// `_from`.
        ///
        /// First, this method clears all the nodes in the branch with root
        /// `_cut1` in dungeon `_to`. Then, assign the nodes of the branch with
        /// root `_cut2` of dungeon `_from` in dungeon `_to`.
        private static bool SwapBranches(
            ref Dungeon _to,
            Dungeon _from,
            int _cut1,
            int _cut2
        ) {
            // Console.WriteLine(" >>>> " + _cut1);
            // Clear the branch with root `_cut1` of the target dungeon (`_to`)
            ClearBranch(ref _to, _cut1);
            // Move branch of dungeon `_from` to dungeon `_to`
            Queue<(int, int)> toVisit = new Queue<(int, int)>();
            toVisit.Enqueue((_cut1, _cut2));
            while (toVisit.Count > 0)
            {
                (int c1, int c2) = toVisit.Dequeue();
                _to.Rooms[c1] = _from.Rooms[c2].Clone();
                _to.Rooms[c1].index = c1;
                // Add the children of the current node
                (int, int)[] nexts = new (int, int)[] {
                    (
                        _to.GetChildIndexByDirection(
                            c1, Util.Direction.Left
                        ),
                        _from.GetChildIndexByDirection(
                            c2, Util.Direction.Left
                        )
                    ),
                    (
                        _to.GetChildIndexByDirection(
                            c1, Util.Direction.Down
                        ),
                        _from.GetChildIndexByDirection(
                            c2, Util.Direction.Down
                        )
                    ),
                    (
                        _to.GetChildIndexByDirection(
                            c1, Util.Direction.Right
                        ),
                        _from.GetChildIndexByDirection(
                            c2, Util.Direction.Right
                        )
                    ),
                };
                Util.Direction[] dirs = new Util.Direction[] {
                    Util.Direction.Left,
                    Util.Direction.Down,
                    Util.Direction.Right
                };
                for (int i = 0; i < nexts.Length; i++)
                {
                    (int n1, int n2) next = nexts[i];
                    if (next.n1 != -1 &&
                        next.n2 != -1 &&
                        _from.Rooms[next.n2] != null
                    ) {
                        toVisit.Enqueue(next);
                    }
                    else if (next.n1 == -1 && next.n2 != -1)
                    {
                        // It is impossible place a new room if there is no
                        // space in the dungeon 1
                        return false;
                    }
                }
            }
            return true;
        }

        /// Erase the rooms from dungeon ternary heap and grid.
        private static void ClearBranch(
            ref Dungeon _dungeon,
            int _cut
        ) {
            Queue<int> toVisit = new Queue<int>();
            toVisit.Enqueue(_cut);
            while (toVisit.Count > 0)
            {
                // Get the current room and erase it
                int current = toVisit.Dequeue();
                Room room = _dungeon.Rooms[current];
                _dungeon.grid[room.X, room.Y] = null;
                _dungeon.Rooms[current] = null;
                // Add the children of the current room
                int[] nexts = new int[] {
                    _dungeon.GetChildIndexByDirection(
                        current, Util.Direction.Left
                    ),
                    _dungeon.GetChildIndexByDirection(
                        current, Util.Direction.Down
                    ),
                    _dungeon.GetChildIndexByDirection(
                        current, Util.Direction.Right
                    ),
                };
                foreach (int next in nexts)
                {
                    if (next != -1 && _dungeon.Rooms[next] != null)
                    {
                        toVisit.Enqueue(next);
                    }
                }
            }
        }
    }
}