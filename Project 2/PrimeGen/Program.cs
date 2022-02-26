/// @author Derek Garcia
namespace PrimeGen
{
    /// <summary>
    /// Main Function that takes in args
    /// </summary>
    public class Program
    {
        private static int MAX_ARGS = 2;
        /// <summary>
        /// Main that takes in args
        /// </summary>
        /// <param name="args">input arguments</param>
        public static void Main(string[] args)
        {
            if (!IsValid(args))
            {
                Console.WriteLine("Usage: dotnet run <bits> <count=1>");
                Console.WriteLine("\t- bits: the number of bits of the prime number, this must be a"); 
                Console.WriteLine("\t  multiple of 8, and at least 32 bits.");
                Console.WriteLine("\t- count - the number of prime numbers to generate, defaults to 1");
                return;
            }
            
            Console.WriteLine("Ok");
            
        }

        private static bool IsValid(string[] args)
        {
            if (args.Length != MAX_ARGS)
            {
                return false;
            }

            try
            {
                Int32.Parse(args[0]);
                Int32.Parse(args[1]);

            }
            catch (FormatException)
            {
                return false;
            }

            return true;
        }
    }
    
    
}