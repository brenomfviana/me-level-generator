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

        /// Return a clone of the individual.
        public Individual Clone()
        {
            Individual individual = new Individual(dungeon.Clone());
            individual.fitness = fitness;
            individual.generation = generation;
            return individual;
        }

        /// Print the individual attributes and the dungeon map.
        public void Debug()
        {
            Console.WriteLine(LevelDebug.INDENT + "G=" + generation);
            Console.WriteLine(LevelDebug.INDENT + "F=" + fitness);
            Console.WriteLine(LevelDebug.INDENT + "MAP=");
            LevelDebug.PrintMap(dungeon, LevelDebug.INDENT);
            Console.WriteLine(LevelDebug.INDENT + "TREE=");
            LevelDebug.PrintTree(dungeon, LevelDebug.INDENT);
        }

        /// Generate and return a random individual.
        public static Individual GetRandom(
            ref Random _rand
        ) {
            Dungeon dungeon = new Dungeon();
            dungeon.GenerateRooms(ref _rand);
            Individual individual = new Individual(dungeon);
            individual.generation = Common.UNKNOWN;
            individual.fitness = Common.UNKNOWN;
            return individual;
        }
    }
}