using System.IO.Hashing;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using SptCommon.Annotations;

namespace Core.Utils;

[Injectable(InjectionType.Singleton)]
public partial class HashUtil(RandomUtil _randomUtil)
{
    /// <summary>
    ///     Create a 24 character MongoId
    /// </summary>
    /// <returns>24 character objectId</returns>
    public string Generate()
    {
        // Allocate a span directly onto the stack, will dispose whenever we finished running
        // Span is recommended to work with stackalloc and we can use stackalloc here because we don't do anything with this afterwards
        Span<byte> objectId = stackalloc byte[12];

        // Time stamp (4 bytes)
        var timestamp = (int) DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        // Convert to big-endian
        objectId[0] = (byte) (timestamp >> 24);
        objectId[1] = (byte) (timestamp >> 16);
        objectId[2] = (byte) (timestamp >> 8);
        objectId[3] = (byte) timestamp;

        // Random value (5 bytes)
        _randomUtil.Random.NextBytes(objectId.Slice(4, 5));

        // Incrementing counter (3 bytes)
        // 24-bit counter
        var counter = _randomUtil.GetInt(0, 16777215);
        objectId[9] = (byte) (counter >> 16);
        objectId[10] = (byte) (counter >> 8);
        objectId[11] = (byte) counter;

        return Convert.ToHexStringLower(objectId);
    }

    /// <summary>
    ///     is the passed in string a valid mongo id
    /// </summary>
    /// <param name="stringToCheck">String to check</param>
    /// <returns>True when string is a valid mongo id</returns>
    public bool IsValidMongoId(string stringToCheck)
    {
        return MongoIdRegex().IsMatch(stringToCheck);
    }

    public string GenerateMd5ForData(string data)
    {
        return GenerateHashForData(HashingAlgorithm.MD5, data);
    }

    public string GenerateSha1ForData(string data)
    {
        return GenerateHashForData(HashingAlgorithm.SHA1, data);
    }

    public uint GenerateCrc32ForData(string data)
    {
        return Crc32.HashToUInt32(new ArraySegment<byte>(Encoding.UTF8.GetBytes(data)));
    }

    /// <summary>
    ///     Create a hash for the data parameter
    /// </summary>
    /// <param name="algorithm">algorithm to use to hash</param>
    /// <param name="data">data to be hashed</param>
    /// <returns>hash value</returns>
    /// <exception cref="NotImplementedException">thrown if the provided algorithm is not implemented</exception>
    /// >
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

        throw new NotImplementedException($"Provided hash algorithm: {algorithm} is not supported.");
    }

    /// <summary>
    ///     Generates an account ID for a profile
    /// </summary>
    /// <returns>Generated account ID</returns>
    public int GenerateAccountId()
    {
        const int min = 1000000;
        const int max = 1999999;

        return _randomUtil.Random.Next(min, max + 1);
    }

    [GeneratedRegex("^[a-fA-F0-9]{24}$")]
    private static partial Regex MongoIdRegex();
}

public enum HashingAlgorithm
{
    MD5,
    SHA1
}
