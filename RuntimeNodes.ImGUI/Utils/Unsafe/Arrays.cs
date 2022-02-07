using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeNodes.ImGUI.Utils.UnsafeUtils
{
    public static class Arrays
    {
        public class AlignedArray<T> : IEnumerable<T>
        {
            public T[] Elements;
            int m_index = -1;
            public int Lenght => m_index + 1;

            public AlignedArray(int capacity)
            {
                Elements = new T[capacity];
            }

            public AlignedArray() : this(0) { }

            public void Resize(int capacity)
            {
                if(Elements.Length < capacity)
                {
                    var newElements = new T[capacity];
                    Array.Copy(Elements, newElements, m_index + 1);
                    Elements = newElements;
                }
            }

            public T PushBack(T val)
            {
                if(Elements.Length < m_index)
                {
                    Resize(Elements.Length*2);
                }
                Elements[++m_index] = val;
                return val;
            }

            public void SetLength(int newLength)
            {
                if (newLength > Lenght)
                    throw new InvalidOperationException("SetLength is not a Resize");
                m_index = newLength - 1;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new AlignedArrayEnumerator<T>(this);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public struct AlignedArrayEnumerator<T> : IEnumerator<T>
        {
            public T Current => m_array.Elements[m_index];

            object IEnumerator.Current => Current;
            bool m_disposed;
            private int m_index;

            private AlignedArray<T> m_array;

            public AlignedArrayEnumerator(AlignedArray<T> arr)
            {
                m_disposed = false;
                m_index = -1;
                m_array = arr;
            }

            public void Dispose()
            {
                m_disposed = true;
            }

            public bool MoveNext()
            {
                if (m_disposed)
                    throw new ObjectDisposedException(nameof(AlignedArrayEnumerator<T>));
                if(m_index < m_array.Lenght)
                {
                    m_index++;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                m_index = -1;
            }
        }
    }
}
