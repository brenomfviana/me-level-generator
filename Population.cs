using System;
using System.Collections.Generic;

namespace LevelGenerator
{
    /// Alias for the coordinate of MAP-Elites matrix.
    using Coordinate = System.ValueTuple<int, int>;

    /// This struct represents a MAP-Elites population.
    ///
    /// The MAP-Elites population is an N-dimensional array of individuals,
    /// where each matrix's ax corresponds to a different feature.
    ///
    /// This particular population is mapped into level's locks and keys. Each
    /// Elite (or matrix cell) corresponds to a combination of different number
    /// of keys and locks.
    public struct Population
    {
        /// The MAP-Elites dimension.
        public (int keys, int locks) dimension { get; }
        /// The MAP-Elites map (a matrix of individuals).
        public Dungeon[,] map { get; }

        /// Population constructor.
        public Population(
            int _keys,
            int _locks
        ) {
            dimension = (_keys, _locks);
            map = new Dungeon[dimension.keys, dimension.locks];
        }

        /// Return the number of Elites of the population.
        public int Count()
        {
            int count = 0;
            for (int k = 0; k < dimension.keys; k++)
            {
                for (int l = 0; l < dimension.locks; l++)
                {
                    if (!(map[k, l] is null))
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        /// Add an individual in the MAP-Elites population.
        ///
        /// First, we identify which Elite the individual is classified in.
        /// Then, if the corresponding Elite is empty, the individual is placed
        /// there. Otherwise, we compare the both old and new individuals, and
        /// the best individual is placed in the corresponding Elite.
        public void PlaceIndividual(
            Dungeon _individual
        ) {
            // Calculate the individual slot (Elite)
            int k = (int) _individual.nKeys;
            int l = (int) _individual.nLocks;
            // Check if the level is within the search space
            if (k >= dimension.keys || l >= dimension.locks)
            {
                return;
            }
            // If the new individual deserves to survive
            if (Fitness.IsBest(_individual, map[k, l]))
            {
                // Then, place the individual in the MAP-Elites population
                map[k, l] = _individual;
            }
        }

        /// Return a list corresponding to the Elites coordinates.
        public List<Coordinate> GetElitesCoordinates()
        {
            List<Coordinate> coordinates = new List<Coordinate>();
            for (int k = 0; k < dimension.keys; k++)
            {
                for (int l = 0; l < dimension.locks; l++)
                {
                    if (!(map[k, l] is null))
                    {
                        coordinates.Add((k, l));
                    }
                }
            }
            return coordinates;
        }

        /// Return a list with the individuals.
        public List<Dungeon> ToList()
        {
            List<Dungeon> list = new List<Dungeon>();
            for (int k = 0; k < dimension.keys; k++)
            {
                for (int l = 0; l < dimension.locks; l++)
                {
                    list.Add(map[k, l]);
                }
            }
            return list;
        }

        /// Print the individuals of the MAP-Elites population.
        // public void Debug()
        // {
        //     for (int d = 0; d < dimension.keys; d++)
        //     {
        //         for (int w = 0; w < dimension.locks; w++)
        //         {
        //             // Print the Elite's features
        //             string log = "Elite ";
        //             log += SearchSpace.AllDifficulties()[d] + "-";
        //             log += ((WeaponType) w);
        //             Console.WriteLine(log);
        //             // Print empty if the Elite is null
        //             if (map[k, l] is null)
        //             {
        //                 Console.WriteLine("  Empty");
        //             }
        //             // Print the Elite's attributes
        //             else
        //             {
        //                 map[k, l].Debug();
        //             }
        //             Console.WriteLine();
        //         }
        //     }
        // }
    }
}