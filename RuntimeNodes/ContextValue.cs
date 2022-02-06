using System;

namespace RuntimeNodes
{
    public abstract class ContextValue : IValueAccessor
    {
        public ulong Id { get; internal set; }
        public object Value { get; protected set; }
        public bool HasValue { get; protected set; }
        public NodeActivator Activator { get; } = new NodeActivator();
        public Type Type { get; internal set; }

        public void SetValue(object value)
        {
            Value = value;
            HasValue = true;
            Activator.Activate();
        }
        public void ResetValue()
        {
            HasValue = false;
            Value = null;
        }
    }
}