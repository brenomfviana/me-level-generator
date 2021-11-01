using System;
using System.Diagnostics;

namespace LevelGenerator
{
    /// This class holds all the fitness-related functions.
    public class Fitness
    {
        /// The error message of cannot compare individuals.
        public static readonly string CANNOT_COMPARE_INDIVIDUALS =
            "There is no way of comparing two null individuals.";

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
                    dfs.FindRoute(ref _rand);
                    neededRooms += dfs.NVisitedRooms;
                }
                _individual.neededRooms = neededRooms / 3.0f;
                // Validate the calculated number of needed rooms
                if (_individual.neededRooms > dungeon.rooms.Count)
                {
                    throw new Exception("Inconsistency! The number of " +
                        "needed rooms is higher than the number of total " +
                        "rooms of the level." +
                        "\n  Total rooms=" + dungeon.rooms.Count +
                        "\n  Needed rooms=" + _individual.neededRooms);
                }
                float fNeededRooms = dungeon.rooms.Count -
                    _individual.neededRooms;
                _individual.fNeededRooms = fNeededRooms;
                // Update the fitness by summing the number of needed rooms and
                // the number of needed locks
                fit += fNeededLocks + fNeededRooms;
            }
            _individual.fGoal = fit;
            // Update the fitness by subtracting the enemy sparsity
            // (the higher the better)
            float sparsity = -EnemySparsity(dungeon, _prs.enemies, _prs.weight);
            _individual.fEnemySparsity = sparsity;
            float std = STDEnemyByRoom(dungeon, _prs.enemies, _prs.inclusive);
            _individual.fSTD = std;
            fit = fit + sparsity + std;
            _individual.fitness = fit;
        }

        /// Calculate and return the enemy sparsity in the entered dungeon.
        ///
        /// If `weight` is true, then the sparsity weights the positions with
        /// the number of enemies in the respective room. Otherwise, each
        /// position will be counted once.
        private static float EnemySparsity(
            Dungeon _dungeon,
            int _enemies,
            bool _weight
        ) {
            // Calculate the average position of enemies
            float avg_x = 0f;
            float avg_y = 0f;
            foreach (Room room in _dungeon.rooms)
            {
                int xp = room.x + _dungeon.minX;
                int yp = room.y + _dungeon.minY;
                avg_x += xp * (_weight ? room.enemies : 1);
                avg_y += yp * (_weight ? room.enemies : 1);
            }
            avg_x = avg_x / _enemies;
            avg_y = avg_y / _enemies;
            // Calculate the sparsity
            float sparsity = 0f;
            foreach (Room room in _dungeon.rooms)
            {
                int xp = room.x + _dungeon.minX;
                int yp = room.y + _dungeon.minY;
                double dist = 0f;
                dist += Math.Pow(xp - avg_x, 2);
                dist += Math.Pow(yp - avg_y, 2);
                dist *= (_weight ? room.enemies : 1);
                sparsity += (float) Math.Sqrt(dist);
            }
            return sparsity / _enemies;
        }

        /// Return the standard deviation of number of enemies by room.
        ///
        /// If `_inclusive` is true, then the standard deviation will consider
        /// all the rooms where enemies can be placed. Otherwise, only
        /// populated rooms will be considered in this calculation.
        private static float STDEnemyByRoom(
            Dungeon _dungeon,
            int _enemies,
            bool _inclusive
        ) {
            // The start and goal rooms are not count
            int rooms = 0;
            if (_inclusive)
            {
                rooms = _dungeon.rooms.Count - 2;
            }
            else
            {
                for (int i = 1; i < _dungeon.rooms.Count; i++)
                {
                    if (_dungeon.rooms[i].enemies > 0)
                    {
                        rooms++;
                    }
                }
            }
            float mean = _enemies / rooms;
            // Calculate standard deviation
            float std = 0f;
            for (int i = 1; i < _dungeon.rooms.Count; i++)
            {
                Room room = _dungeon.rooms[i];
                if ((_inclusive && !room.Equals(_dungeon.GetGoal())) ||
                    (!_inclusive && room.enemies > 0)
                ) {
                    std += (float) Math.Pow(room.enemies - mean, 2);
                }
            }
            return (float) Math.Sqrt(std / rooms);
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
                CANNOT_COMPARE_INDIVIDUALS
            );
            if (_i1 is null) { return false; }
            if (_i2 is null) { return true; }
            return _i2.fitness > _i1.fitness;
        }
    }
}