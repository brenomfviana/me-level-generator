using System;

namespace LevelGenerator
{
    /// This class holds only utility functions and constants.
    public static class Util
    {
        /// The directions in which a room may connect to other rooms.
        ///
        /// The direction `up` is not listed here to avoid positioning conflict
        /// during the room placement.
        public enum Direction
        {
            right = 0,
            down = 1,
            left = 2
        };

        /// Return a random integer percentage (from 0 to 99, 100 numbers).
        public static int RandomPercent(
            ref Random _rand
        ) {
            return _rand.Next(100);
        }
    }
}