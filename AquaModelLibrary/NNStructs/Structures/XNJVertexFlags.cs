using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.NNStructs.Structures
{
    [Flags]
    public enum XNJVertexFlags
    {
        Unknown1 = 1,
        HasXyz = 2,
        Unknown4 = 4,
        HasWeights = 8,
        HasNormals = 16, // 0x00000010
        Unknown32 = 32, // 0x00000020
        HasColors = 64, // 0x00000040
        HasAlpha = 128, // 0x00000080
        HasUvs = 256, // 0x00000100
        HasVector4 = 512, // 0x00000200
    }
}
