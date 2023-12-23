﻿using AquaModelLibrary.Extensions.Readers;
using System.Text;

namespace AquaModelLibrary.Helpers.Ice
{
    public class IceMethods
    {
        public static void SkipIceEnvelope(BufferedStreamReaderBE<MemoryStream> sr, string[] extensionsToCheck, ref string type, ref int offset)
        {
            foreach (var ext in extensionsToCheck)
            {
                if (type.Equals(ext))
                {
                    sr.Seek(0xC, SeekOrigin.Begin);
                    int headJunkSize = sr.Read<int>();

                    sr.Seek(headJunkSize - 0x10, SeekOrigin.Current);
                    type = Encoding.UTF8.GetString(BitConverter.GetBytes(sr.Peek<int>()));
                    offset += headJunkSize;
                    break;
                }
            }
        }

        public static byte[] RemoveIceEnvelope(byte[] inData)
        {
            byte[] outData;
            var headerSize = BitConverter.ToInt32(inData, 0xC);
            outData = new byte[inData.Length - headerSize];
            Array.Copy(inData, headerSize, outData, 0, outData.Length);

            return outData;
        }
    }
}
