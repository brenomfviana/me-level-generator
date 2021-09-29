using System;
using System.Diagnostics;

namespace LevelGenerator
{
    /// This class holds all the fitness-related functions.
    public class Fitness
    {
        /// Fitness is based in the number of rooms, number of keys and locks, the linear coefficient and the number of locks used by the A*
        public static void Calculate(
            ref Individual _individual,
            ref Random _rand
        ) {
            Dungeon dungeon = _individual.dungeon;
            int nV = 20;
            int nK = 4;
            int nL = 4;
            float lCoef = 1.5f;
            // If the level has no locked door nor key
            if (dungeon.keys == 0 || dungeon.locks == 0)
            {
                _individual.fitness = -1f;
                return;
            }
            float avgUsedRoom = 0.0f;
            // Only use the A* if there is a lock in the dungeon
            // System.Console.WriteLine("Begin A*");
            dungeon.neededLocks = AStar.FindRoute(dungeon);
            for (int i = 0; i < 3; ++i)
            {
                DFS dfs = new DFS(dungeon);
                dfs.FindRoute(dungeon, ref _rand);
                avgUsedRoom += dfs.NVisitedRooms;
            }
            dungeon.neededRooms = avgUsedRoom / 3.0f;
            // System.Console.WriteLine("Needed Rooms: " + dungeon.neededRooms);
            if (dungeon.neededRooms > dungeon.RoomList.Count)
            {
                Console.WriteLine("SOMETHING IS REALLY WRONG! Nrooms: " + dungeon.RoomList.Count + "  Used: " + dungeon.neededRooms);
                Console.ReadKey();
            }
            if (dungeon.neededLocks > dungeon.locks)
            {
                Console.WriteLine("SOMETHING IS REALLY WRONG!");
                Console.WriteLine(dungeon.neededLocks);
                Console.WriteLine(dungeon.locks);
            }

            float fit = (2*(System.Math.Abs(nV - dungeon.RoomList.Count) + System.Math.Abs(nK - dungeon.keys) + System.Math.Abs(nL - dungeon.locks) + System.Math.Abs(lCoef - dungeon.AvgChildren)) + (dungeon.locks * 0.8f - dungeon.neededLocks) + (dungeon.RoomList.Count - dungeon.neededRooms));

            _individual.fitness = fit;
        }

        /// Return true if the first individual (`_i1`) is best than the second
        /// (`_i2`), and false otherwise.
        ///
        /// The best is the individual that is closest to the local goal in the
        /// MAP-Elites population. This is, the best is the one that's fitness
        /// has the lesser value. If `_i1` is null, then `_i2` is the best
        /// individual. If `_i2` is null, then `_i1` is the best individual. If
        /// both individuals are null, then the comparison cannot be performed.
        public static bool IsBest(
            Individual _i1,
            Individual _i2
        ) {
            Debug.Assert(
                _i1 != null || _i2 != null,
                Common.CANNOT_COMPARE_INDIVIDUALS
            );
            if (_i1 is null) { return false; }
            if (_i2 is null) { return true; }
            return _i2.fitness > _i1.fitness;
        }
    }
}