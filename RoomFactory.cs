using System;
using System.Collections.Generic;

namespace LevelGenerator
{
    /// This class is responsible for create rooms of dungeons.
    public class RoomFactory
    {
        /// Probability of normal rooms to be created.
        public static readonly float PROB_NORMAL_ROOM = 70f;
        /// Probability of rooms with keys to be created.
        public static readonly float PROB_KEY_ROOM = 15f;
        /// Probability of locked rooms to be created.
        public static readonly float PROB_LOCKER_ROOM = 15f;

        /// List of IDs of the available keys.
        private static List<int> availableKeys = new List<int>();
        /// List of IDs of the used keys.
        private static List<int> usedKeys = new List<int>();

        /// Get method for the list of IDs of the available keys.
        public static List<int> AvailableKeys { get => availableKeys; }
        /// Get method for the list of IDs of the used keys.
        public static List<int> UsedKeys { get => usedKeys; }

        /// Create and return the root room of a dungeon.
        ///
        /// The root room is a normal room.
        public static Room CreateRoot()
        {
            availableKeys.Clear();
            usedKeys.Clear();
            return new Room();
        }

        /// Create and return a new random room of a dungeon.
        ///
        /// The created room will have one of the following types: a normal
        /// room, a room with a key, or a locked room. Besides, locks and keys
        /// are placed in the dungeon without bound one to the other.
        public static Room CreateRoom(
            ref Random _rand
        ) {
            // Choose a random probability for the type of room to be created
            int prob = Util.RandomPercent(ref _rand);
            // Initialize the new room
            Room room = null;
            // Probability penalty for levels with exceding number of locks
            float penalty = 0.0f;
            // The more keys without locks higher the chances to create a lock
            if (availableKeys.Count > 0)
            {
                penalty = availableKeys.Count * 0.1f;
            }
            // Check the probability and create the respective type of room
            if (PROB_NORMAL_ROOM - penalty > prob)
            {
                // Create a normal room
                room = new Room();
            }
            else if (PROB_NORMAL_ROOM + PROB_KEY_ROOM - penalty > prob ||
                // A lock can only exist if a room with a key has already been
                // created, else, the lock room is turned into a key room
                availableKeys.Count == 0
            ) {
                // Create a room with a key
                room = new Room(RoomType.key);
                // Add the room ID to the list of available keys
                availableKeys.Add(room.RoomId);
            }
            else
            {
                // Get the key ID that open the lock
                int key = availableKeys[0];
                availableKeys.RemoveAt(0);
                // Create a locked room
                room = new Room(RoomType.locked, key);
                // Add the key ID that opens the created locked room
                usedKeys.Add(room.RoomId);
            }
            return room;
        }
    }
}