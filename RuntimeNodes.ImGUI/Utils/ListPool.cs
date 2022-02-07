using System;
using System.Collections.Generic;

namespace RuntimeNodes.ImGUI.Utils
{

    public static class ListPool<T>
    {
        public struct UsageHandle : IDisposable
        {
            private List<T> m_collection;

            public UsageHandle(List<T> collection)
            {
                m_collection = collection;
            }

            public void Dispose()
            {
                Release(ref m_collection);
            }
        }

        private static Stack<List<T>> s_stack = new Stack<List<T>>();

        /// <summary>
        /// This method is supposed to be used like this: using var x = ListPool.TakeScoped(out var yourTemporaryCollection);
        /// </summary>
        public static UsageHandle Take(int capacity, out List<T> temporaryCollection)
        {
            temporaryCollection = Take(capacity);
            return new UsageHandle(temporaryCollection);
        }

        /// <summary>
        /// This method is supposed to be used like this: using var x = ListPool.TakeScoped(out var yourTemporaryCollection);
        /// </summary>
        public static UsageHandle Take(out List<T> temporaryList)
        {
            temporaryList = Take();
            return new UsageHandle(temporaryList);
        }

        public static List<T> Take()
        {
            return s_stack.Count > 0 ? s_stack.Pop() : new List<T>();
        }

        public static List<T> Take(int capacity)
        {
            if (s_stack.Count > 0)
            {
                var list = s_stack.Pop();
                if (list.Capacity < capacity)
                    list.Capacity = capacity;
                return list;
            }
            else
                return new List<T>(capacity);
        }

        /// <summary>
        /// Release take a ref because this will reset the list reference to null, allowing to ensure the list wont be modified later even if it has been Theorically Released
        /// C# still allows to copy the reference before to pass it to Release(), but this is still a better protection than nothing.
        /// </summary>
        public static void Release(ref List<T> list)
        {
            list.Clear();
            s_stack.Push(list);
            list = null;
        }
    }

}
