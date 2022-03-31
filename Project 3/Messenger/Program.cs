/*
 * Project 2
 * Finds Primes of a given bit size
 * 
 * @author Derek Garcia
 */


using System.Numerics;
using System.Text.Json;

namespace Messenger
{
    public static class Program
    {
        private const int E = 5113;
        private static bool ValidateInput(string[] args)
        {
           
            
            switch (args[0])
            {
                case "keyGen":
                    return args.Length == 2;
                
                case "sendKey":
                    return args.Length == 2;
                
                case "getKey":
                    return args.Length == 2;
                
                case "sendMsg":
                    return args.Length == 3;
                
                case "getMsg":
                    return args.Length == 2;
                
                default:
                    return false;
            }
        }
        
        private static bool ParseInput(string[] args)
        {
            if (args.Length == 0)
                return false;
                
            switch (args[0])
            {
                case "keyGen":
                    if (args.Length == 2 || !int.TryParse(args[1], out var keySize))
                    {
                        return false;
                    }
                    else
                    {
                        DoKeyGen(keySize);
                    }
                    break;
                
                case "sendKey":
                    break;
                
                case "getKey":
                    break;
                
                case "sendMsg":
                    break;
                
                case "getMsg":
                    break;
                
                default:
                    return false;
            }

            return true;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("usage: dotnet run <option> <other arguments>");
                
            Console.WriteLine(
                "\t- keyGen <keySize>: generate a keypair of size keySize bits (public and private keys) and " +
                "store them locally on the disk");
                
            Console.WriteLine(
                "\t- sendKey <email>: sends the public key that was generated in the keyGen phase and send it to " +
                "the server, with the email address given");

            Console.WriteLine(
                "\t- getKey <email>: this will retrieve public key for a particular user with that email");

            Console.WriteLine(
                "\t- sendMsg <email> <plaintext>: this will take a plaintext message, encrypt it using the public" +
                " key of the person you are sending it to, based on their email address.");

            Console.WriteLine(
                "\t- getMsg <email>: this will retrieve a message for a particular user.");
        }

        private static void DoKeyGen(int keySize)
        {
            var pg = new PrimeGen();
    
            var lenP = (int) (keySize / 2 + keySize * 0.2);

            var p = pg.FindPrime(lenP);
            var q = pg.FindPrime(keySize - lenP);

            var nonce = p * q;
            var r =  (p - 1) * (q - 1);

            var publicKey = new Key(nonce, new BigInteger(E), true);
            var privateKey = new Key(nonce, new BigInteger(E).ModInverse(r), false);

            var km = new KeyManager();
        }

        public static void Main(string[] args)
        {
            
            
            // var ws = new WebClient();   // init web client

            // await ws.Connect("http://kayrun.cs.rit.edu:5000/Key/jsb@cs.rit.edu");
            // Print error if 
            if (!ParseInput(args))
            {
                PrintUsage();
                return;
            }

            var web = "AAAAAwEAAQAAAQB7w4yJG+kH5BXhL9lgeCxkNqKeIIyC0zzG0FYJu5/WVa7xCdXGSmG3pEEpyEPhe81L9zb1qWpnn" +
                      "9yoiMPPawtDoZ26Um0LA/MAx/n4UdBENyWYd807+ex1h/uJ/GHgeZI/8yZ5LapCTNXaAwXvTfSY4OTG9hEgTJ6uK7cM11hn/q" +
                      "K07EnH1beaGoj/FOATFPqpLkDaz/fOkRQIQr6F41ks0PIJXjzmMeIJdUhBsluJaU/pllHqjTDFk2uBOSQr5g0WFeCVLfss0E" +
                      "Ybkbx3BsLtvThDgphBc98KOU2gx3o+Tm5U1oTT/tZdUjrWq8iPWzI+JMrG1RtZEVVeewOFT5sn";
            

            /*
             * private key emails: list of all emails that I have sent to sever using that private key
             * pub key email: NONE< don't touch
             */
        }
        
    }

    public class Key
    {
        private readonly BigInteger _nonce; // N
        private readonly BigInteger _prime; // E or D

        public Key(BigInteger nonce, BigInteger prime, bool isPublic)
        {
            _nonce = nonce;
            _prime = prime;
            IsPublic = isPublic;
        }

        public bool IsPublic { get; }

        public BigInteger GetNonce()
        {
            return _nonce;
        }

        public BigInteger GetPrime()
        {
            return _prime;
        }
    

    }

    public class KeyManager
    {

        private byte[] GetNBytes(byte[] source, int startIndex, int numBytes)
        {
            
            var section = new byte[numBytes];

            Array.Copy(source, startIndex, section, 0, numBytes);

            // always return little endian form
            if (BitConverter.IsLittleEndian)
                Array.Reverse(section);
                
            return section;
        }
        
        private string Base64Encode(Key key)
        {
            var E = key.GetPrime().ToByteArray();
            Array.Reverse(E);
            
            var e = BitConverter.GetBytes(E.Length);
            Array.Reverse(e);

            var N = key.GetNonce().ToByteArray();
            Array.Reverse(N);
            
            var n = BitConverter.GetBytes(N.Length);
            Array.Reverse(n);

            var combined = Array.Empty<byte>().Concat(e).Concat(E).Concat(n).Concat(N).ToArray();

            return Convert.ToBase64String(combined);
            
        }

        private Key Base64Decode(string encoding)
        {
            var keyBytes = Convert.FromBase64String(encoding);

            // convert 1st 4 bytes to 'e'
            var e = BitConverter.ToInt32(GetNBytes(keyBytes, 0, 4), 0);
            var E = new BigInteger(GetNBytes(keyBytes, 4, e));
            
            var n = BitConverter.ToInt32(GetNBytes(keyBytes, e+4, 4), 0);
            var N = new BigInteger(GetNBytes(keyBytes, e+8, n));

            return new Key(N, E, true);
        }

        public void storeKey(Key key)
        {
            
        }
    }
    
    public class WebClient
    {
        private readonly HttpClient _client = new HttpClient();
        
        /// <summary>
        /// Connect to given url
        /// </summary>
        /// <param name="url"></param>
        public async Task Connect(string url)
        {
            // attempt to connect to server
            try	
            {
                var json = await _client.GetStringAsync(url);
                ParseJson(json);
                
            }
            // report err
            catch(HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");	
                Console.WriteLine("Message :{0} ",e.Message);
            }
            
            
        }

        private void ParseJson(string json)
        {
            var jsonDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            if (jsonDict == null)
                return;

            Console.WriteLine(jsonDict["key"]);
            
            var keyBytes = Convert.FromBase64String(jsonDict["key"]);

            // convert 1st 4 bytes to 'e'
            var e = BitConverter.ToInt32(GetNBytes(keyBytes, 0, 4), 0);
            var E = new BigInteger(GetNBytes(keyBytes, 4, e));
            
            var n = BitConverter.ToInt32(GetNBytes(keyBytes, e+4, 4), 0);
            var N = new BigInteger(GetNBytes(keyBytes, e+8, n));
            
       
        }

        private byte[] GetNBytes(byte[] source, int startIndex, int numBytes)
        {
            
            var section = new byte[numBytes];

            Array.Copy(source, startIndex, section, 0, numBytes);

            // always return little endian form
            if (BitConverter.IsLittleEndian)
                Array.Reverse(section);
                
            return section;
        }
            
    }
    
}