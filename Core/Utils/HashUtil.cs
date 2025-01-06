using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Core.Annotations;

namespace Core.Utils;

[Injectable(InjectionType.Singleton)]
public partial class HashUtil
{
	/// <summary>
	/// Create a 24 character id using the sha256 algorithm + current timestamp
	/// </summary>
	/// <returns>24 character hash</returns>
	public static string Generate()
	{
		throw new NotImplementedException();
	}
	
	/// <summary>
	/// is the passed in string a valid mongo id
	/// </summary>
	/// <param name="stringToCheck">String to check</param>
	/// <returns>True when string is a valid mongo id</returns>
	public static bool IsValidMongoId(string stringToCheck)
	{
		return MongoIdRegex().IsMatch(stringToCheck);
	}
	
	public static string GenerateMd5ForData(string data)
	{
		return GenerateHashForData(HashingAlgorithm.MD5, data);
	}

	public static string GenerateSha1ForData(string data)
	{
		return GenerateHashForData(HashingAlgorithm.SHA1, data);
	}
	
	public static string GenerateCrc32ForData(string data)
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
	public static string GenerateHashForData(HashingAlgorithm algorithm, string data)
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
	public static int GenerateAccountId()
	{
		const int min = 1000000;
		const int max = 1999999;
		
		var random = new Random();
		
		return random.Next() * (max - min + 1) + min;
	}
	
	[GeneratedRegex("^[a-fA-F0-9]{24}$", RegexOptions.IgnoreCase, "en")]
	private static partial Regex MongoIdRegex();
}

public enum HashingAlgorithm
{
	MD5,
	SHA1,
}