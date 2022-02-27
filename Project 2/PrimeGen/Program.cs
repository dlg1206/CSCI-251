/// @author Derek Garcia
namespace PrimeGen
{
    /// <summary>
    /// Main Function that takes in args
    /// </summary>
    public class Program
    {
        private const int MaxArgs = 2;
        /// <summary>
        /// Main that takes in args
        /// </summary>
        /// <param name="args">input arguments</param>
        public static void Main(string[] args)
        {
            // Check if correct arg count
            if (args.Length != MaxArgs)
            {
                PrintUsage();
                return;
            }

            // Check both inputs are valid ints
            int numBits, count;
            if (!int.TryParse(args[0], out numBits) || !int.TryParse(args[1], out count))
            {
                PrintUsage();
                return;
            }
            
            Console.WriteLine("Ok");
        }

        /// <summary>
        /// Prints usage of the program
        /// </summary>
        private static void PrintUsage()
        {
            Console.WriteLine("Usage: dotnet run <bits> <count=1>");
            Console.WriteLine("\t- bits: the number of bits of the prime number, this must be a"); 
            Console.WriteLine("\t  multiple of 8, and at least 32 bits.");
            Console.WriteLine("\t- count - the number of prime numbers to generate, defaults to 1");
        }
    }
    
    
}