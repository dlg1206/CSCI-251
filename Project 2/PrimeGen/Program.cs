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
    
    public class PrimeFinder
    {
        private RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        private byte[] _numBytes;
        private int _count;
        private Object _lock = new Object();

        public PrimeFinder(int numBits, int count)
        {
            _numBytes = new Byte[numBits / 4];    // convert bits to bytes
            _count = count;
        }
        
        public void FindPrime()
        {
            // get rnd bytes
            // make big int
            // test int
            // print if prime
            // else loop

            int curCount = 0;
            int end = _count;
            BigInteger bi;

            Parallel.For(0, end, prime =>
            {


                lock (_lock)
                {
                    _rng.GetBytes(_numBytes);
                     bi = new BigInteger(_numBytes);
                }

                
                if ( !bi.IsEven )
                {
                    Interlocked.Add(ref curCount, 1);
                    Console.WriteLine("{0}: {1}", curCount, bi);
                }
                
                
                if (_count == curCount)
                {
                    Interlocked.Decrement(ref end);
                }
                else
                {
                    Interlocked.Add(ref end, 1);
                }
                
                

                
            });

           

        }
    }
    
    
}