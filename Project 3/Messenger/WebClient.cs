﻿/*
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

public class JsonMessage
{
    public string email { get; set; }
    public string content { get; set; }
        
}
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
        

        public async Task SendKey(KeyManager keyManager, string email)
        {
    
            // Attempt to PUT
            try
            {
                // single email to public key send, 
                var content = new StringContent(
                    keyManager.AddEmail(true, email), 
                    Encoding.UTF8, "application/json"
                    );
               
                var response = await _client.PutAsync(KeyAddress + email, content);    // send to server

                Console.WriteLine(response.IsSuccessStatusCode ? "Key saved" : "Key was not saved");
            }
            // report error if one occurs
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
           
            keyManager.AddEmail(false, email);
        }

        public async Task GetKey(KeyManager keyManager, string email)
        {
            // Attempt get
            try
            {
                var jsonString = await _client.GetStringAsync(KeyAddress + email);

                if (jsonString == "")
                    return;
                
                var jsonObj = JsonSerializer.Deserialize<JsonObject>(jsonString);

                var sw = File.CreateText(email + ".key");
                sw.WriteLine(jsonObj);
                sw.Close();
                
            }
            // Report Error
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        public async Task SendMsg(KeyManager keyManager, string email, string plaintext)
        {
            string jsonString;
            try
            {
                jsonString = File.ReadAllText(email + ".key");
            }
            catch
            {
                Console.WriteLine("Key does not exist for " + email);
                return;
            }
            
            var jsonObj = JsonSerializer.Deserialize<JsonObject>(jsonString);

            var base64Key = jsonObj?["key"];
                
            if(base64Key == null)
                return;
            
            var publicKey = keyManager.Base64Decode(base64Key.AsValue().ToString());
            var jsonMsg = new JsonMessage
            {
                email = email,
                content = keyManager.Encrypt(publicKey, plaintext)
            };
            
            

            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(jsonMsg), 
                    Encoding.UTF8, "application/json"
                );
                // todo website address?
                var response = await _client.PutAsync(MessageAddress + email, content);    // send to server

                Console.WriteLine(response.IsSuccessStatusCode ? "Ok" : "failed");
            }
            // Report Error
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }



    }