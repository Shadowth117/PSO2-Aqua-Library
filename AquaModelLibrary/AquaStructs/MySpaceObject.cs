using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.AquaStructs
{
    public class MySpaceObjectsSettings : AquaCommon //Yes really
    {
        public List<MSOEntryObject> msoEntries = new List<MSOEntryObject>();
        public List<MSOCategoryObject> msoGroups = new List<MSOCategoryObject>();
        public List<MSOCategoryObject> msoTypes = new List<MSOCategoryObject>();
        public MSOHeader msoHeader;

        public struct MSOHeader
        {
            public int entryOffset;
            public int entryCount;
            public int groupOffset;
            public int groupCount;

            public int typeOffset;
            public int typeCount;
        }

        public class MSOEntryObject
        {
            public MSOEntry msoEntry;

            public string asciiName = null;
            public string asciiTrait1 = null;
            public string asciiTrait2 = null;
            public string asciiTrait3 = null;
            public string asciiTrait4 = null;
            public string asciiTrait5 = null;
            public string asciiTrait6 = null;
            public string asciiTrait7 = null;
            public string asciiTrait8 = null;
        }

        public struct MSOEntry
        {
            public int asciiNameOffset;
            public int gameVersion;          //A guess, reminiscent of NGS versioning
            public int asciiTrait1Offset;
            public int asciiTrait2Offset;

            public int itemId;               //A guess based on how other item ids look. Really dunno here
            public Vector3 vec3_14;

            public int asciiTrait3Offset;
            public int asciiTrait4Offset;
            public int asciiTrait5Offset;
            public int int_2C;

            public int asciiTrait6Offset;
            public int asciiTrait7Offset;
            public int int_38;
            public int asciiTrait8Offset;

            public byte bt_40;
            public byte bt_41;
            public byte bt_42;
            public byte bt_43;

            public int reserve_44;
        }

        public class MSOCategoryObject
        {
            public MSOAddressPair pair;
            public string name = "";
        }

        public struct MSOAddressPair
        {
            public int offset;
            public int count;
        }
    }
}
