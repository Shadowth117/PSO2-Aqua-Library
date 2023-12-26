using AquaModelLibrary.Extensions.Readers;
using AquaModelLibrary.Helpers;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    /// <summary>
    /// Really just for testing undocumented NIFL files
    /// </summary>
    public class AquaGeneric : AquaCommon
    {
        public AquaGeneric(byte[] file, int filler, string fileName)
        {
            Read(file, new string[] { StringHelpers.ExtToIceEnvExt(Path.GetExtension(fileName)) });
        }
        public AquaGeneric(BufferedStreamReaderBE<MemoryStream> streamReader, int filler, string fileName)
        {
            Read(streamReader, new string[] { StringHelpers.ExtToIceEnvExt(Path.GetExtension(fileName)) });
        }

        public AquaGeneric(byte[] file, string _ext)
        {
            Read(file, new string[] { _ext });
        }
        public AquaGeneric(BufferedStreamReaderBE<MemoryStream> streamReader, string _ext)
        {
            Read(streamReader, new string[] { _ext });
        }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            return;
        }
    }
}
