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
            { 1, new [] { 2, 1, 1, 1,
                1} },
            { 2, new [] { 0, 1, 1, 1} },
            { 3, new [] { 2, 2, 0, 0} },
            { 4, new [] { 2, 2, 0, 3, 
                0, 2, 0} },
            { 5, new [] { 2, 0, 0, 0, 
                3, 2, 0, 0, 
                3, 2, 0, 2,
                0} },
            { 6, new [] { 0, 0, 0, 0, 
                0} },
            { 7, new [] { 0, 1} },
            { 8, new [] { 3, 3, 3, 3,
                0} },
            { 9, new [] { 0, 0, 0} }
        };
    }
}
