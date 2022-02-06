using RuntimeNodes.MathOperators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeNodes
{
    public interface IMathOperator
    {
        object Substract(object a, object b);
        object Add(object a, object b);
        object Multiply(object a, object b);
        object Divide(object a, object b);
    }


    public class MathOperatorResolver
    {
        static Dictionary<Type, IMathOperator> Resgitered = new Dictionary<Type, IMathOperator>();
        public static IMathOperator Get(Type type)
        {
            if (type.IsPrimitive)
                return BasicTypesOperators.Get(type);

            if (Resgitered.TryGetValue(type, out IMathOperator mathOperator))
                return mathOperator;

            throw new InvalidOperationException($"IMathOperator for the type {type.FullName ?? type.Name} does not exists");
        }
        public static void Register(Type type, IMathOperator mathOperator)
        {
            if (Resgitered.ContainsKey(type))
                throw new InvalidOperationException($"IMathOperator for the type {type.FullName ?? type.Name} is already registered");
            Resgitered.Add(type, mathOperator);
        }
    }
}
