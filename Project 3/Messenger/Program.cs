/*
 * Project 2
 * Finds Primes of a given bit size
 * 
 * @author Derek Garcia
 */


using System.Net;
using System.Text.Json;

namespace Messenger
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var ws = new WebClient();   // init web client

            await ws.Connect("http://kayrun.cs.rit.edu:5000/Key/jsb@cs.rit.edu");
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
            var test = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            
            foreach (var foo in test)
            {
                Console.WriteLine("Key: " + foo.Key);
                Console.WriteLine("val: " + foo.Value);
                Console.WriteLine();
            }
            
        }
            
    }
    
}