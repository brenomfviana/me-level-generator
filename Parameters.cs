namespace LevelGenerator
{
    /// This struct holds the parameters of the evolutionary level generator.
    public struct Parameters
    {
        // TODO: Remove
        public int nV, nK, nL;
        public float lCoef;

        /// The seed that initializes the random generator.
        public int seed { get; }
        /// The maximum number of generations.
        public int generations { get; }
        /// The initial population size.
        public int initial { get; }
        /// The mutation chance.
        public int mutation { get; }
        /// The crossover chance.
        public int crossover { get; }
        /// The number of competitors of tournament selection.
        public int competitors { get; }

        /// Parameters constructor.
        public Parameters(
            int _seed,
            int _generations,
            int _initial,
            int _mutation,
            int _crossover,
            int _competitors
        ) {
            nV = 20;
            nK = 4;
            nL = 4;
            lCoef = 1.5f;
            seed = _seed;
            generations = _generations;
            initial = _initial;
            mutation = _mutation;
            crossover = _crossover;
            competitors = _competitors;
        }
    }
}