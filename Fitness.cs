using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace LevelGenerator
{
    /// This class holds the fitness operator.
    public class Fitness
    {
        //Fitness is based in the number of rooms, number of keys and locks, the linear coefficient and the number of locks used by the A*
        public static float Calculate(
            Dungeon ind,
            int nV,
            int nK,
            int nL,
            float lCoef,
            ref Random rand
        ) {
            float avgUsedRoom = 0.0f;
            //Only use the A* if there is a lock in the dungeon
            if (ind.nLocks > 0)
            {
                //System.Console.WriteLine("Begin A*");
                ind.neededLocks = AStar.FindRoute(ind);
                for (int i = 0; i < 3; ++i)
                {
                    DFS dfs = new DFS(ind);
                    dfs.FindRoute(ind, ref rand);
                    avgUsedRoom += dfs.NVisitedRooms;
                }
                ind.neededRooms = avgUsedRoom / 3.0f;
                //System.Console.WriteLine("Needed Rooms: " + ind.neededRooms);
                if (ind.neededRooms > ind.RoomList.Count)
                {
                    System.Console.WriteLine("SOMETHING IS REALLY WRONG! Nrooms: " + ind.RoomList.Count + "  Used: " + ind.neededRooms);
                    System.Console.ReadKey();
                }
                if (ind.neededLocks > ind.nLocks)
                    System.Console.WriteLine("SOMETHING IS REALLY WRONG!");
                return (2*(System.Math.Abs(nV - ind.RoomList.Count) + System.Math.Abs(nK - ind.nKeys) + System.Math.Abs(nL - ind.nLocks) + System.Math.Abs(lCoef - ind.AvgChildren)) + (ind.nLocks * 0.8f - ind.neededLocks) + (ind.RoomList.Count - ind.neededRooms));
            }
            else
                return (2*(System.Math.Abs(nV - ind.RoomList.Count) + System.Math.Abs(nK - ind.nKeys) + System.Math.Abs(nL - ind.nLocks) + System.Math.Abs(lCoef - ind.AvgChildren)));
            
            //return System.Math.Abs(nV - ind.RoomList.Count) + System.Math.Abs(nK - ind.nKeys) + System.Math.Abs(nL - ind.nLocks) + System.Math.Abs(lCoef - ind.AvgChildren);
        }
    }
}