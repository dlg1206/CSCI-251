/*
 * file: WebClient.cs
 * Description: Handles all client-server interactions
 * 
 * @author Derek Garcia
 */


using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Messenger;

public class WebClient
    {
        private readonly HttpClient _client = new HttpClient();     // client that connects to server

        /// <summary>
        /// Getter for Message address
        /// </summary>
        public string MessageAddress => "http://kayrun.cs.rit.edu:5000/Message/";
        
        /// <summary>
        /// Getter for Key address
        /// </summary>
        public string KeyAddress => "http://kayrun.cs.rit.edu:5000/Key/";
        

        public async Task SendKey(string email, KeyManger keyManager)
        {
      
            // Attempt to PUT
            try
            {

                var content = new StringContent(
                    File.ReadAllText(keyManager.PublicKey), 
                    Encoding.UTF8, "application/json"
                    );
                
                await _client.PutAsync(KeyAddress + email, content);    // send to server

            }
            // report error if one occurs
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
           
            keyManager.AddEmail(false, email);
        }

        public async Task GetKey(string email, KeyManger keyManger)
        {
            // Attempt get
            try
            {
                var jsonString = await _client.GetStringAsync(KeyAddress + email);

                if (jsonString == "")
                    return;
                
                var jsonObj = JsonSerializer.Deserialize<JsonObject>(jsonString);

                var base64Key = jsonObj?["key"];
                
                if(base64Key == null)
                    return;

                var key = keyManger.Base64Decode(base64Key.AsValue().ToString());

              
                keyManger.StoreKey(key, email + ".key");
                

            }
            // Report Error
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
            
            
            
        }

       
            
    }