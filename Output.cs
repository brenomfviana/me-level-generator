using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace LevelGenerator
{
    class Output
    {
        /// The operational system directory separator char.
        private static readonly char SEPARATOR = Path.DirectorySeparatorChar;
        /// The filename separator char.
        private static readonly char FILENAME_SEPARATOR = '-';
        /// This constant must be used to initialize empty strings or convert
        /// values of other types during concatenation.
        private static readonly string EMPTY_STR = "";
        /// Results folder name.
        /// This folder saves the collected data to evaluate the approach.
        private static readonly string DATA_FOLDER_NAME = @"results";

        // Define the room codes for printing purposes.
        public enum RoomCode
        {
            N = 0,   // Room
            C = 100, // Corridor
            B = 101, // Boss room or dungeon exit
            E = 102, // Empty space
        }

        /// Write the collected data from the evolutionary process.
        public static void WriteData(
            Individual _individual,
            Data _data
        ) {
            // Get the dungeon component
            Dungeon dungeon = _individual.dungeon;

            // Create target
            Directory.CreateDirectory(DATA_FOLDER_NAME);
            // Create the base name for the entered parameters
            string basename = GetFolderName(_data);
            // Create the result folder for the entered parameters
            string folder = DATA_FOLDER_NAME + SEPARATOR + basename;
            Directory.CreateDirectory(folder);

            // Calculate the number of files in the folder
            int count = Directory.GetFiles(folder, "*.txt").Length;
            // Build the filename
            string filename = folder + SEPARATOR + "level" + count + ".txt";

            // Initialize the grid bounds
            int minX = Util.LEVEL_GRID_OFFSET;
            int minY = Util.LEVEL_GRID_OFFSET;
            int maxX = -Util.LEVEL_GRID_OFFSET;
            int maxY = -Util.LEVEL_GRID_OFFSET;
            // The list of keys and locks in the level
            List<int> keys = new List<int>();
            List<int> locks = new List<int>();

            // Calculate the grid bounds and get the level keys and locked doors
            foreach (Room room in dungeon.RoomList)
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
                    map[i, j] = (int) RoomCode.E;
                }
            }

            // Set the corridors, keys and locked rooms
            RoomGrid grid = dungeon.roomGrid;
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
                            map[iep, jep] = (int) Output.RoomCode.N;
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
                                (int) Output.RoomCode.B :
                                (int) Output.RoomCode.N;
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
                                map[x, y] = (int) Output.RoomCode.C;
                            }
                        }
                    }
                }
            }

            using (StreamWriter writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
                writer.WriteLine(sizeX * 2);
                writer.WriteLine(sizeY * 2);
                for (int i = 0; i < sizeX * 2; ++i)
                {
                    for (int j = 0; j < sizeY * 2; ++j)
                    {
                        if (map[i, j] != (int) Output.RoomCode.E)
                        {
                            writer.WriteLine(i);
                            writer.WriteLine(j);
                            if (i + minX * 2 == 0 && j + minY * 2 == 0)
                            {
                                writer.WriteLine("s");
                            }
                            else if (map[i, j] == (int) Output.RoomCode.C)
                            {
                                writer.WriteLine("c");
                            }
                            else if (map[i, j] == (int) Output.RoomCode.B)
                            {
                                writer.WriteLine("B");
                            }
                            else
                            {
                                writer.WriteLine(map[i, j]);
                            }
                        }
                    }
                }
                writer.Flush();
                writer.Close();
            }
        }

        /// Return the folder name for the entered parameters.
        private static string GetFolderName(
            Data _data
        ) {
            Parameters prs = _data.parameters;
            string foldername = EMPTY_STR;
            foldername += EMPTY_STR + prs.generations + FILENAME_SEPARATOR;
            foldername += EMPTY_STR + prs.population + FILENAME_SEPARATOR;
            foldername += EMPTY_STR + prs.mutation + FILENAME_SEPARATOR;
            foldername += EMPTY_STR + prs.crossover + FILENAME_SEPARATOR;
            foldername += EMPTY_STR + prs.competitors;
            return foldername;
        }
    }
}
