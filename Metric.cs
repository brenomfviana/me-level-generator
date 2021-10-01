using System;
using System.Collections.Generic;

namespace LevelGenerator
{
    /// This class holds the dungeon levels measurement-related functions.
    class Metric
    {
        /// Calculate and return the coefficient of level exploration.
        public static float CoefficientExploration(
            Individual _individual
        ) {
            Dungeon dungeon = _individual.dungeon;
            Room start = dungeon.GetStart();
            Room target = dungeon.GetGoal();
            List<Room> reached = Metric.FloodFill(dungeon, start, target);
            return (float) reached.Count / dungeon.GetNumberOfRooms();
        }

        /// Return all the reached room in the search for the target room.
        ///
        /// To do so, this method performs the flood-fill approach to search
        /// from the starting room for the target room.
        public static List<Room> FloodFill(
            Dungeon _dungeon,
            Room _start,
            Room _target
        ) {
            List<Room> reached = new List<Room>();
            Queue<Room> unvisited = new Queue<Room>();
            unvisited.Enqueue(_start);
            while (unvisited.Count > 0)
            {
                Room current = unvisited.Dequeue();
                if (reached.Contains(current)) { continue; }
                reached.Add(current);
                if (current.Equals(_target) || _target == null) { break; }
                foreach (Room neighbor in current.GetNeighbors())
                {
                    if (neighbor != null)
                    {
                        unvisited.Enqueue(neighbor);
                    }
                }
            }
            return reached;
        }
    }
}