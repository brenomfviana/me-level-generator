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
            Individual individual = _parent.Clone();
            if (MUTATION_TYPE_RATE > Common.RandomPercent(ref _rand))
            {
                individual.dungeon.AddLockAndKey(ref _rand);
            }
            else
            {
                individual.dungeon.RemoveLockAndKey(ref _rand);
            }
            return individual;
        }
    }
}