using AquaModelLibrary.Data.Gamecube;
using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.CustomRoboBattleRevolution.Model.Common
{
    public class CRBRMaterialData
    {
        public List<CRBRTextureInfo> textureList = new List<CRBRTextureInfo>();
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

        public CRBRMaterialData(BufferedStreamReaderBE<MemoryStream> sr, int offset, CRBRModel model)
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
#endif 
            if(textureInfoOffset != 0)
            {
                var textureOffset = textureInfoOffset;
                do
                {
                    sr.Seek(textureOffset + offset, SeekOrigin.Begin);
                    var texture = new CRBRTextureInfo(sr, offset);
                    textureList.Add(texture);
                    textureOffset = texture.nextTextureInfoOffset;

                    //Add to texture dictionary if it's not there, otherwise shallow copy it
                    if(texture.texture != null && texture.texture.textureBufferOffset != 0)
                    {
                        if (!model.Textures.ContainsKey(texture.texture.textureBufferOffset))
                        {
                            texture.texture.textureBuffer = sr.ReadBytes(texture.texture.textureBufferOffset + offset, (GCTextureInfo.GCBPP[texture.texture.textureFormat]
                                * texture.texture.textureWidth * texture.texture.textureHeight) / 8);
                            model.Textures.Add(texture.texture.textureBufferOffset, texture.texture);
                        }
                        else
                        {
                            texture.texture.textureBuffer = model.Textures[texture.texture.textureBufferOffset].textureBuffer;
                        }
                    }
                       
                } while (textureOffset != 0);
            }
            if(materialColorDataOffset != 0)
            {
                sr.Seek(materialColorDataOffset + offset, SeekOrigin.Begin);
                matColor = new CRBRMaterialColorData(sr);
            }
        }
    }
}
