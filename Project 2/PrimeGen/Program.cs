/// @author Derek Garcia


using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;


namespace PrimeGen
{
    

    
    /// <summary>
    /// Main Function that takes in args
    /// </summary>
    public static class Program
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

  
            var sw = new Stopwatch();
            sw.Start();
            FindPrime(numBits, count);
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

        private static readonly object Lock = new object();     // lock object for printing
        
        /// <summary>
        /// Finds numerous primes
        /// </summary>
        /// <param name="numBits">size of the prime</param>
        /// <param name="count">number of primes to make</param>
        private static void FindPrime(int numBits, int count)
        {
            
            // init vars
            var curCount = 0;
            var rng = RandomNumberGenerator.Create();
            var numBytes = new byte[numBits / 4];   // convert bits to bytes
            BigInteger bi;

            // While number of primes not found, keep checking
            Parallel.For(0, Int64.MaxValue,  (i, state) =>
            {
                // If meet count, break
                if (curCount == count)
                    state.Break();
                
                // Make a randing big int
                rng.GetBytes(numBytes);
                bi = new BigInteger(numBytes);
                bi = BigInteger.Abs(bi);
                
                
                // Even isn't prime / skip if found all primes
                if ( !bi.IsEven && curCount != count)
                {
                    // Further Prime checking
                    var probPrime = true;
                    if (bi > 3)
                        probPrime = bi.IsProbablyPrime();   // TODO Causes problems
                    
                    // If probably prime
                    if (probPrime)
                    {
                        Interlocked.Add(ref curCount, 1);   // update count
                        lock (Lock)
                        {
                            Console.WriteLine("{0}: {1}", curCount, bi);    // print prime
                        }
                    }
                }

            });
            
        
        }

        /// <summary>
        /// Miller–Rabin algorithm to test if the given number is probably
        /// prime
        /// </summary>
        /// <param name="value">number to test</param>
        /// <param name="k">number of witnesses</param>
        /// <returns></returns>
        private static bool IsProbablyPrime(this BigInteger value, int k = 10)
        {
            
            // Init values
            var n = value - 1;
            var r = 0;
            var mod = n % 2 ^ r;

            // find largest factor n that is a power of 2
            while (mod == 0)
            {
                Interlocked.Increment(ref r);
                mod = n % 2 ^ r;
            }
        
            // Get odd coefficient
            var d = n / 2 ^ r;

            // Witness Loop
            for (var i = 0; i <= k; i++)
            {
                // Get a random big int in the range of 2 < a < n - 2
                BigInteger a;
                do
                {
                    byte[] rand = new byte[n.GetByteCount()];
                    RandomNumberGenerator.Create().GetBytes(rand);
                    a = new BigInteger(rand);
                } while (a < 2 || a >= n - 2);

                // get x value
                var x = BigInteger.ModPow(a, d, n);

                
                if (x == 1 || x == n - 1)
                    continue;

                // repeat for r - 1 times
                for(var m = 0; m < r; m++)
                {
                    x = BigInteger.ModPow(x, 2, n);
                    // if true, continue Witness Loop
                    if (x == n - 1)
                        break;
                }
                // if true, return composite
                if (x != n - 1)
                    return false;

            }
            // Pass all tests, probably prime
            return true;
        }


        
    }
    
}