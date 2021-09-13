using System;
using System.Collections.Generic;

namespace LevelGenerator
{
    /// This class holds the selector operator.
    public class Selection
    {
        public static void Tournament(
            List<Dungeon> pop,
            ref int parent1,
            ref int parent2,
            ref Random rand
        ) {
            HashSet<int> posHash = new HashSet<int>();
            List<int> parentPosL = new List<int>();
            do
            {
                int pos = rand.Next(pop.Count);
                if (posHash.Add(pos))
                {
                    parentPosL.Add(pos);
                }
            } while (posHash.Count != 4);
            parent1 = pop[parentPosL[0]].fitness < pop[parentPosL[1]].fitness ? parentPosL[0] : parentPosL[1];
            parent2 = pop[parentPosL[2]].fitness < pop[parentPosL[3]].fitness ? parentPosL[2] : parentPosL[3];
        }
    }
}