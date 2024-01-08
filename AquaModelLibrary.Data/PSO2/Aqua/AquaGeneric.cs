using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    /// <summary>
    /// Really just for testing undocumented NIFL files
    /// </summary>
    public class AquaGeneric : AquaCommon
    {
        public BufferedStreamReaderBE<MemoryStream> agSR = null;
        public AquaGeneric(byte[] file, string fileName)
        {
            Read(file, new string[] { StringHelpers.ExtToIceEnvExt(Path.GetExtension(fileName)) });
        }
        public AquaGeneric(BufferedStreamReaderBE<MemoryStream> streamReader, string fileName)
        {
            Read(streamReader, new string[] { StringHelpers.ExtToIceEnvExt(Path.GetExtension(fileName)) });
        }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            agSR = sr;
        }
    }
}
