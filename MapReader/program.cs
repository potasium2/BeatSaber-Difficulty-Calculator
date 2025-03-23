using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapReader
{
    internal class program
    {
        static void Main()
        {
            for (int i = 0; i < 14; i++)
                MapReader.MapReader.MapReaderCalculator(i, true);
        }
    }
}
