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
        // Collected data. Remove from this class.
        public int neededLocks = 0;
        // Collected data. Remove from this class.
        public float neededRooms = 0;

        // Collected data (linearity). Remove from this class.
        private float avgChildren;
        public float AvgChildren { get => avgChildren; set => avgChildren = value; }

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

        public void CalcAvgChildren()
        {
            avgChildren = 0.0f;
            int childCount;
            int childLess = 0;
            foreach (Room room in dungeon.Rooms)
            {
                if (room == null) { continue; }
                childCount = 0;
                // Get the current room children
                int[] children = new int[] {
                    dungeon.GetChildIndexByDirection(
                        room.index, Util.Direction.Left
                    ),
                    dungeon.GetChildIndexByDirection(
                        room.index, Util.Direction.Down
                    ),
                    dungeon.GetChildIndexByDirection(
                        room.index, Util.Direction.Right
                    ),
                };
                //
                foreach (int child in children)
                {
                    if (child != -1 &&
                        dungeon.Rooms[child] != null &&
                        dungeon.GetParent(child) != null)
                    {
                        childCount += 1;
                    }
                }
                if (childCount == 0)
                {
                    childLess++;
                }
                avgChildren += childCount;
            }
            avgChildren = avgChildren / (dungeon.Rooms.Count - childLess);
        }

        /// Print the individual attributes and the dungeon map.
        public void Debug()
        {
            Console.WriteLine(LevelDebug.INDENT + "G=" + generation);
            Console.WriteLine(LevelDebug.INDENT + "F=" + fitness);
            Console.WriteLine(LevelDebug.INDENT + "MAP=");
            LevelDebug.PrintMap(dungeon, LevelDebug.INDENT);
            // Console.WriteLine(LevelDebug.INDENT + "TREE=");
            // LevelDebug.PrintTree(dungeon, LevelDebug.INDENT);
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