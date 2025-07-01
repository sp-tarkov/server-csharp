using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;

namespace UnitTests.Tests.Utils
{
    /// <summary>
    /// Unit tests for the <see cref="MongoId"/> struct.
    /// </summary>
    [TestClass]
    public class MongoIdTests
    {
        /// <summary>
        /// Test that generates 1000 <see cref="MongoId"/> and ensures they are all valid. <br/>
        /// Validity is checked by ensuring the ID is non-empty, exactly 24 characters, and matches the expected format.
        /// </summary>
        [TestMethod]
        public void Generate_ShouldProduceValidMongoIds()
        {
            var invalidIds = new List<string>();

            for (var i = 0; i < 1000; i++)
            {
                var id = new MongoId();
                var idStr = id.ToString();

                if (string.IsNullOrWhiteSpace(idStr) || idStr.Length != 24 || !id.IsValidMongoId())
                {
                    invalidIds.Add(idStr);
                }
            }

            Assert.AreEqual(
                0,
                invalidIds.Count,
                $"Invalid MongoIds found: {string.Join(", ", invalidIds)}"
            );
        }
    }
}
