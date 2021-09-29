using System;
using System.Collections.Generic;

namespace LevelGenerator
{
    /// This class represents a dungeon level.
    ///
    /// The dungeon consists of a ternary heap that connect the rooms (nodes)
    /// in left, bottom, and right directions.
    public class Dungeon
    {
        /// The max capacity of the ternary heap.
        public static readonly int CAPACITY = (int) Math.Pow(15f, 3f) + 1;

        /// Number of keys.
        public int keys;
        /// Number of locked doors.
        public int locks;
        /// Ternary heap of rooms.
        private List<Room> rooms;
        /// Grid of rooms.
        public RoomGrid grid;

        /// Return the ternary heap.
        public List<Room> Rooms { get => rooms; set => rooms = value; }

        public Dungeon()
        {
            // Initialize the heap with a fixed capacity
            rooms = new List<Room>();
            for (int i = 0; i < CAPACITY; i++) { rooms.Add(null); }
            // Create the root node (the starting room)
            Room root = RoomFactory.CreateRoot();
            root.index = 0;
            rooms[0] = root;
            grid = new RoomGrid();
            grid[root.X, root.Y] = root;
        }

        /// Makes a deep copy of the dungeon, also sets right the parent, children and neighbors of the copied rooms now that grid information is available
        public Dungeon Clone()
        {
            Dungeon clone = new Dungeon();
            clone.rooms = new List<Room>(new Room[CAPACITY]);
            clone.grid = new RoomGrid();
            clone.keys = keys;
            clone.locks = locks;
            foreach (Room room in rooms)
            {
                if (room != null) {
                    Room aux = room.Clone();
                    clone.grid[aux.X, aux.Y] = aux;
                    clone.rooms[aux.index] = aux;
                }
            }
            return clone;
        }

        /// Instantiates a room and tries to add it as a child of the actual room, considering its direction and position
        /// If there is not a room in the grid at the given coordinates, create the room, add it to the room list
        /// and also enqueue it so it can be explored later
        public void InstantiateRoom(
            int _parent,
            Util.Direction _dir,
            ref Random _rand
        ) {
            if (ValidateChild(_parent, _dir))
            {
                Room _child = RoomFactory.CreateRoom(ref _rand);
                InsertChild(_parent, _dir, ref _child);
            }
        }

        /// Removes the nodes that will be taken out of the dungeon from the dungeon's grid.
        public void RemoveFromGrid(
            int _room
        ) {
            if (_room >= 0 && _room < CAPACITY && rooms[_room] != null)
            {
                // Remove the room from the grid
                grid[rooms[_room].X, rooms[_room].Y] = null;
                // Get the room children
                int[] children = new int[] {
                    GetChildIndexByDirection(
                        _room, Util.Direction.Left
                    ),
                    GetChildIndexByDirection(
                        _room, Util.Direction.Down
                    ),
                    GetChildIndexByDirection(
                        _room, Util.Direction.Right
                    ),
                };
                // Remove the remaining rooms in the branch
                foreach (int child in children)
                {
                    if (child < 0 &&
                        child >= CAPACITY &&
                        rooms[child] == null
                    ) {
                        continue;
                    }
                    Room parent = GetParent(child);
                    if (parent != null &&
                        parent.Equals(rooms[_room])
                    ) {
                        RemoveFromGrid(child);
                    }
                }
            }
        }

        /// Updates the grid from the dungeon with the position of all the new
        /// rooms based on the new rotation of the traded room. If a room
        /// already exists in the grid, "ignores" all the children node of this
        /// room.
        public void RefreshGrid(
            int _room,
            Util.Direction dir
        ) {
            if (_room < 0 ||
                _room >= CAPACITY ||
                rooms[_room] == null
            ) {
                return;
            }
            Room parent = GetParent(_room);
            if (ValidateChild(parent.index, dir))
            {
                (int x, int y) = GetChildPositionInGrid(parent.index, dir);
                if (grid[x, y] == null)
                {
                    // Set the room in the grid
                    rooms[_room].X = x;
                    rooms[_room].Y = y;
                    grid[x, y] = rooms[_room];
                    rooms[_room].ParentDirection = dir;
                    rooms[_room].Rotation = (rooms[parent.index].Rotation + 90) % 360;
                }
                // Get the room children
                int[] children = new int[] {
                    GetChildIndexByDirection(
                        _room, Util.Direction.Left
                    ),
                    GetChildIndexByDirection(
                        _room, Util.Direction.Down
                    ),
                    GetChildIndexByDirection(
                        _room, Util.Direction.Right
                    ),
                };
                Util.Direction[] dirs = new Util.Direction[] {
                    Util.Direction.Left,
                    Util.Direction.Down,
                    Util.Direction.Right
                };
                // Refresh the remaining rooms in the branch
                for (int i = 0; i < children.Length; i++)
                {
                    int child = children[i];
                    if (child == -1 || rooms[child] == null)
                    {
                        continue;
                    }
                    parent = GetParent(child);
                    if (parent != null &&
                        parent.Equals(rooms[_room]) &&
                        !rooms[child].Equals(parent)
                    ) {
                        RefreshGrid(child, dirs[i]);
                    }
                    else
                    {
                        rooms[child] = null;
                    }
                }
            }
            else
            {
                // If the room cannot be placed in the calculated grid position
                // Then all the branch starting from this room must be erased
                RemoveTrash(_room);
            }
        }

        /// Erase a branch starting from the root `_room`.
        private void RemoveTrash(
            int _room
        ) {
            rooms[_room] = null;
            // Get the room children
            int[] children = new int[] {
                GetChildIndexByDirection(
                    _room, Util.Direction.Left
                ),
                GetChildIndexByDirection(
                    _room, Util.Direction.Down
                ),
                GetChildIndexByDirection(
                    _room, Util.Direction.Right
                ),
            };
            for (int i = 0; i < children.Length; i++)
            {
                int child = children[i];
                if (child != -1 && rooms[child] != null)
                {
                    RemoveTrash(child);
                }
            }
        }

        /// While a node (room) has not been visited, or while the max depth of the tree has not been reached, visit each node and create its children.
        public void GenerateRooms(
            ref Random _rand
        ) {
            int current = 0;
            Queue<int> toVisit = new Queue<int>();
            toVisit.Enqueue(current);
            while (toVisit.Count > 0)
            {
                // Get the current room
                current = toVisit.Dequeue();
                int currentDepth = GetDepth(current);
                // If max depth has been reached, stop creating children
                if (currentDepth > Constants.MAX_DEPTH)
                {
                    toVisit.Clear();
                    break;
                }
                // Check how many children the node will have, if any
                int prob = Util.RandomPercent(ref _rand);
                // The parent node has 100% chance to have children, then, at each height, 10% of chance to NOT have children is added.
                // If a node has a child, create it with the RoomFactory, insert it as a child of the actual node in the right place
                // Also enqueue it to be visited later and add it to the list of the rooms of this dungeon
                float chance = (Constants.PROB_HAS_CHILD * (1 - currentDepth / (Constants.MAX_DEPTH + 1)));
                if (chance >= prob)
                {
                    Util.Direction dir = (Util.Direction) _rand.Next(3);
                    prob = Util.RandomPercent(ref _rand);

                    if (prob < Constants.PROB_1_CHILD)
                    {
                        InstantiateRoom(current, dir, ref _rand);
                    }
                    else if (prob < (Constants.PROB_1_CHILD + Constants.PROB_2_CHILD))
                    {
                        InstantiateRoom(current, dir, ref _rand);
                        Util.Direction dir2;
                        do
                        {
                            dir2 = (Util.Direction) _rand.Next(3);
                        } while (dir == dir2);
                        InstantiateRoom(current, dir2, ref _rand);
                    }
                    else
                    {
                        InstantiateRoom(current, Util.Direction.Right, ref _rand);
                        InstantiateRoom(current, Util.Direction.Down, ref _rand);
                        InstantiateRoom(current, Util.Direction.Left, ref _rand);
                    }
                }
                // Get the current room children
                int[] children = new int[] {
                    GetChildIndexByDirection(
                        current, Util.Direction.Left
                    ),
                    GetChildIndexByDirection(
                        current, Util.Direction.Down
                    ),
                    GetChildIndexByDirection(
                        current, Util.Direction.Right
                    ),
                };
                //
                foreach (int child in children)
                {
                    if (child != -1 && rooms[child] != null)
                    {
                        toVisit.Enqueue(child);
                    }
                }
            }
            // Get the number of keys and locked doors of the created dungeon
            keys = RoomFactory.AvailableKeys.Count + RoomFactory.UsedKeys.Count;
            locks = RoomFactory.UsedKeys.Count;
        }

        /// Recreate the room list by visiting all the rooms in the tree and adding them to the list while also counting the number of locks and keys
        public void FixDungeon()
        {
            Queue<int> toVisit = new Queue<int>();
            toVisit.Enqueue(0);
            keys = 0;
            locks = 0;
            while (toVisit.Count > 0)
            {
                int current = toVisit.Dequeue();
                if (rooms[current].RoomType == RoomType.key)
                {
                    keys++;
                }
                else if (rooms[current].RoomType == RoomType.locked)
                {
                    locks++;
                }
                // Get the current room children
                int[] children = new int[] {
                    GetChildIndexByDirection(
                        current, Util.Direction.Left
                    ),
                    GetChildIndexByDirection(
                        current, Util.Direction.Down
                    ),
                    GetChildIndexByDirection(
                        current, Util.Direction.Right
                    ),
                };
                //
                foreach (int child in children)
                {
                    if (child >= 0 &&
                        child < CAPACITY &&
                        rooms[child] != null
                    ) {
                        if (rooms[current].Equals(GetParent(child)))
                        {
                            toVisit.Enqueue(child);
                        }
                    }
                }
            }
        }

        /// Add lock and key.
        public void AddLockAndKey(
            ref Random _rand
        ) {
            int current = 0;
            Queue<int> toVisit = new Queue<int>();
            toVisit.Enqueue(current);
            //
            bool hasKey = false;
            bool hasLock = false;
            int lockId = -1;
            //
            while (toVisit.Count > 0 && !hasLock)
            {
                current = toVisit.Dequeue();
                if (rooms[current].RoomType == RoomType.normal
                    && !rooms[current].Equals(rooms[0])
                ) {
                    if (Util.RandomPercent(ref _rand) <= RoomFactory.PROB_KEY_ROOM + RoomFactory.PROB_LOCKER_ROOM
                    ) {
                        if (!hasKey)
                        {
                            rooms[current].RoomType = RoomType.key;
                            rooms[current].RoomId = Room.getNextId();
                            rooms[current].KeyToOpen = rooms[current].RoomId;
                            lockId = rooms[current].RoomId;
                            hasKey = true;
                            grid[rooms[current].X, rooms[current].Y] = rooms[current];
                        }
                        else
                        {
                            rooms[current].RoomType = RoomType.locked;
                            rooms[current].KeyToOpen = lockId;
                            hasLock = true;
                            grid[rooms[current].X, rooms[current].Y] = rooms[current];
                        }
                    }
                }
                // Add the children of the current node
                Room next = GetChildByDirection(current, Util.Direction.Left);
                if (next != null)
                {
                    toVisit.Enqueue(next.index);
                }
                next = GetChildByDirection(current, Util.Direction.Down);
                if (next != null)
                {
                    toVisit.Enqueue(next.index);
                }
                next = GetChildByDirection(current, Util.Direction.Right);
                if (next != null)
                {
                    toVisit.Enqueue(next.index);
                }
            }
        }

        /// Remove lock and key.
        public void RemoveLockAndKey(
            ref Random _rand
        ) {
            int current = 0;
            Queue<int> toVisit = new Queue<int>();
            toVisit.Enqueue(current);
            //
            int removeKey = _rand.Next(keys);
            int removeLock = removeKey;
            bool hasKey = false;
            bool hasLock = false;
            int lockId = -1;
            int keyCount = 0;
            foreach (Room room in rooms)
            {
                if (room != null && room.RoomType == RoomType.key)
                {
                    if (removeKey == keyCount)
                    {
                        lockId = room.RoomId;
                    }
                    keyCount++;
                }
            }
            while (toVisit.Count > 0 && (!hasLock || !hasKey))
            {
                current = toVisit.Dequeue();
                if (rooms[current].RoomType == RoomType.key)
                {
                    if (rooms[current].RoomId == lockId)
                    {
                        rooms[current].RoomType = RoomType.normal;
                        rooms[current].KeyToOpen = -1;
                        lockId = rooms[current].RoomId;
                        grid[rooms[current].X, rooms[current].Y] = rooms[current];
                        hasKey = true;
                    }
                }
                // Add the children of the current node
                Room next = GetChildByDirection(current, Util.Direction.Left);
                if (next != null)
                {
                    toVisit.Enqueue(next.index);
                }
                next = GetChildByDirection(current, Util.Direction.Down);
                if (next != null)
                {
                    toVisit.Enqueue(next.index);
                }
                next = GetChildByDirection(current, Util.Direction.Right);
                if (next != null)
                {
                    toVisit.Enqueue(next.index);
                }
            }
            current = 0;
            toVisit.Clear();
            toVisit.Enqueue(current);
            while (toVisit.Count > 0 && !hasLock)
            {
                current = toVisit.Dequeue();
                if (rooms[current].RoomType == RoomType.locked)
                {
                    if (rooms[current].KeyToOpen == lockId)
                    {
                        rooms[current].RoomType = RoomType.normal;
                        rooms[current].KeyToOpen = -1;
                        hasLock = true;
                    }
                }
                // Add the children of the current node
                Room next = GetChildByDirection(current, Util.Direction.Left);
                if (next != null)
                {
                    toVisit.Enqueue(next.index);
                }
                next = GetChildByDirection(current, Util.Direction.Down);
                if (next != null)
                {
                    toVisit.Enqueue(next.index);
                }
                next = GetChildByDirection(current, Util.Direction.Right);
                if (next != null)
                {
                    toVisit.Enqueue(next.index);
                }
            }
            keys -= Convert.ToInt32(hasKey);
            locks -= Convert.ToInt32(hasLock);
        }

        /// Get the number of non-null rooms.
        public List<Room> GetNonNullRooms()
        {
            List<Room> rs = new List<Room>();
            foreach (Room room in rooms)
            {
                if (room != null)
                {
                    rs.Add(room);
                }
            }
            return rs;
        }

        /// Get the number of children of the entered parent room.
        public int GetNumberOfChildren(
            int _parent
        ) {
            int counter = 0;
            int[] indexes = new int[] {
                _parent * 3 + 1,
                _parent * 3 + 2,
                _parent * 3 + 3
            };
            foreach (int index in indexes)
            {
                if (CAPACITY > index && rooms[index] != null) {
                    counter++;
                }
            }
            return counter;
        }

        /// Get the child of the parent room in the entered direction.
        public Room GetChildByDirection(
            int _parent,
            Util.Direction _dir
        ) {
            int index;
            switch (_dir)
            {
                case Util.Direction.Left:
                    index = _parent * 3 + 1;
                    if (index >= CAPACITY) { break; }
                    return rooms[index];
                case Util.Direction.Down:
                    index = _parent * 3 + 2;
                    if (index >= CAPACITY) { break; }
                    return rooms[index];
                case Util.Direction.Right:
                    index = _parent * 3 + 3;
                    if (index >= CAPACITY) { break; }
                    return rooms[index];
            }
            return null;
        }

        /// Return the depth of the entered room.
        public int GetDepth(
            int _room
        ) {
            if (_room == 0)
            {
                return 0;
            }
            return GetDepth(GetParentIndex(_room)) + 1;
        }

        /// Get the child of the entered parent room in the entered direction.
        public int GetChildIndexByDirection(
            int _parent,
            Util.Direction _dir
        ) {
            int index;
            switch (_dir)
            {
                case Util.Direction.Left:
                    index = _parent * 3 + 1;
                    if (index >= CAPACITY) { break; }
                    return index;
                case Util.Direction.Down:
                    index = _parent * 3 + 2;
                    if (index >= CAPACITY) { break; }
                    return index;
                case Util.Direction.Right:
                    index = _parent * 3 + 3;
                    if (index >= CAPACITY) { break; }
                    return index;
            }
            return -1;
        }

        /// Return the parent of the entered child.
        public Room GetParent(
            int _child
        ) {
            // If the entered child is the root, then return null
            if (_child == 0) { return null; }
            // Return the parent of the entered child
            return rooms[(int) (_child - 1) / 3];
        }

        /// Return the parent of the entered child.
        public int GetParentIndex(
            int _child
        ) {
            // If the entered child is the root, then return null
            if (_child == 0) { return -1; }
            // Return the parent of the entered child
            return (int) (_child - 1) / 3;
        }

        /// Check if a room can have a new child in the entered direction.
        ///
        /// Return true if a room can be placed as a child of the entered room
        /// parent and in the entered position (i.e., the position in the grid
        /// is empty), and false, otherwise.
        public bool ValidateChild(
            int _parent,
            Util.Direction _dir
        ) {
            (int x, int y) = GetChildPositionInGrid(_parent, _dir);
            return grid[x, y] == null;
        }

        /// Insert the entered room in the dungeon (ternary heap and grid).
        ///
        /// First, this method calculates both X and Y positions of the entered
        /// child room based on the parent rotation and coordinates, and on the
        /// direction of insertion. Then, it checks the position is empty in
        /// the dungeon grid, if so, then, it places the entered child room in
        /// the calculated position and rotation.
        public bool InsertChild(
            int _parent,
            Util.Direction _dir,
            ref Room _child
        ) {
            // Check if the position of the new room in the grid is empty and
            // if the position in the ternary heap is valid; if so, then insert
            // the room in the ternary heap and in the grid
            (int x, int y) = GetChildPositionInGrid(_parent, _dir);
            int index = GetChildIndexByDirection(_parent, _dir);
            if (grid[x, y] == null && index >= 0 && index < CAPACITY)
            {
                _child.X = x;
                _child.Y = y;
                _child.index = index;
                _child.ParentDirection = _dir;
                _child.Rotation = (rooms[_parent].Rotation + 90) % 360;
                rooms[index] = _child;
                grid[_child.X, _child.Y] = _child;
                return true;
            }
            return false;
        }

        /// Return a tuple corresponding to the position in the dungeon grid of
        /// the child of a parent in the entered direction.
        public (int, int) GetChildPositionInGrid(
            int _parent,
            Util.Direction _dir
        ) {
            int x = RoomGrid.LEVEL_GRID_OFFSET + 1;
            int y = RoomGrid.LEVEL_GRID_OFFSET + 1;
            if (rooms[_parent] == null) { return (x, y); }
            switch (_dir)
            {
                case Util.Direction.Right:
                    if ((rooms[_parent].Rotation / 90) % 2 != 0)
                    {
                        x = rooms[_parent].X;
                        if (rooms[_parent].Rotation == 90)
                        {
                            y = rooms[_parent].Y + 1;
                        }
                        else
                        {
                            y = rooms[_parent].Y - 1;
                        }
                    }
                    else
                    {
                        if (rooms[_parent].Rotation == 0)
                        {
                            x = rooms[_parent].X + 1;
                        }
                        else
                        {
                            x = rooms[_parent].X - 1;
                        }
                        y = rooms[_parent].Y;
                    }
                    break;

                case Util.Direction.Down:
                    if ((rooms[_parent].Rotation / 90) % 2 != 0)
                    {
                        y = rooms[_parent].Y;
                        if (rooms[_parent].Rotation == 90)
                        {
                            x = rooms[_parent].X + 1;
                        }
                        else
                        {
                            x = rooms[_parent].X - 1;
                        }
                    }
                    else
                    {
                        if (rooms[_parent].Rotation == 0)
                        {
                            y = rooms[_parent].Y - 1;
                        }
                        else
                        {
                            y = rooms[_parent].Y + 1;
                        }
                        x = rooms[_parent].X;
                    }
                    break;

                case Util.Direction.Left:
                    if ((rooms[_parent].Rotation / 90) % 2 != 0)
                    {
                        x = rooms[_parent].X;
                        if (rooms[_parent].Rotation == 90)
                        {
                            y = rooms[_parent].Y - 1;
                        }
                        else
                        {
                            y = rooms[_parent].Y + 1;
                        }
                    }
                    else
                    {
                        if (rooms[_parent].Rotation == 0)
                        {
                            x = rooms[_parent].X - 1;
                        }
                        else
                        {
                            x = rooms[_parent].X + 1;
                        }
                        y = rooms[_parent].Y;
                    }
                    break;
            }

            return (x, y);
        }

        /// Fix the newly inserted branch by reinserting in it the mission
        /// rooms that were in the old branch while maintaining their order of
        /// appearence to guarantee the feasibility
        public void FixBranch(
            int _root,
            List<int> _missionRooms,
            ref Random _rand
        ) {
            Queue<int> newMissionRooms = new Queue<int>();
            // If both lock and keys are in the branch, give them new IDs also,
            // add all the mission rooms in the new mission rooms list
            for(int i = 0; i < _missionRooms.Count - 1; i++)
            {
                for(int j = i+1; j < _missionRooms.Count; j++)
                {
                    if(_missionRooms[i] == -_missionRooms[j])
                    {
                        int aux = Room.getNextId();
                        if (_missionRooms[i] > 0)
                        {
                            _missionRooms[i] = aux;
                        }
                        else
                        {
                            _missionRooms[i] = -aux;
                        }
                        _missionRooms[j] = -_missionRooms[i];
                    }
                }
                newMissionRooms.Enqueue(_missionRooms[i]);
            }
            // Add the last mission room, which normally wouldn't be added, but
            // only if it exists
            if(_missionRooms.Count > 0)
            {
                newMissionRooms.Enqueue(_missionRooms[_missionRooms.Count - 1]);
            }
            //
            Queue<int> toVisit = new Queue<int>();
            Queue<int> visited = new Queue<int>();
            // The actual room is the root of the branch
            toVisit.Enqueue(_root);
            // Enqueue all the rooms
            while (toVisit.Count > 0)
            {
                int current = toVisit.Dequeue();
                visited.Enqueue(current);
                // Get the current room children
                int[] children = new int[] {
                    GetChildIndexByDirection(
                        current, Util.Direction.Left
                    ),
                    GetChildIndexByDirection(
                        current, Util.Direction.Down
                    ),
                    GetChildIndexByDirection(
                        current, Util.Direction.Right
                    ),
                };
                //
                foreach (int child in children)
                {
                    if (child >= 0 &&
                        child < CAPACITY &&
                        rooms[child] != null
                    ) {
                        if (rooms[current].Equals(GetParent(child)))
                        {
                            toVisit.Enqueue(child);
                        }
                    }
                }
            }
            // Try to place all the mission rooms in the branch randomly. If
            // the number of remaining rooms is the same as the number of
            // mission rooms, every room must be a mission one, so we finish
            // this while loop.
            while(visited.Count > newMissionRooms.Count)
            {
                int current = visited.Dequeue();
                int prob = Util.RandomPercent(ref _rand);
                // If there is a mission room left, check the random number and
                // see if it will be placed in the actual room or not
                if (newMissionRooms.Count > 0)
                {
                    if (RoomFactory.PROB_NORMAL_ROOM > prob)
                    {
                        rooms[current].RoomType = RoomType.normal;
                        rooms[current].KeyToOpen = -1;
                    }
                    else
                    {
                        // If the room has a key, then the key must have an id
                        // and this id is added to the list of available keys
                        int missionId = newMissionRooms.Dequeue();
                        if (missionId > 0)
                        {
                            rooms[current].RoomType = RoomType.key;
                            rooms[current].RoomId = missionId;
                            rooms[current].KeyToOpen = missionId;
                        }
                        else
                        {
                            // Create a locked room with the id of the room
                            // that contains the key to open it
                            rooms[current].RoomType = RoomType.locked;
                            rooms[current].KeyToOpen = -missionId;
                        }
                    }
                }
                else
                {
                    rooms[current].RoomType = RoomType.normal;
                    rooms[current].KeyToOpen = -1;
                }
            }
            // If there are rooms not visited, then all the next rooms must be
            // mission ones
            while(visited.Count > 0)
            {
                int current = visited.Dequeue();
                int missionId = newMissionRooms.Dequeue();
                if (missionId > 0)
                {
                    rooms[current].RoomType = RoomType.key;
                    rooms[current].RoomId = missionId;
                    rooms[current].KeyToOpen = missionId;
                }
                else
                {
                    //Creates a locked room with the id of the room that contains the key to open it
                    rooms[current].RoomType = RoomType.locked;
                    rooms[current].KeyToOpen = -missionId;
                }
            }
        }
    }
}