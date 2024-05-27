using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Helpers.Readers;
using ArchiveLib;

namespace AquaModelLibrary.Data.BillyHatcher
{
    //Probably effect data, with animation and the like included.
    //Unclear where model data is pulled from or referenced.
    //Maybe the index before anim offsets is the model index?
    public class PolyAnim
    {
        public List<short> texShortsList = new List<short>();
        public List<DataSet1> dataSet1s = new List<DataSet1>();
        public List<DataSet2> dataSet2s = new List<DataSet2>();
        public NJTextureList texList = new NJTextureList();
        public PuyoFile gvm = null;

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

            //Offset 1
            sr.Seek(offset + offset1, SeekOrigin.Begin);
            for (int i = 0; i < count1; i++)
            {
                DataSet1 dataSet1 = new DataSet1();
                dataSet1.usesExtraData = sr.ReadBE<int>();
                dataSet1.offset = sr.ReadBE<int>();

                var bookmark = sr.Position;
                sr.Seek(offset + dataSet1.offset, SeekOrigin.Begin);
                if(dataSet1.usesExtraData > 1)
                {
                    throw new Exception();
                }
                if(dataSet1.usesExtraData == 1)
                {
                    dataSet1.data.bt0 = sr.ReadBE<byte>();
                    dataSet1.data.bt1 = sr.ReadBE<byte>();
                    dataSet1.data.bt2 = sr.ReadBE<byte>();
                    dataSet1.data.bt3 = sr.ReadBE<byte>();

                    dataSet1.data.bt4 = sr.ReadBE<byte>();
                    dataSet1.data.bt5 = sr.ReadBE<byte>();
                    dataSet1.data.bt6 = sr.ReadBE<byte>();
                    dataSet1.data.bt7 = sr.ReadBE<byte>();

                    dataSet1.data.bt8 = sr.ReadBE<byte>();
                    dataSet1.data.bt9 = sr.ReadBE<byte>();
                    dataSet1.data.btA = sr.ReadBE<byte>();
                    dataSet1.data.btB = sr.ReadBE<byte>();

                    dataSet1.data.offset0 = sr.ReadBE<int>();
                    dataSet1.data.offset1 = sr.ReadBE<int>();
                    dataSet1.data.mainOffset = sr.ReadBE<int>();
                    dataSet1.data.unkInt0 = sr.ReadBE<int>();

                    sr.Seek(offset + dataSet1.data.offset0, SeekOrigin.Begin);
                    dataSet1.data.unkInt1 = sr.ReadBE<int>();
                    dataSet1.data.unkInt2 = sr.ReadBE<int>();

                    sr.Seek(offset + dataSet1.data.offset1, SeekOrigin.Begin);
                    dataSet1.data.unkInt3 = sr.ReadBE<int>();

                    sr.Seek(offset + dataSet1.data.mainOffset, SeekOrigin.Begin);
                }
                dataSet1.data.mainInt0 = sr.ReadBE<int>();
                dataSet1.data.mainInt1 = sr.ReadBE<int>();

                dataSet1s.Add(dataSet1);
                sr.Seek(bookmark, SeekOrigin.Begin);
            }

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

            //Texlist + GVM offset
            sr.Seek(offset + texList_GvmOffset, SeekOrigin.Begin);
            var texShortCount = sr.ReadBE<short>();
            var unkTexShort = sr.ReadBE<short>();

            var texList_GvmOffset2 = sr.ReadBE<int>();
            for(int i = 0; i < texShortCount; i++)
            {
                texShortsList.Add(sr.ReadBE<short>());
            }

            sr.Seek(offset + texList_GvmOffset2, SeekOrigin.Begin);
            var texListOffset = sr.ReadBE<int>();
            var gvmOffset = sr.ReadBE<int>();

            sr.Seek(offset + texListOffset, SeekOrigin.Begin);
            texList = new NJTextureList(sr, offset);

            sr.Seek(offset + gvmOffset, SeekOrigin.Begin);
            gvm = new PuyoFile(GVMUtil.ReadGVMBytes(sr));
        }

        public class DataSet0
        {
            public int unkFlag;
            public int offset;

            public DataSet0Inner data = new DataSet0Inner();
        }

        public struct DataSet0Inner
        {
            /// <summary>
            /// Observed 0 and 3
            /// </summary>
            public ushort flags0;
            /// <summary>
            /// Observed 0x14 and 0x64
            /// </summary>
            public ushort flags1;
            public int int_04;
            public int int_08;
            public int offset0;
            public int offset1;

        }

        public class DataSet1
        {
            /// <summary>
            /// If 0, offset data is only 8 bytes. If 1, it is 0x30 total.
            /// </summary>
            public int usesExtraData;
            public int offset;

            public DataSet1Inner data = new DataSet1Inner();
        }

        public struct DataSet1Inner
        {
            public byte bt0;
            public byte bt1;
            public byte bt2;
            public byte bt3;

            public byte bt4;
            public byte bt5;
            public byte bt6;
            public byte bt7;

            public byte bt8;
            public byte bt9;
            public byte btA;
            public byte btB;

            public int offset0;
            public int offset1;
            public int mainOffset;
            public int unkInt0;

            //Offset0
            public int unkInt1;
            public int unkInt2;

            //Offset1
            public int unkInt3;

            //MainOffset - This is the only data if useExtraData for its reference is 1
            public int mainInt0;
            public int mainInt1;
        }

        public class DataSet2
        {
            public ushort dataCount;
            public ushort unk0;
            public int dataOffset;

            /// <summary>
            /// The shorts here should be unique. Probably ids for something or other in the polyanim.
            /// </summary>
            public List<short> dataList = new List<short>();
        }
    }
}
