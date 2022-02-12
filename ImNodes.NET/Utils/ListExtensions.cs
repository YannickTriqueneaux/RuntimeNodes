using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImNodes.NET.Utils
{
    public static class ListExtensions
    {
        public static void Resize<T>(this List<T> list, int size, Func<T> constructor)
        {
            int nbToAdd = size - list.Count;
            for (int i = 0; i < nbToAdd; ++i)
                list.Add(constructor());
        }

        public static void Resize<T>(this List<T> list, int size)
            where T : new()
        {
            int nbToAdd = size - list.Count;
            for (int i = 0; i < nbToAdd; ++i)
                list.Add(new T());
        }
    }
}
