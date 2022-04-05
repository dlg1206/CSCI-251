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
using System.Text.Json.Serialization;

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
                var signedPublicKey = keyManager.SignKey(true,email);

                if (signedPublicKey.Equals(""))
                    return;

                // single email to public key send, 
                var content = new StringContent(
                    signedPublicKey, 
                    Encoding.UTF8, "application/json"
                    );
               
                var response = await _client.PutAsync(KeyAddress + email, content);    // send to server

                Console.WriteLine(response.IsSuccessStatusCode ? "Key saved" : "Key was not saved");
                
                keyManager.SignKey(false, email);
            }
            // report error if one occurs
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
           
            
        }

        public async Task GetKey(string email)
        {
            // Attempt get
            try
            {
                var jsonString = await _client.GetStringAsync(KeyAddress + email);

                if (jsonString == "")
                {
                    Console.WriteLine("No key was found for " + email);
                    return;
                }
                
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

            var jsonString = keyManager.GetPublicKey(email);
            
            if(jsonString.Equals(""))
                return;
            
            var jsonObj = JsonSerializer.Deserialize<JsonObject>(jsonString);

            var base64Key = jsonObj?["key"];
            if(base64Key == null)
                return;
            
            var publicKey = new Key(base64Key.AsValue().ToString());
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
                var response = await _client.PutAsync(MessageAddress + email, content);    // send to server

                Console.WriteLine(response.IsSuccessStatusCode ? "Message Written" : "Message was not written");
            }
            // Report Error
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        public async Task GetMsg(KeyManager keyManager, string email)
        {
            var base64PrivateKey = keyManager.GetPrivateKey(email);
            
            if(base64PrivateKey == "")
                return;
            var privateKey = new Key(base64PrivateKey);
            try
            {
                var jsonString = await _client.GetStringAsync(MessageAddress + email);

                if (jsonString == "")
                    return;
                
                var jsonObj = JsonSerializer.Deserialize<JsonObject>(jsonString);

                var base64Content = jsonObj?["content"];
                if(base64Content == null)
                    return;
                

                var plaintext = keyManager.Decrypt(privateKey, base64Content.AsValue().ToString());

                Console.WriteLine(plaintext);

            }
            // Report Error
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
            
            
            
        }



    }