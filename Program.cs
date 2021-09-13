/// This program is a Level Generator that evolves levels via a MAP-Elites
/// Genetic Algorithm. This algorithm is an extension of the level generator
/// introduced by Pereira et al. [1]. The output of this program is a set of
/// levels written in JSON files.
///
/// This level generator receives seven arguments:
/// - [Optional] the separately save flag (`-s`);
///   * if the flag `-s` is entered, then the levels on the final population
///     will be written separately, each one in a single JSON file.
/// - a random seed;
/// - the number of generations;
/// - the initial population size;
/// - the mutation chance;
/// - the crossover chance, and;
/// - the number of tournament competitors.
///
/// Author: Breno M. F. Viana.
///
/// References
///
/// [1] Pereira, Leonardo Tortoro, et al. "Procedural generation of dungeons'
/// maps and locked-door missions through an evolutionary algorithm validated
/// with players." Expert Systems with Applications 180 (2021): 115009.

using System;

namespace LevelGenerator
{
    class Program
    {
        /// The minimum number of program parameters (arguments).
        private const int NUMBER_OF_PARAMETERS = 6;

        /// Flag of saving all levels separately.
        private const string SAVE_SEPARATELY = "-s";

        /// Error code for bad arguments.
        private const int ERROR_BAD_ARGUMENTS = 0xA0;

        static void Main(
            string[] args
        ) {
            // Check if the expected number of parameters were entered
            if (args.Length < NUMBER_OF_PARAMETERS)
            {
                Console.WriteLine("ERROR: Invalid number of parameters!");
                System.Environment.Exit(ERROR_BAD_ARGUMENTS);
            }
            // Has the separately save flag been entered?
            bool separately = args[0] == SAVE_SEPARATELY;
            // If so, then the evolutionary parameters are the next
            int i = separately ? 1 : 0;
            // Define the evolutionary parameters
            Parameters prs = new Parameters(
                int.Parse(args[i++]), // Random seed
                int.Parse(args[i++]), // Number of generations
                int.Parse(args[i++]), // Initial population size
                int.Parse(args[i++]), // Mutation chance
                int.Parse(args[i++]), // Crossover chance
                int.Parse(args[i])    // Number of tournament competitors
            );
            // Prepare the evolutionary process
            LevelGenerator generator = new LevelGenerator(prs);
            // Start the generative process and generate a set of levels
            generator.Evolve();
        }
    }
}