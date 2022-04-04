/*
 * file: WebClient.cs
 * Description: Handles all client-server interactions
 * 
 * @author Derek Garcia
 */


using System.Numerics;
using System.Text;
using System.Text.Json;

namespace Messenger;

public class WebClient
    {
        private readonly HttpClient _client = new HttpClient();     // client that connects to server

        /// <summary>
        /// Getter for Message address
        /// </summary>
        public string MessageAddress => "http://kayrun.cs.rit.edu:5000/Message/email";
        
        /// <summary>
        /// Getter for Key address
        /// </summary>
        public string KeyAddress => "http://kayrun.cs.rit.edu:5000/Key/email";

        
        /// <summary>
        /// PUT / POST method to server
        /// </summary>
        /// <param name="destination">Address to PUT to</param>
        /// <param name="message">Information to PUT</param>
        private async Task Put(string destination, string message)
        {
            // Attempt to PUT
            try
            {

                var content = new StringContent(message, Encoding.UTF8, "application/json");
                await _client.PutAsync(destination, content);    // send to server

            }
            // report error if one occurs
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        /// <summary>
        /// GET method to server
        /// </summary>
        /// <param name="destination">Address to GET from</param>
        private async Task Get(string destination)
        {
            // Attempt get
            try
            {
                await _client.GetAsync(destination);
            }
            // Report Error
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
            // await Put(KeyAddress, keyManager.GetJsonKey(true));
            keyManager.AddEmail(false, email);
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