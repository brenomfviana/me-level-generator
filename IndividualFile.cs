using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LevelGenerator
{
    /// This class defines how the generated individuals must be written.
    [Serializable]
    public class IndividualFile
    {
        [JsonInclude]
        public Dimensions dimensions;
        [JsonInclude]
        public List<Room> rooms;
        [JsonInclude]
        public int generation;
        [JsonInclude]
        public float fitness;
        [JsonInclude]
        public int numberOfRooms;
        [JsonInclude]
        public int numberOfKeys;
        [JsonInclude]
        public int numberOfLocks;
        [JsonInclude]
        public float linearCoefficient;
        [JsonInclude]
        public int neededLocks;
        [JsonInclude]
        public float neededRooms;
        [JsonInclude]
        public float exploration;
        [JsonInclude]
        public float leniency;
        [JsonInclude]
        public float fGoal;
        [JsonInclude]
        public float fRooms;
        [JsonInclude]
        public float fKeys;
        [JsonInclude]
        public float fLocks;
        [JsonInclude]
        public float fLinearCoefficient;
        [JsonInclude]
        public float fEnemySparsity;
        [JsonInclude]
        public float fNeededRooms;
        [JsonInclude]
        public float fNeededLocks;

        public IndividualFile()
        {
            rooms = new List<Room>();
        }

        /// This class holds the features that a room may have.
        [Serializable]
        public class Room
        {
            [JsonInclude]
            public Coordinates coordinates;
            [JsonInclude]
            public string type;
            [JsonInclude]
            public List<int> keys;
            [JsonInclude]
            public List<int> locks;
            public int enemies = -1;

            [DefaultValue(-1), JsonInclude]
            public int Enemies { get => enemies; set => enemies = value; }
        }
    }

    /// This class holds the level dimensions.
    [Serializable]
    public class Dimensions
    {
        private int width;
        private int height;

        public Dimensions(
            int _width,
            int _height
        ) {
            Width = _width;
            Height = _height;
        }

        [JsonInclude]
        public int Width { get => width; set => width = value; }
        [JsonInclude]
        public int Height { get => height; set => height = value; }
    }

    /// This class holds the coordinates of a level room.
    [Serializable]
    public class Coordinates
    {
        private int x;
        private int y;

        public Coordinates(
            int _x,
            int _y
        ) {
            X = _x;
            Y = _y;
        }

        [DefaultValue(-1)]
        public int X { get => x; set => x = value; }
        [DefaultValue(-1)]
        public int Y { get => y; set => y = value; }

        public override bool Equals(
            object _obj
        ) {
            if (_obj is null || !GetType().Equals(_obj.GetType()))
            {
                return false;
            }
            Coordinates coordinates = (Coordinates) _obj;
            return coordinates.x == x && coordinates.y == y;
        }

        public override string ToString()
        {
            return $"Coordinates X = {X}, Y = {Y}";
        }

        public override int GetHashCode()
        {
            return x ^ y;
        }
    }
}