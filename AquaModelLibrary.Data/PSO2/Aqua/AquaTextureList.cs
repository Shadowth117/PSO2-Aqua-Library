using System.Collections.Generic;

namespace AquaModelLibrary.AquaStructs
{
    //TXL files for pso2 aren't used the same as NN or Ninja due to models always having stringe references to textures
    public class AquaTextureList
    {
        public TxlHeader header;
        public List<string> texList = new List<string>();
        public List<string> iceList = new List<string>();
        public List<List<byte[]>> dataList = new List<List<byte[]>>(); //RGBA Colors, and some other data depending on texture type

        public struct TxlHeader
        {
            public int texCount;
            public int iceCount;
            public int int_08;
            public int int_0C;

            public int texOffsetListOffset;
            public int dataOffsetListOffset;
            public int iceNamesOffset; //But why?
            public int iceOffsetListOffset;
        }
    }
}
