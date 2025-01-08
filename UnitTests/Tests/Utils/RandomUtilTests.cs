using Core.Utils;

namespace UnitTests.Tests.Utils;

[TestClass]
public sealed class RandomUtilTests
{
	private readonly RandomUtil _randomUtil = new();
	
	[TestMethod]
	public void GetIntTest()
	{
		// Run 100 test cases
		for (var i = 0; i < 100; i++)
		{
			var result = _randomUtil.GetInt(0, 10);

			if (result < 0 || result > 10)
			{
				Assert.Fail($"GetInt(0, 10) out of range. Expected range [0, 10] but was {result}.");
			}
		}
	}
	
	[TestMethod]
	public void GetIntExTest()
	{
		// Run 100 test cases
		for (var i = 0; i < 100; i++)
		{
			var result = _randomUtil.GetIntEx(10);

			if (result < 1 || result > 9)
			{
				Assert.Fail($"GetInt(10) out of range. Expected range [1, 9] but was {result}.");
			}
		}
	}
	
	[TestMethod]
	public void GetFloatTest()
	{
		// Run 100 test cases
		for (var i = 0; i < 100; i++)
		{
			var result = _randomUtil.GetFloat(0f, 10f);

			if (result < 0f || result >= 9f)
			{
				Assert.Fail($"GetFloat(0f, 10f) out of range. Expected range [0.0f, 9.999f] but was {result}.");
			}
		}
	}
	
	[TestMethod]
	public void GetPercentOfValueTest()
	{
		const float expected = 45.5f;
		var result = _randomUtil.GetPercentOfValue(45.5f, 100f);

		Assert.AreEqual(
			expected, 
			result, 
			0.0001f, 
			$"GetPercentOfValue(45.5f, 100f) out of range. Expected: {expected}. Actual: {result}.");
	}
	
	[TestMethod]
	public void ReduceValueByPercentTest()
	{
		const float expected = 54.5f;
		var result = _randomUtil.ReduceValueByPercent(100f, 45.5f);

		Assert.AreEqual(
			expected, 
			result, 
			0.0001f, 
			$"ReduceValueByPercent(100f, 45.5f) out of range. Expected: {expected}. Actual: {result}.");
	}
	
	[TestMethod]
	public void GetChance100Test()
	{
		for (var i = 0; i < 100; i++)
		{
			const bool expectedTrue = true;
			var resultTrue = _randomUtil.GetChance100(100f);
			
			Assert.AreEqual(
				expectedTrue, 
				resultTrue, 
				$"GetChance100(100f) out of range. Expected: {expectedTrue}. Actual: {resultTrue}.");
		}
		
		for (var i = 0; i < 100; i++)
		{
			const bool expectedFalse = false;
			var resultFalse = _randomUtil.GetChance100(0f);
		
			Assert.AreEqual(
				expectedFalse, 
				resultFalse, 
				$"GetChance100(0f) out of range. Expected: {expectedFalse}. Actual: {resultFalse}.");
		}
	}
	
	// TODO: Missing methods between these two
	
	[TestMethod]
	public void RandIntTest()
	{
		for (var i = 0; i < 100; i++)
		{
			var result = _randomUtil.RandInt(0, 10);

			if (result < 0 || result > 9)
			{
				Assert.Fail($"RandInt(0, 10) out of range. Expected range [0, 9] but was {result}.");
			}
		}
		
		for (var i = 0; i < 100; i++)
		{
			var result = _randomUtil.RandInt(10, null);

			if (result < 0 || result > 9)
			{
				Assert.Fail($"RandInt(10, null) out of range. Expected range [0, 9] but was {result}.");
			}
		}
	}
	
	[TestMethod]
	public void RandNumTest()
	{
		for (var i = 0; i < 100; i++)
		{
			var result = _randomUtil.RandNum(0, 10);

			if (result < 0 || result > 9)
			{
				Assert.Fail($"RandNum(0, 10) out of range. Expected range [0, 9.999d] but was {result}.");
			}

			if (_randomUtil.GetNumberPrecision(result) > RandomUtil.MaxSignificantDigits)
			{
				Assert.Fail($"RandNum(0, 10) precision of {result} exceeds the allowable precision ({RandomUtil.MaxSignificantDigits}) for the given values.");
			}
		}
		
		for (var i = 0; i < 100; i++)
		{
			var result = _randomUtil.RandNum(10);

			if (result < 0 || result > 9)
			{
				Assert.Fail($"RandNum(10) out of range. Expected range [0, 9.999d] but was {result}.");
			}
			
			if (_randomUtil.GetNumberPrecision(result) > RandomUtil.MaxSignificantDigits)
			{
				Assert.Fail($"RandNum(10) precision of {result} exceeds the allowable precision ({RandomUtil.MaxSignificantDigits}) for the given values.");
			}
		}
	}
}