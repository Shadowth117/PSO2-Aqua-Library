using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary
{
    public class AquaBTI_MotionConfig : AquaCommon
    {
        public BTIHeader header = new BTIHeader();
        public List<BTIEntryObject> btiEntries = new List<BTIEntryObject>();
        public static int btiEntrySize = 0x74;

        public struct BTIHeader
        {
            public uint entryPtr;
            public int entryCount;
            public float animLength;
        }

        public struct BTIEntry
        {
            public int additionPtr;
            public int nodePtr;
            public short sht_08;
            public short sht_0A;
            public float startFrame;

            public float float_10;

            public Vector3 pos;
            public float float_20;

            public Vector3 eulerRot;
            public float float_30;

            public Vector3 scale;
            public float endFrame;

            public float float_44;
            public float float_48;
            public float float_4C;
            public float float_50;

            public int field_54;
            public int field_58;
            public int field_5C;
            public int field_60;

            public int field_64;
            public Vector3 vec3_68;
        }

        public class BTIEntryObject
        {
            public BTIEntry entry = new BTIEntry();
            public string addition = ""; //Usually an effect
            public string node = "";
        }

    }
}
