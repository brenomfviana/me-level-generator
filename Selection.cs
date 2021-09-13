using System;
using System.Collections.Generic;

namespace LevelGenerator
{
    /// This class holds the selector operator.
    public class Selection
    {
        /// Select individuals from the population.
        public static Dungeon[] Select(
            int _amount,
            int _competitors,
            List<Dungeon> _pop,
            ref Random _rand
        ) {
            // Select `_amount` individuals
            Dungeon[] individuals = new Dungeon[_amount];
            //
            HashSet<int> posHash = new HashSet<int>();
            List<int> parentPosL = new List<int>();
            do
            {
                int pos = _rand.Next(_pop.Count);
                if (posHash.Add(pos))
                {
                    parentPosL.Add(pos);
                }
            } while (posHash.Count != _competitors);
            //
            int parent1 = _pop[parentPosL[0]].fitness < _pop[parentPosL[1]].fitness ? parentPosL[0] : parentPosL[1];
            int parent2;
            if (_pop[parentPosL[2]].fitness < _pop[parentPosL[3]].fitness)
            {
                parent2 = parentPosL[2];
            }
            else
            {
                parent2 = parentPosL[3];
            }
            //
            if (_amount == 2)
            {
                individuals[0] = _pop[parent1];
                individuals[1] = _pop[parent2];
            }
            else
            {
                int parent = _pop[parent1].fitness < _pop[parent2].fitness ? parent1 : parent2;
                individuals[0] = _pop[parent];
            }

            // Return all selected individuals
            return individuals;
        }
    }
}