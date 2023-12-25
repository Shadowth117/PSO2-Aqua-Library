using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData
{
    //Texture Settings
    public struct TSTA
    {
        public int tag; //0x60, type 0x9 //0x16, always in classic. In 0xC33, often 0x17
        public int texUsageOrder; //0x61, type 0x9  //0,1,2, 3 etc. PSO2 TSETs (Texture sets) require specific textures in specfic places. There should be a new TSTA if using a texture in a different slot for some reason.
        public int modelUVSet;    //0x62, type 0x9  //Observed as -1, 0, 1, and 2. 3 and maybe more theoretically usable. 0 is default, -1 is for _t maps or any map that doesn't use UVs. 1 is for _k maps.
        public Vector3 unkVector0; //0x63, type 0x4A, 0x1 //0, -0, 0, often.
        public float unkFloat2; //0x64, type 0x9 //0
        public float unkFloat3; //0x65, type 0x9 //0

        public float unkFloat4; //0x66, type 0x9 //0
        public int unkInt3; //0x67, type 0x9 //1 or sometimes 3
        public int unkInt4; //0x68, type 0x9 //1 or sometimes 3
        public int unkInt5; //0x69, type 0x9 //1

        public float unkFloat0; //0x6A, type 0xA //0
        public float unkFloat1; //0x6B, type 0xA //0
        public PSO2String texName; //0x6C, type 0x2 //Texture filename (includes extension)

        public TSTA(Dictionary<int, object> tstaRaw)
        {
            tag = (int)tstaRaw[0x60];
            texUsageOrder = (int)tstaRaw[0x61];
            modelUVSet = (int)tstaRaw[0x62];
            unkVector0 = (Vector3)tstaRaw[0x63];
            unkFloat2 = (int)tstaRaw[0x64];
            unkFloat3 = (int)tstaRaw[0x65];
            unkFloat4 = (int)tstaRaw[0x66];
            unkInt3 = (int)tstaRaw[0x67];
            unkInt4 = (int)tstaRaw[0x68];
            unkInt5 = (int)tstaRaw[0x69];
            unkFloat0 = (float)tstaRaw[0x6A];
            unkFloat1 = (float)tstaRaw[0x6B];
            texName = new PSO2String((byte[])tstaRaw[0x6C]);
        }

        public bool Equals(TSTA c)
        {

            // Optimization for a common success case.
            if (ReferenceEquals(this, c))
            {
                return true;
            }

            // If run-time types are not exactly the same, return false.
            if (GetType() != c.GetType())
            {
                return false;
            }

            return tag == c.tag && texUsageOrder == c.texUsageOrder && modelUVSet == c.modelUVSet && unkVector0 == c.unkVector0 && unkFloat2 == c.unkFloat2 && unkFloat3 == c.unkFloat3 && unkFloat4 == c.unkFloat4
                && unkInt3 == c.unkInt3 && unkInt4 == c.unkInt4 && unkInt5 == c.unkInt5 && unkFloat0 == c.unkFloat0 && unkFloat1 == c.unkFloat1 && texName == c.texName;
        }

        public static bool operator ==(TSTA lhs, TSTA rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(TSTA lhs, TSTA rhs) => !(lhs == rhs);

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
