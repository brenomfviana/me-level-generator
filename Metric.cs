using System.Collections.Generic;

namespace LevelGenerator
{
    /// This class holds the dungeon levels measurement-related functions.
    public class Metric
    {
        /// Calculate and return the level linearity.
        ///
        /// This function calculates the number of rooms for each number of
        /// neighbors, i.e., the number of rooms with 1, 2, 3, and 4 neighbors.
        /// A neighbor can be both a parent or a child. Each amount of
        /// neighbors has a weight of linearity: the rooms with 4 neighbors
        /// have a weight of 0, because this room has the maximum number of
        /// possible paths; the rooms with 3 neighbors have a weight of 0.5;
        /// the rooms with 2 neighbors have a weight of 1, because there is
        /// only two possible paths; the rooms with a single neighbor have a
        /// weight of 0, because dead-ends are ignored.
        public static float Linearity(
            Individual _individual
        ) {
            // Initialize the queue to perform breadth-first search
            Queue<Room> toVisit = new Queue<Room>();
            toVisit.Enqueue(_individual.dungeon.Rooms[0]);
            // Count the number of rooms for each number of neighbors and
            // the number of non-dead-end rooms
            int[] counter = new int[] {0, 0, 0, 0, 0};
            int nonDeadEndRooms = 0;
            while (toVisit.Count > 0)
            {
                Room current = toVisit.Dequeue();
                Room[] nexts = new Room[] {
                    current.left,
                    current.bottom,
                    current.right
                };
                int c = 0;
                foreach (Room next in nexts)
                {
                    if (next != null)
                    {
                        c++;
                        toVisit.Enqueue(next);
                    }
                }
                if (current.parent != null)
                {
                    c++;
                }
                counter[c]++;
                if (c > 1)
                {
                    nonDeadEndRooms++;
                }
            }
            // Calculate and return the level linearity
            float linearity = 0f;
            float[] weight = new float[] {0f, 0f, 1f, 0.5f, 0f};
            for (int i = 0; i < counter.Length; i++)
            {
                linearity += counter[i] * weight[i];
            }
            linearity = linearity / nonDeadEndRooms;
            return linearity;
        }
    }
}