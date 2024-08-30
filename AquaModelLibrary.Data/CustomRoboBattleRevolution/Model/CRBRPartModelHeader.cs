using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.CustomRoboBattleRevolution.Model
{
    public class CRBRPartModelHeader : CRBRModelHeader
    {
        /// <summary>
        /// All offsets in the file are offset by this initial offset
        /// </summary>
        //public int offset;
        public int fileSize;
        public int offset0;
        public int offset0Count;

        public int unkBytesOffset0;
        public int unkCount0; //Usually -1
        public int offsetTableOffset;
        public int offsetTableCount;

        public int unkBytesOffset1;
        public int unkCount1;

        //Probably padding to 0x20 from here
        public int int_28;
        public int int_2C;

        public int int_30;
        public int int_34;
        public int int_38;
        public int int_3C;

        public CRBRPartModelHeader() { }

        public CRBRPartModelHeader(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        public CRBRPartModelHeader(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            offset = sr.ReadBE<int>();
            fileSize = sr.ReadBE<int>();
            offset0 = sr.ReadBE<int>();
            offset0Count = sr.ReadBE<int>();

            unkBytesOffset0 = sr.ReadBE<int>();
            unkCount0 = sr.ReadBE<int>();
            offsetTableOffset = sr.ReadBE<int>();
            offsetTableCount = sr.ReadBE<int>();

            unkBytesOffset1 = sr.ReadBE<int>();
            unkCount1 = sr.ReadBE<int>();
            int_28 = sr.ReadBE<int>();
            int_2C = sr.ReadBE<int>();

            int_30 = sr.ReadBE<int>();
            int_34 = sr.ReadBE<int>();
            int_38 = sr.ReadBE<int>();
            int_3C = sr.ReadBE<int>();
        }
    }
}
