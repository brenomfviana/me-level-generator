using System;
using System.Diagnostics;

namespace LevelGenerator
{
    /// This class holds all the fitness-related functions.
    public class Fitness
    {
        /// Calculate the fitness value of the entered individual.
        ///
        /// An individual's fitness is defined by two factors: the user aimed
        /// settings and the gameplay factor. The user aimed settings are
        /// measured by the distance of the aimed number of rooms, number of
        /// keys, number of locks and the linear coefficient. The gameplay
        /// factor sums: (1) the distance between the total number of locks
        /// weighted by 0.8 and the number of needed locks to open to finish
        /// the level; (2) the distance between the total number of rooms and
        /// the number of needed rooms to visit to finish the level, and; (3)
        /// the negative value of the sparsity of enemies. The last item is
        /// negative because this fitness aims to minimize its value while
        /// maximizing the sparsity of enemies.
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
            float fRooms = Math.Abs(_prs.rooms - rooms);
            float fKeys = Math.Abs(_prs.keys - keys);
            float fLocks = Math.Abs(_prs.locks - locks);
            float fLC = Math.Abs(_prs.linearCoefficient - linearCoefficient);
            _individual.fRooms = fRooms;
            _individual.fKeys = fKeys;
            _individual.fLocks = fLocks;
            _individual.fLinearCoefficient = fLC;
            float distance = fRooms + fKeys + fLocks + fLC;
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
                float fNeededLocks = dungeon.lockIds.Count * 0.8f -
                    _individual.neededLocks;
                _individual.fNeededLocks = fNeededLocks;
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
                float fNeededRooms = dungeon.Rooms.Count -
                    _individual.neededRooms;
                _individual.fNeededRooms = fNeededRooms;
                // Update the fitness by summing the number of needed rooms and
                // the number of needed locks
                fit += fNeededLocks + fNeededRooms;
            }
            _individual.fGoal = fit;
            // Update the fitness by subtracting the enemy sparsity
            // (the higher the better)
            float sparsity = -EnemySparsity(dungeon, _prs.enemies);
            _individual.fEnemySparsity = sparsity;
            fit = fit + sparsity;
            _individual.fitness = fit;
        }

        /// Calculate and return the enemy sparsity in the entered dungeon.
        private static float EnemySparsity(
            Dungeon _dungeon,
            int _enemies
        ) {
            // Calculate the average position of enemies
            float avg_x = 0f;
            float avg_y = 0f;
            foreach (Room room in _dungeon.rooms)
            {
                avg_x += room.x * room.enemies;
                avg_y += room.y * room.enemies;
            }
            avg_x = avg_x / _enemies;
            avg_y = avg_y / _enemies;
            // Calculate the sparsity
            float sparsity = 0f;
            foreach (Room room in _dungeon.rooms)
            {
                for (int i = 0; i < room.enemies; i++)
                {
                    sparsity += Math.Abs(room.x - avg_x);
                    sparsity += Math.Abs(room.y - avg_y);
                }
            }
            return sparsity / (_enemies + _enemies);
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