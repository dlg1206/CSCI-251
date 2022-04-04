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

namespace Messenger;

/// <summary>
/// Key class used during key generation
/// </summary>
public class Key
{
    // Prime Value
    private readonly BigInteger _nonce;     // N
    private readonly BigInteger _prime;     // Public: E | Private: D

    
    /// <summary>
    /// Key Constructor, makes a key
    /// </summary>
    /// <param name="nonce">N value for the key</param>
    /// <param name="prime">E or D value for this key if its public or private respectivly</param>
    /// <param name="isPublic">States whether this is a public or private key</param>
    public Key(BigInteger nonce, BigInteger prime, bool isPublic)
    {
        _nonce = nonce;
        _prime = prime;
        IsPublic = isPublic;
    }

    /// <summary>
    /// Gets key public / private status
    /// </summary>
    public bool IsPublic { get; }

    /// <summary>
    /// Get N value
    /// </summary>
    /// <returns>N value</returns>
    public BigInteger GetNonce()
    {
        return _nonce;
    }

    
    /// <summary>
    /// Gets Prime value
    /// </summary>
    /// <returns>Prime value</returns>
    public BigInteger GetPrime()
    {
        return _prime;
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
    public string[]? email { get; set; }
    
    /// <summary>
    /// Getter-Setter for Base64 encoded string for this key
    /// </summary>
    public string? key { get; set; }
}

public class JsonPublicKey
{
    public string? email { get; set; }
    
    public string? key { get; set; }
}



/// <summary>
/// Main key manager class. Handles key generation, storage, encoding and decoding
/// </summary>
public class KeyManager
{
    private const int _E = 5113;     // Constant 'E' value chosen
   
    // key storage file names
    public string PublicKey => "public.key";
    public string PrivateKey => "private.key";

    /// <summary>
    /// Stores a given key locally
    /// </summary>
    /// <param name="key">key to store</param>
    /// <param name="fileName">name of the storage file</param>
    public void StoreKey(Key key, string fileName)
    {
        using var sw = File.CreateText(fileName);
        // covert it into json key
        if (key.IsPublic)
        {
            var jsonKey = new JsonPublicKey
            {
                email = "",
                key = Base64Encode(key),
            };
            sw.WriteLine(JsonSerializer.Serialize(jsonKey));
        }
        else
        {
            var jsonKey = new JsonPrivateKey
            {
                email = Array.Empty<string>(),
                key = Base64Encode(key),
            };
            sw.WriteLine(JsonSerializer.Serialize(jsonKey));
        }

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
    
    
    /// <summary>
    /// Encodes a key to a Base64 Encoding
    /// </summary>
    /// <param name="key">key to encode</param>
    /// <returns>Base64 encoding of the given key</returns>
    private string Base64Encode(Key key)
    {
        // Get byte arrays and set them to little Endian form
        
        var E = key.GetPrime().ToByteArray();
        Array.Reverse(E);
        
        var e = BitConverter.GetBytes(E.Length);
        Array.Reverse(e);

        var N = key.GetNonce().ToByteArray();
        Array.Reverse(N);
        
        var n = BitConverter.GetBytes(N.Length);
        Array.Reverse(n);

        // Combine results
        var combined = Array.Empty<byte>().Concat(e).Concat(E).Concat(n).Concat(N).ToArray();

        // convert to Base64
        return Convert.ToBase64String(combined);
        
    }

    
    /// <summary>
    /// Decodes a Base64 Encoded key into a key
    /// </summary>
    /// <param name="encoding">Base64 Encoding of a Key</param>
    /// <returns>Decoded Key</returns>
    public Key Base64Decode(string encoding)
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

        // Return new key
        return new Key(N, E, true);     // all decoded keys will be public
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
        var publicKey = new Key(nonce, new BigInteger(_E), true);
        var privateKey = new Key(nonce, new BigInteger(_E).ModInverse(r), false);

        // store each key
        StoreKey(publicKey, PublicKey);
        StoreKey(privateKey, PrivateKey);
    }

    
    /// <summary>
    /// Adds an email associated with a public or private key
    /// </summary>
    /// <param name="isPublic">is the key public</param>
    /// <param name="email">email to add</param>
    public void AddEmail(bool isPublic, string email)
    {
        var fileName = isPublic ? PublicKey : PrivateKey;   // get file to update
        
        // get JSON obj
        var jsonObj = JsonSerializer.Deserialize<JsonObject>(File.ReadAllText(fileName));

        // null reference check
        var emails = jsonObj?["Emails"];
        if(emails == null)
            return;

        // TODO check if email exists in email array
        if (!emails.ToString().Contains(email))
            emails.AsArray().Add(email);

        // update local key
        using var sw = File.CreateText(fileName);
        sw.WriteLine(JsonSerializer.Serialize(jsonObj));
    }

    public string Encrypt(Key publicKey, string plaintext)
    {
        var P = new BigInteger(Encoding.ASCII.GetBytes(plaintext));

        var ciphertext = BigInteger.ModPow(P, publicKey.GetPrime(), publicKey.GetNonce());

        return Convert.ToBase64String(ciphertext.ToByteArray());

    }
    
  
    
}