using System;
using System.Diagnostics;

namespace LevelGenerator
{
    /// This class holds the fitness operator.
    public class Fitness
    {
        /// Fitness is based in the number of rooms, number of keys and locks, the linear coefficient and the number of locks used by the A*
        public static void Calculate(
            ref Dungeon _individual,
            ref Random rand
        ) {
            int nV = 20;
            int nK = 4;
            int nL = 4;
            float lCoef = 1.5f;
            // If the level has no locked door nor key
            if (_individual.nKeys == 0 || _individual.nLocks == 0)
            {
                _individual.fitness = -1f;
                return;
            }
            float avgUsedRoom = 0.0f;
            // Only use the A* if there is a lock in the dungeon
            // System.Console.WriteLine("Begin A*");
            _individual.neededLocks = AStar.FindRoute(_individual);
            for (int i = 0; i < 3; ++i)
            {
                DFS dfs = new DFS(_individual);
                dfs.FindRoute(_individual, ref rand);
                avgUsedRoom += dfs.NVisitedRooms;
            }
            _individual.neededRooms = avgUsedRoom / 3.0f;
            // System.Console.WriteLine("Needed Rooms: " + _individual.neededRooms);
            if (_individual.neededRooms > _individual.RoomList.Count)
            {
                Console.WriteLine("SOMETHING IS REALLY WRONG! Nrooms: " + _individual.RoomList.Count + "  Used: " + _individual.neededRooms);
                Console.ReadKey();
            }
            if (_individual.neededLocks > _individual.nLocks)
            {
                Console.WriteLine("SOMETHING IS REALLY WRONG!");
                Console.WriteLine(_individual.neededLocks);
                Console.WriteLine(_individual.nLocks);
            }

            float fit = (2*(System.Math.Abs(nV - _individual.RoomList.Count) + System.Math.Abs(nK - _individual.nKeys) + System.Math.Abs(nL - _individual.nLocks) + System.Math.Abs(lCoef - _individual.AvgChildren)) + (_individual.nLocks * 0.8f - _individual.neededLocks) + (_individual.RoomList.Count - _individual.neededRooms));

            _individual.fitness = fit;
        }

        /// Return true if the first individual (`_i1`) is best than the second
        /// (`_i2`), and false otherwise.
        public static bool IsBest(
            Dungeon _i1,
            Dungeon _i2
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