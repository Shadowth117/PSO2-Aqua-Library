using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.Collections;
using System.IO.Compression;

namespace AquaModelLibrary.Helpers
{
    public class CompressionHelper
    {
        public static byte[] ZlibDefaultDecompress(byte[] bytes, int unCompressedSize)
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

        /// <summary>
        /// Used in some Capcom games
        /// Adapted from https://github.com/Ceberus132/pzz_compression/blob/master/pzz_compression.py
        /// </summary>
        public static byte[] PZZDecompress(byte[] cmpData)
        {
            List<byte> decompData = new List<byte>();
            //We need the filesize as a multiple of 2
            int sizeMultiple = cmpData.Length & ~1;

            int controlBytes = 0;
            int controlBit = -1;

            for(int i = 0; i < sizeMultiple; i += 2)
            {
                if(controlBit < 0)
                {
                    controlBytes = cmpData[i] | (cmpData[i + 1] << 8);
                    controlBit = 15;
                    i += 2;
                }

                int compressFlag = controlBytes & (1 << controlBit);
                controlBit -= 1;

                if(compressFlag > 0)
                {
                    var c = cmpData[i] | (cmpData[i + 1] << 8);
                    var offset = (c & 0x7FF) << 1;

                    //Check for end of compressed data
                    if(offset == 0)
                    {
                        break;
                    }
                    var count = (c >> 11) << 1;
                    if (count == 0)
                    {
                        i += 2;
                        c = cmpData[i] | (cmpData[i + 1] << 8);
                        count = c << 1;
                    }

                    var index = decompData.Count - offset;
                    for(var j = 0; j < count; j++)
                    {
                        decompData.Add(decompData[index + j]);
                    }
                } else
                {
                    decompData.Add(cmpData[i]);
                    decompData.Add(cmpData[i + 1]);
                }
            }

            return decompData.ToArray();
        }

    }
}
