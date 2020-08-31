using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.AquaStructs
{
    public unsafe class AquaPackage
    {
        public struct AFPMain //Aqua File Package; AQP/AQW Header
        {
            public int magic;
            public int fileCount; //Container file count. Seems to be used for level of detail models in observed cases; game simply won't try using LOD models if not provided. 
            public int reserve0;
            public int unkInt0; //Always 1
            AFPBase afpBase;
        }

        //Used at the junction between two models as well as in AFPMain
        public struct AFPBase
        {
            public fixed byte fileName[0x20]; //AQO filename
            public int paddingOffset; //Offset to padding, 0x10 from NEND start 
            public int afpOffsetAddition; //Amount added to offsets by the AFP? Always seems to be 0x30. If using afp however, 0x40 should be added, so perhaps this is something else.
            public int paddingOffsetAdjusted; //Offset to padding + 0x40 of afp header, 0x10 from NEND start 
            public int fileTypeCString; //aqo or tro for models. aqv or trv for texture/uv anims. The game doesn't seem to care what's here though, frankly.
        }
    }
}
