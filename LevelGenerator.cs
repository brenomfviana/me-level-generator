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
            DateTime start = DateTime.Now;
            Evolution();
            DateTime end = DateTime.Now;
            data.duration = (end - start).TotalSeconds;
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
                Individual individual = Individual.GetRandom(ref rand);
                individual.CalculateLinearCoefficient();
                Fitness.Calculate(ref individual, ref rand);
                pop.PlaceIndividual(individual);
            }

            // Evolve the population
            for (int g = 0; g < prs.generations; g++)
            {
                // Initialize the offspring list
                List<Individual> offspring = new List<Individual>();

                // Apply the evolutionary operators
                if (prs.crossover > Common.RandomPercent(ref rand))
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
                        children[i].dungeon.Fix();
                        children[i].CalculateLinearCoefficient();
                        Fitness.Calculate(ref children[i], ref rand);
                        offspring.Add(children[i]);
                    }
                }
                if (prs.mutation > Common.RandomPercent(ref rand))
                {
                    // Select a parent
                    Individual parent = Selection.Select(
                        MUTATION_PARENT, prs.competitors, pop, ref rand
                    )[0];
                    // Apply mutation
                    Individual individual = Mutation.Apply(parent, ref rand);
                    // Add the new individual in the offspring list
                    individual.dungeon.Fix();
                    individual.CalculateLinearCoefficient();
                    Fitness.Calculate(ref individual, ref rand);
                    offspring.Add(individual);
                }

                // Place the offspring in the MAP-Elites population
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