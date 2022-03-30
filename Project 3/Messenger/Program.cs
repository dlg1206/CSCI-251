/*
 * Project 2
 * Finds Primes of a given bit size
 * 
 * @author Derek Garcia
 */


using System.Net;
using System.Numerics;
using System.Text.Json;

namespace Messenger
{
    public static class Program
    {
        
        private static bool ValidateInput(string[] args)
        {
            if (args.Length == 0)
                return false;
            
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

        private static void ParseInput(string[] args)
        {
            switch (args[0])
            {
                case "keyGen":
                    return;
                
                case "sendKey":
                    return;
                
                case "getKey":
                    return;
                
                case "sendMsg":
                    return;
                
                case "getMsg":
                    return;
                
                default:
                    return;
            }
        }

        public static async Task Main(string[] args)
        {
            
            // Print error if 
            if (!ValidateInput(args))
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
            //var ws = new WebClient();   // init web client

            //await ws.Connect("http://kayrun.cs.rit.edu:5000/Key/jsb@cs.rit.edu");
            Console.WriteLine("foo");
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