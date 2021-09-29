using System;

namespace LevelGenerator
{
    /// This class holds the mutation operator.
    public class Mutation
    {
        /// The rate at which the mutation adds or removes a lock and a key.
        private static int MUTATION_TYPE_RATE = 50;

        /// Reproduce a new individual by mutating a parent.
        public static Individual Apply(
            Individual _parent,
            ref Random _rand
        ) {
            // Clone the parent
            Individual individual = _parent.Clone();
            // Choose randomly which mutation will be applied
            if (MUTATION_TYPE_RATE > Common.RandomPercent(ref _rand))
            {
                // Add a new lock and a new key in the new individual
                individual.dungeon.AddLockAndKey(ref _rand);
            }
            else
            {
                // Remove a lock and a key from the new individual randomly
                individual.dungeon.RemoveLockAndKey(ref _rand);
            }
            // Return the mutated individual
            return individual;
        }
    }
}