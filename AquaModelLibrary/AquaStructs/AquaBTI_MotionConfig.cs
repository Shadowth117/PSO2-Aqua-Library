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
            public float float_08;
            public float float_0C;

            public Vector3 vec1_10;
            public float float_1C;

            public Vector3 vec3_20;
            public float float_2C;

            public Vector3 vec3_30;
            public float float_3C;

            public float float_40;
            public float float_44;
            public float float_48;
            public float float_4C;

            public int field_50;
            public int field_54;
            public int field_58;
            public int field_5C;

            public float float_60;
            public int field_64;
            public Vector3 vec3_68;
            public int unkStringPtr;
            public Vector3 vec3_78;
        }

        public class BTIEntryObject
        {
            public BTIEntry entry;
            public string addition; //Usually an effect
            public string node;
            public string unkString;
        }

    }
}
