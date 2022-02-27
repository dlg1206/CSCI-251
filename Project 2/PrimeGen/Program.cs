/// @author Derek Garcia


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

            var pf = new PrimeFinder(numBits, count);
            pf.FindPrime();
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
    
    
    public class PrimeFinder
    {
        private RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        private byte[] _numBytes;
        private int _count;

        public PrimeFinder(int numBits, int count)
        {
            _numBytes = new Byte[numBits / 4];    // convert bits to bytes
            _count = count;
        }
        
        public void FindPrime()
        {
            int curCount = 0;
            int end = _count + 1;

            Parallel.For(0, end, prime =>
            {
                _rng.GetBytes(_numBytes);
                BigInteger bi = new BigInteger(_numBytes);
                
                

                if ( !bi.IsEven )
                {
                    Interlocked.Add(ref _count, 1);
                    Console.WriteLine(bi);
                }

                if (_count == curCount)
                {
                    end--;
                }
                else
                {
                    end++;
                }

                
            });

            // get rnd bytes
            // make big int
            // test int
            // print if prime
            // else loop


        }
    }
    
    
}