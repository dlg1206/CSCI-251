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
    public Key(BigInteger nonce, BigInteger prime)
    {
        Nonce = nonce;
        Prime = prime;
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

    public BigInteger Nonce { get; }
    
    public BigInteger Prime { get; }

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
    
    
    
    

    
    /// <summary>
    /// Decodes a Base64 Encoded key into a key
    /// </summary>
    /// <param name="encoding">Base64 Encoding of a Key</param>
    /// <returns>Decoded Key</returns>
    public JsonPublicKey Base64Decode(string encoding)
    {
        var keyBytes = Convert.FromBase64String(encoding);      // get initial bytes

        // convert 1st 4 bytes to 'e'
        var e = BitConverter.ToInt32(GetNBytes(keyBytes, 0, 4), 0);
        
        // convert 'e' bytes to E
        var E = new BigInteger(GetNBytes(keyBytes, 4, e));
        
        // get n
        var n = BitConverter.ToInt32(GetNBytes(keyBytes, e+4, 4), 0);
        
        // convert 'n' bytes to N
        var N = new BigInteger(GetNBytes(keyBytes, e+8, n));
        
        // key.SetValues(N, E);
        // Return new key
        return new JsonPublicKey
        {
            // publicKey.SetValues(nonce, new BigInteger(_E));
            email = "",
            key = encoding
        };   
    }
    
    
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
        var publicKey = new Key(nonce, _E);
        var privateKey = new Key(nonce, _E.ModInverse(r));

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
    public string AddEmail(bool isPublic, string email)
    {
        var fileName = isPublic ? PublicKey : PrivateKey;   // get file to update
        
        // get JSON obj
        var jsonObj = JsonSerializer.Deserialize<JsonObject>(File.ReadAllText(fileName));

        // null reference check
        var emails = jsonObj?["email"];
        if(emails == null)
            return "";
        
        if (!isPublic)
        {
            emails.AsArray().Add(email);
            // update local key
            using var sw = File.CreateText(fileName);
            sw.WriteLine(JsonSerializer.Serialize(jsonObj));
            return "";
        }
        
        jsonObj["email"] = JsonValue.Create(email);
        return JsonSerializer.Serialize(jsonObj);
        
        
    }

    public string Encrypt(JsonPublicKey publicKey, string plaintext)
    {
        var P = new BigInteger(Encoding.ASCII.GetBytes(plaintext));
        
        var keyBytes = Convert.FromBase64String(publicKey.key);      // get initial bytes

        // convert 1st 4 bytes to 'e'
        var e = BitConverter.ToInt32(GetNBytes(keyBytes, 0, 4), 0);
        
        // convert 'e' bytes to E
        var E = new BigInteger(GetNBytes(keyBytes, 4, e));
        
        // get n
        var n = BitConverter.ToInt32(GetNBytes(keyBytes, e+4, 4), 0);
        
        // convert 'n' bytes to N
        var N = new BigInteger(GetNBytes(keyBytes, e+8, n));

        var ciphertext = BigInteger.ModPow(P, E, N);
        
        return Convert.ToBase64String(ciphertext.ToByteArray());

    }

    public string GetPrivateKey(string email)
    {
        try
        {
            var jsonObj = JsonSerializer.Deserialize<JsonObject>(File.ReadAllText(PrivateKey));

            var emails = jsonObj?["email"];
            if(emails == null)
                return "";

            var haveKey = false;
            // todo better way to check if in private
            foreach (var storedEmail in emails.AsArray())
            {
                
                haveKey = storedEmail.ToString().Equals(email);

                if (haveKey)
                    break;
            }
            return haveKey ? jsonObj["key"].ToString() : "";
            
        }
        catch
        {
            Console.WriteLine("No private key found");
            throw;
        }
 
    }
    
}