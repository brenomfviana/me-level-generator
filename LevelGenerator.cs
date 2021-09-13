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

            //Creates the first population of dungeons and generate their rooms
            List<Dungeon> dungeons = new List<Dungeon>(Constants.POP_SIZE);
            for (int i = 0; i < dungeons.Capacity; ++i) // Generate the first population
            {
                Dungeon individual = new Dungeon();
                individual.GenerateRooms(ref rand);
                dungeons.Add(individual);
            }
            //Console.WriteLine("Created");
            double min = Double.MaxValue;
            double actual;
            Dungeon aux = dungeons[0];

            //Evolve all the generations from the GA
            for (int gen = 0; gen < Constants.GENERATIONS; ++gen)
            {

                foreach (Dungeon dun in dungeons)
                {
                    dun.fitness = GA.Fitness(dun, prs.nV, prs.nK, prs.nL, prs.lCoef, ref rand);
                }

                //Elitism
                aux = dungeons[0];
                foreach (Dungeon dun in dungeons)
                {
                    //Interface.PrintNumericalGridWithConnections(dun);
                    actual = dun.fitness;
                    if (min > actual)
                    {
                        min = actual;
                        aux = dun;
                    }
                }

                //Create the child population by doing the crossover and mutation
                List<Dungeon> childPop = new List<Dungeon>(dungeons.Count);
                for (int i = 0; i < (dungeons.Count / 2); i++)
                {
                    int parentIdx1 = 0, parentIdx2 = 1;
                    GA.Tournament(dungeons, ref parentIdx1, ref parentIdx2, ref rand);
                    //Console.WriteLine("Selected!");
                    Dungeon parent1 = dungeons[parentIdx1].Copy();
                    Dungeon parent2 = dungeons[parentIdx2].Copy();

                    //The children weren't used, so the method was changed, as the crossover happens in the parents' copies
                    try
                    {
                        GA.Crossover(ref parent1, ref parent2, ref rand);

                        //Mutation is disabled for now as it must be fixed
                        aux = dungeons[0];
                        GA.Mutation(ref parent1, ref rand);
                        GA.Mutation(ref parent2, ref rand);
                        //Console.WriteLine("Mutated");
                        //aux.FixRoomList();
                        parent1.FixRoomList();
                        parent2.FixRoomList();
                    }
                    catch (System.Exception e)
                    {
                        System.Console.WriteLine(e.Message);
                        return;
                    }
                    //Calculate the average number of children from the rooms in each children
                    parent1.CalcAvgChildren();
                    parent2.CalcAvgChildren();
                    //Console.WriteLine("Averaged");
                    //Add the children to the new population
                    childPop.Add(parent1);
                    childPop.Add(parent2);
                    //Console.WriteLine("Added");
                }

                //Elitism
                childPop[0] = aux;
                dungeons = childPop;
                //Console.WriteLine("Elit");
                //Console.WriteLine("GEN "+gen+" COMPLETED!");

            }
            //Find the best individual in the final population and print it as the answer
            min = Double.MaxValue;
            aux = dungeons[0];
            foreach (Dungeon dun in dungeons)
            {
                GA.Fitness(dun, prs.nV, prs.nK, prs.nL, prs.lCoef, ref rand);
                actual = dun.fitness;
                if (min > actual)
                {
                    min = actual;
                    aux = dun;
                }
            }
            //Console.WriteLine("Found best");
            //Console.WriteLine("AVGChildren: " + aux.AvgChildren+ " desiredKeys: "+aux.DesiredKeys);
            //Interface.PrintGridWithConnections(aux.roomGrid);
            //Console.WriteLine("Fitness: "+min);

            // Save CSV
            // CSVManager.SaveCSVLevel(id, aux.nKeys, aux.nLocks, aux.RoomList.Count, aux.AvgChildren, aux.neededLocks, aux.neededRooms, min, time, Constants.RESULTSFILE+"-"+prs.nV+"-" + prs.nK + "-" + prs.nL + "-" + prs.lCoef + ".csv");
            CSVManager.SaveCSVLevel(0, aux.nKeys, aux.nLocks, aux.RoomList.Count, aux.AvgChildren, aux.neededLocks, aux.neededRooms, min, 0, Constants.RESULTSFILE+"-"+prs.nV+"-" + prs.nK + ".csv");

            //Console.WriteLine("Saved!");
            //Console.WriteLine("AVGChildren: " + aux.AvgChildren + "Fitness: " + min);
            //Console.WriteLine("Locks: " + aux.nLocks + "Needed: " + aux.neededLocks);
            Interface.PrintNumericalGridWithConnections(aux, prs);
            //Console.WriteLine("OVER!");
            //Console.ReadLine();
            //AStar.FindRoute(aux);
            dungeons.Clear();
        }
    }
}