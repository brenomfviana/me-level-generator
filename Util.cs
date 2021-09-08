using System;

namespace LevelGenerator
{
    public static class Util
    {
        public static Random rnd = new Random();

        public enum Direction
        {
            right = 0,
            down = 1,
            left = 2
        };

        private static int ID = 0;

        public static int getNextId()
        {
            return ID++;
        }
    }
}