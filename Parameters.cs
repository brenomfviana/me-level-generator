using System.Text.Json.Serialization;

namespace LevelGenerator
{
    /// This struct holds the parameters of the evolutionary level generator.
    public struct Parameters
    {
        /// The seed that initializes the random generator.
        [JsonInclude]
        public int seed { get; }
        /// The maximum number of generations.
        [JsonInclude]
        public int generations { get; }
        /// The initial population size.
        [JsonInclude]
        public int population { get; }
        /// The mutation chance.
        [JsonInclude]
        public int mutation { get; }
        /// The crossover chance.
        [JsonInclude]
        public int crossover { get; }
        /// The number of competitors of tournament selection.
        [JsonInclude]
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