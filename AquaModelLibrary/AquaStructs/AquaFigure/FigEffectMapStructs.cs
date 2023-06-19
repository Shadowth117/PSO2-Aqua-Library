using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.AquaStructs.AquaFigure
{
    public static class FigEffectMapStructs
    {
        //0 = int, 1 = float, 2 = string pointer, 3 - Color?. The struct starts with the type value of the struct itself, as an int.
        //Since there are numerous with unique type ids, they are stored this way instead of as proper struct definitions
        public static Dictionary<int, int[]> effectMappings = new Dictionary<int, int[]>()
        {
            { 0, new [] { 2 } }, //? Might just be a null one. Samples so far seem to always be in the garbage data area after NIFL text storage.
            { 1, new [] { 2, 1, 1, 1, 1} },
            { 2, new [] { 0, 1, 1, 1} },
            { 3, new [] { 2, 2, 0, 0} },
            { 4, new [] { 2, 2, 0, 0, 0, 2, 2} },
            { 5, new [] { 2, 0, 0, 0, 0, 2, 0, 0, 0, 2, 0, 2, 0} },
            { 6, new [] { 0, 0, 0, 0, 0} },
            { 7, new [] { 0, 1} },
            { 8, new [] { 0, 0, 0, 0, 0} },
            { 9, new [] { 0, 0, 0} },
            { 10, new [] { 0, 0} },
            { 11, new [] { 1} },
            { 12, new [] { 0} },
            { 13, new [] { 2, 0, 1} },
            { 14, new [] { 0, 0, 0, 0, 0} },
            { 15, new [] { 2} },
            { 16, new [] { 0, 1} },
            { 17, new [] { 2, 2, 0, 0, 1} },
            { 18, new [] { 0} },
            { 20, new [] { 2, 0} },
            { 21, new [] { 2, 0, 2} },
            { 23, new [] { 2, 2} },
            { 24, new [] { 0} },
            { 25, new [] { 1, 1, 1, 1, 2} },
            { 26, new [] { 2, 1, 1, 1, 0} },
            { 30, new [] { 2, 0, 1} },
            { 31, new [] { 2, 0, 2, 0, 2, 0} },
            { 32, new [] { 1, 0} },
            { 33, new [] { 2, 0} },
            { 34, new [] { 2} },
            { 35, new [] { 2, 1, 1, 1, 1, 1, 1, 1, 0} },
            { 36, new [] { 2, 1, 1, 1, 0, 1, 0, 0, 0, 0, 1, 1, 2, 0, 0} },
            { 37, new [] { 2, 0, 0} },
            { 38, new [] { 2, 0, 0} },
            { 39, new [] { 2, 0, 0, 1} },
            { 40, new [] { 0} },
            { 41, new [] { 0} },
            { 42, new [] { 2, 1, 0, 0} },
            { 43, new [] { 2} },
            { 44, new [] { 2, 2, 0} },
            { 45, new [] { 2, 0, 0, 0, 0} },
            { 47, new int[0] }, //Empty
            { 48, new [] { 0, 0, 0, 0} }, 
            { 49, new [] { 0, 0, 0} }, 
            { 50, new [] { 2} }, 
            { 51, new [] { 2, 0} }, 
            { 53, new [] { 0, 0} }, 
            { 54, new [] { 2, 2, 2, 2} }, 
            { 55, new [] { 0, 1, 0, 1, 1} }, 
            { 56, new [] { 2, 1, 1, 2, 1, 1, 2, 1, 1, 0, 0, 0, 0, 0, 0, 2, 2, 0, 0} }, 
            { 58, new [] { 2, 0, 0} }, 
            { 59, new [] { 2, 1, 0} }, 
            { 60, new [] { 2, 2, 2, 1, 1, 0} }, 
            { 61, new [] { 0, 2, 0, 0} }, 
            { 62, new [] { 0, 0, 0, 0, 0, 0} }, 
            { 64, new [] { 2, 2, 2} }, 
            { 65, new [] { 2, 2} }, 
            { 66, new [] { 0, 0, 0} }, 
            { 67, new [] { 2, 2} }, 
            { 68, new [] { 2} }, 
            { 69, new [] { 0, 0, 0} }, 
            { 70, new [] { 2, 0, 0} }, 
            { 71, new [] { 2, 2, 1, 0} }, 
            { 72, new [] { 0, 2, 2, 0} }, 
            { 73, new [] { 2, 0, 0, 0} }, 
            { 74, new [] { 0, 0} }, 
            { 75, new [] { 2, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0} }, 
            { 76, new [] { 2, 1, 0, 0, 0, 0, 0, 0} }, 
            { 77, new [] { 0, 0} }, 
            { 78, new [] { 0, 0, 0, 0} }, 
            { 79, new [] { 0, 0, 0} }, 
            { 80, new [] { 2, 2, 0, 0, 0, 2, 0} }, 
            { 81, new [] { 2, 2, 2, 0, 0} }, 
            { 82, new [] { 2, 0} },
            { 95, new [] { 2, 2} },
            { 98, new [] { 1, 0} },
            { 143, new [] { 0, 0} },
            { 149, new [] { 0, 0, 2, 0} },
            { 151, new [] { 2, 1, 0, 1, 0} },
            { 156, new [] { 0} },
            { 166, new [] { 2, 0, 0, 0} },
            { 170, new [] { 2, 2, 0, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 0} },
        };
    }
}
