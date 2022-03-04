/*
 * Project 2
 * Finds Primes of a given bit size
 * 
 * @author Derek Garcia
 */

using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;


namespace PrimeGen
{
    public static class Program
    {
        // For error checking
        private const int MaxArgs = 2;
        private const int MinArgs = 1;
        private const int MinBits = 32;
        
        // For FindPrime
        private const int BitsPerByte = 8;
        
        // For Miller-Rabin Algorithm
        private const bool Composite = false;
        private const bool ProbablyPrime = true;

        /// <summary>
        /// Main that takes in args
        /// </summary>
        /// <param name="args">input arguments</param>
        public static void Main(string[] args)
        {
            // Check if correct arg count
            if (args.Length is > MaxArgs or < MinArgs)
            {
                PrintUsage();
                return;
            }
            
            // Check if bits are a number
            if (!int.TryParse(args[0], out var numBits))
            {
                PrintUsage();
                return;
            }
            
            // Check if bits are >= 32 and a multiple of 8
            if (numBits < 32 || numBits % BitsPerByte != 0)
            {
                PrintUsage();
                return;
            }
            
            // Checks if count is a number, defaults to 1 (since if reach here numBits is valid)
            int count;
            // no count defaults to 1
            if (args.Length == MinArgs)
            {
                count = 1;
            }
            // Else test valid input
            else  if (!int.TryParse(args[1], out count))
            {
                PrintUsage();
                return;
            }
            
           

            // Write bit information
            Console.WriteLine("BitLength: {0} bits", numBits);
            
            // start timer
            var sw = new Stopwatch();
            sw.Start();
            
            // Get desired number of primes
            for (int i = 1; i <= count; i++)
            {
                var prime = FindPrime(numBits);
                Console.WriteLine("{0}: {1}", i, prime);    // print prime
                
                // print newline if not final prime to print
                if(i < count)
                    Console.WriteLine();
                
                // todo REMOVE THIS WHEN SUBMITTING
                if (prime % new BigInteger(2) == 0)
                {
                    Console.WriteLine("\tEVEN");
                }
            }
            // TODO Performance issues; 1 8192 ran several minutes and nothing; target 42 sec
            // Stop timer and report elapsed time
            sw.Stop();
            Console.WriteLine("Time to Generate: {0}", sw.Elapsed);
        }
        
        
        /// <summary>
        /// Prints the correct usage of the program
        /// </summary>
        private static void PrintUsage()
        {
            Console.WriteLine("Usage: dotnet run <bits> <count=1>");
            Console.WriteLine("\t- bits: the number of bits of the prime number, this must be a"); 
            Console.WriteLine("\t  multiple of 8, and at least 32 bits.");
            Console.WriteLine("\t- count - the number of prime numbers to generate, defaults to 1");
        }

        
        private static readonly  object Lock = new object();    // for use in assigning primes
        /// <summary>
        /// Finds a single probably prime number
        /// </summary>
        /// <param name="numBits">size of the prime</param>
        /// <returns>A probably prime number</returns>
        private static BigInteger? FindPrime(int numBits)
        {
            // init vars
            BigInteger? prime = null;
            var bytes = numBits / BitsPerByte;  // convert bits to bytes
            var rng = RandomNumberGenerator.Create();

            // Loop until fine a probably prime number
            Parallel.For(0, Int32.MaxValue, (i, state) =>
            {
                
                // Make random BigInteger
                var numBytes = new byte[bytes];
                rng.GetBytes(numBytes);
                BigInteger bi = new BigInteger(numBytes);
                bi = BigInteger.Abs(bi);    // abs so only positive values

                // If rand BigInt fails quick check or Miller-Rabin test, don't continue
                if (!bi.InitialPrimeCheck() || !bi.IsProbablyPrime()) return;
                
                // If reach here then bi is valid prime
                lock (Lock)
                    prime ??= bi;   // Assign value of bi to prime if its null
                
                // prime has been assigned, can stop threads
                state.Stop();
            });
            
            // return prime that was found
            return prime;
        }


        /// <summary>
        /// Extension method for a BigInteger that does an initial prime test
        /// by dividing the value by the first 15 primes
        /// </summary>
        /// <param name="value">value to test if prime</param>
        /// <returns>true if passes, false otherwise</returns>
        private static bool InitialPrimeCheck(this BigInteger value)
        {
            // 2 is the only even prime number
            if (value == 2)
                return true;
            
            // Check if value can be divided by the first 15 primes
            return (value % new BigInteger(2) != 0 &&
                    value % new BigInteger(3) != 0 &&
                    value % new BigInteger(5) != 0 &&
                    value % new BigInteger(7) != 0 &&
                    value % new BigInteger(11) != 0 &&
                    value % new BigInteger(13) != 0 &&
                    value % new BigInteger(17) != 0 &&
                    value % new BigInteger(19) != 0 &&
                    value % new BigInteger(23) != 0 &&
                    value % new BigInteger(29) != 0 &&
                    value % new BigInteger(31) != 0 &&
                    value % new BigInteger(37) != 0 &&
                    value % new BigInteger(41) != 0 &&
                    value % new BigInteger(43) != 0 &&
                    value % new BigInteger(47) != 0);
            
        }
        
        
        /// <summary>
        /// Miller–Rabin algorithm to test if the given 
        /// number is probably prime
        /// </summary>
        /// <param name="n">number to test</param>
        /// <param name="k">number of witnesses</param>
        /// <returns>true if passes, false otherwise</returns>
        private static bool IsProbablyPrime(this BigInteger n, int k = 10)
        {
            // Double checks that n is valid for Miller-Rabin
            
            // 2 is the smallest prime
            if (n < new BigInteger(2))
                return Composite;

            // 2 is only prime that is even
            if (n == 2)
                return ProbablyPrime;
            
            // != 2 and is even
            if (n != 2 && n % 2 == 0)
                return Composite;
            
            // Begin Miller-Rabin; Init values
            var r = 0;
            var d = n - 1;

            // Find an odd value d such that n == 2^r * d + 1
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
                    byte[] rand = new byte[n.GetByteCount()];
                    RandomNumberGenerator.Create().GetBytes(rand);
                    a = new BigInteger(rand);
                } while (a < 2 || a >= n - 2);

                // get x value
                var x = BigInteger.ModPow(a, d, n);
                
                // if x == 1 or x == n - 1, continue Witness Loop
                if (x == 1 || x == n - 1)
                    continue;

                // repeat for r - 1 times
                for(var m = 1; m < r; m++)
                {
                    x = BigInteger.ModPow(x, 2, n);
                   
                    // if true, return composite
                    if (x == 1)
                        return Composite;
                    
                    // if true, continue Witness Loop
                    if (x == n - 1)
                        break;
                }
                // if true, return composite
                if (x != n - 1)
                    return Composite;
            }
            // Pass all tests, probably prime
            return ProbablyPrime;
        }
    }
}