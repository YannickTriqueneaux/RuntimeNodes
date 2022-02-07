using NUnit.Framework;

namespace RuntimeNodes.UnitTest
{
    class TestMathOperations
    {
        [Test]
        public void TestAdd()
        {
            ExecutionContext context = new ExecutionContext();
            context.Initialize(new NodeActivator());
            
            AddNode addNode = new AddNode { Id = 1 };
            
            context.AddNode(addNode);
            context.RootActivator.AddListener(addNode);

            var a = new ConstantValue { Id = 2, Type = typeof(int) };
            var b = new ConstantValue { Id = 3, Type = typeof(int) };
            var result = new RuntimeValue { Id = 4, Type = typeof(int) };
            a.SetValue(1);
            b.SetValue(3);

            context.Select();
            
            context.AddContextValue(a);
            context.AddContextValue(b);
            context.AddContextValue(result);
            
            context.ConnectNode(addNode.Id, a.Id, 0);
            context.ConnectNode(addNode.Id, b.Id, 1);
            context.ConnectNode(addNode.Id, result.Id, 2);

            context.AddRunOnce(addNode);
            addNode.TryActivate();

            context.Unselect();

            Assert.AreEqual(result.HasValue, false);//value not set yet
            Assert.AreEqual(result.Value, null);//value not set yet
            context.Step();
            Assert.AreEqual(result.HasValue, true);
            Assert.AreEqual(result.Value, 4);//1 + 3 = 4

            for (int i = 0; i < 100; ++i)
            {
                context.Step();//The value shouldn't change
                Assert.AreEqual(result.HasValue, true);
                Assert.AreEqual(result.Value, 4);
            }
        }
    }
}
