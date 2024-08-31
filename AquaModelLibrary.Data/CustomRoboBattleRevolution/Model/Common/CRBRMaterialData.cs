using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.CustomRoboBattleRevolution.Model.Common
{
    public class CRBRMaterialData
    {
        public CRBRTextureInfo textureInfo = new CRBRTextureInfo();
        public CRBRMaterialColorData matColor = new CRBRMaterialColorData();

        public int int_00;
        /// <summary>
        /// NOT an offset
        /// </summary>
        public int unkValue;
        public int textureInfoOffset;
        public int materialColorDataOffset;

        public int int_10;
        public int int_14;

        public CRBRMaterialData() { }

        public CRBRMaterialData(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            int_00 = sr.ReadBE<int>();
            unkValue = sr.ReadBE<int>();
            textureInfoOffset = sr.ReadBE<int>();
            materialColorDataOffset = sr.ReadBE<int>();

            int_10 = sr.ReadBE<int>();
            int_14 = sr.ReadBE<int>();

#if DEBUG
            if(int_00 != 0)
            {
                throw new NotImplementedException();
            }
            if (int_10 != 0)
            {
                throw new NotImplementedException();
            }
            if (int_14 != 0)
            {
                throw new NotImplementedException();
            }
#endif 
            if(textureInfoOffset != 0)
            {
                sr.Seek(textureInfoOffset + offset, SeekOrigin.Begin);
                textureInfo = new CRBRTextureInfo(sr, offset);
            }
            if(materialColorDataOffset != 0)
            {
                sr.Seek(materialColorDataOffset + offset, SeekOrigin.Begin);
                matColor = new CRBRMaterialColorData(sr);
            }
        }
    }
}
