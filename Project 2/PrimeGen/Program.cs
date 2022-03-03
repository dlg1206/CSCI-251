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
            var curCount = 0;
            sw.Start();
            while (curCount < count)
            {
                BigInteger prime = FindPrime(numBits);

                // if (!prime.IsEven)
                // {
                //     Console.WriteLine("{0}: {1}", ++curCount, prime);    // print prime
                // }
                Console.WriteLine("{0}: {1}", ++curCount, prime);    // print prime

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

        private static object _lock = new object();
        /// <summary>
        /// Finds numerous primes
        /// </summary>
        /// <param name="numBits">size of the prime</param>
        /// <returns>Prime number of give bits</returns>
        private static BigInteger FindPrime(int numBits)
        {
            
            // init vars
            BigInteger bi = 0;
            var bytes = numBits / 8;
            var biTwo = new BigInteger(2);



            // While number of primes not found, keep checking
            Parallel.For(0, Int32.MaxValue, (i, state) =>
            {
                var rng = RandomNumberGenerator.Create();
                var numBytes = new byte[bytes];   // convert bits to bytes
                var isOdd = false;

                // Make a randing big int
                rng.GetBytes(numBytes);
                bi = new BigInteger(numBytes);
                bi = BigInteger.Abs(bi);
                
                
                // Even isn't prime
                //bi % biTwo == 1;
                //Console.WriteLine(bi);
                if ( true )
                {
                   
                    // If probably prime, stop all threads
                    if ( bi <= 2 || bi.IsProbablyPrime() )
                    {
                        state.Stop();
                    }
                }
            });
            // bug Not reached
            return bi;
        }
        
        // TODO Delete this!
        private static BigInteger GetPrime2(int numBits)
        {
            // init vars
            var rng = RandomNumberGenerator.Create();
            var numBytes = new byte[numBits / 8]; // convert bits to bytes

            // While number of primes not found, keep checking
            for (;;)
            {


                // Make a randing big int
                rng.GetBytes(numBytes);
                BigInteger bi = new BigInteger(numBytes);
                bi = BigInteger.Abs(bi);


                // Even isn't prime
                if (!bi.IsEven)
                {
                    // Further Prime checking
                    var probPrime = true;
                    if (bi > 3)
                        probPrime = bi.IsProbablyPrime(); // TODO Causes problems

                    // If probably prime
                    if (probPrime)
                    {
                        return bi;
                        
                    }
                }
            }
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
            var r = 0;
            var d = value - 1;

            // find largest factor n that is a power of 2
            while (d % 2 == 0)
            {
                r++;
                d /= 2;
            }

            // Witness Loop
            for (var i = 0; i <= k; i++)
            {
                // Get a random big int in the range of 2 < a < n - 2
                BigInteger a;
                do
                {
                    byte[] rand = new byte[value.GetByteCount()];
                    RandomNumberGenerator.Create().GetBytes(rand);
                    a = new BigInteger(rand);
                } while (a < 2 || a >= value - 2);

                // get x value
                var x = BigInteger.ModPow(a, d, value);

                
                if (x == 1 || x == value - 1)
                    continue;

                // repeat for r - 1 times
                for(var m = 1; m < r; m++)
                {
                    x = BigInteger.ModPow(x, 2, value);
    
                    if (x == 1)
                    {
                        return false;
                    }
                    
                    // if true, continue Witness Loop
                    if (x == value - 1)
                        break;
                }
                // if true, return composite
                if (x != value - 1)
                    return false;

            }
            // Pass all tests, probably prime
            return true;
        }


        
    }
    
}