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
				Assert.Fail("GetInt() out of range.");
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
				Assert.Fail("GetIntEx() out of range.");
			}
		}
	}
}