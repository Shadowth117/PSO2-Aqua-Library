using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.IO.Compression;

namespace AquaModelLibrary.Helpers
{
    public class CompressionHelper
    {
        public static byte[] ZlibDefaultDecompress(byte[] bytes, int unCompressedSize = int.MaxValue)
        {
            var output = new byte[unCompressedSize];
            int realSize;
            using (MemoryStream memory_stream = new MemoryStream(bytes, false))
            {
                using (InflaterInputStream inflater = new InflaterInputStream(memory_stream))
                    realSize = inflater.Read(output, 0, unCompressedSize);
            }

            if(realSize < unCompressedSize)
            {
                byte[] tempBuffer = new byte[realSize];
                Array.Copy(output, 0, tempBuffer, 0, realSize);
                output = tempBuffer;
            }

            return output;
        }
    }
}
