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

            var pf = new PrimeFinder(numBits, count);
            var sw = new Stopwatch();
            sw.Start();
            pf.FindPrime();
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
        private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        private readonly byte[] _numBytes;  // for bigint creation
        private readonly int _count;  
        private readonly Object _lock = new Object();

        /// <summary>
        /// Constructor for finding primes
        /// </summary>
        /// <param name="numBits">number of bits of the prime</param>
        /// <param name="count">number of primes to make</param>
        public PrimeFinder(int numBits, int count)
        {
            _numBytes = new Byte[numBits / 4];    // convert bits to bytes
            _count = count;
        }
        
        
        /// <summary>
        /// Finds prime with constructor parameters
        /// </summary>
        public void FindPrime()
        {
            // get rnd bytes
            // make big int
            // test int
            // print if prime
            // else loop

            // init vars
            int curCount = 0;
            int end = _count + 1;
            BigInteger bi;

            // While number of primes not found, keep checking
            Parallel.For(0, end, prime =>
            {
                // Make a new big int
                lock (_lock)
                {
                    _rng.GetBytes(_numBytes);
                     bi = new BigInteger(_numBytes);
                }
                
                // Basic prime checking (if even then not prime)
                // TODO IsProbablyPrime implementation
                if ( !bi.IsEven )
                {
                    Interlocked.Add(ref curCount, 1);   // update count
                    Console.WriteLine("{0}: {1}", curCount, bi);    // report prime
                    // TODO negative primes?
                    // todo incorrect number of primes
                }
                
                // if number of primes met, "move" end condition to be met
                if (_count == curCount)
                {
                    Interlocked.Decrement(ref end);
                }
                // else "move" end condition forward 1
                else
                {
                    Interlocked.Add(ref end, 1);
                }
            });
        }
    }
    
    
}