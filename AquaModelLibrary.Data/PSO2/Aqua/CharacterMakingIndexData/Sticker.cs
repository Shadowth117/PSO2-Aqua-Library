namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    //BDP2(stickers)
    public class StickerObject : BaseCMXObject
    {
        public Sticker sticker;
        public string texString = null;
    }

    public struct Sticker
    {
        public int id;
        public int texStringPtr;
        public int reserve0;
    }
}
