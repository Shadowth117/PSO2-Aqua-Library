namespace AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData
{
    public struct AFPMain //Aqua File Package; AQP/AQW Header
    {
        public int magic;
        public int fileCount; //Container file count. Seems to be used only for level of detail models in observed cases; game simply won't try using LOD models if not provided. 
        public int reserve0;
        public int unkInt0; //Always 1

        public byte[] GetBytes()
        {
            List<byte> outBytes = new List<byte>();
            outBytes.AddRange(BitConverter.GetBytes(magic));
            outBytes.AddRange(BitConverter.GetBytes(fileCount));
            outBytes.AddRange(BitConverter.GetBytes(reserve0));
            outBytes.AddRange(BitConverter.GetBytes(unkInt0));

            return outBytes.ToArray();
        }

        public static byte[] GetBytes(int fileCount)
        {
            List<byte> outBytes = new List<byte>();
            outBytes.AddRange( new byte[] { 0x61, 0x66, 0x70, 0 });
            outBytes.AddRange(BitConverter.GetBytes(fileCount));
            outBytes.AddRange(new byte[] { 0, 0, 0 ,0, 
                                           0x1, 0, 0, 0});

            return outBytes.ToArray();
        }
    }
}
