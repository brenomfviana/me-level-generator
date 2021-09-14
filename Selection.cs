using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace LevelGenerator
{
    /// Alias for the coordinate of MAP-Elites matrix.
    using Coordinate = System.ValueTuple<int, int>;

    /// This class holds the selector operator.
    public class Selection
    {
        /// Select individuals from the MAP-Elites population.
        ///
        /// This function ensures that the same individual will not be selected
        /// for the same selection process. To do so, we use an auxiliary list
        /// composed of the individuals' coordinates in the MAP-Elites
        /// population. Instead of selecting directly an individual, we select
        /// its coordinate from the auxiliary list and remove it then it is not
        /// available for the next selection.
        public static Individual[] Select(
            int _amount,
            int _competitors,
            Population _pop,
            ref Random _rand
        ) {
            // Get the list of Elites' coordinates (the available competitors)
            List<Coordinate> avco = _pop.GetElitesCoordinates();
            // Ensure the population size is enough for the tournament
            Debug.Assert(
                avco.Count - _amount > _competitors,
                Util.NOT_ENOUGH_COMPETITORS
            );
            // Select `_amount` individuals
            Individual[] individuals = new Individual[_amount];
            for (int i = 0; i < _amount; i++)
            {
                // Perform tournament selection with `_competitors` competitors
                (Coordinate coordinate, Individual individual) = Tournament(
                    _competitors, // Number of competitors
                    _pop,         // Population
                    avco,         // List of available competitors
                    ref _rand     // Random number generator
                );
                // Select a new individual
                individuals[i] = individual;
                // Remove selected individual from available competitors
                avco.Remove(coordinate);
            }
            // Return all selected individuals
            return individuals;
        }

        /// Perform tournament selection of a single individual.
        ///
        /// This function ensures that the same individual will not be selected
        /// for the same tournament selection process. To do so, we apply the
        /// same process explained in `Select` function.
        static (Coordinate, Individual) Tournament(
            int _competitors,
            Population _pop,
            List<Coordinate> _cs,
            ref Random _rand
        ) {
            // List of available competitors
            List<Coordinate> avco = new List<Coordinate>(_cs);
            // Initialize the list of competitors
            Individual[] competitors = new Individual[_competitors];
            // Initialize competitors' coordinates
            Coordinate[] coordinates = new Coordinate[_competitors];
            // Select competitors
            for (int i = 0; i < _competitors; i++)
            {
                // Get a random available coordinate
                (int x, int y) rc = Util.RandomElementFromList(avco, ref _rand);
                // Get the competitor corresponding to the chosen coordinate and
                // add the individual to the chosen competitors list
                competitors[i] = _pop.map[rc.x, rc.y];
                coordinates[i] = rc;
                // Remove the competitor from available competitors
                avco.Remove(rc);
            }
            // Find the tournament winner and its coordinate in the population
            Individual winner = null;
            Coordinate coordinate = (Util.UNKNOWN, Util.UNKNOWN);
            for (int i = 0; i < _competitors; i++)
            {
                if (winner is null || competitors[i].fitness > winner.fitness)
                {
                    winner = competitors[i];
                    coordinate = coordinates[i];
                }
            }
            // Return the tournament winner and its coordinate
            return (coordinate, winner);
        }
    }
}