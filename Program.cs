using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LevelGenerator
{
    internal class LevelGenerator
    {
        public static void Main(string[] args)
        {
            // Get arguments - example: dotnet run 20 4 1.5 -warn 0
            int nV = int.Parse(args[0]);
            int nM = int.Parse(args[1]);
            float lCoef = float.Parse(args[2]);
            // Build parameters
            Parameters prs = new Parameters(nV, nM, nM, lCoef);

            //Creates the first population of dungeons and generate their rooms
            List<Dungeon> dungeons;
            System.Diagnostics.Stopwatch watch;
            for (int id = 0; id < 1; ++id) // ### 1000 rodadas
            {
                //Console.WriteLine("New round!");
                watch = System.Diagnostics.Stopwatch.StartNew();

                dungeons = new List<Dungeon>(Constants.POP_SIZE);
                for (int i = 0; i < dungeons.Capacity; ++i) // Generate the first population
                {
                    Dungeon individual = new Dungeon();
                    individual.GenerateRooms();
                    dungeons.Add(individual);
                }
                //Console.WriteLine("Created");
                double min = Double.MaxValue;
                double actual;
                Dungeon aux = dungeons[0];

                //Evolve all the generations from the GA
                for (int gen = 0; gen < Constants.GENERATIONS; ++gen)
                {

                    //Interface.PrintNumericalGridWithConnections(dungeons[0]);
                    //Interface.PrintTree(dungeons[0].RoomList[0]);
                    //Interface.PrintGrid(dungeons[0].roomGrid);
                    //Console.WriteLine("NEW GENERATION: "+gen);
                    /*Console.WriteLine("Will Print");
                    for(int i = 0; i < dungeons.Count; ++i)
                    {
                        Interface.PrintNumericalGridWithConnections(dungeons[i]);
                    }
                    Console.WriteLine("Printed");*/

                    foreach (Dungeon dun in dungeons)
                    {
                        //Interface.PrintNumericalGridWithConnections(dun);
                        dun.fitness = GA.Fitness(dun, prs.nV, prs.nK, prs.nL, prs.lCoef);
                        //Console.ReadKey();
                        //Console.Clear();
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
                    Parallel.For(0, (dungeons.Count / 2), (i) =>
                    {
                        int parentIdx1 = 0, parentIdx2 = 1;
                        GA.Tournament(dungeons, ref parentIdx1, ref parentIdx2);
                        //Console.WriteLine("Selected!");
                        Dungeon parent1 = dungeons[parentIdx1].Copy();
                        Dungeon parent2 = dungeons[parentIdx2].Copy();

                        //GA.Crossover(ref parent1, ref parent2, ref child1, ref child2);
                        //The children weren't used, so the method was changed, as the crossover happens in the parents' copies
                        try
                        {
                            //Console.WriteLine("Will Cross");
                            //Interface.PrintNumericalGridWithConnections(parent1);
                            //Interface.PrintNumericalGridWithConnections(parent2);

                            GA.Crossover(ref parent1, ref parent2);
                            //Console.WriteLine("Crossed!");
                            //Console.WriteLine("Crossed");
                            //Interface.PrintNumericalGridWithConnections(parent1);
                            //Interface.PrintNumericalGridWithConnections(parent2);

                            //Mutation is disabled for now as it must be fixed
                            aux = dungeons[0];
                            GA.Mutation(ref parent1);
                            GA.Mutation(ref parent2);
                            //Console.WriteLine("Mutated");
                            //aux.FixRoomList();
                            parent1.FixRoomList();
                            parent2.FixRoomList();
                        }
                        catch (System.Exception e)
                        {
                            System.Console.WriteLine(e.Message);
                            Util.OpenUri("https://stackoverflow.com/search?q=" + e.Message);
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
                    });

                    //Elitism
                    childPop[0] = aux;
                    dungeons = childPop;
                    //Console.WriteLine("Elit");
                    //Console.WriteLine("GEN "+gen+" COMPLETED!");

                }
                /*
                for (int i = 0; i < dungeons.Count; ++i)
                {
                    Interface.PrintNumericalGridWithConnections(dungeons[i]);
                }*/
                //Find the best individual in the final population and print it as the answer
                min = Double.MaxValue;
                aux = dungeons[0];
                foreach (Dungeon dun in dungeons)
                {
                    GA.Fitness(dun, prs.nV, prs.nK, prs.nL, prs.lCoef);
                    actual = dun.fitness;
                    if (min > actual)
                    {
                        min = actual;
                        aux = dun;
                    }
                }
                //Console.WriteLine("Found best");
                watch.Stop();
                long time = watch.ElapsedMilliseconds;
                //Console.WriteLine("AVGChildren: " + aux.AvgChildren+ " desiredKeys: "+aux.DesiredKeys);
                //Interface.PrintGridWithConnections(aux.roomGrid);
                //Console.WriteLine("Fitness: "+min);

                // Save CSV
                // CSVManager.SaveCSVLevel(id, aux.nKeys, aux.nLocks, aux.RoomList.Count, aux.AvgChildren, aux.neededLocks, aux.neededRooms, min, time, Constants.RESULTSFILE+"-"+prs.nV+"-" + prs.nK + "-" + prs.nL + "-" + prs.lCoef + ".csv");
                CSVManager.SaveCSVLevel(id, aux.nKeys, aux.nLocks, aux.RoomList.Count, aux.AvgChildren, aux.neededLocks, aux.neededRooms, min, time, Constants.RESULTSFILE+"-"+prs.nV+"-" + prs.nK + ".csv");

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
}
