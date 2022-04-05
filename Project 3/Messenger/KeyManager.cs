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
/// Enhanced Key storage class that holds the Prime / Nonce values
/// and can encode / decode itself
/// </summary>
public class Key
{
    /// <summary>
    /// Numeric constructor. Create Key when Prime and Nonce are known
    /// </summary>
    /// <param name="prime">E or D primes for public / private keys respectively</param>
    /// <param name="nonce">N prime value</param>
    public Key(BigInteger prime, BigInteger nonce)
    {
        Prime = prime;
        Nonce = nonce;
    }

    /// <summary>
    /// Base64 Constructor / DecodeFromBase64. Decodes a Base64 Encoding to get Prime and Nonce value,
    /// which is then used to store in the key
    /// </summary>
    /// <param name="base64Encoding">Base64 Encoding of the Key</param>
    public Key(string base64Encoding)
    {
        var keyBytes = Convert.FromBase64String(base64Encoding);      // get initial bytes

        // convert 1st 4 bytes to 'e'
        var e = BitConverter.ToInt32(GetNBytes(keyBytes, 0, 4), 0);
        
        // convert 'e' bytes to E
        Prime = new BigInteger(GetNBytes(keyBytes, 4, e));
        
        // get n
        var n = BitConverter.ToInt32(GetNBytes(keyBytes, e+4, 4), 0);
        
        // convert 'n' bytes to N
        Nonce = new BigInteger(GetNBytes(keyBytes, e+8, n));
    }
    
    
    /// <summary>
    /// Gets a set amount of bytes from a given byte array
    /// </summary>
    /// <param name="source">Byte array to take section from</param>
    /// <param name="startIndex">Index to start at</param>
    /// <param name="numBytes">Number of bytes to copy</param>
    /// <returns>Sub byte array of the source</returns>
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
    /// Encodes this key to a Base64 Encoding
    /// </summary>
    /// <returns>Base64 Encoding of this Key</returns>
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

    
    /// <summary>
    /// Getter for Prime (E for Public, D for Private) value
    /// </summary>
    public BigInteger Prime { get; }
    
    
    /// <summary>
    /// Getter for Nonce Value
    /// </summary>
    public BigInteger Nonce { get; }

    
    /// <summary>
    /// Convert this key to a JsonPublicKey
    /// </summary>
    /// <returns>new JsonPublicKey</returns>
    public JsonPublicKey ToPublicKey()
    {
        return new JsonPublicKey
        {
            email = "",
            key = EncodeToBase64()
        };
    }

    
    /// <summary>
    /// Convert this key to a JsonPrivateKey
    /// </summary>
    /// <returns>new JsonPrivateKey</returns>
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
/// Json interpretation of a Public Key
/// </summary>
public class JsonPublicKey
{
    /// <summary>
    /// Getter-Setter for email 'signature' associated with this key
    /// </summary>
    public string email { get; set; }
    
    /// <summary>
    /// Getter-Setter for Base64 encoded string for this key
    /// </summary>
    public string key { get; set; }
}


/// <summary>
/// Json interpretation of a Private Key
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



/// <summary>
/// Main key manager class. Handles generation, storage, encoding / decoding,
/// and encryption / decryption of keys
/// </summary>
public class KeyManager
{
    private readonly BigInteger _E = new BigInteger(5113);     // Constant 'E' value chosen
   
    // key storage file names
    private const string PublicKey = "public.key";
    private const string PrivateKey = "private.key";
    
    // Json Obj Access fields
    private const string Email = "email";
    private const string Key = "key";
    
    private const string Empty = "";                // String return values
    private const string KeyExtension = ".key";     // Key extension for storing keys locally

    
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

        // todo using stream writer correct?
        // store values
        using var sw1 = File.CreateText(PublicKey);
        sw1.WriteLine(JsonSerializer.Serialize(publicKey.ToPublicKey()));
        sw1.Close();
        
        using var sw2 = File.CreateText(PrivateKey);
        sw2.WriteLine(JsonSerializer.Serialize(privateKey.ToPrivateKey()));
        sw2.Close();
    }

    
    /// <summary>
    ///  'Signs' a public or private key with a given email
    /// </summary>
    /// <param name="isPublic">is the key public</param>
    /// <param name="email">email to add</param>
    /// <returns>'signed' key as Json string</returns>
    public string SignKey(bool isPublic, string email)
    {
        var fileName = isPublic ? PublicKey : PrivateKey;   // get correct file name 
        
        // attempt to get key and sign it
        try
        {
            var jsonObj = JsonSerializer.Deserialize<JsonObject>(File.ReadAllText(fileName));   // get key
            
            // access if key exists
            if (jsonObj != null)
            {
                // If public, sign a copy
                if (isPublic)
                {
                    jsonObj[Email] = JsonValue.Create(email);
                }
                // If private, sign private key and update the local private key
                else
                {
                    jsonObj[Email]?.AsArray().Add(email);   // add email
                    
                    // update local key
                    using var sw = File.CreateText(fileName);
                    sw.WriteLine(JsonSerializer.Serialize(jsonObj));
                    sw.Close();
                }
                return JsonSerializer.Serialize(jsonObj);   // return result
            }
            
        }
        // catch if couldn't open file
        catch
        {
            Console.WriteLine(fileName + " does not exist locally and cannot be signed");
        }
        return Empty;   // return nothing if jsonObj was null
    }

    /// <summary>
    /// Encrypts plaintext using a public key
    /// </summary>
    /// <param name="publicKey">public key to encrypt with</param>
    /// <param name="plaintext">plaintext to encrypt</param>
    /// <returns>Base64 encoded string of the ciphtertext</returns>
    public string Encrypt(Key publicKey, string plaintext)
    {
        var P = new BigInteger(Encoding.ASCII.GetBytes(plaintext));

        // C = (P^E) mod N
        var C = BigInteger.ModPow(P, publicKey.Prime, publicKey.Nonce);
        
        return Convert.ToBase64String(C.ToByteArray());
    }

    
    /// <summary>
    /// Decrypts a Base64 Encoded ciphertext to plaintext using
    /// a private key
    /// </summary>
    /// <param name="privateKey">private key to decrypt with</param>
    /// <param name="ciphertext">Base64 encoded ciphertext to decrypt</param>
    /// <returns>Plaintext string</returns>
    public string Decrypt(Key privateKey, string ciphertext)
    {
        var contentBytes = Convert.FromBase64String(ciphertext);    // convert from Base64

        var C = new BigInteger(contentBytes);

        // P = (C^D) mod N
        var P = BigInteger.ModPow(C, privateKey.Prime, privateKey.Nonce);

        return Encoding.ASCII.GetString(P.ToByteArray());   // convert bytes to ASCII string
    }

    
    /// <summary>
    /// Get the JSON string of a locally stored Public Key
    /// </summary>
    /// <param name="email">email associated with the public key</param>
    /// <returns>Json string of Public Key, if one exists</returns>
    public string GetPublicKey(string email)
    {
        // attempt to return stored key contents
        try
        {
            return File.ReadAllText(email + KeyExtension);
        }
        // report if not found
        catch
        {
            Console.WriteLine("Key does not exist for " + email);
        }

        return Empty;   // no public key was found
    }

    /// <summary>
    /// Get the JSON string of a locally stored Private Key
    /// </summary>
    /// <param name="email">email associated with the private key</param>
    /// <returns>Json string of Private Key, if one exists</returns>
    public string GetPrivateKey(string email)
    {
        // attempt to return stored key
        try
        {
            var jsonObj = JsonSerializer.Deserialize<JsonObject>(File.ReadAllText(PrivateKey));
            
            // Check if Email isn't null
            if(jsonObj?[Email] == null)
                return Empty;

            // Ensure have private key for the given email
            var haveKey = false;
            // todo better way to check if in private
            foreach (var storedEmail in jsonObj[Email]?.AsArray()!)
            {
                if(storedEmail != null)
                    haveKey = storedEmail.ToString().Equals(email);

                if (haveKey)
                    break;
            }
            
            // If have private key for the given email, return json string
            if (haveKey && jsonObj[Key] != null)
                return jsonObj[Key]?.ToString()!;

            // Else report not found
            Console.WriteLine("Private key for " + email + " not found locally");

        }
        // No local private exists
        catch
        {
            Console.WriteLine("No private key found");
        }

        return Empty;   // no private key or no private key for the given email
    }
    
}