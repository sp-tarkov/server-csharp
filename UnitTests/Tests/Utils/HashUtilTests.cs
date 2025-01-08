using Core.Utils;

namespace UnitTests.Tests.Utils;

[TestClass]
public class HashUtilTests
{
	private readonly HashUtil _hashUtil = new();
	
	[TestMethod]
	public void IsValidMongoIdTest()
	{
		// Invalid mongoId character
		var ResultBadChar = _hashUtil.IsValidMongoId("677ddb67406e9918a0264bbz");
		
		Assert.AreEqual(
			false, 
			ResultBadChar,
			"IsValidMongoId() `677ddb67406e9918a0264bbz` contains invalid char `z`, but result was true");
		
		// Invalid mongoId length
		var resultBadLength = _hashUtil.IsValidMongoId("677ddb67406e9918a0264bbcc");
		
		Assert.AreEqual(
			false, 
			resultBadLength,
			"IsValidMongoId() `677ddb67406e9918a0264bbcc` is 25 characters, but result was true");
		
		// Valid mongoId
		var resultPass = _hashUtil.IsValidMongoId("677ddb67406e9918a0264bbc");
		
		Assert.AreEqual(
			true, 
			resultPass,
			"IsValidMongoId() `677ddb67406e9918a0264bbc` is a valid mongoId, but result was false");
	}
}