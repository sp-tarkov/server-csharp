using Core.Utils;

namespace UnitTests.Tests.Utils;

[TestClass]
public class HashUtilTests
{
	// protected HashUtil _hashUtil = new(new RandomUtil());
	//
	// [TestMethod]
	// public void GenerateTest()
	// {
	// 	// Generate 100 MongoId's
	// 	for (var i = 0; i < 100; i++)
	// 	{
	// 		// Invalid mongoId character
	// 		var result = _hashUtil.Generate();
	// 	
	// 		// Invalid mongoId length
	// 		var test = _hashUtil.IsValidMongoId(result);
	// 	
	// 		Assert.AreEqual(
	// 			true, 
	// 			test,
	// 			$"IsValidMongoId() `{result}` is not a valid MongoId.");
	// 	}
	// }
	//
	// [TestMethod]
	// public void IsValidMongoIdTest()
	// {
	// 	// Invalid mongoId character
	// 	var ResultBadChar = _hashUtil.IsValidMongoId("677ddb67406e9918a0264bbz");
	// 	
	// 	Assert.AreEqual(
	// 		false, 
	// 		ResultBadChar,
	// 		"IsValidMongoId() `677ddb67406e9918a0264bbz` contains invalid char `z`, but result was true");
	// 	
	// 	// Invalid mongoId length
	// 	var resultBadLength = _hashUtil.IsValidMongoId("677ddb67406e9918a0264bbcc");
	// 	
	// 	Assert.AreEqual(
	// 		false, 
	// 		resultBadLength,
	// 		"IsValidMongoId() `677ddb67406e9918a0264bbcc` is 25 characters, but result was true");
	// 	
	// 	// Valid mongoId
	// 	var resultPass = _hashUtil.IsValidMongoId("677ddb67406e9918a0264bbc");
	// 	
	// 	Assert.AreEqual(
	// 		true, 
	// 		resultPass,
	// 		"IsValidMongoId() `677ddb67406e9918a0264bbc` is a valid mongoId, but result was false");
	// }
}
