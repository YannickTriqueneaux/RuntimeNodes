using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeNodes.MathOperators
{
    static class BasicTypesOperators
    {
        public static IMathOperator Get(Type basicType)
        {
            var typeCode = Type.GetTypeCode(basicType);
            switch(typeCode)
            {
                case TypeCode.SByte:
                    return SByteOp.Instance;
                case TypeCode.Byte:
                    return UByteOp.Instance;
                case TypeCode.Int16:
                    return Int16Op.Instance;
                case TypeCode.UInt16:
                    return UInt16Op.Instance;
                case TypeCode.Int32:
                    return Int32Op.Instance;
                case TypeCode.UInt32:
                    return UInt32Op.Instance;
                case TypeCode.Int64:
                    return Int64Op.Instance;
                case TypeCode.UInt64:
                    return UInt64Op.Instance;
                case TypeCode.Single:
                    return FloatOp.Instance;
                case TypeCode.Double:
                    return DoubleOp.Instance;
            }
            return null;
        }

        class SByteOp : IMathOperator
        {
            public static IMathOperator Instance { get; } = new SByteOp();

            public object Add(object a, object b)
            {
                return (sbyte)a + (sbyte)b;
            }

            public object Divide(object a, object b)
            {
                return (sbyte)a / (sbyte)b;
            }

            public object Multiply(object a, object b)
            {
                return (sbyte)a * (sbyte)b;
            }

            public object Substract(object a, object b)
            {
                return (sbyte)a - (sbyte)b;
            }
        }
        class UByteOp : IMathOperator
        {
            public static IMathOperator Instance { get; } = new UByteOp();
            public object Add(object a, object b)
            {
                return (byte)a + (byte)b;
            }

            public object Divide(object a, object b)
            {
                return (byte)a / (byte)b;
            }

            public object Multiply(object a, object b)
            {
                return (byte)a * (byte)b;
            }

            public object Substract(object a, object b)
            {
                return (byte)a - (byte)b;
            }
        }
        class Int16Op : IMathOperator
        {
            public static IMathOperator Instance { get; } = new Int16Op();
            public object Add(object a, object b)
            {
                return (Int16)a + (Int16)b;
            }

            public object Divide(object a, object b)
            {
                return (Int16)a / (Int16)b;
            }

            public object Multiply(object a, object b)
            {
                return (Int16)a * (Int16)b;
            }

            public object Substract(object a, object b)
            {
                return (Int16)a - (Int16)b;
            }
        }
        class UInt16Op : IMathOperator
        {
            public static IMathOperator Instance { get; } = new UInt16Op();
            public object Add(object a, object b)
            {
                return (UInt16)a + (UInt16)b;
            }

            public object Divide(object a, object b)
            {
                return (UInt16)a / (UInt16)b;
            }

            public object Multiply(object a, object b)
            {
                return (UInt16)a * (UInt16)b;
            }

            public object Substract(object a, object b)
            {
                return (UInt16)a - (UInt16)b;
            }
        }
        class Int32Op : IMathOperator
        {
            public static IMathOperator Instance { get; } = new Int32Op();
            public object Add(object a, object b)
            {
                return (Int32)a + (Int32)b;
            }

            public object Divide(object a, object b)
            {
                return (Int32)a / (Int32)b;
            }

            public object Multiply(object a, object b)
            {
                return (Int32)a * (Int32)b;
            }

            public object Substract(object a, object b)
            {
                return (Int32)a - (Int32)b;
            }
        }
        class UInt32Op : IMathOperator
        {
            public static IMathOperator Instance { get; } = new UInt32Op();
            public object Add(object a, object b)
            {
                return (UInt32)a + (UInt32)b;
            }

            public object Divide(object a, object b)
            {
                return (UInt32)a / (UInt32)b;
            }

            public object Multiply(object a, object b)
            {
                return (UInt32)a * (UInt32)b;
            }

            public object Substract(object a, object b)
            {
                return (UInt32)a - (UInt32)b;
            }
        }
        class Int64Op : IMathOperator
        {
            public static IMathOperator Instance { get; } = new Int64Op();
            public object Add(object a, object b)
            {
                return (Int64)a + (Int64)b;
            }

            public object Divide(object a, object b)
            {
                return (Int64)a / (Int64)b;
            }

            public object Multiply(object a, object b)
            {
                return (Int64)a * (Int64)b;
            }

            public object Substract(object a, object b)
            {
                return (Int64)a - (Int64)b;
            }
        }
        class UInt64Op : IMathOperator
        {
            public static IMathOperator Instance { get; } = new UInt64Op();
            public object Add(object a, object b)
            {
                return (UInt64)a + (UInt64)b;
            }

            public object Divide(object a, object b)
            {
                return (UInt64)a / (UInt64)b;
            }

            public object Multiply(object a, object b)
            {
                return (UInt64)a * (UInt64)b;
            }

            public object Substract(object a, object b)
            {
                return (UInt64)a - (UInt64)b;
            }
        }

        class FloatOp : IMathOperator
        {
            public static IMathOperator Instance { get; } = new FloatOp();
            public object Add(object a, object b)
            {
                return (float)a + (float)b;
            }

            public object Divide(object a, object b)
            {
                return (float)a / (float)b;
            }

            public object Multiply(object a, object b)
            {
                return (float)a * (float)b;
            }

            public object Substract(object a, object b)
            {
                return (float)a - (float)b;
            }
        }

        class DoubleOp : IMathOperator
        {
            public static IMathOperator Instance { get; } = new DoubleOp();
            public object Add(object a, object b)
            {
                return (double)a + (double)b;
            }

            public object Divide(object a, object b)
            {
                return (double)a / (double)b;
            }

            public object Multiply(object a, object b)
            {
                return (double)a * (double)b;
            }

            public object Substract(object a, object b)
            {
                return (double)a - (double)b;
            }
        }
    }
}
