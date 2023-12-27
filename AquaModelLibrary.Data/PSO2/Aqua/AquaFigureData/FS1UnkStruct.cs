using AquaModelLibrary.Helpers.Readers;
using Reloaded.Memory.Streams;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaFigureData
{
    public class FS1UnkStruct0Object
    {
        public FS1UnkStruct0 fs1struct0;
        public string text0 = null;
        public string text1 = null;
        public string text2 = null;
        public string text3 = null;

        public FS1UnkStruct0Object(int offset, int address, BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            streamReader.Seek(address, SeekOrigin.Begin);
            fs1struct0 = streamReader.Read<FS1UnkStruct0>();
            text0 = streamReader.ReadCStringValidOffset(fs1struct0.text0Ptr, offset);
            text1 = streamReader.ReadCStringValidOffset(fs1struct0.text1Ptr, offset);
            text2 = streamReader.ReadCStringValidOffset(fs1struct0.text2Ptr, offset);
            text3 = streamReader.ReadCStringValidOffset(fs1struct0.text3Ptr, offset);
        }
    }

    public struct FS1UnkStruct0
    {
        public int text0Ptr;
        public int text1Ptr;
        public int text2Ptr;
        public int text3Ptr;

        public int int_10;
        public float float_14;
        public ushort ushort_18;
        public ushort ushort_1C;
    }
}
