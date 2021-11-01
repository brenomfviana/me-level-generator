/// This program is a Level Generator that evolves levels via a MAP-Elites
/// Genetic Algorithm. This algorithm is an extension of the level generator
/// introduced by Pereira et al. [1]. The output of this program is a set of
/// levels written in JSON files.
///
/// This level generator receives seven arguments:
/// - a random seed;
/// - the maximum time;
/// - the initial population size;
/// - the mutation chance;
/// - the number of tournament competitors;
/// - weight or not the enemy sparsity;
/// - include or not empty rooms in enemy STD;
/// - the number of rooms;
/// - the number of keys;
/// - the number of locks;
/// - the number of enemies, and;
/// - the linear coefficient.
///
/// Author: Breno M. F. Viana.
///
/// References
///
/// [1] Pereira, Leonardo Tortoro, et al. "Procedural generation of dungeons'
/// maps and locked-door missions through an evolutionary algorithm validated
/// with players." Expert Systems with Applications 180 (2021): 115009.

using System;
using System.Diagnostics;

namespace LevelGenerator
{
    class Program
    {
        /// The minimum number of program parameters (arguments).
        private const int NUMBER_OF_PARAMETERS = 12;

        /// The error code for bad arguments.
        private const int ERROR_BAD_ARGUMENTS = 0xA0;
        /// The error message of not enough population.
        public static readonly string TOO_MUCH_COMPETITORS =
            "The number of competitors is higher than the population size; " +
            "in this way, tournament selection is impossible.";
        /// The error message of not enough competitors.
        public static readonly string TOO_FEW_COMPETITORS =
            "The number of competitors is not enough for a tournament.";

        static void Main(
            string[] _args
        ) {
            // Check if the expected number of parameters were entered
            if (_args.Length < NUMBER_OF_PARAMETERS)
            {
                Console.WriteLine("ERROR: Invalid number of parameters!");
                System.Environment.Exit(ERROR_BAD_ARGUMENTS);
            }
            // Define the evolutionary parameters
            Parameters prs = new Parameters(
                int.Parse(_args[0]), // Random seed
                int.Parse(_args[1]), // Maximum time
                int.Parse(_args[2]), // Initial population size
                int.Parse(_args[3]), // Mutation chance
                int.Parse(_args[4]), // Number of tournament competitors
                bool.Parse(_args[5]), // Weight or not the enemy sparsity
                bool.Parse(_args[6]), // Include or not empty rooms in enemy STD
                int.Parse(_args[7]), // Number of rooms
                int.Parse(_args[8]), // Number of keys
                int.Parse(_args[9]), // Number of locks
                int.Parse(_args[10]), // Number of enemies
                float.Parse(_args[11]) // Linear coefficient
            );
            // Ensure the population size is enough for the tournament
            Debug.Assert(
                prs.population >= prs.competitors,
                TOO_MUCH_COMPETITORS
            );
            // Ensure the number of competitors is valid
            Debug.Assert(
                prs.competitors > 1,
                TOO_FEW_COMPETITORS
            );
            // Run the generator and save the results and the collected data
            LevelGenerator generator = new LevelGenerator(prs);
            generator.Evolve();
            generator.Solution.Debug();
            Output.WriteData(generator.Solution, generator.Data);
        }
    }
}