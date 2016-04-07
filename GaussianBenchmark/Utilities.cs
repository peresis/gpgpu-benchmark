using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaussianBenchmark
{
    public static class Utilities
    {
        public static int Clamp(this int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static byte GetNthByte(this uint number, int n)
        {
            return (byte)((number >> (8 * n)) & 0xff);
        }
    }
}
