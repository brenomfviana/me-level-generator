using System;
using System.Collections.Generic;

namespace LevelGenerator
{
    /// This class holds the evolutionary level generation algorithm.
    public class LevelGenerator
    {
        /// The number of parents to be selected for crossover.
        private static readonly int CROSSOVER_PARENTS = 2;
        /// The number of parents to be selected for mutation.
        private static readonly int MUTATION_PARENT = 1;

        /// The evolutionary parameters.
        private Parameters prs;
        /// The found MAP-Elites population.
        private Population solution;
        /// The evolutionary process' collected data.
        private Data data;

        /// Level Generator constructor.
        public LevelGenerator(
            Parameters _prs
        ) {
            prs = _prs;
            // Initialize the data to be collected
            data = new Data();
            data.parameters = prs;
        }

        /// Return the collected data from the evolutionary process.
        public Data GetData()
        {
            return data;
        }

        /// Return the found MAP-Elites population.
        public Population GetSolution()
        {
            return solution;
        }

        /// Generate and return a set of levels.
        public Population Evolve()
        {
            // Get starting time
            DateTime start = DateTime.Now;
            // Run evolutionary process
            Evolution();
            // Get ending time
            DateTime end = DateTime.Now;
            // Get the duration time
            data.duration = (end - start).TotalSeconds;
            // Return the found individuals
            return solution;
        }

        /// Perform the level evolution process.
        private void Evolution()
        {
            // Initialize the random generator
            Random rand = new Random(prs.seed);

            Population pop = new Population(5, 5);

            // Generate the initial population
            while (pop.Count() < prs.population)
            {
                // Create a new random individual
                Individual individual = Individual.GetRandom(ref rand);
                // Calculate the individual fitness
                individual.CalcAvgChildren();
                Fitness.Calculate(ref individual, ref rand);
                // Place the individual in the MAP-Elites
                pop.PlaceIndividual(individual);
            }

            // Evolve the population
            for (int g = 0; g < prs.generations; g++)
            {
                // Initialize the offspring list
                List<Individual> offspring = new List<Individual>();

                // Apply the evolutionary operators
                if (prs.crossover > Util.RandomPercent(ref rand))
                {
                    // Select two different parents
                    Individual[] parents = Selection.Select(
                        CROSSOVER_PARENTS, prs.competitors, pop, ref rand
                    );
                    // Apply crossover and get the resulting children
                    Individual[] children = Crossover.Apply(
                        parents[0], parents[1], ref rand
                    );
                    // Add the new individuals in the offspring list
                    for (int i = 0; i < children.Length; i++)
                    {
                        // Calculate the new individual fitness
                        children[i].CalcAvgChildren();
                        children[i].dungeon.FixNumberLockKey();
                        Fitness.Calculate(ref children[i], ref rand);
                        // Add the new individual in the offspring
                        offspring.Add(children[i]);
                    }
                }
                if (prs.mutation > Util.RandomPercent(ref rand))
                {
                    // Select and mutate a parent
                    Individual parent = Selection.Select(
                        MUTATION_PARENT, prs.competitors, pop, ref rand
                    )[0];
                    Individual individual = Mutation.Apply(parent, ref rand);
                    // Calculate the new individual fitness
                    individual.CalcAvgChildren();
                    individual.dungeon.FixNumberLockKey();
                    Fitness.Calculate(ref individual, ref rand);
                    // Add the new individual in the offspring
                    offspring.Add(individual);
                }

                foreach (Individual individual in offspring)
                {
                    individual.generation = g;
                    pop.PlaceIndividual(individual);
                }
            }

            // Get the final population (solution)
            solution = pop;
        }
    }
}