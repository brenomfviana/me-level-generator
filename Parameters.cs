using System.Text.Json.Serialization;

namespace LevelGenerator
{
    /// This struct holds the parameters of the evolutionary level generator.
    public struct Parameters
    {
        /// The seed that initializes the random generator.
        [JsonInclude]
        public int seed { get; }
        /// The maximum time.
        [JsonInclude]
        public int time { get; }
        /// The initial population size.
        [JsonInclude]
        public int population { get; }
        /// The mutation chance.
        [JsonInclude]
        public int mutation { get; }
        /// The number of competitors of tournament selection.
        [JsonInclude]
        public int competitors { get; }
        /// If it is true, the sparsity weights the positions with the number
        /// of enemies in the respective room.
        [JsonInclude]
        public bool weight { get; }
        /// If it is true, the standard deviation will consider all the rooms
        /// where enemies can be placed.
        [JsonInclude]
        public bool inclusive { get; }
        /// The aimed number of rooms.
        [JsonInclude]
        public int rooms { get; }
        /// The aimed number of keys.
        [JsonInclude]
        public int keys { get; }
        /// The aimed number of locks.
        [JsonInclude]
        public int locks { get; }
        /// The aimed number of enemies.
        [JsonInclude]
        public int enemies { get; }
        /// The aimed linear coefficient.
        [JsonInclude]
        public float linearCoefficient { get; }

        /// Parameters constructor.
        public Parameters(
            int _seed,
            int _time,
            int _population,
            int _mutation,
            int _competitors,
            bool _weight,
            bool _inclusive,
            int _rooms,
            int _keys,
            int _locks,
            int _enemies,
            float _linearCoefficient
        ) {
            seed = _seed;
            time = _time;
            population = _population;
            mutation = _mutation;
            competitors = _competitors;
            weight = _weight;
            inclusive = _inclusive;
            rooms = _rooms;
            keys = _keys;
            locks = _locks;
            enemies = _enemies;
            linearCoefficient = _linearCoefficient;
        }
    }
}