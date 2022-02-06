using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeNodes
{
    class Program
    {
        
        static void Main(string[] args)
        {
            ExecutionContext context = new ExecutionContext();
            context.Initialize(new NodeActivator());

            for (int i = 0; i < 100; ++i)
                context.Step();//empty test


            AddNode addNode = new AddNode { Id = 1 };


            context.AddNode(addNode);
            context.RootActivator.AddListener(addNode);

            var a = new ConstantValue { Id = 2, Type = typeof(int)};
            var b = new ConstantValue { Id = 3, Type = typeof(int)};
            var result = new RuntimeValue { Id = 4, Type = typeof(int)};
            a.SetValue(1);
            b.SetValue(2);

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

            for (int i = 0; i < 100; ++i)
                context.Step();
        }
    }
}
