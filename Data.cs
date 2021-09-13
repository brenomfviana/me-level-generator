using System;
using System.Collections.Generic;
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
        public double duration { get; set; }
        // [JsonInclude]
        // public List<Individual> initial { get; set; }
        // [JsonInclude]
        // public List<Individual> intermediate { get; set; }
        // [JsonInclude]
        // public List<Individual> final { get; set; }
    }
}