using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Helpers.PSO2;

namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    //BDP2(stickers)
    public class StickerObject : BaseCMXObject
    {
        public Sticker sticker;
        public string texString = null;

        public StickerObject() { }

        public StickerObject(List<Dictionary<int, object>> bdp2Raw)
        {
            sticker.id = (int)bdp2Raw[0][0xFF];

            texString = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(bdp2Raw[0], 0x20)).GetString();
            sticker.reserve0 = VTBFMethods.GetObject<int>(bdp2Raw[0], 0x21);
        }
    }

    public struct Sticker
    {
        public int id;
        public int texStringPtr;
        public int reserve0;
    }
}
