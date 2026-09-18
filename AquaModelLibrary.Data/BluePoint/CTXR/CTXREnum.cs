namespace AquaModelLibrary.Data.BluePoint.CTXR
{
    public enum CTextureType : int
    {
        Standard = 0,
        CubeMap = 1,
        Volume = 2,
        LargeUI = 8, //Texture atlas? Only seen in Demon's Souls Remake for VERY large GUI textures or fonts
    }

    /// <summary>
    /// Only 0 and 9 are ever used for Demon's Souls, and likely most games. 
    /// </summary>
    public enum CTileMode : byte
    {
        None = 0,
        Tile256B = 1,
        Tile4KB = 5,
        Tile64KB = 9,
    }
}
