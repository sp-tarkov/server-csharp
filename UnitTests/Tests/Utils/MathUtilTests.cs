using Core.Utils;

namespace UnitTests.Tests.Utils;

[TestClass]
public class MathUtilTests
{
	protected MathUtil _mathUtil = new();
	
	[TestMethod]
	public void ListSumTest()
	{
		var test = new List<double> { 1.1f, 2.1f, 3.3f };
		const double expected = 6.5f;
		
		var actual = _mathUtil.ListSum(test);
		
		Assert.AreEqual(expected, actual,
			$"ListSum() Expected: {expected}, Actual: {actual}");
	}
	
	[TestMethod]
	public void ListCumSumTest()
	{
		var test = new List<double> { 1f, 2f, 3f, 4f };
		var expected = new List<double> { 1f, 3f, 6f, 10f };
		
		var actual = _mathUtil.ListCumSum(test);

		for (var i = 0; i < actual.Count; i++)
		{
			if (Math.Abs(expected[i] - actual[i]) > 0.00001f)
			{
				Assert.Fail($"ListCumSum() Expected: {string.Join(", ", expected)}, Actual: {string.Join(", ", actual)}");
			}
		}
	}
	
	[TestMethod]
	public void ListProductTest()
	{
		var test = new List<double> { 1f, 2f, 3f, 4f };
		var expected = new List<double> { 2f, 4f, 6f, 8f };
		
		var actual = _mathUtil.ListProduct(test, 2);

		for (var i = 0; i < actual.Count; i++)
		{
			if (Math.Abs(expected[i] - actual[i]) > 0.00001f)
			{
				Assert.Fail($"ListProduct() Expected: {string.Join(", ", expected)}, Actual: {string.Join(", ", actual)}");
			}
		}
	}
	
	[TestMethod]
	public void ListAddTest()
	{
		var test = new List<double> { 1f, 2f, 3f, 4f };
		var expected = new List<double> { 3f, 4f, 5f, 6f };
		
		var actual = _mathUtil.ListAdd(test, 2);

		for (var i = 0; i < actual.Count; i++)
		{
			if (Math.Abs(expected[i] - actual[i]) > 0.00001f)
			{
				Assert.Fail($"ListProduct() Expected: {string.Join(", ", expected)}, Actual: {string.Join(", ", actual)}");
			}
		}
	}
	
	[TestMethod]
	public void MapToRangeTest()
	{
		const double expected = 2;
		
		var actual = _mathUtil.MapToRange(0.5, 0, 1, 1, 3);
		
		Assert.AreEqual(expected, actual, 
			$"MapToRange() Expected: {expected}, Actual: {actual}");
	}
}
