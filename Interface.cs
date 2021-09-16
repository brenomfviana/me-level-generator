using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace LevelGenerator
{
    class Interface
    {
        /// Write the collected data from the evolutionary process.
        public static void WriteData(
            Dungeon _dungeon,
            Parameters _prs
        ) {
            // Create target
            string folder = @"results";
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            folder += "/" + _prs.generations + "-" + _prs.population;
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            // Calculate the number of files in the folder
            int count = Directory.GetFiles(folder, "*.txt").Length;

            Room actualRoom, parent;
            RoomGrid grid = _dungeon.roomGrid;
            RoomType type;
            int x, y, iPositive, jPositive;
            string filename = folder + "/level" + count + ".txt";
            string filenameRG = "dataRoomGenerator.txt";
            bool isRoom;

            List<int> lockedRooms = new List<int>();
            List<int> keys = new List<int>();
            int minX, minY, maxX, maxY;
            minX = Constants.MATRIXOFFSET;
            minY = Constants.MATRIXOFFSET;
            maxX = -Constants.MATRIXOFFSET;
            maxY = -Constants.MATRIXOFFSET;
            foreach (Room room in _dungeon.RoomList)
            {
                if (room.RoomType == RoomType.key)
                    keys.Add(room.KeyToOpen);
                else if (room.RoomType == RoomType.locked)
                    lockedRooms.Add(room.KeyToOpen);
                if (room.X < minX)
                    minX = room.X;
                if (room.Y < minY)
                    minY = room.Y;
                if (room.X > maxX)
                    maxX = room.X;
                if (room.Y > maxY)
                    maxY = room.Y;
            }

            // IEnumerable<Dictionary> orderedKeys;
            // System.Console.Write("XMin: " + minX + " Xmax: "+maxX+ " YMin: "+minY+" YMax: "+maxY+"\n");
            int sizeX = maxX - minX +1;
            int sizeY = maxY - minY +1;
            int[,] map = new int[2*sizeX, 2*sizeY];

            for (int i = 0; i < 2*sizeX; ++i)
            {
                for (int j = 0; j < 2*sizeY; ++j)
                {
                    map[i, j] = 101;
                }
            }

            /*foreach (KeyValuePair<int, int> keys in orderKeys)
            {
                //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                Console.WriteLine(string.Format("Key = {0}, Value = {1}", keys.Key, keys.Value));
            }*/

            for (int i = minX; i < maxX+1; ++i)
            {
                for (int j = minY; j < maxY+1; ++j)
                {
                    iPositive = i - minX;
                    jPositive = j - minY;
                    actualRoom = grid[i, j];
                    if (actualRoom != null)
                    {
                        type = actualRoom.RoomType;
                        if (type == RoomType.normal)
                        {
                            map[iPositive * 2, jPositive * 2] = 0;
                        }
                        else if (type == RoomType.key)
                        {
                            map[iPositive * 2, jPositive * 2] = keys.IndexOf(actualRoom.KeyToOpen) + 1;
                        }
                        else if (type == RoomType.locked)
                        {
                            if (lockedRooms.IndexOf(actualRoom.KeyToOpen) == lockedRooms.Count - 1)
                                map[iPositive * 2, jPositive * 2] = 102;
                            else
                                map[iPositive * 2, jPositive * 2] = 0;
                        }
                        else
                        {
                            Console.WriteLine("Something went wrong printing the tree!\n");
                            Console.WriteLine("This Room type does not exist!\n\n");
                        }
                        parent = actualRoom.Parent;
                        if (parent != null)
                        {

                            x = parent.X - actualRoom.X + 2 * iPositive;
                            y = parent.Y - actualRoom.Y + 2 * jPositive;
                            if (type == RoomType.locked)
                                map[x, y] = -(keys.IndexOf(actualRoom.KeyToOpen) + 1);
                            else
                                map[x, y] = 100;
                        }
                    }
                    else
                    {
                        // map[iPositive * 2, jPositive * 2] = 'O';
                    }
                }
            }
            using (StreamWriter writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
                using (StreamWriter writerRG = new StreamWriter(filenameRG, false, Encoding.UTF8))
                {
                    writer.WriteLine(sizeX * 2);
                    writer.WriteLine(sizeY * 2);
                    writerRG.WriteLine(sizeX * 2);
                    writerRG.WriteLine(sizeY * 2);
                    for (int i = 0; i < sizeX * 2; ++i)
                    {
                        for (int j = 0; j < sizeY * 2; ++j)
                        {
                            isRoom = false;
                            if (map[i, j] == 0)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkCyan;
                            }
                            else if (map[i, j] == 100)
                            {
                                Console.ForegroundColor = ConsoleColor.Magenta;
                            }
                            else if (map[i, j] == 7)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                            }
                            else if (map[i, j] == 101)
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            else if (map[i, j] == 102)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                            }
                            else if (map[i, j] > 0)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                            }
                            else if (map[i, j] < 0)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                            }


                            if (map[i, j] == 101)
                            {
                                Console.Write("  ");
                                // writer.WriteLine(" ");
                            }
                            else
                            {
                                writer.WriteLine(i);
                                writer.WriteLine(j);
                                writerRG.WriteLine(i);
                                writerRG.WriteLine(j);
                                if (i + minX * 2 == 0 && j + minY * 2 == 0)
                                {
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.Write(" s");
                                    writer.WriteLine("s");
                                    writerRG.WriteLine("s");
                                    isRoom = true;
                                }
                                else if (map[i, j] == 100)
                                {
                                    Console.Write(" c");
                                    writer.WriteLine("c");
                                    writerRG.WriteLine("c");
                                }
                                else if (map[i, j] == 102)
                                {
                                    Console.Write(" B");
                                    writer.WriteLine("B");
                                    writerRG.WriteLine("B");
                                    isRoom = true;
                                }
                                else if (map[i, j] < 0)
                                {
                                    Console.Write("{0,2}", map[i, j]);
                                    writer.WriteLine(map[i, j]);
                                    writerRG.WriteLine(map[i, j]);
                                }
                                else if (map[i, j] > 0)
                                {
                                    Console.Write("{0,2}", map[i, j]);
                                    writer.WriteLine(map[i, j]);
                                    writerRG.WriteLine(map[i, j]);
                                    isRoom = true;
                                }
                                else
                                {
                                    Console.Write("{0,2}", map[i, j]);
                                    writer.WriteLine(map[i, j]);
                                    writerRG.WriteLine(map[i, j]);
                                    isRoom = true;
                                }

                                if (isRoom)
                                {
                                    if (j > 0)
                                    {
                                        if (map[i, j - 1] < 0 || map[i, j - 1] == 100)
                                            writerRG.WriteLine(1);
                                        else
                                            writerRG.WriteLine(0);
                                    }
                                    else
                                        writerRG.WriteLine(0);
                                    if (i < sizeX * 2 - 1)
                                    {
                                        if (map[i + 1, j] < 0 || map[i + 1, j] == 100)
                                            writerRG.WriteLine(1);
                                        else
                                            writerRG.WriteLine(0);
                                    }
                                    else
                                        writerRG.WriteLine(0);
                                    if (j < sizeY * 2 - 1)
                                    {
                                        if (map[i, j + 1] < 0 || map[i, j + 1] == 100)
                                            writerRG.WriteLine(1);
                                        else
                                            writerRG.WriteLine(0);
                                    }
                                    else
                                        writerRG.WriteLine(0);
                                    if (i > 0)
                                    {
                                        if (map[i - 1, j] < 0 || map[i - 1, j] == 100)
                                            writerRG.WriteLine(1);
                                        else
                                            writerRG.WriteLine(0);
                                    }
                                    else
                                        writerRG.WriteLine(0);
                                }
                            }
                        }
                        Console.Write("\n");
                        // writer.Write("\r\n");
                    }
                    writer.Flush();
                    writer.Close();
                    writerRG.Flush();
                    writerRG.Close();
                    Console.Write("\n");
                }
            }
        }
    }
}
