/*
 * Project 2
 * Finds Primes of a given bit size
 * 
 * @author Derek Garcia
 */


using System.Numerics;
using System.Text;
using System.Text.Json;


namespace Messenger
{
    public class Program
    {
        
        private readonly KeyManger _keyManger = new KeyManger();
        private readonly WebClient _webClient = new WebClient();

        private bool ParseInput(string[] args)
        {
            if (args.Length == 0)
                return false;
                
            switch (args[0])
            {
                case "keyGen":
                    if (args.Length != 2 || !int.TryParse(args[1], out var keySize))
                    {
                        return false;
                    }
                    else
                    {
                        _keyManger.keyGen(keySize);
                    }
                    break;
                
                case "sendKey":
                    if (args.Length != 2)
                    {
                        return false;
                    }
                    else
                    {
                        // send public
                        // add email to private key email
                        // http://kayrun.cs.rit.edu:5000/Key/email

                        _webClient.SendKey(args[1]);
                        

                    }
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

        private void PrintUsage()
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



        public static void Main(string[] args)
        {

            var p = new Program();
            // var ws = new WebClient();   // init web client

            // await ws.Connect("http://kayrun.cs.rit.edu:5000/Key/jsb@cs.rit.edu");
            // Print error if 
            if (!p.ParseInput(args))
            {
                p.PrintUsage();
            }

            // var web = "AAAAAwEAAQAAAQB7w4yJG+kH5BXhL9lgeCxkNqKeIIyC0zzG0FYJu5/WVa7xCdXGSmG3pEEpyEPhe81L9zb1qWpnn" +
            //           "9yoiMPPawtDoZ26Um0LA/MAx/n4UdBENyWYd807+ex1h/uJ/GHgeZI/8yZ5LapCTNXaAwXvTfSY4OTG9hEgTJ6uK7cM11hn/q" +
            //           "K07EnH1beaGoj/FOATFPqpLkDaz/fOkRQIQr6F41ks0PIJXjzmMeIJdUhBsluJaU/pllHqjTDFk2uBOSQr5g0WFeCVLfss0E" +
            //           "Ybkbx3BsLtvThDgphBc98KOU2gx3o+Tm5U1oTT/tZdUjrWq8iPWzI+JMrG1RtZEVVeewOFT5sn";
            

            /*
             * private key emails: list of all emails that I have sent to sever using that private key
             * pub key email: NONE< don't touch
             */
        }
        
    }
    public class JsonKey
    {
        public string[]? Email { get; set; }
        public string? EncodedKey { get; set; }
        public bool IsPublic { get; set; }
    }
   


    public class WebClient
    {
        private readonly HttpClient _client = new HttpClient();
        
        public string MessageAddress => "http://kayrun.cs.rit.edu:5000/Message/email";
        public string KeyAddress => "http://kayrun.cs.rit.edu:5000/Key/email";

        private async Task Put(string destination, string message)
        {
            // send public
            // add email to private key email

            try
            {

                var content = new StringContent(message, Encoding.UTF8, "application/json");
                var response = await _client.PutAsync(destination, content);

            }
            // report err
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }

          
        }

        private async Task Get(string destination)
        {
            try
            {
                var response = await _client.GetAsync(destination);
            }
            // report err
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
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
        
        public async Task SendKey(string email)
        {
            // send public
            // add email to private key email
            // http://kayrun.cs.rit.edu:5000/Key/email
            
            // await _webClient.Put(_webClient.KeyAddress, _keyManager.GetJsonKey(true));
            
            
            
            
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