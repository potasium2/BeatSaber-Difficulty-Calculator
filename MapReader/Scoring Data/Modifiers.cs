using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapReader.MapData
{
    internal class Modifiers
    {
        public static double speedMultiplier = 1.0; // Covers FS and SFS
        public const double angleLeniency = 60; // Covers SA (NM is 60, SA is 45)
        public static bool ghostNotes = false; // Virtually HD
    }
}
