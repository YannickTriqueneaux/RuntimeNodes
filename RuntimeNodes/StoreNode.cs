using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeNodes
{
    public class CollectionData<T> : IMultipleValuesAccessor
    {
        private int _currentIndex = -1;
        private List<object> _container;

        public IValueAccessor _current = null;
        public IValueAccessor Current { get; private set; }

        public Type Type => this.GetType();

        public object Value => this;

        public bool HasValue { get; private set; }

        public void Dispose()
        {
            Reset();
            _current = null;
            _container = null;
        }

        public bool MoveNext()
        {
            if (_container.Count > 0 && _currentIndex < _container.Count)
            {
                _current.SetValue(_container[++_currentIndex]);
                return true;
            }
            _current.ResetValue();
            return false;
        }

        public bool ConsumeNext()
        {
            if (MoveNext())
            {
                _container.RemoveAt(_currentIndex);
                --_currentIndex;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            _current.ResetValue();
            _currentIndex = -1;
        }

        public void SetValue(object value)
        {
            throw new NotImplementedException();
        }

        public void ResetValue()
        {
            throw new NotImplementedException();
        }

        public void PushValue(object value)
        {
            _container.Add(value);
        }
    }
}
