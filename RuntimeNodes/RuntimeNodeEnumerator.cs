using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeNodes
{
    public interface IRuntimeNodeEnumerator<T> : IEnumerator<T>
    {
        bool ConsumeNext();
    }
}
