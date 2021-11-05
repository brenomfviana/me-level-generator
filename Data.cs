using System;
using System.Text.Json.Serialization;

namespace LevelGenerator
{
    /// This struct holds the most relevant data of the evolutionary process.
    [Serializable]
    public struct Data
    {
        [JsonInclude]
        public Parameters parameters { get; set; }
        [JsonInclude]
        public int generations { get; set; }
        [JsonInclude]
        public double duration { get; set; }
    }
}