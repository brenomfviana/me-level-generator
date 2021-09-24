namespace LevelGenerator
{
    /// The types of rooms that a dungeon may have.
    public enum RoomType{
        normal = 1 << 0, // Normal room
        key = 1 << 1,    // Room with a key
        locked = 1 << 2, // Locked room
    };

    /// This class represents a room of a dungeon.
    public class Room
    {
        /// Counter of IDs of rooms.
        private static int ID = 0;

        /// Return the next room ID.
        public static int getNextId()
        {
            return ID++;
        }

        /// The reference of the room in ternary heap.
        public int index;
        /// Room ID. It is based on the sequential identifier.
        private int roomId;
        /// Type of the room.
        private RoomType type;
        /// The key that open this room (if it is locked).
        private int keyToOpen = -1;
        /// Room x position in the grid.
        private int x = 0;
        /// Room y position in the grid.
        private int y = 0;
        private int depth = 0;
        /// Rotation of the individual's parent position related to the normal
        /// cartesian orientation (with 0 meaning the parent is in the North of
        /// the child (Above), 90 being in the East (Right) and so on) Will be
        /// later used to construct the grid of the room
        private int rotation = 0;
        /// Saves the direction of the parent (if it is right, bottom or left
        /// child) Reduces operations at crossover
        private Util.Direction parentDirection = Util.Direction.Down;

        public int Depth { get => depth; set => depth = value; }
        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }
        public int RoomId { get => roomId; set => roomId = value; }
        public Util.Direction ParentDirection { get => parentDirection; set => parentDirection = value; }
        public int Rotation { get => rotation; set => rotation = value; }
        public RoomType RoomType { get => type; set => type = value; }
        public int KeyToOpen { get => keyToOpen; set => keyToOpen = value; }

        /// Room constructor. The default is a normal room, without a lock so,
        /// without a key to open and without a predefined id
        /// If a key room defines the key to open as its id, and if a locked
        /// one, uses the id of the room which has the key that opens it
        public Room(
            RoomType roomType = RoomType.normal,
            int keyToOpen = -1,
            int id = -1
        ) {
            if (id == -1)
            {
                RoomId = Room.getNextId();
            }
            else
            {
                RoomId = id;
            }
            RoomType = roomType;
            if (RoomType == RoomType.key)
            {
                this.KeyToOpen = roomId;
            }
            else if (RoomType == RoomType.locked)
            {
                this.KeyToOpen = keyToOpen;
            }
        }

        /// Makes a deep copy of a room.
        /// The parent, children and neighboors must be replaced for the right
        /// ones in the dungeon's copy method.
        public Room Clone()
        {
            Room clone = new Room(type, keyToOpen, roomId);
            clone.depth = depth;
            clone.parentDirection = parentDirection;
            clone.rotation = rotation;
            clone.x = x;
            clone.y = y;
            clone.index = index;
            return clone;
        }
    }
}