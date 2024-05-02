using Zamboni;

namespace AquaModelLibrary.Data.BluePoint
{
    public static class CompressionHandler
    {
        public static byte[] CheckCompression(byte[] file)
        {
            var compCheck = BitConverter.ToInt16(file, 0);
            if((compCheck == 0xA8C) || (compCheck == 0xACC))
            {
                var decompLength = BitConverter.ToUInt32(file, file.Length - 4) - 0x60000000L;
                if(decompLength < 0)
                {
                    return file;
                }
                var tempFile = Oodle.OodleDecompress(file, decompLength);
                var newFile = new byte[decompLength + 0xC];
                Array.Copy(file, file.Length - 0xC, newFile, decompLength, 0xC);
                Array.Copy(tempFile, 0, newFile, 0, decompLength);
                file = newFile;
            }
            else if ((compCheck == 0x68C) || (compCheck == 0x6CC))
            {
                var decompLength = BitConverter.ToUInt32(file, file.Length - 4) - 0x80000000L;
                if (decompLength <= 0)
                {
                    return file;
                }
                var tempFile = Oodle.OodleDecompress(file, decompLength);
                var newFile = new byte[decompLength + 0xC];
                Array.Copy(file, file.Length - 0xC, newFile, decompLength, 0xC);
                Array.Copy(tempFile, 0, newFile, 0, decompLength);
                file = newFile;
            }

            return file;
        }
    }
}
