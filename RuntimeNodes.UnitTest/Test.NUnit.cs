using NUnit.Framework;

namespace RuntimeNodes.UnitTest
{
    [TestFixture]
    public class TestNUnit
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void IsNUnitWorking()
        {
            Assert.IsFalse(false, "1 should not be prime");
        }
    }
}
