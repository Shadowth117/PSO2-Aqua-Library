using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BillyHatcher
{
    //Probably effect data, with animation and the like included.
    //Unclear where model data is pulled from or referenced.
    //Maybe the index before anim offsets is the model index?
    public class PolyAnim
    {
        public PolyAnim() { }

        public PolyAnim(byte[] file, int offset)
        {
            Read(file, offset);
        }

        public PolyAnim(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            Read(sr, offset);
        }

        public void Read(byte[] file, int offset)
        {
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr, offset);
            }
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            //Read PolyAnim Header
            var int00 = sr.ReadBE<int>();
            var int04 = sr.ReadBE<int>();
            var count0 = sr.ReadBE<int>();
            var offset0 = sr.ReadBE<int>();

            var count1 = sr.ReadBE<int>();
            var offset1 = sr.ReadBE<int>();
            var int18 = sr.ReadBE<int>();
            var int1C = sr.ReadBE<int>();

            var count2 = sr.ReadBE<int>();
            var offset2 = sr.ReadBE<int>();
            var int28 = sr.ReadBE<int>();
            var texList_GvmOffset = sr.ReadBE<int>();

            var int30 = sr.ReadBE<int>();

            //Offset 2
            sr.Seek(offset + offset2, SeekOrigin.Begin);
            for(int i = 0; i < count2; i++)
            {
                DataSet2 dataSet2 = new DataSet2();
                dataSet2.dataCount = sr.ReadBE<ushort>();
                dataSet2.unk0 = sr.ReadBE<ushort>();
                dataSet2.dataOffset = sr.ReadBE<int>();

                var bookmark = sr.Position;
                sr.Seek(offset + dataSet2.dataOffset, SeekOrigin.Begin);
                for(int j = 0; j < dataSet2.dataCount; j++)
                {
                    dataSet2.dataList.Add(sr.ReadBE<short>());
                }

                sr.Seek(bookmark, SeekOrigin.Begin);
            }
        }

        public List<DataSet2> dataSet2s = new List<DataSet2>();
        public class DataSet2
        {
            public ushort dataCount;
            public ushort unk0;
            public int dataOffset;

            /// <summary>
            /// This shorts here should be unique. Probably ids for something or other in the polyanim.
            /// </summary>
            public List<short> dataList = new List<short>();
        }
    }
}
