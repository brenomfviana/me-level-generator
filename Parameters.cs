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
        /// The number of competitors of tournament selection.
        [JsonInclude]
        public int competitors { get; }
        /// The aimed number of rooms.
        public int rooms { get; }
        /// The aimed number of keys.
        public int keys { get; }
        /// The aimed number of locks.
        public int locks { get; }
        /// The aimed number of enemies.
        public int enemies { get; }
        /// The aimed linear coefficient.
        public float linearCoefficient { get; }

        /// Parameters constructor.
        public Parameters(
            int _seed,
            int _generations,
            int _population,
            int _mutation,
            int _competitors,
            int _rooms,
            int _keys,
            int _locks,
            int _enemies,
            float _linearCoefficient
        ) {
            seed = _seed;
            generations = _generations;
            population = _population;
            mutation = _mutation;
            competitors = _competitors;
            rooms = _rooms;
            keys = _keys;
            locks = _locks;
            enemies = _enemies;
            linearCoefficient = _linearCoefficient;
        }
    }
}