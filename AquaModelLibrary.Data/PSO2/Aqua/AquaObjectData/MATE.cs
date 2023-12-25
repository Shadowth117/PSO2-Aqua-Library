using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Helpers.MathHelpers;
using System;
using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData
{
    public struct MATE
    {
        //Vector4 colors are assumedly based on the particular shader
        public Vector4 diffuseRGBA; //0x30, type 0x4A, variant 0x2 //alpha is always 1 in official
        public Vector4 unkRGBA0;    //0x31, type 0x4A, variant 0x2 //Defaults are .9 .9 .9 1.0
        public Vector4 _sRGBA;      //0x32, type 0x4A, variant 0x2 //Seemingly RGBA for the specular map. 
                                    //Value 3 affects self illum, just as blue, the third RGBA section, affects this in the _s map. A always observed as 1.
        public Vector4 unkRGBA1;    //0x33, type 0x4A, variant 0x2 //Works same as above? A always observed as 1. In NGS, this seems different.

        public int reserve0;        //0x34, type 0x9
        public float unkFloat0;     //0x35, type 0xA //Typically 8 or 32. I default it to 8. Possibly one of the 0-100 material values in max.
        public float unkFloat1;     //0x36, type 0xA //Tyipcally 1
        public int unkInt0;         //0x37, type 0x9 //Typically 100. Almost definitely a Max material 0-100 thing. (PSO2 models pass through 3ds Max in development at some point.)

        public int unkInt1;         //0x38, type 0x9 //Usually 0, sometimes other things
        public PSO2String alphaType; //0x3A, type 0x2 //Fixed length string for the alpha type of the mat. "opaque", "hollow", "blendalpha", and "add" are
                                     //all valid. Add is additive, and uses diffuse alpha for glow effects. Others may be used to denote collision types
        public PSO2String matName;   //0x39, type 0x2 

        public MATE(Dictionary<int, object> mateRaw)
        {
            diffuseRGBA = (Vector4)mateRaw[0x30];
            unkRGBA0 = (Vector4)mateRaw[0x31];
            _sRGBA = (Vector4)mateRaw[0x32];
            unkRGBA1 = (Vector4)mateRaw[0x33];
            reserve0 = (int)mateRaw[0x34];
            unkFloat0 = (float)mateRaw[0x35];
            unkFloat1 = (float)mateRaw[0x36];
            unkInt0 = (int)mateRaw[0x37];
            unkInt1 = (int)mateRaw[0x38];
            alphaType = new PSO2String((byte[])mateRaw[0x3A]);
            matName = new PSO2String((byte[])mateRaw[0x39]);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equiv(object obj)
        {
            MATE c = (MATE)obj;
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

            return reserve0 == c.reserve0 && unkFloat0 == c.unkFloat0 && unkFloat1 == c.unkFloat1 && unkInt0 == c.unkInt0 && unkInt1 == c.unkInt1
                && alphaType == c.alphaType && matName == c.matName && MathExtras.isEqualVec4(diffuseRGBA, c.diffuseRGBA) && MathExtras.isEqualVec4(unkRGBA0, c.unkRGBA0)
                && MathExtras.isEqualVec4(_sRGBA, c._sRGBA) && MathExtras.isEqualVec4(unkRGBA1, c.unkRGBA1);
        }

        public static bool operator ==(MATE lhs, MATE rhs)
        {
            return lhs.Equiv(rhs);
        }

        public static bool operator !=(MATE lhs, MATE rhs) => !(lhs == rhs);

        public override readonly bool Equals(object obj)
        {
            return this == (MATE)obj;
        }
    }
}
