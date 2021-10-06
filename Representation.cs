using System;

namespace LevelGenerator
{
    /// This class represents an individual.
    ///
    /// Individuals are composed of a dungeon, their fitness value, and the
    /// generation when they were created. There are also other attributes to
    /// hold some measurements of the level.
    ///
    /// Why individuals are represented by a class instead of a struct? When
    /// using MAP-Elites some slots may be empty, then the `null` option makes
    /// easier to manage the MAP-Elites population.
    public class Individual
    {
        /// The dungeon level.
        public Dungeon dungeon;
        /// The generation in which the individual was created.
        public int generation;
        /// The individual fitness.
        public float fitness;
        /// The number of locked doors that must be unlocked to reach the goal.
        public int neededLocks;
        /// The number of rooms that must be visited to reach the goal.
        public float neededRooms;
        /// The linear coefficient of the dungeon level.
        public float linearCoefficient;
        /// The coefficient of exploration.
        public float exploration;
        /// The leniency degree.
        public float leniency;

        /// Individual contructor.
        public Individual(
            Dungeon _dungeon
        ) {
            dungeon = _dungeon;
            generation = Common.UNKNOWN;
            fitness = Common.UNKNOWN;
            neededLocks = 0;
            neededRooms = 0;
            linearCoefficient = 0f;
        }

        /// Return a clone of the individual.
        public Individual Clone()
        {
            Individual individual = new Individual(dungeon.Clone());
            individual.fitness = fitness;
            individual.generation = generation;
            individual.neededLocks = neededLocks;
            individual.neededRooms = neededRooms;
            individual.linearCoefficient = linearCoefficient;
            return individual;
        }

        /// Calculate the linear coefficient of the dungeon level.
        public void CalculateLinearCoefficient()
        {
            linearCoefficient = 0f;
            int leafs = 0;
            foreach (Room room in dungeon.Rooms)
            {
                int childs = 0;
                if (room.right != null && room.right.parent != null)
                {
                    childs++;
                }
                if (room.left != null && room.left.parent != null)
                {
                    childs++;
                }
                if (room.bottom != null && room.bottom.parent != null)
                {
                    childs++;
                }
                if (childs == 0)
                {
                    leafs++;
                }
                linearCoefficient += childs;
            }
            int total = dungeon.Rooms.Count;
            linearCoefficient = linearCoefficient / (total - leafs);
        }

        /// Print the individual attributes and the dungeon map.
        public void Debug()
        {
            Console.WriteLine(LevelDebug.INDENT + "G=" + generation);
            Console.WriteLine(LevelDebug.INDENT + "F=" + fitness);
            Console.WriteLine(LevelDebug.INDENT + "MAP=");
            LevelDebug.PrintMap(dungeon, LevelDebug.INDENT);
        }

        /// Generate and return a random individual.
        public static Individual GetRandom(
            int _enemies,
            ref Random _rand
        ) {
            Dungeon dungeon = new Dungeon();
            dungeon.GenerateRooms(ref _rand);
            dungeon.PlaceEnemies(_enemies, ref _rand);
            return new Individual(dungeon);
        }
    }
}