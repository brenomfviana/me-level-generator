using System;

namespace LevelGenerator
{
    /// This class holds the mutation operator.
    public class Mutation
    {
        /// The rate at which the mutation adds a new lock and key.
        private static int MUTATION_RATE = 50;

        /// Reproduce a new individual by mutating a parent.
        public static void Apply(
            ref Dungeon _parent,
            ref Random _rand
        ) {
            // Choose randomly which mutation will be applied
            if (MUTATION_RATE > Util.RandomPercent(ref _rand))
            {
                // Add a new lock and a new key in the dungeon
                _parent.AddLockAndKey(ref _rand);
            }
            else
            {
                // Remove a lock and a key from the dungeon randomly
                _parent.RemoveLockAndKey(ref _rand);
            }
            // Recreate the dugeons' list of rooms
            _parent.FixRoomList();
        }
    }
}