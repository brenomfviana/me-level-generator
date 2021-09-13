using System;

namespace LevelGenerator
{
    /// This class holds the mutation operator.
    public class Mutation
    {
        /// The rate at which the mutation adds or removes a lock and a key.
        private static int MUTATION_TYPE_RATE = 50;

        /// Reproduce a new individual by mutating a parent.
        public static Dungeon Apply(
            Dungeon _parent,
            ref Random _rand
        ) {
            // Clone parent
            Dungeon individual = _parent.Clone();
            // Choose randomly which mutation will be applied
            if (MUTATION_TYPE_RATE > Util.RandomPercent(ref _rand))
            {
                // Add a new lock and a new key in the new individual
                individual.AddLockAndKey(ref _rand);
            }
            else
            {
                // Remove a lock and a key from the new individual randomly
                individual.RemoveLockAndKey(ref _rand);
            }
            // Return mutated individual
            return individual;
        }
    }
}