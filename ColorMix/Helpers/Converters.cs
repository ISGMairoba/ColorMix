using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMix.Helpers
{
    class Converters
    {
        public int[] CmykToRgb(int c, int m, int y, int k)
        {
            int r, g, b;
            r = (int)Math.Round(255.0 * (1 - c / 100) * (1 - k / 100));
            g = (int)Math.Round(255.0 * (1 - m / 100) * (1 - k / 100));
            b = (int)Math.Round(255.0 * (1 - y / 100) * (1 - k / 100));
            return [r, g, b];
        }
    }
}
