using System;
using System.Collections.Generic;

namespace LevelGenerator
{
    /// This class holds only utility functions and constants.
    public static class Util
    {
        /// Unknown reference.
        public static readonly int UNKNOWN = -1;
        /// The level grid offset.
        public static readonly int LEVEL_GRID_OFFSET = 150;
        /// The error message of not enough competitors.
        public static readonly string NOT_ENOUGH_COMPETITORS =
            "There are not enough individuals in the input population to " +
            "perform this operation.";
        /// The error message of not enough competitors.
        public static readonly string CANNOT_COMPARE_INDIVIDUALS =
            "There is no way of comparing two null individuals.";

        /// The directions in which a room may connect to other rooms.
        ///
        /// The direction `up` is not listed here to avoid positioning conflict
        /// during the room placement.
        public enum Direction
        {
            Right = 0,
            Down = 1,
            Left = 2
        };

        /// Return a random integer percentage (from 0 to 99, 100 numbers).
        public static int RandomPercent(
            ref Random _rand
        ) {
            return _rand.Next(100);
        }

        /// Return a random element from the input list.
        public static T RandomElementFromList<T>(
            List<T> _range,
            ref Random _rand
        ) {
            return _range[_rand.Next(0, _range.Count)];
        }
    }
}