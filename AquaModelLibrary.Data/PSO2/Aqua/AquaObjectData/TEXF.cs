using AquaModelLibrary.Data.DataTypes.SetLengthStrings;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData
{
    //Laid out in same order as TSTA. Seemingly redundant.
    public struct TEXF
    {
        public PSO2String texName; //0x80, type 0x2 //Texture filename (includes extension)

        public TEXF(byte[] bytes)
        {
            texName = new PSO2String(bytes);
        }
        public TEXF(string str)
        {
            texName = new PSO2String(str);
        }
    }
}
