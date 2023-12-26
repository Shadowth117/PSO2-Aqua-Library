using System.Text;

namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    public class VTBF_COLObject : BaseCMXObject
    {
        public VTBF_COL vtbfCol = null;
        public string utf8Name = null;
        public string utf16Name = null;

        public VTBF_COLObject(List<Dictionary<int, object>> colRaw)
        {
            vtbfCol = new VTBF_COL();
            if (colRaw[0].ContainsKey(0xFF))
            {
                vtbfCol.id = (int)colRaw[0][0xFF];
            }
            else
            {
                vtbfCol.id = -1;
            }
            vtbfCol.utf8String = (byte[])colRaw[0][0x31];

            //Convert shorts to be read as utf16 string
            if (colRaw[0].ContainsKey(0x32))
            {
                var shorts = (short[])colRaw[0][0x32];
                var bytes = new byte[shorts.Length * 2];
                Buffer.BlockCopy(shorts, 0, bytes, 0, shorts.Length * 2);

                vtbfCol.utf16String = bytes;
                utf16Name = Encoding.Unicode.GetString(vtbfCol.utf16String);
            }

            vtbfCol.colorData = (byte[])colRaw[0][0x30];
            utf8Name = Encoding.UTF8.GetString(vtbfCol.utf8String);
        }
    }

    //Character color data ranges as PixelFormat.Format32bppRgb. 21 x 6 pixels. Every 3 columns is one area, but only the middle column seems used ingame.
    //Left and right columns seem like they may have been related to either ingame shading, as the deuman one does not go nearly as dark as the others, but it's hard to say.
    public class VTBF_COL
    {
        public int id;
        public byte[] utf8String = null;
        public byte[] utf16String = null;
        public byte[] colorData = null;   //Should be 0x1F8 bytes. 
    }
}
