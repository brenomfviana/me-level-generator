using System;
using System.Diagnostics;

namespace LevelGenerator
{
    /// This class holds the fitness operator.
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
                _individual.fitness = 100000000f;
                return;
            }
            float avgUsedRoom = 0.0f;
            // Only use the A* if there is a lock in the dungeon
            // System.Console.WriteLine("Begin A*");
            _individual.neededLocks = AStar.FindRoute(dungeon);
            for (int i = 0; i < 3; ++i)
            {
                DFS dfs = new DFS(dungeon);
                dfs.FindRoute(dungeon, ref _rand);
                avgUsedRoom += dfs.NVisitedRooms;
            }
            _individual.neededRooms = avgUsedRoom / 3.0f;
            // System.Console.WriteLine("Needed Rooms: " + dungeon.neededRooms);
            if (_individual.neededRooms > dungeon.Rooms.Count)
            {
                Console.WriteLine("SOMETHING IS REALLY WRONG! Nrooms: " + dungeon.Rooms.Count + "  Used: " + _individual.neededRooms);
                Console.ReadKey();
            }
            if (_individual.neededLocks > dungeon.locks)
            {
                Console.WriteLine("SOMETHING IS REALLY WRONG!");
                Console.WriteLine(_individual.neededLocks);
                Console.WriteLine(dungeon.locks);
            }

            int amount = dungeon.GetNonNullRooms().Count;
            float fit = (
                2 * (System.Math.Abs(nV - amount) +
                System.Math.Abs(nK - dungeon.keys) +
                System.Math.Abs(nL - dungeon.locks) +
                System.Math.Abs(lCoef - _individual.AvgChildren)) +
                System.Math.Abs(dungeon.locks * 0.8f - _individual.neededLocks) +
                System.Math.Abs(amount - _individual.neededRooms)
            );
            _individual.fitness = fit;
        }

        /// Return true if the first individual (`_i1`) is best than the second
        /// (`_i2`), and false otherwise.
        public static bool IsBest(
            Individual _i1,
            Individual _i2
        ) {
            // Ensure that both enemies are not null.
            Debug.Assert(
                _i1 != null || _i2 != null,
                Util.CANNOT_COMPARE_INDIVIDUALS
            );
            // If `_i1` is null, then `_i2` is the best individual
            if (_i1 == null)
            {
                return false;
            }
            // If `_i2` is null, then `_i1` is the best individual
            if (_i2 == null)
            {
                return true;
            }
            // Check which individual is the best
            return _i2.fitness > _i1.fitness;
        }
    }
}