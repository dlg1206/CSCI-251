/*
 * Project 2
 * Finds Primes of a given bit size
 * 
 * @author Derek Garcia
 */


using System.Net;

namespace Messenger
{
    public static class Program
    {
        // HttpClient is intended to be instantiated once per application, rather than per-use. See Remarks.
        public static async Task Main(string[] args)
        {
            var ws = new WebClient();

            await ws.Connect("http://kayrun.cs.rit.edu:5000/Key/jsb@cs.rit.edu");
        }
        
    }
    
    public class WebClient
    {
        readonly HttpClient Client = new HttpClient();


        public async Task Connect(string url)
        {
            try	
            {
                HttpResponseMessage response = await Client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                Console.WriteLine(responseBody);
            }
            catch(HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");	
                Console.WriteLine("Message :{0} ",e.Message);
            }
        }
            
    }
    
}