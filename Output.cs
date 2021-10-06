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
        /// The general target for the search for files.
        private static readonly string SEARCH_FOR = "*";
        /// String to initialize empty strings or convert values of other types
        /// during concatenation.
        private static readonly string EMPTY_STR = "";
        /// The default JSON options.
        private static readonly JsonSerializerOptions JSON_OPTIONS =
            new JsonSerializerOptions(){ WriteIndented = true };
        /// The results folder name.
        /// This folder stores the collected data to evaluate the approach.
        private static readonly string RESULTS_FOLDER_NAME = @"results";
        /// The filename of the evolutionary process data.
        /// This file is created only when the enemies are saved separately.
        private static readonly string DATA_FILENAME = @"data";

        /// Write the collected data from the evolutionary process.
        public static void WriteData(
            Population _solution,
            Data _data
        ) {
            // Create folder to store the results
            Directory.CreateDirectory(RESULTS_FOLDER_NAME);
            // Create the base name according to the entered parameters
            string basename = RESULTS_FOLDER_NAME + SEPARATOR;
            basename += GetFolderName(_data);
            Directory.CreateDirectory(basename);
            // Calculate the number of directories in the folder
            int count = Directory.GetDirectories(
                basename, SEARCH_FOR, SearchOption.TopDirectoryOnly
            ).Length;
            // Create a folder for the resulting set of dungeon levels
            basename = basename + SEPARATOR + EMPTY_STR + count;
            Directory.CreateDirectory(basename);
            // Save the evolutionary process data
            string datafn = basename + SEPARATOR + DATA_FILENAME + JSON;
            string json = JsonSerializer.Serialize(_data, JSON_OPTIONS);
            File.WriteAllText(datafn, json);
            // Save each individual in the create folder
            for (int e = 0; e < _solution.dimension.exp; e++)
            {
                for (int l = 0; l < _solution.dimension.len; l++)
                {
                    Individual individual = _solution.map[e, l];
                    if (individual != null)
                    {
                        SaveLevel(individual, basename, (e, l));
                    }
                }
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
            foldername += EMPTY_STR + prs.competitors;
            return foldername;
        }

        private static void SaveLevel(
            Individual _individual,
            string _basename,
            (int x, int y) _coordinate
        ) {
            // Get the dungeon component
            Dungeon dungeon = _individual.dungeon;
            // Initialize the grid bounds
            int minX = RoomGrid.LEVEL_GRID_OFFSET;
            int minY = RoomGrid.LEVEL_GRID_OFFSET;
            int maxX = -RoomGrid.LEVEL_GRID_OFFSET;
            int maxY = -RoomGrid.LEVEL_GRID_OFFSET;
            // The list of keys and locks in the level
            List<int> keys = new List<int>();
            List<int> locks = new List<int>();

            // Calculate the grid bounds and get the level keys and locked doors
            foreach (Room room in dungeon.Rooms)
            {
                // Update grid bounds
                minX = minX > room.x ? room.x : minX;
                minY = minY > room.y ? room.y : minY;
                maxX = room.x > maxX ? room.x : maxX;
                maxY = room.y > maxY ? room.y : maxY;
                // Add the keys and locked doors in the level
                if (room.type == RoomType.Key) {
                    keys.Add(room.key);
                }
                if (room.type == RoomType.Locked)
                {
                    locks.Add(room.key);
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
                    map[i, j] = (int) Common.RoomCode.E;
                }
            }

            // Set the corridors, keys and locked rooms
            RoomGrid grid = dungeon.grid;
            for (int i = minX; i < maxX + 1; i++)
            {
                for (int j = minY; j < maxY + 1; j++)
                {
                    // Get the even positions
                    int iep = (i - minX) * 2;
                    int jep = (j - minY) * 2;
                    // Get the respective room
                    Room current = grid[i, j];
                    if (current != null)
                    {
                        if (current.type == RoomType.Normal)
                        {
                            map[iep, jep] = (int) Common.RoomCode.N;
                        }
                        else if (current.type == RoomType.Key)
                        {
                            int _key = keys.IndexOf(current.key);
                            map[iep, jep] = _key + 1;
                        }
                        else if (current.type == RoomType.Locked)
                        {
                            int _lock = locks.IndexOf(current.key);
                            map[iep, jep] = _lock == locks.Count - 1 ?
                                (int) Common.RoomCode.B :
                                (int) Common.RoomCode.N;
                        }
                        // Get current room parent
                        Room parent = current.parent;
                        if (parent != null)
                        {
                            // Get the corridor between both rooms
                            int x = parent.x - current.x + iep;
                            int y = parent.y - current.y + jep;
                            // If the current room is locked
                            if (current.type == RoomType.Locked)
                            {
                                // Then, the corridor is locked
                                int _key = keys.IndexOf(current.key);
                                map[x, y] = -(_key + 1);
                            }
                            else
                            {
                                // Otherwise it is an usual corridor
                                map[x, y] = (int) Common.RoomCode.C;
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
            ifile.generation = _individual.generation;
            ifile.fitness = _individual.fitness;
            ifile.neededLocks = _individual.neededLocks;
            ifile.neededRooms = _individual.neededRooms;
            ifile.linearCoefficient = _individual.linearCoefficient;
            ifile.exploration = _individual.exploration;
            ifile.leniency = _individual.leniency;

            // Set the list of rooms
            for (int i = 0; i < sizeX * 2; i++)
            {
                for (int j = 0; j < sizeY * 2; j++)
                {
                    IndividualFile.Room room = null;
                    int x = i / 2 + minX;
                    int y = j / 2 + minY;
                    if (map[i, j] != (int) Common.RoomCode.E)
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
                            room.enemies = grid[x, y].enemies;
                        }
                        else if (map[i, j] == (int) Common.RoomCode.B)
                        {
                            // Set up the boss/goal room
                            room.type = "B";
                            room.enemies = grid[x, y].enemies;
                        }
                        else if (map[i, j] == (int) Common.RoomCode.C)
                        {
                            // Set up corridor
                            room.type = "c";
                        }
                        else if (map[i, j] < 0)
                        {
                            // Set up the a locked corridor
                            room.type = "c";
                            room.locks = new List<int> { map[i, j] };
                        }
                        else if (map[i, j] > 0)
                        {
                            // Set up a room with a key
                            room.keys = new List<int> { map[i, j] };
                            room.enemies = grid[x, y].enemies;
                        }
                        else
                        {
                            // Set up a normal room
                            if (grid[x, y] != null)
                            {
                                room.enemies = grid[x, y].enemies;
                            }
                        }
                    }
                    // If the room exists, then add it to the list of rooms
                    if (room != null)
                    {
                        ifile.rooms.Add(room);
                    }
                }
            }

            // Build the filename
            string filename = _basename + SEPARATOR +
                "level" + FILENAME_SEPARATOR +
                _coordinate.x + FILENAME_SEPARATOR +
                _coordinate.y + JSON;
            // Serialize and write the level file
            string json = JsonSerializer.Serialize(ifile, JSON_OPTIONS);
            File.WriteAllText(filename, json);
        }
    }
}