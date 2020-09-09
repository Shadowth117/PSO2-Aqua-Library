using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.AquaStructs
{
    unsafe class PrmParameters
    {
        //Seems to somehow link the PRM entries together when the count isn't 0
        List<short> groupIndices = new List<short>();

        struct PRMHeader
        {
            public int magic;
            public int entryCount;
            public int groupIndexCount;
            public int entryVersion;
        }

        struct PRMEntry01
        {
            public float unk1;
            public float unk2;
            public float unk3;
            public float oftenOne;
            public fixed byte color[4];
            public float unk4;
            public float unk5;
            public float unk6;
            public float unk7;
            public Vector3 unkVector;
        }

        struct PRMEntry02
        {

        }

        struct PRMEntry03
        {
            public float unk1;
            public float unk2;
            public float unk3;
            public fixed byte color[4];
            public float unk4;
            public float unk5;
            public float unk6;
            public float unk7;
        }
    }
}
