﻿/// @author Derek Garcia


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
            for (int i = 0; i < count; i++)
            {
                var prime = FindPrime(numBits);
                Console.WriteLine("{0}: {1}", i+1, prime);    // print prime

                if (prime % new BigInteger(2) == 0)
                {
                    Console.WriteLine("\tEVEN");
                }
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
        private static BigInteger? FindPrime(int numBits)
        {
            
            // init vars
            BigInteger? prime = null;
            var bytes = numBits / 8;
            var rng = RandomNumberGenerator.Create();

            // While number of primes not found, keep checking
            Parallel.For(0, Int32.MaxValue, (i, state) =>
            {
                // If unlocked bi is changed while checking
         
                var numBytes = new byte[bytes];   // convert bits to bytes
                // Make a randing big int
                //Console.WriteLine("making new bi");
                rng.GetBytes(numBytes);
                BigInteger bi = new BigInteger(numBytes);
                bi = BigInteger.Abs(bi);
               // Console.WriteLine("made new bi");
                
                
                if (bi.InitialPrimeCheck() && bi.IsProbablyPrime())
                {
                    // Just for printing
                    // lock (_lock)
                    // {
                    //     Console.WriteLine("{0} has found prime {1}", Task.CurrentId, bi);
                    //     Console.WriteLine("\t{0} init check result for {1}: {2}", Task.CurrentId, bi ,bi.InitialPrimeCheck());
                    //     Console.WriteLine("\t{0} Is prob check result for {1}: {2}", Task.CurrentId, bi, bi.IsProbablyPrime());
                    // }
                    

                    lock (_lock)
                    {
                        if (prime == null)
                        {
                            
                            //Console.WriteLine("\t\tbi of t{0} is {1}", Task.CurrentId ,bi);
                            // todo bi changes from above to below
                            prime = bi;
                           // Console.WriteLine("\t\t{0} has assigned prime to {1}", Task.CurrentId, prime);
                            
                            
                        }

                    }
                    
                    state.Stop();
                    
                }
                else
                {
                   // Console.WriteLine("\tfailed");
                }
             

            });
            //Console.WriteLine("\t\tReturning prime {0}", prime);
            return prime;
        }


        private static bool InitialPrimeCheck(this BigInteger value)
        {
            
            // 2 is the only even prime number
            if (value == 2)
            {
                return true;
            }
            
            // // check if can divide by other primes
            // if (value % new BigInteger(2) == 0)
            // {
            //     return false;
            // }
            //
            // if (value % new BigInteger(3) == 0)
            // {
            //     return false;
            // }
            //
            // if (value % new BigInteger(5) == 0)
            // {
            //     return false;
            // }
            //
            // if (value % new BigInteger(7) == 0)
            // {
            //     return false;
            // }
            //
            // if (value % new BigInteger(11) == 0)
            // {
            //     return false;
            // }

            return (value % new BigInteger(2) != 0 &&
                    value % new BigInteger(3) != 0 &&
                    value % new BigInteger(5) != 0 &&
                    value % new BigInteger(7) != 0 &&
                    value % new BigInteger(11) != 0 &&
                    value % new BigInteger(13) != 0 &&
                    value % new BigInteger(17) != 0 &&
                    value % new BigInteger(19) != 0 &&
                    value % new BigInteger(23) != 0 &&
                    value % new BigInteger(29) != 0);
            
        }
        
        /// <summary>
        /// Miller–Rabin algorithm to test if the given number is probably
        /// prime
        /// </summary>
        /// <param name="n">number to test</param>
        /// <param name="k">number of witnesses</param>
        /// <returns></returns>
        private static bool IsProbablyPrime(this BigInteger n, int k = 10)
        {
            // 2 is the smallest prime
            if (n < new BigInteger(2))
                return false;

            // 2 is only prime that is even
            if (n == 2)
                return true;
            
            // != 2 and is even
            if (n != 2 && n % 2 == 0)
                return false;
            
            // Init values
            var r = 0;
            var d = n - 1;

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
                    byte[] rand = new byte[n.GetByteCount()];
                    RandomNumberGenerator.Create().GetBytes(rand);
                    a = new BigInteger(rand);
                } while (a < 2 || a >= n - 2);

                // get x value
                var x = BigInteger.ModPow(a, d, n);
                
                // if x == 1 or x == n - 1, continue witnessloop
                if (x == 1 || x == n - 1)
                    continue;

                // repeat for r - 1 times
                for(var m = 1; m < r; m++)
                {
                    x = BigInteger.ModPow(x, 2, n);

                    if (x == 1)
                    {
                        return false;
                    }
                        
                    
                    // if true, continue Witness Loop
                    if (x == n - 1)
                        break;
                }
                // if true, return composite
                if (x != n - 1)
                {
                    return false;
                }
                  
            }
            // Pass all tests, probably prime
            return true;
        }


        
    }
    
}