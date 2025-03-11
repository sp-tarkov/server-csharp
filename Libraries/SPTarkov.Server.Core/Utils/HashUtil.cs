using System.IO.Hashing;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Utils;

[Injectable(InjectionType.Singleton)]
public class HashUtil(RandomUtil _randomUtil)
{
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
}

public enum HashingAlgorithm
{
    MD5,
    SHA1
}
