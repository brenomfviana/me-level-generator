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
    /// This particular population is mapped into level's locked doors and
    /// keys. Each Elite (or matrix cell) corresponds to a combination of
    /// different number of keys and locked doors.
    public struct Population
    {
        /// The MAP-Elites dimension. The dimension is defined by the number of
        /// keys multiplied by the number of locked doors.
        public (int keys, int locks) dimension { get; }
        /// The MAP-Elites map (a matrix of individuals).
        public Individual[,] map { get; }

        /// Population constructor.
        public Population(
            int _keys,
            int _locks
        ) {
            dimension = (_keys, _locks);
            map = new Individual[dimension.keys, dimension.locks];
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

        /// Add an individual in the MAP-Elites population.
        ///
        /// First, we identify which Elite the individual is classified in.
        /// Then, if the corresponding Elite is empty, the individual is placed
        /// there. Otherwise, we compare the both old and new individuals, and
        /// the best individual is placed in the corresponding Elite.
        public void PlaceIndividual(
            Individual _individual
        ) {
            // Calculate the individual slot (Elite)
            int k = (int) _individual.dungeon.keys;
            int l = (int) _individual.dungeon.locks;
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

        /// Return a list with the population individuals.
        public List<Individual> ToList()
        {
            List<Individual> list = new List<Individual>();
            for (int k = 0; k < dimension.keys; k++)
            {
                for (int l = 0; l < dimension.locks; l++)
                {
                    list.Add(map[k, l]);
                }
            }
            return list;
        }

        /// Print all the individuals of the MAP-Elites population.
        public void Debug()
        {
            for (int k = 0; k < dimension.keys; k++)
            {
                for (int l = 0; l < dimension.locks; l++)
                {
                    string log = "Elite ";
                    log += "" + k + "-";
                    log += "" + l;
                    Console.WriteLine(log);
                    if (map[k, l] is null)
                    {
                        Console.WriteLine(LevelDebug.INDENT + "Empty");
                    }
                    else
                    {
                        map[k, l].Debug();
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}