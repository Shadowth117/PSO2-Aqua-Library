using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.AquaStructs
{
    public class MySpaceObjectsSettings : AquaCommon //Yes really
    {
        public List<MSOEntryObject> msoEntries = new List<MSOEntryObject>();
        public int entryOffset = -1;
        public int entryCount = -1;

        public class MSOEntryObject
        {
            public MSOEntry msoEntry;

            public string asciiName = null;
            public string utf8Descriptor = null;
            public string asciiTrait1 = null;
            public string asciiTrait2 = null;
            public string asciiTrait3 = null;
            public string asciiTrait4 = null;
            public string groupName = null;
            public string asciiTrait5 = null;
        }

        public struct MSOEntry
        {
            public int asciiNameOffset;
            public int int_04;
            public int int_08;
            public int utf8DescriptorOffset;

            public int int_10;
            public int asciiTrait1Offset;
            public int int_18;
            public Vector3 vec3_1C;

            //0x28
            public int asciiTrait2Offset;
            public int asciiTrait3Offset;

            public int asciiTrait4Offset;
            public int groupNameOffset;
            public int int_38;
            public int asciiTrait5Offset;

            public ushort usht_40;
            public ushort usht_42;
            public ushort usht_44;
            public ushort usht_46;
        }
    }
}
