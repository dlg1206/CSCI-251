/*
 * file: KeyManager.cs
 * Description: Contains the KeyManager class and all key sub classes it depends on
 * 
 * @author Derek Garcia
 */

using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Messenger;

public class Key
{
    public Key(BigInteger prime, BigInteger nonce)
    {
        
        Prime = prime;
        Nonce = nonce;
    }

    public Key(string base64Encoding)
    {
        DecodeFromBase64(base64Encoding);
    }
    
    /// <summary>
    /// Gets a set amount of bytes from a given byte array
    /// </summary>
    /// <param name="source">Byte array to take section from</param>
    /// <param name="startIndex">Index to start at</param>
    /// <param name="numBytes">Number of bytes to copy</param>
    /// <returns></returns>
    private byte[] GetNBytes(byte[] source, int startIndex, int numBytes)
    {
        var section = new byte[numBytes];   // init copy array

        // copy section
        Array.Copy(source, startIndex, section, 0, numBytes);

        // always return little endian form
        if (BitConverter.IsLittleEndian)
            Array.Reverse(section);
            
        return section;
    }
    
    private string EncodeToBase64()
    {
        // Get byte arrays and set them to little Endian form
        
        var E = Prime.ToByteArray();
        Array.Reverse(E);
        
        var e = BitConverter.GetBytes(E.Length);
        Array.Reverse(e);

        var N = Nonce.ToByteArray();
        Array.Reverse(N);
        
        var n = BitConverter.GetBytes(N.Length);
        Array.Reverse(n);

        // Combine results
        var combined = Array.Empty<byte>().Concat(e).Concat(E).Concat(n).Concat(N).ToArray();

        // convert to Base64
        return Convert.ToBase64String(combined);
        
    }
    
    private void DecodeFromBase64(string encoding)
    {
        var keyBytes = Convert.FromBase64String(encoding);      // get initial bytes

        // convert 1st 4 bytes to 'e'
        var e = BitConverter.ToInt32(GetNBytes(keyBytes, 0, 4), 0);
        
        // convert 'e' bytes to E
        Prime = new BigInteger(GetNBytes(keyBytes, 4, e));
        
        // get n
        var n = BitConverter.ToInt32(GetNBytes(keyBytes, e+4, 4), 0);
        
        // convert 'n' bytes to N
        Nonce = new BigInteger(GetNBytes(keyBytes, e+8, n));
        
        
    }

    public BigInteger Nonce { get; private set; }
    
    public BigInteger Prime { get; private set; }

    public JsonPublicKey ToPublicKey()
    {
        return new JsonPublicKey
        {
            email = "",
            key = EncodeToBase64()
        };
    }

    public JsonPrivateKey ToPrivateKey()
    {
        return new JsonPrivateKey
        {
            email = Array.Empty<string>(),
            key = EncodeToBase64()
        };
    }
}

/// <summary>
/// Json interpretation of the key
/// </summary>
public class JsonPrivateKey
{
    /// <summary>
    /// Getter-Setter for Emails associated with this key
    /// </summary>
    public string[] email { get; set; }
    
    /// <summary>
    /// Getter-Setter for Base64 encoded string for this key
    /// </summary>
    public string key { get; set; }
}

public class JsonPublicKey
{
    public string email { get; set; }
    
    public string key { get; set; }
}



/// <summary>
/// Main key manager class. Handles key generation, storage, encoding and decoding
/// </summary>
public class KeyManager
{
    private readonly BigInteger _E = new BigInteger(5113);     // Constant 'E' value chosen
   
    // key storage file names
    public string PublicKey => "public.key";
    public string PrivateKey => "private.key";

    


    /// <summary>
    /// Generates a public-private key pair and stores them locally
    /// </summary>
    /// <param name="keySize">size of the key to generate</param>
    public void KeyGen(int keySize)
    {
        var pg = new PrimeGen();
        
        // find length of p
        var lenP = (int) ((int) (keySize / 2) + keySize * 0.2);

        // generate p and q
        var p = pg.FindPrime(lenP);
        var q = pg.FindPrime(keySize - lenP);

        // calculate n and r
        var nonce = p * q;
        var r =  (p - 1) * (q - 1);

        // create respective keys
        var publicKey = new Key( _E, nonce);
        var privateKey = new Key(_E.ModInverse(r), nonce);

        // store values
        using var sw = File.CreateText(PublicKey);
        sw.WriteLine(JsonSerializer.Serialize(publicKey.ToPublicKey()));
        sw.Close();
        
        var sw2 = File.CreateText(PrivateKey);
        sw2.WriteLine(JsonSerializer.Serialize(privateKey.ToPrivateKey()));
        sw2.Close();
      
    }

    
    /// <summary>
    /// Adds an email associated with a public or private key
    /// </summary>
    /// <param name="isPublic">is the key public</param>
    /// <param name="email">email to add</param>
    public string SignKey(bool isPublic, string email)
    {
        var fileName = isPublic ? PublicKey : PrivateKey;
        try
        {
            
            var jsonObj = JsonSerializer.Deserialize<JsonObject>(File.ReadAllText(fileName));
            
            if (jsonObj != null)
            {
                if (isPublic)
                {
                    jsonObj["email"] = JsonValue.Create(email);
                }
                else
                {
                    jsonObj["email"]?.AsArray().Add(email);
                    // update local key
                    using var sw = File.CreateText(fileName);
                    sw.WriteLine(JsonSerializer.Serialize(jsonObj));
                    sw.Close();
                }
                return JsonSerializer.Serialize(jsonObj);
            }
            
        }
        catch
        {
            Console.WriteLine(fileName + " does not exist locally and cannot be signed");
        }
        return "";
    }

    public string Encrypt(Key publicKey, string plaintext)
    {
        var P = new BigInteger(Encoding.ASCII.GetBytes(plaintext));

        var C = BigInteger.ModPow(P, publicKey.Prime, publicKey.Nonce);
        
        return Convert.ToBase64String(C.ToByteArray());

    }

    public string Decrypt(Key privateKey, string ciphertext)
    {
        var contentBytes = Convert.FromBase64String(ciphertext);

        var C = new BigInteger(contentBytes);

        var P = BigInteger.ModPow(C, privateKey.Prime, privateKey.Nonce);

        return Encoding.ASCII.GetString(P.ToByteArray());
    }

    public string GetPublicKey(string email)
    {
        try
        {
            return File.ReadAllText(email + ".key");
        }
        catch
        {
            Console.WriteLine("Key does not exist for " + email);
        }

        return "";
    }

    public string GetPrivateKey(string email)
    {
        try
        {
            var jsonObj = JsonSerializer.Deserialize<JsonObject>(File.ReadAllText(PrivateKey));
            
            if(jsonObj?["email"] == null)
                return "";

            var haveKey = false;
            // todo better way to check if in private
            foreach (var storedEmail in jsonObj["email"]?.AsArray()!)
            {
                if(storedEmail != null)
                    haveKey = storedEmail.ToString().Equals(email);

                if (haveKey)
                    break;
            }

            if (haveKey && jsonObj["key"] != null)
                return jsonObj["key"]?.ToString()!;

            Console.WriteLine("Private key for " + email + " not found locally");

        }
        catch
        {
            Console.WriteLine("No private key found");
        }

        return "";
    }
    
}