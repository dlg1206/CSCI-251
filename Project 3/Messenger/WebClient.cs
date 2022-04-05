/*
 * file: WebClient.cs
 * Description: Handles all client-server interactions
 * 
 * @author Derek Garcia
 */


using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Messenger;

/// <summary>
/// Json message to send to the server
/// </summary>
public class JsonMessage
{
    /// <summary>
    /// Getter / Setter for destination / source email
    /// </summary>
    public string? email { get; set; }
    
    /// <summary>
    /// Getter / Setter for message content
    /// </summary>
    public string? content { get; set; }
        
}


/// <summary>
/// Main class that handles server-client PUT / GET actions including:
///     sendKey, getKey, sendMsg, getMsg
/// </summary>
public class WebClient
    {
        private readonly HttpClient _client = new HttpClient();     // client that connects to server
        
        // Server access addresses
        private const string KeyAddress = "http://kayrun.cs.rit.edu:5000/Key/";
        private const string MessageAddress = "http://kayrun.cs.rit.edu:5000/Message/";

        private const string MediaType = "application/json";    // Media type for sending info to server
        
        // Json Obj Access fields
        private const string Email = "email";
        private const string Key = "key";
        private const string Content = "content";
        
        private const string Empty = "";                // String return values
        private const string KeyExtension = ".key";     // Key extension for storing keys locally
        
        
        /// <summary>
        /// Sends a key to the server
        /// </summary>
        /// <param name="keyManager">keyManager that handles key methods</param>
        /// <param name="email">email to 'sign' key with</param>
        public async Task SendKey(KeyManager keyManager, string email)
        {
    
            // Attempt to send key to server
            try
            {
                var signedPublicKey = keyManager.SignKey(true,email);   // sign key

                // break if error signing key
                if (signedPublicKey.Equals(Empty))
                    return;

                // Create content to send
                var content = new StringContent(
                    signedPublicKey, 
                    Encoding.UTF8, 
                    MediaType
                    );
               
                var response = await _client.PutAsync(KeyAddress + email, content);    // send to server
                
                // report key status
                Console.WriteLine(response.IsSuccessStatusCode ? "Key saved" : "Key was not saved");
                
                keyManager.SignKey(false, email);   // update local private key
            }
            // report error if one occurs
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        /// <summary>
        /// Gets a Public key from the server
        /// </summary>
        /// <param name="email">email of the public key to get</param>
        public async Task GetKey(string email)
        {
            // Attempt get key
            try
            {
                var jsonString = await _client.GetStringAsync(KeyAddress + email);  // get key

                // Report and break if no key was found
                if (jsonString.Equals(Empty))
                {
                    Console.WriteLine("No key was found for " + email);
                    return;
                }
                
                // Else store Public Key locally
                var jsonObj = JsonSerializer.Deserialize<JsonObject>(jsonString);

                var sw = File.CreateText(email + KeyExtension);
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

        
        /// <summary>
        /// Send a message to another user on the server
        /// </summary>
        /// <param name="keyManager">keyManager that handles key methods</param>
        /// <param name="email">email to send message to</param>
        /// <param name="plaintext">plaintext message to send</param>
        public async Task SendMsg(KeyManager keyManager, string email, string plaintext)
        {

            var jsonString = keyManager.GetPublicKey(email);    // get locally stored public key for the email
            
            // break if don't have public key for email
            if(jsonString.Equals(Empty))
                return;
            
            // Else attempt to convert jsonString to key
            var jsonObj = JsonSerializer.Deserialize<JsonObject>(jsonString);
            var base64Key = jsonObj?[Key];
            
            // break if key is null
            if(base64Key == null)
                return;
            
            var publicKey = new Key(base64Key.AsValue().ToString());    // convert key
            
            // build JsonMessage
            var jsonMsg = new JsonMessage
            {
                email = email,
                content = keyManager.Encrypt(publicKey, plaintext)
            };
            
            // attempt to PUT to server
            try
            {
                // Create content to send
                var content = new StringContent(
                    JsonSerializer.Serialize(jsonMsg), 
                    Encoding.UTF8, 
                    MediaType
                );
                
                var response = await _client.PutAsync(MessageAddress + email, content);    // send to server

                // report result
                Console.WriteLine(response.IsSuccessStatusCode ? "Message Written" : "Message was not written");
            }
            // Report Error
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        
        /// <summary>
        /// Gets a message from the server
        /// </summary>
        /// <param name="keyManager">keyManager that handles key methods</param>
        /// <param name="email">email to get the messages for</param>
        public async Task GetMsg(KeyManager keyManager, string email)
        {
            var base64PrivateKey = keyManager.GetPrivateKey(email);     // get locally stored private key
            
            // break if don't have private key for email
            if(base64PrivateKey.Equals(Empty))
                return;
            
            var privateKey = new Key(base64PrivateKey);     // concert to key
            
            // attempt GET msg from server
            try
            {
                var jsonString = await _client.GetStringAsync(MessageAddress + email);      // get from server
                
                // break if nothing found
                if (jsonString.Equals(Empty))
                    return;
                
                // else get Base64 message
                var jsonObj = JsonSerializer.Deserialize<JsonObject>(jsonString);
                var base64Content = jsonObj?[Content];
                
                // break if message is null
                if(base64Content == null)
                    return;
                
                // else decrypt and output plaintext
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