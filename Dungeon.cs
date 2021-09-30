using System;
using System.Collections;
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
        /// The list of rooms (easier to add neighbors).
        private List<Room> rooms;
        /// Room Grid, where a reference to all the existing room will be maintained for quick access when creating nodes.
        public RoomGrid grid;

        // Collected data. Remove from this class.
        public int neededLocks;
        // Collected data. Remove from this class.
        public float neededRooms;
        // What is the function of this?
        private int desiredKeys;
        // What is the function of this?
        private float avgChildren;
        // Queue with the notes that need to be visited
        // - Why they need to be visited?
        public Queue toVisit;

        public List<Room> Rooms { get => rooms; }
        public float AvgChildren { get => avgChildren; set => avgChildren = value; }

        public Dungeon()
        {
            toVisit = new Queue();
            rooms = new List<Room>();
            Room root = RoomFactory.CreateRoot();
            Rooms.Add(root);
            toVisit.Enqueue(root);
            grid = new RoomGrid();
            grid[root.x, root.y] = root;
            neededRooms = 0;
            neededLocks = 0;
        }

        /*
         * Makes a deep copy of the dungeon, also sets right the parent, children and neighbors of the copied rooms
         * now that grid information is available
         */
        public Dungeon Clone()
        {
            Dungeon copyDungeon = new Dungeon();
            copyDungeon.toVisit = new Queue();
            copyDungeon.rooms = new List<Room>();
            copyDungeon.grid = new RoomGrid();
            copyDungeon.keys = keys;
            copyDungeon.locks = locks;
            copyDungeon.desiredKeys = desiredKeys;
            copyDungeon.avgChildren = avgChildren;
            Room aux;
            foreach (Room oldRoom in rooms)
            {
                aux = oldRoom.Clone();
                copyDungeon.rooms.Add(aux);
                copyDungeon.grid[oldRoom.x, oldRoom.y] = aux;
            }
            //Need to use the grid to copy the neighboors, children and parent
            //Checks the position of the node in the grid and then substitutes the old room with the copied one
            foreach (Room newRoom in copyDungeon.rooms)
            {
                if (newRoom.parent != null)
                {
                    newRoom.parent = copyDungeon.grid[newRoom.parent.x, newRoom.parent.y];
                }
                if (newRoom.right != null)
                {
                    newRoom.right = copyDungeon.grid[newRoom.right.x, newRoom.right.y];
                }
                if (newRoom.bottom != null)
                {
                    newRoom.bottom = copyDungeon.grid[newRoom.bottom.x, newRoom.bottom.y];
                }
                if (newRoom.left != null)
                {
                    newRoom.left = copyDungeon.grid[newRoom.left.x, newRoom.left.y];
                }
            }
            return copyDungeon;
        }

        public void CalcAvgChildren()
        {
            avgChildren = 0.0f;
            int childCount;
            int childLess = 0;
            foreach (Room room in Rooms)
            {
                childCount = 0;
                if (room.right != null && room.right.parent != null)
                    childCount += 1;
                if (room.left != null && room.left.parent != null)
                    childCount += 1;
                if (room.bottom != null && room.bottom.parent != null)
                    childCount += 1;
                if (childCount == 0)
                    childLess++;
                avgChildren += childCount;
            }
            avgChildren = avgChildren / (Rooms.Count - childLess);
        }

        /*
         *  Instantiates a room and tries to add it as a child of the actual room, considering its direction and position
         *  If there is not a room in the grid at the entered coordinates, create the room, add it to the room list
         *  and also enqueue it so it can be explored later
         */
        public void InstantiateRoom(
            ref Room child,
            ref Room actualRoom,
            Common.Direction dir,
            ref Random rand
        ) {
            if (actualRoom.ValidateChild(dir, grid))
            {
                child = RoomFactory.CreateRoom(ref rand);
                actualRoom.InsertChild(ref grid, ref child, dir);
                child.parentDirection = dir;
                toVisit.Enqueue(child);
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
        public void RefreshGrid(ref Room newRoom)
        {
            bool hasInserted;
            if (newRoom != null)
            {
                grid[newRoom.x, newRoom.y] = newRoom;
                rooms.Add(newRoom);
                Room aux = newRoom.left;
                if (aux != null && aux.parent != null && aux.parent.Equals(newRoom))
                {
                    hasInserted = newRoom.ValidateChild(Common.Direction.Left, grid);
                    if (hasInserted)
                    {
                        newRoom.InsertChild(ref grid, ref aux, Common.Direction.Left);
                        RefreshGrid(ref aux);
                    }
                    else
                    {
                        newRoom.left = null;
                    }
                }
                aux = newRoom.bottom;
                if (aux != null && aux.parent != null && aux.parent.Equals(newRoom))
                {
                    hasInserted = newRoom.ValidateChild(Common.Direction.Down, grid);
                    if (hasInserted)
                    {
                        newRoom.InsertChild(ref grid, ref aux, Common.Direction.Down);
                        RefreshGrid(ref aux);
                    }
                    else
                    {
                        newRoom.bottom = null;
                    }
                }
                aux = newRoom.right;
                if (aux != null && aux.parent != null && aux.parent.Equals(newRoom))
                {
                    hasInserted = newRoom.ValidateChild(Common.Direction.Right, grid);
                    if (hasInserted)
                    {
                        newRoom.InsertChild(ref grid, ref aux, Common.Direction.Right);
                        RefreshGrid(ref aux);
                    }
                    else
                        newRoom.right = null;
                }
            }
        }

        /*
         * While a node (room) has not been visited, or while the max depth of the tree has not been reached, visit each node and create its children
         */
        public void GenerateRooms(
            ref Random _rand
        ) {
            int prob;
            while (toVisit.Count > 0)
            {
                Room actualRoom = toVisit.Dequeue() as Room;
                int actualDepth = actualRoom.Depth;
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
                        InstantiateRoom(ref child, ref actualRoom, dir, ref _rand);
                    }
                    else if (prob < PROB_CHILD * 2)
                    {
                        InstantiateRoom(ref child, ref actualRoom, dir, ref _rand);
                        Common.Direction dir2;
                        do
                        {
                            dir2 = Common.RandomElementFromArray(Common.AllDirections(), ref _rand);
                        } while (dir == dir2);
                        InstantiateRoom(ref child, ref actualRoom, dir2, ref _rand);
                    }
                    else
                    {
                        InstantiateRoom(ref child, ref actualRoom, Common.Direction.Right, ref _rand);
                        InstantiateRoom(ref child, ref actualRoom, Common.Direction.Down, ref _rand);
                        InstantiateRoom(ref child, ref actualRoom, Common.Direction.Left, ref _rand);
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
                Room actualRoom = toVisit.Dequeue() as Room;
                rooms.Add(actualRoom);
                if (actualRoom.type == RoomType.Key)
                    keys++;
                else if (actualRoom.type == RoomType.Locked)
                    locks++;
                if (actualRoom.left != null)
                    toVisit.Enqueue(actualRoom.left);
                if (actualRoom.bottom != null)
                    toVisit.Enqueue(actualRoom.bottom);
                if (actualRoom.right != null)
                    toVisit.Enqueue(actualRoom.right);
            }
        }

        /*
         * Add lock and key
         */
        public void AddLockAndKey(ref Random rand)
        {
            Room actualRoom;
            Room child;
            actualRoom = rooms[0];
            toVisit.Clear();
            toVisit.Enqueue(actualRoom);
            bool hasKey = false;
            bool hasLock = false;
            int lockId = -1;

            while (toVisit.Count > 0 && !hasLock)
            {
                actualRoom = toVisit.Dequeue() as Room;
                if (actualRoom.type == RoomType.Normal && !actualRoom.Equals(rooms[0]))
                {
                    if (Common.RandomPercent(ref rand) <= RoomFactory.PROB_KEY_ROOM + RoomFactory.PROB_LOCKER_ROOM)
                    {
                        if (!hasKey)
                        {
                            actualRoom.type = RoomType.Key;
                            actualRoom.id = Room.GetNextId();
                            actualRoom.key = actualRoom.id;
                            lockId = actualRoom.id;
                            hasKey = true;
                            grid[actualRoom.x, actualRoom.y] = actualRoom;
                        }
                        else
                        {
                            actualRoom.type = RoomType.Locked;
                            actualRoom.key = lockId;
                            hasLock = true;
                            grid[actualRoom.x, actualRoom.y] = actualRoom;
                        }
                    }
                }
                child = actualRoom.left;
                if (child != null)
                    if (actualRoom.Equals(child.parent))
                    {
                        toVisit.Enqueue(child);
                    }
                child = actualRoom.bottom;
                if (child != null)
                    if (actualRoom.Equals(child.parent))
                    {
                        toVisit.Enqueue(child);
                    }
                child = actualRoom.right;
                if (child != null)
                    if (actualRoom.Equals(child.parent))
                    {
                        toVisit.Enqueue(child);
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
            Room actualRoom;
            Room child;
            actualRoom = rooms[0];
            toVisit.Clear();
            toVisit.Enqueue(actualRoom);
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
                actualRoom = toVisit.Dequeue() as Room;
                if (actualRoom.type == RoomType.Key)
                {
                    //Console.WriteLine("KeyId:" + actualRoom.id);
                    if (actualRoom.id == lockId)
                    {
                        actualRoom.type = RoomType.Normal;
                        actualRoom.key = -1;
                        lockId = actualRoom.id;
                        grid[actualRoom.x, actualRoom.y] = actualRoom;
                        hasKey = true;
                    }
                }
                child = actualRoom.left;
                if (child != null)
                    if (actualRoom.Equals(child.parent))
                    {
                        toVisit.Enqueue(child);
                    }
                child = actualRoom.bottom;
                if (child != null)
                    if (actualRoom.Equals(child.parent))
                    {
                        toVisit.Enqueue(child);
                    }
                child = actualRoom.right;
                if (child != null)
                    if (actualRoom.Equals(child.parent))
                    {
                        toVisit.Enqueue(child);
                    }

            }

            actualRoom = rooms[0];
            toVisit.Clear();
            toVisit.Enqueue(actualRoom);

            while (toVisit.Count > 0 && !hasLock)
            {
                actualRoom = toVisit.Dequeue() as Room;
                if (actualRoom.type == RoomType.Locked)
                {
                    if (actualRoom.key == lockId)
                    {
                        actualRoom.type = RoomType.Normal;
                        actualRoom.key = -1;
                        hasLock = true;
                    }
                }
                child = actualRoom.left;
                if (child != null)
                    if (actualRoom.Equals(child.parent))
                    {
                        toVisit.Enqueue(child);
                    }
                child = actualRoom.bottom;
                if (child != null)
                    if (actualRoom.Equals(child.parent))
                    {
                        toVisit.Enqueue(child);
                    }
                child = actualRoom.right;
                if (child != null)
                    if (actualRoom.Equals(child.parent))
                    {
                        toVisit.Enqueue(child);
                    }

            }

            keys -= Convert.ToInt32(hasKey);
            locks -= Convert.ToInt32(hasLock);
        }
    }
}