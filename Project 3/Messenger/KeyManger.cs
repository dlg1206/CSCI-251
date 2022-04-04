using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Messenger;

public class Key
{
    private readonly BigInteger _nonce; // N
    private readonly BigInteger _prime; // E or D

    public Key(BigInteger nonce, BigInteger prime, bool isPublic)
    {
        _nonce = nonce;
        _prime = prime;
        IsPublic = isPublic;
    }

    public bool IsPublic { get; }

    public BigInteger GetNonce()
    {
        return _nonce;
    }

    public BigInteger GetPrime()
    {
        return _prime;
    }
}

public class JsonKey
{
    public string[]? Emails { get; set; }
    public string? EncodedKey { get; set; }
}

public class KeyManger
{
    private const int E = 5113;
    private const string PublicKey = "public.key";
    private const string PrivateKey = "private.key";

    private void StoreKey(Key key)
    {
        var jsonKey = new JsonKey
        {
            Emails = Array.Empty<string>(),
            EncodedKey = Base64Encode(key),
        };

        var fileName = key.IsPublic ? PublicKey : PrivateKey;

        using StreamWriter sw = File.CreateText(fileName);
        sw.WriteLine(JsonSerializer.Serialize(jsonKey));
        
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
    
    private string Base64Encode(Key key)
    {
        var E = key.GetPrime().ToByteArray();
        Array.Reverse(E);
        
        var e = BitConverter.GetBytes(E.Length);
        Array.Reverse(e);

        var N = key.GetNonce().ToByteArray();
        Array.Reverse(N);
        
        var n = BitConverter.GetBytes(N.Length);
        Array.Reverse(n);

        var combined = Array.Empty<byte>().Concat(e).Concat(E).Concat(n).Concat(N).ToArray();

        return Convert.ToBase64String(combined);
        
    }

    private Key Base64Decode(string encoding)
    {
        var keyBytes = Convert.FromBase64String(encoding);

        // convert 1st 4 bytes to 'e'
        var e = BitConverter.ToInt32(GetNBytes(keyBytes, 0, 4), 0);
        var E = new BigInteger(GetNBytes(keyBytes, 4, e));
        
        var n = BitConverter.ToInt32(GetNBytes(keyBytes, e+4, 4), 0);
        var N = new BigInteger(GetNBytes(keyBytes, e+8, n));

        return new Key(N, E, true);
    }
    
    public void KeyGen(int keySize)
    {
        var pg = new PrimeGen();
    
        var lenP = (int) ((int) (keySize / 2) + keySize * 0.2);

        var p = pg.FindPrime(lenP);
        var q = pg.FindPrime(keySize - lenP);

        var nonce = p * q;
        var r =  (p - 1) * (q - 1);

        var publicKey = new Key(nonce, new BigInteger(E), true);
        var privateKey = new Key(nonce, new BigInteger(E).ModInverse(r), false);

        StoreKey(publicKey);
        StoreKey(privateKey);
    }

    public void AddEmail(bool isPublic, string email)
    {
        var fileName = isPublic ? PublicKey : PrivateKey;
        
        var jsonObj = JsonSerializer.Deserialize<JsonObject>(File.ReadAllText(fileName));

        var emails = jsonObj?["Emails"];
        
        if(emails == null)
            return;

        // TODO check if email exists in email array
        if (!emails.ToString().Contains(email))
            emails.AsArray().Add(email);


        using var sw = File.CreateText(fileName);
        sw.WriteLine(JsonSerializer.Serialize(jsonObj));
        

    }

    public string GetJsonKey(bool isPublic)
    {
        var fileName = isPublic ? PublicKey : PrivateKey;
        // var foo = new JsonObject.Parse()
        return File.ReadAllText(fileName);
            

    }
    
   
    
    
}