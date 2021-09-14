using System;
using System.Text.Json.Serialization;

namespace LevelGenerator
{
    /// This class represents an individual.
    ///
    /// Individuals are composed of a dungeon, their fitness value, and the
    /// generation when they were created.
    ///
    /// Why individuals are represented by a class instead of a struct? When
    /// using MAP-Elites some slots may be empty, then the `null` option makes
    /// easier to manage the MAP-Elites population.
    public class Individual
    {
        public Dungeon dungeon;
        [JsonInclude]
        public float fitness;
        [JsonInclude]
        public int generation;

        /// Individual contructor.
        public Individual(
            Dungeon _dungeon
        ) {
            dungeon = _dungeon;
        }

        public Individual Clone()
        {
            Individual individual = new Individual(dungeon.Clone());
            individual.fitness = fitness;
            individual.generation = generation;
            return individual;
        }

        /// Return a random individual.
        public static Individual GetRandom(
            ref Random _rand
        ) {
            // Generate a new random dungeon
            Dungeon dungeon = new Dungeon();
            dungeon.GenerateRooms(ref _rand);
            // Initialize the new individual
            Individual individual = new Individual(dungeon);
            individual.generation = Util.UNKNOWN;
            individual.fitness = Util.UNKNOWN;
            // Return the generated individual
            return individual;
        }
    }
}