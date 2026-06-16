using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public static class Counter
    {
        public static int HeavyCount { get; set; }
        public static int CloneCount { get; set; }

        public static void Reset()
        {
            HeavyCount = 0;
            CloneCount = 0;
        }
    }
}
