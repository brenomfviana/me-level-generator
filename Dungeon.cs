using System;
using System.Collections.Generic;

namespace LevelGenerator
{
    /// This class represents a dungeon level.
    public class Dungeon
    {
        public static readonly float PROB_HAS_CHILD = 100f;
        public static readonly float PROB_CHILD = 100f / 3;
        public static readonly int MAX_DEPTH = 5;

        /// The number of keys.
        public int keys;
        /// The number of locked doors.
        public int locks;
        /// Room Grid, where a reference to all the existing room will be maintained for quick access when creating nodes.
        public RoomGrid grid;
        /// The list of rooms (easier to add neighbors).
        private List<Room> rooms;
        public List<Room> Rooms { get => rooms; }

        /// Dungeon constructor.
        ///
        /// Create and return a new dungeon with the starting room.
        public Dungeon()
        {
            rooms = new List<Room>();
            Room root = RoomFactory.CreateRoot();
            Rooms.Add(root);
            grid = new RoomGrid();
            grid[root.x, root.y] = root;
        }

        /*
         * Makes a deep copy of the dungeon, also sets right the parent, children and neighbors of the copied rooms
         * now that grid information is available
         */
        public Dungeon Clone()
        {
            Dungeon dungeon = new Dungeon();
            dungeon.rooms = new List<Room>();
            dungeon.grid = new RoomGrid();
            dungeon.keys = keys;
            dungeon.locks = locks;
            Room aux;
            foreach (Room old in rooms)
            {
                aux = old.Clone();
                dungeon.rooms.Add(aux);
                dungeon.grid[old.x, old.y] = aux;
            }
            //Need to use the grid to copy the neighboors, children and parent
            //Checks the position of the node in the grid and then substitutes the old room with the copied one
            foreach (Room room in dungeon.rooms)
            {
                if (room.parent != null)
                {
                    room.parent = dungeon.grid[room.parent.x, room.parent.y];
                }
                if (room.right != null)
                {
                    room.right = dungeon.grid[room.right.x, room.right.y];
                }
                if (room.bottom != null)
                {
                    room.bottom = dungeon.grid[room.bottom.x, room.bottom.y];
                }
                if (room.left != null)
                {
                    room.left = dungeon.grid[room.left.x, room.left.y];
                }
            }
            return dungeon;
        }

        /*
         *  Instantiates a room and tries to add it as a child of the actual room, considering its direction and position
         *  If there is not a room in the grid at the entered coordinates, create the room, add it to the room list
         *  and also enqueue it so it can be explored later
         */
        public void InstantiateRoom(
            ref Room child,
            ref Room current,
            Common.Direction dir,
            ref Random rand
        ) {
            if (current.ValidateChild(dir, grid))
            {
                child = RoomFactory.CreateRoom(ref rand);
                current.InsertChild(ref grid, ref child, dir);
                child.parentDirection = dir;
                Rooms.Add(child);
                grid[child.x, child.y] = child;
            }
        }

        /*
         * Removes the nodes that will be taken out of the dungeon from the dungeon's grid
         */
        public void RemoveFromGrid(Room cut)
        {
            if (cut != null)
            {
                grid[cut.x, cut.y] = null;
                rooms.Remove(cut);
                if (cut.left != null && cut.left.parent != null && cut.left.parent.Equals(cut))
                {
                    RemoveFromGrid(cut.left);
                }
                if (cut.bottom != null && cut.bottom.parent != null && cut.bottom.parent.Equals(cut))
                {
                    RemoveFromGrid(cut.bottom);
                }

                if (cut.right != null && cut.right.parent != null && cut.right.parent.Equals(cut))
                {
                    RemoveFromGrid(cut.right);
                }
            }
        }

        /*
         * Updates the grid from the dungeon with the position of all the new rooms based on the new rotation of the traded room
         * If a room already exists in the grid, "ignores" all the children node of this room
         */
        public void RefreshGrid(ref Room room)
        {
            bool hasInserted;
            if (room != null)
            {
                grid[room.x, room.y] = room;
                rooms.Add(room);
                Room aux = room.left;
                if (aux != null && aux.parent != null && aux.parent.Equals(room))
                {
                    hasInserted = room.ValidateChild(Common.Direction.Left, grid);
                    if (hasInserted)
                    {
                        room.InsertChild(ref grid, ref aux, Common.Direction.Left);
                        RefreshGrid(ref aux);
                    }
                    else
                    {
                        room.left = null;
                    }
                }
                aux = room.bottom;
                if (aux != null && aux.parent != null && aux.parent.Equals(room))
                {
                    hasInserted = room.ValidateChild(Common.Direction.Down, grid);
                    if (hasInserted)
                    {
                        room.InsertChild(ref grid, ref aux, Common.Direction.Down);
                        RefreshGrid(ref aux);
                    }
                    else
                    {
                        room.bottom = null;
                    }
                }
                aux = room.right;
                if (aux != null && aux.parent != null && aux.parent.Equals(room))
                {
                    hasInserted = room.ValidateChild(Common.Direction.Right, grid);
                    if (hasInserted)
                    {
                        room.InsertChild(ref grid, ref aux, Common.Direction.Right);
                        RefreshGrid(ref aux);
                    }
                    else
                        room.right = null;
                }
            }
        }

        /*
         * While a node (room) has not been visited, or while the max depth of the tree has not been reached, visit each node and create its children
         */
        public void GenerateRooms(
            ref Random _rand
        ) {
            Queue<Room> toVisit = new Queue<Room>();
            toVisit.Enqueue(rooms[0]);
            int prob;
            while (toVisit.Count > 0)
            {
                Room current = toVisit.Dequeue();
                int actualDepth = current.depth;
                //If max depth allowed has been reached, stop creating children
                if (actualDepth > MAX_DEPTH)
                {
                    toVisit.Clear();
                    break;
                }
                //Check how many children the node will have, if any.
                prob = Common.RandomPercent(ref _rand);
                //Console.WriteLine(prob);
                //The parent node has 100% chance to have children, then, at each height, 10% of chance to NOT have children is added.
                //If a node has a child, create it with the RoomFactory, insert it as a child of the actual node in the right place
                //Also enqueue it to be visited later and add it to the list of the rooms of this dungeon
                if (prob <= (PROB_HAS_CHILD * (1 - actualDepth / (MAX_DEPTH + 1))))
                {
                    Room child = null;
                    Common.Direction dir = Common.RandomElementFromArray(Common.AllDirections(), ref _rand);
                    prob = Common.RandomPercent(ref _rand);

                    if (prob < PROB_CHILD)
                    {
                        InstantiateRoom(ref child, ref current, dir, ref _rand);
                    }
                    else if (prob < PROB_CHILD * 2)
                    {
                        InstantiateRoom(ref child, ref current, dir, ref _rand);
                        Common.Direction dir2;
                        do
                        {
                            dir2 = Common.RandomElementFromArray(Common.AllDirections(), ref _rand);
                        } while (dir == dir2);
                        InstantiateRoom(ref child, ref current, dir2, ref _rand);
                    }
                    else
                    {
                        InstantiateRoom(ref child, ref current, Common.Direction.Right, ref _rand);
                        InstantiateRoom(ref child, ref current, Common.Direction.Down, ref _rand);
                        InstantiateRoom(ref child, ref current, Common.Direction.Left, ref _rand);
                    }
                }
                //
                Room[] children = new Room[] {
                    current.left,
                    current.bottom,
                    current.right
                };
                foreach (Room child in children)
                {
                    if (child != null && current.Equals(child.parent))
                    {
                        toVisit.Enqueue(child);
                    }
                }
            }
            keys = RoomFactory.AvailableKeys.Count + RoomFactory.UsedKeys.Count;
            locks = RoomFactory.UsedKeys.Count;
        }

        /*
         * Recreates the room list by visiting all the rooms in the tree and adding them to the list while also counting the number of locks and keys
         **/
        public void FixRooms()
        {
            Queue<Room> toVisit = new Queue<Room>();
            toVisit.Enqueue(rooms[0]);
            keys = 0;
            locks = 0;
            rooms.Clear();
            while (toVisit.Count > 0)
            {
                Room current = toVisit.Dequeue();
                rooms.Add(current);
                if (current.type == RoomType.Key)
                    keys++;
                else if (current.type == RoomType.Locked)
                    locks++;
                //
                Room[] children = new Room[] {
                    current.left,
                    current.bottom,
                    current.right
                };
                foreach (Room child in children)
                {
                    if (child != null && current.Equals(child.parent))
                    {
                        toVisit.Enqueue(child);
                    }
                }
            }
        }

        /*
         * Add lock and key
         */
        public void AddLockAndKey(ref Random rand)
        {
            Queue<Room> toVisit = new Queue<Room>();
            toVisit.Enqueue(rooms[0]);
            bool hasKey = false;
            bool hasLock = false;
            int lockId = -1;
            while (toVisit.Count > 0 && !hasLock)
            {
                Room current = toVisit.Dequeue();
                if (current.type == RoomType.Normal && !current.Equals(rooms[0]))
                {
                    if (Common.RandomPercent(ref rand) <= RoomFactory.PROB_KEY_ROOM + RoomFactory.PROB_LOCKER_ROOM)
                    {
                        if (!hasKey)
                        {
                            current.type = RoomType.Key;
                            current.id = Room.GetNextId();
                            current.key = current.id;
                            lockId = current.id;
                            hasKey = true;
                            grid[current.x, current.y] = current;
                        }
                        else
                        {
                            current.type = RoomType.Locked;
                            current.key = lockId;
                            hasLock = true;
                            grid[current.x, current.y] = current;
                        }
                    }
                }
                //
                Room[] children = new Room[] {
                    current.left,
                    current.bottom,
                    current.right
                };
                foreach (Room child in children)
                {
                    if (child != null && current.Equals(child.parent))
                    {
                        toVisit.Enqueue(child);
                    }
                }
            }
        }

        /*
         * Add lock and key
         */
        public void RemoveLockAndKey(
            ref Random rand
        ) {
            int removeKey = Common.RandomInt((0, keys - 1), ref rand);
            int removeLock = removeKey;
            Room current;
            current = rooms[0];
            Queue<Room> toVisit = new Queue<Room>();
            toVisit.Enqueue(current);
            bool hasKey = false;
            bool hasLock = false;
            int lockId = -1;
            int keyCount = 0;
            foreach (Room r in rooms)
            {
                if (r.type == RoomType.Key)
                {
                    if (removeKey == keyCount)
                        lockId = r.id;
                    keyCount++;
                }
            }
            //Console.WriteLine("Searching Id:" + lockId);
            while (toVisit.Count > 0 && (!hasLock || !hasKey))
            {
                current = toVisit.Dequeue();
                if (current.type == RoomType.Key)
                {
                    //Console.WriteLine("KeyId:" + current.id);
                    if (current.id == lockId)
                    {
                        current.type = RoomType.Normal;
                        current.key = -1;
                        lockId = current.id;
                        grid[current.x, current.y] = current;
                        hasKey = true;
                    }
                }
                //
                Room[] children = new Room[] {
                    current.left,
                    current.bottom,
                    current.right
                };
                foreach (Room child in children)
                {
                    if (child != null && current.Equals(child.parent))
                    {
                        toVisit.Enqueue(child);
                    }
                }
            }

            current = rooms[0];
            toVisit.Clear();
            toVisit.Enqueue(current);

            while (toVisit.Count > 0 && !hasLock)
            {
                current = toVisit.Dequeue();
                if (current.type == RoomType.Locked)
                {
                    if (current.key == lockId)
                    {
                        current.type = RoomType.Normal;
                        current.key = -1;
                        hasLock = true;
                    }
                }
                //
                Room[] children = new Room[] {
                    current.left,
                    current.bottom,
                    current.right
                };
                foreach (Room child in children)
                {
                    if (child != null && current.Equals(child.parent))
                    {
                        toVisit.Enqueue(child);
                    }
                }
            }
            keys -= Convert.ToInt32(hasKey);
            locks -= Convert.ToInt32(hasLock);
        }
    }
}