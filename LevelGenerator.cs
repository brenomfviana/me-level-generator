using System;

namespace LevelGenerator
{
    /// This class holds the evolutionary level generation algorithm.
    public class LevelGenerator
    {
        /// The number of parents to be selected for crossover.
        private static readonly int CROSSOVER_PARENTS = 2;

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
            Population pop = new Population(
                SearchSpace.CoefficientOfExplorationRanges().Length,
                SearchSpace.LeniencyRanges().Length
            );

            // Generate the initial population
            while (pop.Count() < prs.population)
            {
                Individual individual = Individual.GetRandom(
                    prs.enemies, ref rand
                );
                individual.CalculateLinearCoefficient();
                individual.dungeon.Fix(prs.enemies, ref rand);
                Fitness.Calculate(prs, ref individual, ref rand);
                pop.PlaceIndividual(individual);
            }

            // Evolve the population
            for (int g = 0; g < prs.generations; g++)
            {
                // Apply the crossover operation
                Individual[] parents = Selection.Select(
                    CROSSOVER_PARENTS, prs.competitors, pop, ref rand
                );
                Individual[] offspring = Crossover.Apply(
                    parents[0], parents[1], ref rand
                );
                // Apply the mutation operation with a random chance or always
                // that the crossover was not successful
                if (offspring.Length == 0 ||
                    prs.mutation > Common.RandomPercent(ref rand)
                ) {
                    if (offspring.Length == CROSSOVER_PARENTS)
                    {
                        parents[0] = offspring[0];
                        parents[1] = offspring[1];
                    }
                    else
                    {
                        offspring = new Individual[2];
                    }
                    offspring[0] = Mutation.Apply(parents[0], ref rand);
                    offspring[1] = Mutation.Apply(parents[1], ref rand);
                }

                // Place the offspring in the MAP-Elites population
                for (int i = 0; i < offspring.Length; i++)
                {
                    offspring[i].generation = g;
                    offspring[i].dungeon.Fix(prs.enemies, ref rand);
                    offspring[i].CalculateLinearCoefficient();
                    Fitness.Calculate(prs, ref offspring[i], ref rand);
                    pop.PlaceIndividual(offspring[i]);
                }
            }

            // Get the final population (solution)
            solution = pop;
        }
    }
}