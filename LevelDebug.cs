using System;
using System.Collections.Generic;

namespace LevelGenerator
{
    /// This class holds level debug-purpose functions.
    class LevelDebug
    {
        /// Indent of debugging functions and methods.
        public static readonly string INDENT = "  ";

        /// Print the tree representation of the entered level.
        public static void PrintTree(
            Dungeon _dungeon,
            string _indent
        ) {
            // Get tree root
            Room root = _dungeon.RoomList[0];
            // This list holds lists of nodes children
            List<List<Room>> stacks = new List<List<Room>>();
            // Add the root in the list of stacks
            stacks.Add(new List<Room>() { root });
            // Print the tree
            while (stacks.Count > 0)
            {
                // Calculate the last added stack index
                int last = stacks.Count - 1;
                // Get the last added stack
                List<Room> current = stacks[last];
                // Remove empty stacks
                if (current.Count == 0)
                {
                    stacks.RemoveAt(last);
                    continue;
                }
                // Get the first node and remove if from the list of nodes
                Room node = current[0];
                current.RemoveAt(0);
                string indent = _indent;
                // Print the nodes in the current stack
                for (int i = 0; i < last; i++)
                {
                    indent += (stacks[i].Count > 0) ? "|  " : "   ";
                }
                // Tag the nodes with the respective room type
                string tag = "";
                tag += node.RoomType == RoomType.normal ? "N" : "";
                tag += node.RoomType == RoomType.key ? "K" : "";
                tag += node.RoomType == RoomType.locked ? "L" : "";
                Console.WriteLine(indent + "+- " + node.RoomId + "-" + tag);
                // Get non-null children nodes
                List<Room> next = new List<Room>();
                Room[] children = new Room[] {
                    node.LeftChild,
                    node.BottomChild,
                    node.RightChild
                };
                foreach (Room child in children)
                {
                    if (child != null)
                    {
                        next.Add(child);
                    }
                }
                // Keep printing the branch while there are nodes
                if (next.Count > 0)
                {
                    stacks.Add(next);
                }
            }
        }

        public static void PrintMap(
            Dungeon _dungeon,
            string _indent
        ) {
            // Initialize the grid bounds
            int minX = Util.LEVEL_GRID_OFFSET;
            int minY = Util.LEVEL_GRID_OFFSET;
            int maxX = -Util.LEVEL_GRID_OFFSET;
            int maxY = -Util.LEVEL_GRID_OFFSET;
            // The list of keys and locks in the level
            List<int> keys = new List<int>();
            List<int> locks = new List<int>();

            // Calculate the grid bounds and get the level keys and locked doors
            foreach (Room room in _dungeon.RoomList)
            {
                // Update grid bounds
                minX = minX > room.X ? room.X : minX;
                minY = minY > room.Y ? room.Y : minY;
                maxX = room.X > maxX ? room.X : maxX;
                maxY = room.Y > maxY ? room.Y : maxY;
                // Add the keys and locked doors in the level
                if (room.RoomType == RoomType.key) {
                    keys.Add(room.KeyToOpen);
                }
                if (room.RoomType == RoomType.locked)
                {
                    locks.Add(room.KeyToOpen);
                }
            }

            // Initialize the auxiliary map
            int sizeX = maxX - minX + 1;
            int sizeY = maxY - minY + 1;
            int[,] map = new int[2 * sizeX, 2 * sizeY];
            for (int i = 0; i < 2 * sizeX; i++)
            {
                for (int j = 0; j < 2 * sizeY; j++)
                {
                    map[i, j] = (int) Util.RoomCode.E;
                }
            }

            // Set the corridors, keys and locked rooms
            RoomGrid grid = _dungeon.roomGrid;
            for (int i = minX; i < maxX + 1; ++i)
            {
                for (int j = minY; j < maxY + 1; ++j)
                {
                    // Get the even positions
                    int iep = (i - minX) * 2;
                    int jep = (j - minY) * 2;
                    // Get the respective room
                    Room current = grid[i, j];
                    if (current != null)
                    {
                        if (current.RoomType == RoomType.normal)
                        {
                            map[iep, jep] = (int) Util.RoomCode.N;
                        }
                        else if (current.RoomType == RoomType.key)
                        {
                            int _key = keys.IndexOf(current.KeyToOpen);
                            map[iep, jep] = _key + 1;
                        }
                        else if (current.RoomType == RoomType.locked)
                        {
                            int _lock = locks.IndexOf(current.KeyToOpen);
                            map[iep, jep] = _lock == locks.Count - 1 ?
                                (int) Util.RoomCode.B :
                                (int) Util.RoomCode.N;
                        }
                        // Get current room parent
                        Room parent = current.Parent;
                        if (parent != null)
                        {
                            // Get the corridor between both rooms
                            int x = parent.X - current.X + iep;
                            int y = parent.Y - current.Y + jep;
                            // If the current room is locked
                            if (current.RoomType == RoomType.locked)
                            {
                                // Then, the corridor is locked
                                int _key = keys.IndexOf(current.KeyToOpen);
                                map[x, y] = -(_key + 1);
                            }
                            else
                            {
                                // Otherwise it is an usual corridor
                                map[x, y] = (int) Util.RoomCode.C;
                            }
                        }
                    }
                }
            }

            // Print the dungeon in the console
            for (int i = 0; i < sizeX * 2; i++)
            {
                Console.Write(_indent);
                for (int j = 0; j < sizeY * 2; j++)
                {
                    // Set the room color
                    SetRoomColor(map[i, j]);
                    // Check room cores and print the corresponding string code
                    if (map[i, j] == (int) Util.RoomCode.E)
                    {
                        Console.Write("  ");
                    }
                    else
                    {
                        if (i + minX * 2 == 0 && j + minY * 2 == 0)
                        {
                            Console.Write(" s");
                        }
                        else if (map[i, j] == (int) Util.RoomCode.C)
                        {
                            Console.Write(" c");
                        }
                        else if (map[i, j] == (int) Util.RoomCode.B)
                        {
                            Console.Write(" B");
                        }
                        else if (map[i, j] < 0 || map[i, j] > 0)
                        {
                            Console.Write("{0,2}", map[i, j]);
                        }
                        else
                        {
                            Console.Write(" _", map[i, j]);
                        }
                    }
                }
                Console.WriteLine();
            }
        }

        // Define the room color that will be printed on the console.
        private static void SetRoomColor(
            int _code
        ) {
            // If the room is a room
            if (_code == (int) Util.RoomCode.N)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
            }
            // If the room is a boss room
            else if (_code == (int) Util.RoomCode.B)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            // If the room is a corridor
            else if (_code == (int) Util.RoomCode.C)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
            }
            // If there is no room
            else if (_code == (int) Util.RoomCode.E)
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            // If the room has a key
            else if (_code > 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            }
            // If the room is locked
            else if (_code < 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
            }
        }
    }
}