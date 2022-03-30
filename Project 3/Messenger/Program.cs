﻿/*
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