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
            // TODO takes a bit to print
            Console.WriteLine("BitLength: {0} bits", numBits);

  
            var sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < count; i++)
            {
                BigInteger prime = FindPrime(numBits);
                Console.WriteLine("{0}: {1}", i + 1, prime);    // print prime
            }
            // TODO doesn't terminate and reach here
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
        /// <returns>Prime number of give bits</returns>
        private static BigInteger FindPrime(int numBits)
        {
            
            // init vars
            var primeFound = 0;
            var rng = RandomNumberGenerator.Create();
            var numBytes = new byte[numBits / 8];   // convert bits to bytes
            BigInteger bi;
            BigInteger prime = 0;

            // While number of primes not found, keep checking
            Parallel.For(0, Int64.MaxValue,  (i, state) =>
            {
                
                // If meet count, break
                if (primeFound > 0)
                {
                    Console.WriteLine("\tbreak Stopping . . .");
                    state.Stop();   
                }
                    
                
                // Make a randing big int
                rng.GetBytes(numBytes);
                bi = new BigInteger(numBytes);
                bi = BigInteger.Abs(bi);
                
                
                // Even isn't prime / skip if found all primes
                if (!bi.IsEven)
                {
                    // Further Prime checking
                    var probPrime = true;
                    if (bi > 3)
                        probPrime = bi.IsProbablyPrime(); // TODO Causes problems

                    // If probably prime
                    if (probPrime)
                    {
                        Console.WriteLine("Prime was found");
                        Interlocked.Add(ref primeFound, 1); // primeFound = true

                        lock (Lock)
                        {
                            Console.WriteLine("bi is {0}", bi);
                            prime = bi; // set prime to bi
                            Console.WriteLine("prime is set to {0}", prime);
                        }
                        
                        Console.WriteLine("\tprobPrime Stopping . . .");
                        state.Stop();
                    }
                }


            });
            // bug Not reached
            Console.WriteLine("Returning prime");
            return prime;

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