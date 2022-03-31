/*
 * Project 2
 * Finds Primes of a given bit size
 * 
 * @author Derek Garcia
 */

using System.Numerics;
using System.Security.Cryptography;

namespace Messenger;

public static class BigIntExtensions
{
    // For Initial Prime Test
     private static readonly int[] Primes =
            {
                2,3,5,7,11,13,17,19,23,29,31,37,41,43,47,53,59,61,67,71,73,79,83,89,97,101,
                103,107,109,113,127,131,137,139,149,151,157,163,167,173,179,181,191,193,197,
                199,211,223,227,229,233,239,241,251,257,263,269,271,277,281,283,293,307,311,
                313,317,331,337,347,349,353,359,367,373,379,383,389,397,401,409,419,421,431,
                433,439,443,449,457,461,463,467,479,487,491,499,503,509,521,523,541,547,557,
                563,569,571,577,587,593,599,601,607,613,617,619,631,641,643,647,653,659,661,
                673,677,683,691,701,709,719,727,733,739,743,751,757,761,769,773,787,797,809,
                811,821,823,827,829,839,853,857,859,863,877,881,883,887,907,911,919,929,937,
                941,947,953,967,971,977,983,991,997,1009,1013,1019,1021,1031,1033,1039,1049,
                1051,1061,1063,1069,1087,1091,1093,1097,1103,1109,1117,1123,1129,1151,1153,
                1163,1171,1181,1187,1193,1201,1213,1217,1223,1229,1231,1237,1249,1259,1277,
                1279,1283,1289,1291,1297,1301,1303,1307,1319,1321,1327,1361,1367,1373,1381,
                1399,1409,1423,1427,1429,1433,1439,1447,1451,1453,1459,1471,1481,1483,1487,
                1489,1493,1499,1511,1523,1531,1543,1549,1553,1559,1567,1571,1579,1583,1597,
                1601,1607,1609,1613,1619,1621,1627,1637,1657,1663,1667,1669,1693,1697,1699,
                1709,1721,1723,1733,1741,1747,1753,1759,1777,1783,1787,1789,1801,1811,1823,
                1831,1847,1861,1867,1871,1873,1877,1879,1889,1901,1907,1913,1931,1933,1949,
                1951,1973,1979,1987,1993,1997,1999,2003,2011,2017,2027,2029,2039,2053,2063,
                2069,2081,2083,2087,2089,2099,2111,2113,2129,2131,2137,2141,2143,2153,2161,
                2179,2203,2207,2213,2221,2237,2239,2243,2251,2267,2269,2273,2281,2287,2293,
                2297,2309,2311,2333,2339,2341,2347,2351,2357,2371,2377,2381,2383,2389,2393,
                2399,2411,2417,2423,2437,2441,2447,2459,2467,2473,2477,2503,2521,2531,2539,
                2543,2549,2551,2557,2579,2591,2593,2609,2617,2621,2633,2647,2657,2659,2663,
                2671,2677,2683,2687,2689,2693,2699,2707,2711,2713,2719,2729,2731,2741,2749,
            };
     
     // For Miller-Rabin Algorithm
     private const bool Composite = false;
     private const bool ProbablyPrime = true;
    
    /// <summary>
    /// Extension method for a BigInteger that does an initial prime test
    /// by dividing the value by the first 15 primes
    /// </summary>
    /// <param name="value">value to test if prime</param>
    /// <returns>true if passes, false otherwise</returns>
    public static bool InitialPrimeCheck(this BigInteger value)
    {
        // 2 is the only even prime number
        return value == 2 || Primes.All(prime => value % prime != 0);
    }
    
    /// <summary>
    /// Miller–Rabin algorithm to test if the given 
    /// number is probably prime
    /// </summary>
    /// <param name="n">number to test</param>
    /// <param name="k">number of witnesses</param>
    /// <returns>true if passes, false otherwise</returns>
    public static bool IsProbablyPrime(this BigInteger n, int k = 10)
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


public class PrimeGen
{ 
    // For FindPrime
    
    private const int BitsPerByte = 8;
    
    private static readonly object Lock = new object();    // for use in assigning primes
    /// <summary>
    /// Finds a single probably prime number
    /// </summary>
    /// <param name="numBits">size of the prime</param>
    /// <returns>A probably prime number</returns>
    public BigInteger? FindPrime(int numBits)
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

        
        
        
        
        
}

