using NUnit.Framework;

namespace RuntimeNodes.UnitTest
{
    [TestFixture]
    public class TestContext
    {
        [Test]
        public void TestEmptyContext()
        {
            ExecutionContext context = new ExecutionContext();
            context.Initialize(new NodeActivator());

            for (int i = 0; i < 100; ++i)
                context.Step();//empty test
        }
    }
}
