using System;
using System.Diagnostics;

namespace LevelGenerator
{
    /// This class holds all the fitness-related functions.
    public class Fitness
    {
        /// Fitness is based in the number of rooms, number of keys and locks, the linear coefficient and the number of locks used by the A*.
        public static void Calculate(
            Parameters _prs,
            ref Individual _individual,
            ref Random _rand
        ) {
            // Create aliases for the individual's attributes
            Dungeon dungeon = _individual.dungeon;
            int rooms = dungeon.rooms.Count;
            int keys = dungeon.keyIds.Count;
            int locks = dungeon.lockIds.Count;
            float linearCoefficient = _individual.linearCoefficient;
            // Calculate the distance between the attributes of the generated
            // dungeon to the entered parameters
            float distance = Math.Abs(_prs.rooms - rooms) +
                Math.Abs(_prs.keys - keys) +
                Math.Abs(_prs.locks - locks) +
                Math.Abs(_prs.linearCoefficient - linearCoefficient);
            float fit = 2 * distance;
            // If the level has locked doors
            if (dungeon.lockIds.Count > 0)
            {
                // Calculate the number of locks needed to finish the level
                _individual.neededLocks = AStar.FindRoute(dungeon);
                // Validate the calculated number of needed locks
                if (_individual.neededLocks > dungeon.lockIds.Count)
                {
                    throw new Exception("Inconsistency! The number of " +
                        "needed locks is higher than the number of total " +
                        "locks of the level." +
                        "\n  Total locks=" + dungeon.lockIds.Count +
                        "\n  Needed locks=" + _individual.neededLocks);
                }
                // Calculate the number of rooms needed to finish the level
                float neededRooms = 0f;
                for (int i = 0; i < 3; i++)
                {
                    DFS dfs = new DFS(dungeon);
                    dfs.FindRoute(dungeon, ref _rand);
                    neededRooms += dfs.NVisitedRooms;
                }
                _individual.neededRooms = neededRooms / 3.0f;
                // Validate the calculated number of needed rooms
                if (_individual.neededRooms > dungeon.Rooms.Count)
                {
                    throw new Exception("Inconsistency! The number of " +
                        "needed rooms is higher than the number of total " +
                        "rooms of the level." +
                        "\n  Total rooms=" + dungeon.Rooms.Count +
                        "\n  Needed rooms=" + _individual.neededRooms);
                }
                // Update the fitness by summing the needed rooms and locks
                fit += dungeon.lockIds.Count * 0.8f - _individual.neededLocks +
                    dungeon.Rooms.Count - _individual.neededRooms;
            }
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