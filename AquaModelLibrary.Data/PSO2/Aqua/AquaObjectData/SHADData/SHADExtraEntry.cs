using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.SHADData
{
    //Contains strings referencing shader information 
    public struct SHADExtraEntry
    {
        public short entryFlag0;
        public NGSShaderString entryString;
        public short entryFlag1;
        public short entryFlag2;

        public Vector4 entryFloats;
        public SHADExtraEntry(short _entryFlag0, string _entryString, short _entryFlag1, short _entryFlag2)
        {
            entryFlag0 = _entryFlag0;
            entryString.SetString(_entryString);
            entryFlag1 = _entryFlag1;
            entryFlag2 = _entryFlag2;
            entryFloats = new Vector4();
        }

        public SHADExtraEntry(short _entryFlag0, string _entryString, short _entryFlag1, short _entryFlag2, Vector4 _entryFloats)
        {
            entryFlag0 = _entryFlag0;
            entryString.SetString(_entryString);
            entryFlag1 = _entryFlag1;
            entryFlag2 = _entryFlag2;
            entryFloats = _entryFloats;
        }
    }
}
