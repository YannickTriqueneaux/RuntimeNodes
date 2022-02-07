using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeNodes
{
    public abstract class MathOperationNode : ActivableNode, ITypesToResolveNode
    {
        [Input]
        public IValueAccessor A { get; private set; }

        [Input]
        public IValueAccessor B { get; private set; }

        [Output]
        public IValueAccessor Result { get; private set; }
        
        protected IMathOperator _mathOperator;

        public override void Init()
        {
            IValueAccessor[] values = this.UpdateAndGetValueAccessors();
            A = values[0];
            B = values[1];
            Result = values[2];
        }

        public ResolveTypeResult ResolveTypes()
        {
            if (A == null || B == null)
                return ResolveTypeResult.Uncompleted;
            if (Result == null)
                return ResolveTypeResult.Uncompleted;

            Type aType = A.Type;
            Type bType = B.Type;
            if (aType != null && bType == null)
            {
                _mathOperator = MathOperatorResolver.Get(aType);
            }
            else if (aType == null && bType != null)
            {
                _mathOperator = MathOperatorResolver.Get(bType);
            }
            else if (aType != null && bType != null)
            {
                if (aType != bType)
                {
                    return ResolveTypeResult.Failed("A and B type mismatch");
                }

                if (_mathOperator == null)
                    _mathOperator = MathOperatorResolver.Get(aType);

                return ResolveTypeResult.Succeeded;
            }
            return ResolveTypeResult.Uncompleted;
        }
    }

    public class SubstractNode : MathOperationNode
    {
        public override void Step()
        {
            Result.SetValue(_mathOperator.Substract(A.Value, B.Value));
        }
    }

    public class AddNode : MathOperationNode
    {
        public override void Step()
        {
            Result.SetValue(_mathOperator.Add(A.Value, B.Value));
        }
    }

    public class MultiplyNode : MathOperationNode
    {
        public override void Step()
        {
            Result.SetValue(_mathOperator.Multiply(A.Value, B.Value));
        }
    }

    public class DivideNode : MathOperationNode
    {
        public override void Step()
        {
            Result.SetValue(_mathOperator.Divide(A.Value, B.Value));
        }
    }
}
