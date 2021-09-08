using System;

namespace LevelGenerator
{
    static class Constants
    {
        public const int MAX_CHILDREN = 3;
        public const float PROB_HAS_CHILD = 100f;
        public static readonly float[] CHILD_PROB = { 100f / 3, 100f / 3, 100f / 3 };
        public const float PROB_1_CHILD = 100f / 3;
        public const float PROB_2_CHILD = 100f / 3;
        public const float PROB_3_CHILD = 100f / 3;
        public const float PROB_CHILD = 100f / 3;
        public const float PROB_NORMAL_ROOM = 70f;
        public const float PROB_KEY_ROOM = 15f;
        public const float PROB_LOCKER_ROOM = 15f;
        public const int MAX_DEPTH = 20;
        public const float NEIGHBORINGFACTOR = 0.7f;
        public const float CROSSOVER_RATE = 90f;     // #
        public const float MUTATION_RATE = 5f;       // #
        public const float MUTATION0_RATE = 50f;     // #
        public const float MUTATION1_RATE = 50f;     // #
        public const int POP_SIZE = 100;             // #
        public const int GENERATIONS = 50;           // #
        public const int PROB_SHORTCUT = 0;
        public const int MATRIXOFFSET = 150;
        public const string RESULTSFILE = "RESULTS";
    }
}