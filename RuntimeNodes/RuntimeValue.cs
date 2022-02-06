using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeNodes
{
    public interface IValueAccessor
    {
        Type Type { get; }
        object Value { get; }
        bool HasValue { get; }
        void SetValue(object value);
        void ResetValue();
    }

    public interface IMultipleValuesAccessor : IValueAccessor
    {
        void PushValue(object value);
    }
}
