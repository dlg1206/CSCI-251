using System.Numerics;
using System.Text;
using System.Text.Json;

namespace Messenger;

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
        
        public async Task SendKey(string email, KeyManger keyManager)
        {
            // send public
            // add email to private key email
            // http://kayrun.cs.rit.edu:5000/Key/email
            
            // await Put(KeyAddress, keyManager.GetJsonKey(true));
            
            keyManager.AddEmail(true, email);
            
            
            
            
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