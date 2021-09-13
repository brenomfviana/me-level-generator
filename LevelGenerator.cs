using System;
using System.Collections.Generic;

namespace LevelGenerator
{
    /// This class holds the evolutionary level generation algorithm.
    public class LevelGenerator
    {
        /// The number of parents to be selected for crossover.
        private static int CROSSOVER_PARENTS = 2;
        /// The number of parents to be selected for mutation.
        private static int MUTATION_PARENT = 1;

        /// The evolutionary parameters.
        private Parameters prs;

        /// Level Generator constructor.
        public LevelGenerator(
            Parameters _prs
        ) {
            prs = _prs;
        }

        /// Generate and return a set of enemies.
        public void Evolve()
        {
            // Run evolutionary process
            Evolution();
        }

        /// Perform the level evolution process.
        private void Evolution()
        {
            // Initialize the random generator
            Random rand = new Random(prs.seed);

            // Initialize the population
            List<Dungeon> pop = new List<Dungeon>();

            // Generate the initial population
            for (int i = 0; i < prs.population; ++i)
            {
                Dungeon individual = new Dungeon();
                individual.GenerateRooms(ref rand);
                pop.Add(individual);
            }
            double min = Double.MaxValue;
            double actual;
            Dungeon aux = pop[0];

            // Evolve the population
            for (int g = 0; g < prs.generations; g++)
            {
                foreach (Dungeon dun in pop)
                {
                    dun.fitness = Fitness.Calculate(dun, prs.nV, prs.nK, prs.nL, prs.lCoef, ref rand);
                }

                //Elitism
                aux = pop[0];
                foreach (Dungeon dun in pop)
                {
                    //Interface.PrintNumericalGridWithConnections(dun);
                    actual = dun.fitness;
                    if (min > actual)
                    {
                        min = actual;
                        aux = dun;
                    }
                }

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
                    pop.Add(individual);
                }

                // //Calculate the average number of children from the rooms in each children
                // parent1.CalcAvgChildren();
                // parent2.CalcAvgChildren();
            }
            // Find the best individual in the final population and print it as the answer
            min = Double.MaxValue;
            aux = pop[0];
            foreach (Dungeon dun in pop)
            {
                Fitness.Calculate(dun, prs.nV, prs.nK, prs.nL, prs.lCoef, ref rand);
                actual = dun.fitness;
                if (min > actual)
                {
                    min = actual;
                    aux = dun;
                }
            }

            // // Save CSV
            // CSVManager.SaveCSVLevel(0, aux.nKeys, aux.nLocks, aux.RoomList.Count, aux.AvgChildren, aux.neededLocks, aux.neededRooms, min, 0, Constants.RESULTSFILE+"-"+prs.nV+"-" + prs.nK + ".csv");

            //Console.WriteLine("Saved!");
            //Console.WriteLine("AVGChildren: " + aux.AvgChildren + "Fitness: " + min);
            //Console.WriteLine("Locks: " + aux.nLocks + "Needed: " + aux.neededLocks);
            Interface.PrintNumericalGridWithConnections(aux, prs);
        }
    }
}