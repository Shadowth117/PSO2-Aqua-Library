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
        /// Texture images are referenced by offset and can be referenced from multiple materials. 
        /// </summary>
        public Dictionary<int, byte[]> imageData = new Dictionary<int, byte[]>();
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
