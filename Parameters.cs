namespace LevelGenerator
{
    /// This struct holds the parameters of the evolutionary level generator.
    public struct Parameters
    {
        /// The seed that initializes the random generator.
        public int seed { get; }
        /// The maximum number of generations.
        public int generations { get; }
        /// The initial population size.
        public int population { get; }
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
            int _population,
            int _mutation,
            int _crossover,
            int _competitors
        ) {
            seed = _seed;
            generations = _generations;
            population = _population;
            mutation = _mutation;
            crossover = _crossover;
            competitors = _competitors;
        }
    }
}