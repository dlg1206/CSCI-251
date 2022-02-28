/// @author Derek Garcia


using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;


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
            
            Console.WriteLine("BitLength: {0} bits", numBits);

            var pf = new PrimeFinder();
            var sw = new Stopwatch();
            sw.Start();
            pf.FindPrime(numBits, count);
            sw.Stop();
            
            Console.WriteLine("Time to Generate: {0}", sw.Elapsed);
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
    

    /// <summary>
    /// Class to find a given number of primes
    /// </summary>
    public class PrimeFinder
    {
        private readonly object _lock = new object();


        /// <summary>
        /// Finds prime with constructor parameters
        /// </summary>
        public void FindPrime(int numBits, int count)
        {
            // get rnd bytes
            // make big int
            // test int
            // print if prime
            // else loop

            // init vars
            var curCount = 0;
            var rng = RandomNumberGenerator.Create();
            var numBytes = new byte[numBits / 4];
            

            BigInteger bi;

            // While number of primes not found, keep checking
            Parallel.For(0, Int64.MaxValue,  (i, state) =>
            {
                if (curCount == count)
                {
                    state.Break();
                }
                // Make a new big int
              
                 rng.GetBytes(numBytes);
                
                 bi = new BigInteger(numBytes);
                 bi = BigInteger.Abs(bi);
                
                
                // Basic prime checking (if even then not prime)
                // TODO IsProbablyPrime implementation
                if ( !bi.IsEven &&  curCount != count)
                {
                    Interlocked.Add(ref curCount, 1);   // update count

                    lock (_lock)
                    {
                        Console.WriteLine("{0}: {1}", curCount, bi);    // report prime
                    }
                    
                }

                
                
                
            });
        }
    }
    
    
}