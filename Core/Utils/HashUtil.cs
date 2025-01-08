using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Core.Annotations;

namespace Core.Utils;

[Injectable(InjectionType.Singleton)]
public class HashUtil
{
    private readonly Regex MongoIdRegex = new("^[a-fA-F0-9]{24}$");

    private readonly RandomUtil _randomUtil;

    public HashUtil(RandomUtil randomUtil)
    {
        _randomUtil = randomUtil;
    }

    /// <summary>
    /// Create a 24 character MongoId
    /// </summary>
    /// <returns>24 character objectId</returns>
    public string Generate()
    {
        var objectId = new byte[12];

        // Time stamp (4 bytes)
        var timestamp = BitConverter.GetBytes((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        // Convert to big-endian
        Array.Reverse(timestamp);
        Array.Copy(timestamp, 0, objectId, 0, 4);

        // Random value (5 bytes)
        var randomValue = new byte[5];
        _randomUtil.Random.NextBytes(randomValue);
        Array.Copy(randomValue, 0, objectId, 4, 5);

        // Incrementing counter (3 bytes)
        // 24-bit counter
        var counter = BitConverter.GetBytes(_randomUtil.GetInt(0, 16777215));
        Array.Reverse(counter);
        Array.Copy(counter, 0, objectId, 9, 3);

        return Convert.ToHexStringLower(objectId);
    }

    /// <summary>
    /// is the passed in string a valid mongo id
    /// </summary>
    /// <param name="stringToCheck">String to check</param>
    /// <returns>True when string is a valid mongo id</returns>
    public bool IsValidMongoId(string stringToCheck)
    {
        return MongoIdRegex.IsMatch(stringToCheck);
    }

    public string GenerateMd5ForData(string data)
    {
        return GenerateHashForData(HashingAlgorithm.MD5, data);
    }

    public string GenerateSha1ForData(string data)
    {
        return GenerateHashForData(HashingAlgorithm.SHA1, data);
    }

    public string GenerateCrc32ForData(string data)
    {
        // TODO: Could not find a ms way of doing this.
        // May need a custom impl to avoid an external lib. - CJ
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a hash for the data parameter
    /// </summary>
    /// <param name="algorithm">algorithm to use to hash</param>
    /// <param name="data">data to be hashed</param>
    /// <returns>hash value</returns>
    /// <exception cref="NotImplementedException">thrown if the provided algorithm is not implemented</exception>>
    public string GenerateHashForData(HashingAlgorithm algorithm, string data)
    {
        switch (algorithm)
        {
            case HashingAlgorithm.MD5:
                var md5HashData = MD5.HashData(Encoding.UTF8.GetBytes(data));
                return Convert.ToHexString(md5HashData).Replace("-", string.Empty);

            case HashingAlgorithm.SHA1:
                var sha1HashData = SHA1.HashData(Encoding.UTF8.GetBytes(data));
                return Convert.ToHexString(sha1HashData).Replace("-", string.Empty);
        }

        throw new NotImplementedException("Provided hash algorithm is not supported.");
    }

    /// <summary>
    /// Generates an account ID for a profile
    /// </summary>
    /// <returns>Generated account ID</returns>
    public int GenerateAccountId()
    {
        const int min = 1000000;
        const int max = 1999999;

        var random = new Random();

        return random.Next() * (max - min + 1) + min;
    }
}

public enum HashingAlgorithm
{
    MD5,
    SHA1
}