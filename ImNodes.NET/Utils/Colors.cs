using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImNodes.NET.Utils
{
    public class ColorUtils
    {
        public static uint GetColorU32(byte r, byte g, byte b, byte a)
        {
            uint val = 0;
            val = r;
            val <<= 8;
            val += g;
            val <<= 8;
            val += b;
            val <<= 8;
            val += a;
            return val;
        }
    }
}
