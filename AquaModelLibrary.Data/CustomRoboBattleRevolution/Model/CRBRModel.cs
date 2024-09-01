using AquaModelLibrary.Data.CustomRoboBattleRevolution.Model.Common;
using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.CustomRoboBattleRevolution.Model
{
    /// <summary>
    /// Custom Robo Battle Revolution Model. No extension since the original filename is stripped well before going into the game. Contains model and texture data.
    /// </summary>
    public abstract class CRBRModel
    {
        public CRBRModelHeader Header = null;
        /// <summary>
        /// Models will reference the same texture buffer from multiple places so it's just better to keep one copy since in theory texture definitions are identical too, despite being in different areas
        /// </summary>
        public Dictionary<int, CRBRTextureDefinition> Textures = new Dictionary<int, CRBRTextureDefinition>();
        public CRBRModel() { }

        public CRBRModel(byte[] data)
        {
            Read(new BufferedStreamReaderBE<MemoryStream>(new MemoryStream(data)));
        }

        public CRBRModel(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {

        }

    }
}
