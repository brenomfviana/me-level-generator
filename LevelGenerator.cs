using System;
using System.Collections.Generic;

namespace LevelGenerator
{
    /// This class holds the evolutionary level generation algorithm.
    public class LevelGenerator
    {
        /// The number of parents to be selected for crossover.
        private readonly static int CROSSOVER_PARENTS = 2;
        /// The number of parents to be selected for mutation.
        private readonly static int MUTATION_PARENT = 1;

        /// The evolutionary parameters.
        private Parameters prs;
        /// The found MAP-Elites population.
        private Population solution;

        /// Level Generator constructor.
        public LevelGenerator(
            Parameters _prs
        ) {
            prs = _prs;
        }

        /// Return the resulting MAP-Elites population.
        public Population GetSolution()
        {
            return solution;
        }

        /// Generate and return a set of enemies.
        public Population Evolve()
        {
            // Run evolutionary process
            Evolution();
            // Return the found individuals
            return solution;
        }

        /// Perform the level evolution process.
        private void Evolution()
        {
            // Initialize the random generator
            Random rand = new Random(prs.seed);

            // Initialize the MAP-Elites population
            Population pop = new Population(5, 5);

            // Generate the initial population
            while (pop.Count() < prs.population)
            {
                // Create a new random individual
                Dungeon individual = new Dungeon();
                individual.GenerateRooms(ref rand);
                // Calculate the individual fitness
                individual.fitness = Fitness.Calculate(individual, prs.nV, prs.nK, prs.nL, prs.lCoef, ref rand);
                // Place the individual in the MAP-Elites
                pop.PlaceIndividual(individual);
            }

            // Evolve the population
            for (int g = 0; g < prs.generations; g++)
            {
                // Initialize the offspring list
                List<Dungeon> offspring = new List<Dungeon>();

                // Apply the evolutionary operators
                if (prs.crossover > Util.RandomPercent(ref rand))
                {
                    // Select two different parents
                    Dungeon[] parents = Selection.Select(
                        CROSSOVER_PARENTS, prs.competitors, pop, ref rand
                    );
                    // Apply crossover and get the resulting children
                    Dungeon[] children = Crossover.Apply(
                        parents[0], parents[1], ref rand
                    );
                    // Add the new individuals in the offspring list
                    for (int i = 0; i < children.Length; i++)
                    {
                        // Calculate the new individual fitness
                        Fitness.Calculate(children[i], prs.nV, prs.nK, prs.nL, prs.lCoef, ref rand);
                        // Add the new individual in the offspring
                        offspring.Add(children[i]);
                    }
                }
                if (prs.mutation > Util.RandomPercent(ref rand))
                {
                    // Select and mutate a parent
                    Dungeon parent = Selection.Select(
                        MUTATION_PARENT, prs.competitors, pop, ref rand
                    )[0];
                    Dungeon individual = Mutation.Apply(parent, ref rand);
                    // Calculate the new individual fitness
                    Fitness.Calculate(individual, prs.nV, prs.nK, prs.nL, prs.lCoef, ref rand);
                    // Add the new individual in the offspring
                    offspring.Add(individual);
                }

                foreach (Dungeon individual in offspring)
                {
                    individual.FixRoomList();
                    pop.PlaceIndividual(individual);
                }

                // //Calculate the average number of children from the rooms in each children
                // parent1.CalcAvgChildren();
                // parent2.CalcAvgChildren();
            }

            // Get the final population (solution)
            solution = pop;

            // // Save CSV
            // CSVManager.SaveCSVLevel(0, aux.nKeys, aux.nLocks, aux.RoomList.Count, aux.AvgChildren, aux.neededLocks, aux.neededRooms, min, 0, Constants.RESULTSFILE+"-"+prs.nV+"-" + prs.nK + ".csv");
        }
    }
}