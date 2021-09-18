using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace LevelGenerator
{
    class Output
    {
        /// The JSON extension.
        private static readonly string JSON = ".json";
        /// The operational system directory separator char.
        private static readonly char SEPARATOR = Path.DirectorySeparatorChar;
        /// The filename separator char.
        private static readonly char FILENAME_SEPARATOR = '-';
        /// This constant defines that all files will be searched.
        private static readonly string SEARCH_FOR = "*";
        /// This constant must be used to initialize empty strings or convert
        /// values of other types during concatenation.
        private static readonly string EMPTY_STR = "";
        /// Define the JSON options.
        private static readonly JsonSerializerOptions JSON_OPTIONS =
            new JsonSerializerOptions(){ WriteIndented = true };
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
            // Create target
            Directory.CreateDirectory(DATA_FOLDER_NAME);
            // Create the base name for the entered parameters
            string basename = GetFolderName(_data);
            // Create the result folder for the entered parameters
            string folder = DATA_FOLDER_NAME + SEPARATOR + basename;
            Directory.CreateDirectory(folder);

            // Calculate the number of files in the folder
            int count = Directory.GetFiles(folder, SEARCH_FOR + JSON).Length;
            // Build the filename
            string filename = folder + SEPARATOR + "level" + count + JSON;

            // Save the entered level
            SaveLevel(_individual, filename);
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

        /// Return the number of files that are inside the entered folder and
        /// have the entered extension.
        private static int GetNumberOfFiles(
            string _folder,
            string _extension
        ) {
            return Directory.GetFiles(_folder, SEARCH_FOR + _extension).Length;
        }

        private static void SaveLevel(
            Individual _individual,
            string _filename
        ) {
            // Get the dungeon component
            Dungeon dungeon = _individual.dungeon;
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

            // Prepare the level to be written
            IndividualFile ifile = new IndividualFile();
            // Set the level dimensions
            ifile.dimensions = new Dimensions(
                2 * (maxX - minX + 1),
                2 * (maxY - minY + 1)
            );

            // Set the list of rooms
            for (int i = 0; i < sizeX * 2; ++i)
            {
                for (int j = 0; j < sizeY * 2; ++j)
                {
                    // Initialize non-existent room
                    IndividualFile.Room room = null;
                    if (map[i, j] != (int) Output.RoomCode.E)
                    {
                        // Create a new empty room
                        room = new IndividualFile.Room
                        {
                            coordinates = new Coordinates(i, j)
                        };
                        if (i + minX * 2 == 0 && j + minY * 2 == 0)
                        {
                            // Set up the starting room
                            room.type = "s";
                            room.enemies = 0;
                            room.treasures = 0;
                            room.npcs = 0;
                        }
                        else if (map[i, j] == (int) Output.RoomCode.B)
                        {
                            // Set up the boss/goal room
                            room.type = "B";
                            room.enemies = 0;
                            room.treasures = 0;
                            room.npcs = 0;
                        }
                        else if (map[i, j] == (int) Output.RoomCode.C)
                        {
                            // Set up corridor
                            room.type = "c";
                        }
                        else if (map[i, j] < 0)
                        {
                            // Set up the a locked corridor
                            room.locks = new List<int> { map[i, j] };
                        }
                        else if (map[i, j] > 0)
                        {
                            // Set up a room with a key
                            room.keys = new List<int> { map[i, j] };
                            room.enemies = 0;
                            room.treasures = 0;
                            room.npcs = 0;
                        }
                        else
                        {
                            // Set up a normal room
                            room.enemies = 0;
                            room.treasures = 0;
                            room.npcs = 0;
                        }
                    }
                    // If the room exists, then add it to the list of rooms
                    if (room != null)
                    {
                        ifile.rooms.Add(room);
                    }
                }
            }
            // Serialize and write the level file
            string json = JsonSerializer.Serialize(ifile, JSON_OPTIONS);
            File.WriteAllText(_filename, json);
        }
    }
}