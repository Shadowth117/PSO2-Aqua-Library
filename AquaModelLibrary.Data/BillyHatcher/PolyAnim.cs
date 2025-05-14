using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using ArchiveLib;
using System.Numerics;

namespace AquaModelLibrary.Data.BillyHatcher
{
    //Probably effect data, with animation and the like included.
    //Unclear where model data is pulled from or referenced.
    //Maybe the index before anim offsets is the model index?
    public class PolyAnim
    {
        public List<short> texShortsList = new List<short>();
        public List<DataSet0> dataSet0s = new List<DataSet0>();
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

            //Offset 0
            sr.Seek(offset + offset0, SeekOrigin.Begin);
            for(int i = 0; i < count0; i++)
            {
                DataSet0 set = new DataSet0();
                set.variant = sr.ReadBE<int>();
                set.offset = sr.ReadBE<int>();

                var bookmark = sr.Position;

                if(set.offset != -1 && set.variant != -1)
                {
                    sr.Seek(offset + set.offset, SeekOrigin.Begin);
                    switch(set.variant)
                    {
                        case 0:
                            set.data = new DS0Var0(sr, offset);
                            break;
                        case 1:
                        case 2:
                        case 5:
                        case 6:
                        case 7:
                            set.data = new DS0Var1(sr, offset);
                            break;
                        case 3:
                            set.data = new DS0Var3(sr, offset);
                            break;
                        default:
                            throw new Exception("Unexpected DS0 type");

                    }
                }

                dataSet0s.Add(set);
                sr.Seek(bookmark, SeekOrigin.Begin);
            }

            //Offset 1
            sr.Seek(offset + offset1, SeekOrigin.Begin);
            for (int i = 0; i < count1; i++)
            {
                DataSet1 dataSet1 = new DataSet1();
                dataSet1.usesExtraData = sr.ReadBE<int>();
                dataSet1.offset = sr.ReadBE<int>();

                var bookmark = sr.Position;
                sr.Seek(offset + dataSet1.offset, SeekOrigin.Begin);
                if(dataSet1.offset != 0)
                {
                    dataSet1.data = new DataSet1Inner();
                    if (dataSet1.usesExtraData > 1)
                    {
                        throw new Exception();
                    }
                    if (dataSet1.usesExtraData == 1)
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
                        var offset0Count = (dataSet1.data.offset1 - dataSet1.data.offset0) / 4;
                        for (int j = 0; j < offset0Count; j++)
                        {
                            dataSet1.data.unkIntList0.Add(sr.ReadBE<int>());
                        }

                        sr.Seek(offset + dataSet1.data.offset1, SeekOrigin.Begin);
                        var offset1Count = (dataSet1.data.mainOffset - dataSet1.data.offset1) / 4;
                        for(int j = 0; j < offset1Count; j++)
                        {
                            dataSet1.data.unkIntList1.Add(sr.ReadBE<int>());
                        }

                        sr.Seek(offset + dataSet1.data.mainOffset, SeekOrigin.Begin);
                        dataSet1.data.mainInts.Add(sr.ReadBE<int>());
                        if (dataSet1.data.bt7 == 0x2)
                        {
                            dataSet1.data.mainInts.Add(sr.ReadBE<int>());
                        }
                    } else
                    {
                        dataSet1.data.mainInts.Add(sr.ReadBE<int>());
                        dataSet1.data.mainInts.Add(sr.ReadBE<int>());
                    }
                }

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
                dataSet2s.Add(dataSet2);
                sr.Seek(bookmark, SeekOrigin.Begin);
            }

            //Texlist + GVM offset
            sr.Seek(offset + texList_GvmOffset, SeekOrigin.Begin);
            var texListOffset = sr.ReadBE<int>();
            var gvmOffset = sr.ReadBE<int>();

            sr.Seek(offset + texListOffset, SeekOrigin.Begin);
            texList = new NJTextureList(sr, offset);

            sr.Seek(offset + gvmOffset, SeekOrigin.Begin);
            gvm = new PuyoFile(GVMUtil.ReadGVMBytes(sr));
        }

        public void Write(List<byte> outBytes, List<int> pofSets)
        {
            //Write PolyAnim Header
            pofSets.Add(outBytes.Count + 0xC);
            pofSets.Add(outBytes.Count + 0x14);
            pofSets.Add(outBytes.Count + 0x24);
            pofSets.Add(outBytes.Count + 0x2C);

            outBytes.AddValue((int)0);
            outBytes.AddValue((int)0);
            outBytes.AddValue(dataSet0s.Count);
            outBytes.ReserveInt("DataSet0sOffset");

            outBytes.AddValue(dataSet1s.Count);
            outBytes.ReserveInt("DataSet1sOffset");
            outBytes.AddValue((int)0);
            outBytes.AddValue((int)0);

            outBytes.AddValue(dataSet2s.Count);
            outBytes.ReserveInt("DataSet2sOffset");
            outBytes.AddValue((int)0);
            outBytes.ReserveInt("TexListGVMOffset");

            outBytes.AddValue((int)0);

            //Write Data0
            outBytes.FillInt("DataSet0sOffset", outBytes.Count);
            for(int i = 0; i < dataSet0s.Count; i++)
            {
                outBytes.AddValue(dataSet0s[i].variant);
                if (dataSet0s[i].variant != -1 && dataSet0s[i].data != null)
                {
                    pofSets.Add(outBytes.Count);
                }
                outBytes.ReserveInt($"DS0Offset{i}");
            }
            for (int i = 0; i < dataSet0s.Count; i++)
            {
                if (dataSet0s[i].variant != -1 && dataSet0s[i].data != null)
                {
                    outBytes.FillInt($"DS0Offset{i}", outBytes.Count);
                    dataSet0s[i].data.Write(outBytes, pofSets);
                }
            }

            //Write Data1
            outBytes.FillInt("DataSet1sOffset", outBytes.Count);
            for (int i = 0; i < dataSet1s.Count; i++)
            {
                outBytes.AddValue(dataSet1s[i].usesExtraData);
                if (dataSet1s[i].data != null)
                {
                    pofSets.Add(outBytes.Count);
                }
                outBytes.ReserveInt($"DS1Offset{i}");
            }
            for (int i = 0; i < dataSet1s.Count; i++)
            {
                if (dataSet1s[i].data != null)
                {
                    outBytes.FillInt($"DS1Offset{i}", outBytes.Count);
                    if (dataSet1s[i].usesExtraData == 1)
                    {
                        outBytes.Add(dataSet1s[i].data.bt0);
                        outBytes.Add(dataSet1s[i].data.bt1);
                        outBytes.Add(dataSet1s[i].data.bt2);
                        outBytes.Add(dataSet1s[i].data.bt3);

                        outBytes.Add(dataSet1s[i].data.bt4);
                        outBytes.Add(dataSet1s[i].data.bt5);
                        outBytes.Add(dataSet1s[i].data.bt6);
                        outBytes.Add(dataSet1s[i].data.bt7);

                        outBytes.Add(dataSet1s[i].data.bt8);
                        outBytes.Add(dataSet1s[i].data.bt9);
                        outBytes.Add(dataSet1s[i].data.btA);
                        outBytes.Add(dataSet1s[i].data.btB);

                        pofSets.Add(outBytes.Count);
                        outBytes.ReserveInt($"DS1DataOffset0{i}");
                        pofSets.Add(outBytes.Count);
                        outBytes.ReserveInt($"DS1DataOffset1{i}");
                        pofSets.Add(outBytes.Count);
                        outBytes.ReserveInt($"DS1DataMainOffset{i}");
                        outBytes.AddValue(dataSet1s[i].data.unkInt0);

                        outBytes.FillInt($"DS1DataOffset0{i}", outBytes.Count);
                        for (int j = 0; j < dataSet1s[i].data.unkIntList0.Count; j++)
                        {
                            outBytes.AddValue(dataSet1s[i].data.unkIntList0[j]);
                        }
                        outBytes.FillInt($"DS1DataOffset1{i}", outBytes.Count);
                        for(int j = 0; j < dataSet1s[i].data.unkIntList1.Count; j++)
                        {
                            outBytes.AddValue(dataSet1s[i].data.unkIntList1[j]);
                        }
                        outBytes.FillInt($"DS1DataMainOffset{i}", outBytes.Count);
                    }
                    foreach(var value in dataSet1s[i].data.mainInts)
                    {
                        outBytes.AddValue(value);
                    }
                }
            }

            //Write Data2
            outBytes.FillInt("DataSet2sOffset", outBytes.Count);
            for (int i = 0; i < dataSet2s.Count; i++)
            {
                outBytes.AddValue((ushort)dataSet2s[i].dataList.Count);
                outBytes.AddValue(dataSet2s[i].unk0);
                if (dataSet2s[i].dataList.Count > 0)
                {
                    pofSets.Add(outBytes.Count);
                }
                outBytes.ReserveInt($"DS2Offset{i}");
            }
            for (int i = 0; i < dataSet2s.Count; i++)
            {
                outBytes.FillInt($"DS2Offset{i}", outBytes.Count);
                for(int j = 0; j < dataSet2s[i].dataList.Count; j++)
                {
                    outBytes.AddValue(dataSet2s[i].dataList[j]);
                }
            }

            //Write Texlist + GVM
            outBytes.FillInt("TexListGVMOffset", outBytes.Count);

            pofSets.Add(outBytes.Count);
            outBytes.ReserveInt($"TexListOffset");
            pofSets.Add(outBytes.Count);
            outBytes.ReserveInt($"GVMOffset");

            outBytes.FillInt($"TexListOffset", outBytes.Count);
            texList.Write(outBytes, pofSets);
            outBytes.AlignWriter(0x20);

            outBytes.AlignWriter(0x20);
            outBytes.FillInt($"GVMOffset", outBytes.Count);
            gvm.forcedPad = 0x10;
            outBytes.AddRange(gvm.GetBytes());
        }

        public class DataSet0
        {
            public int variant;
            public int offset;

            public DS0Variant data = null;
        }

        public class DS0Variant
        {
            public virtual void Write(List<byte> outBytes, List<int> pofSets)
            {

            }
        }


        public class DS0Var0 : DS0Variant
        {
            /// <summary>
            /// Observed 0 ,3, 4, 5
            /// </summary>
            public ushort flags0;
            /// <summary>
            /// Observed 0x14, 0x64
            /// </summary>
            public ushort flags1;
            public ushort flags2;
            public ushort flags3;
            public ushort flags4;
            public ushort flags5;
            public int chunksOffset;
            public int floatListThingOffset;

            //Float List Thing
            public ushort FLTFlag0;
            public ushort FLTFlag1;
            public int FLTInt_04;
            public float FLTFlt_08;

            public List<FLTPair> fltPairs = new List<FLTPair>();

            public struct FLTPair
            {
                public float FLTFlt;
                public ushort FLTusht0;
                public ushort FLTusht1;
            }

            public DataChunks dataChunks = null;

            public DS0Var0() { }
            public DS0Var0(BufferedStreamReaderBE<MemoryStream> sr, int offset)
            {
                flags0 = sr.ReadBE<ushort>();
                flags1 = sr.ReadBE<ushort>();
                flags2 = sr.ReadBE<ushort>();
                flags3 = sr.ReadBE<ushort>();
                flags4 = sr.ReadBE<ushort>();
                flags5 = sr.ReadBE<ushort>();
                chunksOffset = sr.ReadBE<int>();
                floatListThingOffset = sr.ReadBE<int>();

                if(chunksOffset != 0)
                {
                    sr.Seek(offset + chunksOffset, SeekOrigin.Begin);
                    dataChunks = new DataChunks(sr);
                }

                if(floatListThingOffset != 0)
                {
                    sr.Seek(offset + floatListThingOffset, SeekOrigin.Begin);
                    FLTFlag0 = sr.ReadBE<ushort>();
                    FLTFlag1 = sr.ReadBE<ushort>();
                    FLTInt_04 = sr.ReadBE<int>();
                    FLTFlt_08 = sr.ReadBE<float>();

                    int count = (chunksOffset - floatListThingOffset - 0xC) / 8;
                    for (int j = 0; j < count; j++)
                    {
                        FLTPair fltPair = new FLTPair();
                        fltPair.FLTFlt = sr.ReadBE<float>();
                        fltPair.FLTusht0 = sr.ReadBE<ushort>();
                        fltPair.FLTusht1 = sr.ReadBE<ushort>();
                        fltPairs.Add(fltPair);
                    }
                }
            }

            public override void Write(List<byte> outBytes, List<int> pofSets)
            {
                outBytes.AddValue(flags0);
                outBytes.AddValue(flags1);
                outBytes.AddValue(flags2);
                outBytes.AddValue(flags3);
                outBytes.AddValue(flags4);
                outBytes.AddValue(flags5);

                if(dataChunks != null)
                {
                    pofSets.Add(outBytes.Count);
                }
                outBytes.ReserveInt("DS0ChunkOffset");
                if (fltPairs.Count > 0)
                {
                    pofSets.Add(outBytes.Count);
                }
                outBytes.ReserveInt("DS0FloatThingOffset");

                if (fltPairs.Count > 0)
                {
                    outBytes.FillInt("DS0FloatThingOffset", outBytes.Count);
                    outBytes.AddValue(FLTFlag0);
                    outBytes.AddValue(FLTFlag1);
                    outBytes.AddValue(FLTInt_04);
                    outBytes.AddValue(FLTFlt_08);

                    for(int j = 0; j < fltPairs.Count; j++)
                    {
                        var fltPair = fltPairs[j];
                        outBytes.AddValue(fltPair.FLTFlt);
                        outBytes.AddValue(fltPair.FLTusht0);
                        outBytes.AddValue(fltPair.FLTusht1);
                    }
                }
                if (dataChunks != null)
                {
                    outBytes.FillInt("DS0ChunkOffset", outBytes.Count);
                    outBytes.AddRange(dataChunks.GetBytes());
                }
                outBytes.AlignWriter(0x4);
            }
        }
        public class DS0Var1 : DS0Variant
        {
            public ushort flags0;
            public ushort flags1;
            public int int_04;
            public int int_08;
            public ushort flags2;
            public ushort flags3;

            public float flt_10;
            public float flt_14;
            public float flt_18;
            public float flt_1C;

            public float flt_20;
            public int int_24;
            public int int_28;
            public int dataChunks0Offset;

            public ushort flags_30;
            public ushort flags_32;
            public int int_34;
            public float flt_38;
            public int int_3C;

            public int int_40;
            public float flt_44;
            public float flt_48;
            public int dataChunks1Offset;

            public DataChunks dataChunks0 = null;
            public DataChunks dataChunks1 = null;

            public DS0Var1() { }
            public DS0Var1(BufferedStreamReaderBE<MemoryStream> sr, int offset)
            {
                flags0 = sr.ReadBE<ushort>();
                flags1 = sr.ReadBE<ushort>();
                int_04 = sr.ReadBE<int>();
                int_08 = sr.ReadBE<int>();
                flags2 = sr.ReadBE<ushort>();
                flags3 = sr.ReadBE<ushort>();

                flt_10 = sr.ReadBE<float>();
                flt_14 = sr.ReadBE<float>();
                flt_18 = sr.ReadBE<float>();
                flt_1C = sr.ReadBE<float>();

                flt_20 = sr.ReadBE<float>();
                int_24 = sr.ReadBE<int>();
                int_28 = sr.ReadBE<int>();
                dataChunks0Offset = sr.ReadBE<int>();

                flags_30 = sr.ReadBE<ushort>();
                flags_32 = sr.ReadBE<ushort>();
                int_34 = sr.ReadBE<int>();
                flt_38 = sr.ReadBE<float>();
                int_3C = sr.ReadBE<int>();

                int_40 = sr.ReadBE<int>();
                flt_44 = sr.ReadBE<float>();
                flt_48 = sr.ReadBE<float>();
                dataChunks1Offset = sr.ReadBE<int>();

                if(dataChunks0Offset != 0)
                {
                    sr.Seek(offset + dataChunks0Offset, SeekOrigin.Begin);
                    dataChunks0 = new DataChunks(sr);
                }
                if(dataChunks1Offset != 0)
                {
                    sr.Seek(offset + dataChunks1Offset, SeekOrigin.Begin);
                    dataChunks1 = new DataChunks(sr);
                }
            }
            public override void Write(List<byte> outBytes, List<int> pofSets)
            {
                outBytes.AddValue(flags0);
                outBytes.AddValue(flags1);
                outBytes.AddValue(int_04);
                outBytes.AddValue(int_08);
                outBytes.AddValue(flags2);
                outBytes.AddValue(flags3);

                outBytes.AddValue(flt_10);
                outBytes.AddValue(flt_14);
                outBytes.AddValue(flt_18);
                outBytes.AddValue(flt_1C);

                outBytes.AddValue(flt_20);
                outBytes.AddValue(int_24);
                outBytes.AddValue(int_28);
                if(dataChunks0 != null)
                {
                    pofSets.Add(outBytes.Count);
                }
                outBytes.ReserveInt("DSV1DataChunks0");

                outBytes.AddValue(flags_30);
                outBytes.AddValue(flags_32);
                outBytes.AddValue(int_34);
                outBytes.AddValue(flt_38);
                outBytes.AddValue(int_3C);

                outBytes.AddValue(int_40);
                outBytes.AddValue(flt_44);
                outBytes.AddValue(flt_48);
                if (dataChunks1 != null)
                {
                    pofSets.Add(outBytes.Count);
                }
                outBytes.ReserveInt("DSV1DataChunks1");

                if(dataChunks0 != null)
                {
                    outBytes.FillInt("DSV1DataChunks0", outBytes.Count);
                    outBytes.AddRange(dataChunks0.GetBytes());
                }
                if (dataChunks1 != null)
                {
                    outBytes.FillInt("DSV1DataChunks1", outBytes.Count);
                    outBytes.AddRange(dataChunks1.GetBytes());
                }
                outBytes.AlignWriter(0x4);
            }
        }
        public class DS0Var3 : DS0Variant
        {
            public ushort flags0;
            public ushort flags1;
            public int int_04;
            public int dataChunksOffset;

            public DataChunks dataChunks = null;

            public DS0Var3() { }
            public DS0Var3(BufferedStreamReaderBE<MemoryStream> sr, int offset)
            {
                flags0 = sr.ReadBE<ushort>();
                flags1 = sr.ReadBE<ushort>();
                int_04 = sr.ReadBE<int>();
                dataChunksOffset = sr.ReadBE<int>();

                if(dataChunksOffset != 0)
                {
                    sr.Seek(offset + dataChunksOffset, SeekOrigin.Begin);
                    dataChunks = new DataChunks(sr);
                }
            }
            public override void Write(List<byte> outBytes, List<int> pofSets)
            {
                outBytes.AddValue(flags0);
                outBytes.AddValue(flags1);
                outBytes.AddValue(int_04);
                if (dataChunks != null)
                {
                    pofSets.Add(outBytes.Count);
                }
                outBytes.ReserveInt("DSV1DataChunks");

                if (dataChunks != null)
                {
                    outBytes.FillInt("DSV1DataChunks", outBytes.Count);
                    outBytes.AddRange(dataChunks.GetBytes());
                }
                outBytes.AlignWriter(0x4);
            }
        }

        //These definitely aren't entirely right, but they get read enough to be able to write them back
        public class DataChunks
        {
            public List<DataChunk> chunks = new List<DataChunk>();
            public DataChunks() { }
            public DataChunks(BufferedStreamReaderBE<MemoryStream> sr)
            {
                bool keepReading = true;
                while (keepReading && sr.Position < sr.BaseStream.Length)
                {
                    var chunkType = sr.Read<byte>();
                    switch (chunkType)
                    {
                        case 0x00:
                            chunks.Add(new Chunk_00());
                            break;
                        case 0x01:
                            chunks.Add(new Chunk_01());
                            break;
                        case 0x02:
                            chunks.Add(new Chunk_02());
                            break;
                        case 0x03:
                            chunks.Add(new Chunk_03());
                            break;
                        case 0x04:
                            chunks.Add(new Chunk_04());
                            break;
                        case 0x05:
                            chunks.Add(new Chunk_05());
                            break;
                        case 0x06:
                            chunks.Add(new Chunk_06());
                            break;
                        case 0x07:
                            chunks.Add(new Chunk_07());
                            break;
                        case 0x08:
                            chunks.Add(new Chunk_08());
                            break;
                        case 0x0A:
                            chunks.Add(new Chunk_0A());
                            break;
                        case 0x0C:
                            chunks.Add(new Chunk_0C());
                            break;
                        case 0x0E:
                            chunks.Add(new Chunk_0E());
                            break;
                        case 0x0F:
                            chunks.Add(new Chunk_0F());
                            break;
                        case 0x13:
                            chunks.Add(new Chunk_13());
                            break;
                        case 0x14:
                            chunks.Add(new Chunk_14());
                            break;
                        case 0x15:
                            chunks.Add(new Chunk_15());
                            break;
                        case 0x16:
                            chunks.Add(new Chunk_16());
                            break;
                        case 0x19:
                            chunks.Add(new Chunk_19());
                            break;
                        case 0x1B:
                            chunks.Add(new Chunk_1B());
                            break;
                        case 0x1E:
                            chunks.Add(new Chunk_1E());
                            break;
                        case 0x20:
                            chunks.Add(new Chunk_20(sr));
                            break;
                        case 0x28:
                            chunks.Add(new Chunk_28());
                            break;
                        case 0x2C:
                            chunks.Add(new Chunk_2C());
                            break;
                        case 0x41:
                            chunks.Add(new Chunk_41(sr));
                            break;
                        case 0x42:
                            chunks.Add(new Chunk_42(sr));
                            break;
                        case 0x43:
                            chunks.Add(new Chunk_43(sr));
                            break;
                        case 0x44:
                            chunks.Add(new Chunk_44(sr));
                            break;
                        case 0x61:
                            chunks.Add(new Chunk_61());
                            break;
                        case 0x80:
                            chunks.Add(new Chunk_80(sr));
                            break;
                        case 0x81:
                            chunks.Add(new Chunk_81(sr));
                            break;
                        case 0x82:
                            chunks.Add(new Chunk_82(sr));
                            break;
                        case 0x85:
                            chunks.Add(new Chunk_85(sr));
                            break;
                        case 0x86:
                            chunks.Add(new Chunk_86(sr));
                            break;
                        case 0x87:
                            chunks.Add(new Chunk_87(sr));
                            break;
                        case 0x88:
                            chunks.Add(new Chunk_88(sr));
                            break;
                        case 0x89:
                            chunks.Add(new Chunk_89(sr));
                            break;
                        case 0x8A:
                            chunks.Add(new Chunk_8A(sr));
                            break;
                        case 0x8B:
                            chunks.Add(new Chunk_8B(sr));
                            break;
                        case 0x8C:
                            chunks.Add(new Chunk_8C(sr));
                            break;
                        case 0x8D:
                            chunks.Add(new Chunk_8D(sr));
                            break;
                        case 0x90:
                            chunks.Add(new Chunk_90(sr));
                            break;
                        case 0x91:
                            chunks.Add(new Chunk_91(sr));
                            break;
                        case 0x92:
                            chunks.Add(new Chunk_92(sr));
                            break;
                        case 0x93:
                            chunks.Add(new Chunk_93(sr));
                            break;
                        case 0x94:
                            chunks.Add(new Chunk_94(sr));
                            break;
                        case 0x96:
                            chunks.Add(new Chunk_96(sr));
                            break;
                        case 0x98:
                            chunks.Add(new Chunk_98(sr));
                            break;
                        case 0x99:
                            chunks.Add(new Chunk_99(sr));
                            break;
                        case 0x9A:
                            chunks.Add(new Chunk_9A(sr));
                            break;
                        case 0xA2:
                            chunks.Add(new Chunk_A2(sr));
                            break;
                        case 0xA3:
                            chunks.Add(new Chunk_A3(sr));
                            break;
                        case 0xC0:
                            chunks.Add(new Chunk_C0());
                            break;
                        case 0xC7:
                            chunks.Add(new Chunk_C7(sr));
                            break;
                        case 0xC8:
                            chunks.Add(new Chunk_C8());
                            break;
                        case 0xCF:
                            chunks.Add(new Chunk_CF(sr));
                            break;
                        case 0xD0:
                            chunks.Add(new Chunk_D0(sr));
                            break;
                        case 0xD1:
                            chunks.Add(new Chunk_D1(sr));
                            break;
                        case 0xD3:
                            chunks.Add(new Chunk_D3(sr));
                            break;
                        case 0xD4:
                            chunks.Add(new Chunk_D4(sr));
                            break;
                        case 0xD5:
                            chunks.Add(new Chunk_D5(sr));
                            break;
                        case 0xD6:
                            chunks.Add(new Chunk_D6(sr));
                            break;
                        case 0xD7:
                            chunks.Add(new Chunk_D7(sr));
                            break;
                        case 0xDF:
                            chunks.Add(new Chunk_DF(sr));
                            break;
                        case 0xE0:
                            chunks.Add(new Chunk_E0(sr));
                            break;
                        case 0xE1:
                            chunks.Add(new Chunk_E1(sr));
                            break;
                        case 0xE2:
                            chunks.Add(new Chunk_E2(sr));
                            break;
                        case 0xE7:
                            chunks.Add(new Chunk_E7(sr));
                            break;
                        case 0xF0:
                            chunks.Add(new Chunk_F0(sr));
                            break;
                        case 0xF1:
                            chunks.Add(new Chunk_F1());
                            break;
                        case 0xF2:
                            chunks.Add(new Chunk_F2());
                            break;
                        case 0xF3:
                            chunks.Add(new Chunk_F3());
                            break;
                        case 0xF4:
                            chunks.Add(new Chunk_F4(sr));
                            break;
                        case 0xF6:
                            chunks.Add(new Chunk_F6(sr));
                            break;
                        case 0xF9:
                            chunks.Add(new Chunk_F9(sr));
                            break;
                        case 0xFA:
                            chunks.Add(new Chunk_FA(sr));
                            break;
                        case 0xFF:
                            chunks.Add(new Chunk_FF());
                            keepReading = false;
                            break;
                        default:
                            throw new Exception($"Unexpected chunk type {chunkType:X} at {(sr.Position - 1):X}");
                    }
                }
            }

            public byte[] GetBytes()
            {
                List<byte> outBytes = new List<byte>();
                foreach(var chunk in chunks)
                {
                    outBytes.AddRange(chunk.GetBytes());
                }

                return outBytes.ToArray();
            }

            public abstract class DataChunk 
            {
                public abstract byte[] GetBytes();
            }

            public class Chunk_00 : DataChunk
            {
                public Chunk_00() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0x00 };
                }
            }
            public class Chunk_01 : DataChunk
            {
                public Chunk_01() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0x01 };
                }
            }
            public class Chunk_02 : DataChunk
            {
                public Chunk_02() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0x02 };
                }
            }
            public class Chunk_03 : DataChunk
            {
                public Chunk_03() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0x03 };
                }
            }
            public class Chunk_04 : DataChunk
            {
                public Chunk_04() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0x04 };
                }
            }
            public class Chunk_05 : DataChunk
            {
                public Chunk_05() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0x05 };
                }
            }
            public class Chunk_06 : DataChunk
            {
                public Chunk_06() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0x06 };
                }
            }
            public class Chunk_07 : DataChunk
            {
                public Chunk_07() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0x07 };
                }
            }
            public class Chunk_08 : DataChunk
            {
                public Chunk_08() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0x08 };
                }
            }
            public class Chunk_0A : DataChunk
            {
                public Chunk_0A() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0x0A };
                }
            }
            public class Chunk_0C : DataChunk
            {
                public Chunk_0C() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0x0C };
                }
            }
            public class Chunk_0E : DataChunk
            {
                public Chunk_0E() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0x0E };
                }
            }
            public class Chunk_0F : DataChunk
            {
                public Chunk_0F() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0x0F };
                }
            }
            public class Chunk_13 : DataChunk
            {
                public Chunk_13() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0x13 };
                }
            }
            public class Chunk_14 : DataChunk
            {
                public Chunk_14() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0x14 };
                }
            }
            public class Chunk_15 : DataChunk
            {
                public Chunk_15() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0x15 };
                }
            }
            public class Chunk_16 : DataChunk
            {
                public Chunk_16() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0x16 };
                }
            }
            public class Chunk_19 : DataChunk
            {
                public Chunk_19() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0x19 };
                }
            }
            public class Chunk_1B : DataChunk
            {
                public Chunk_1B() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0x1B };
                }
            }
            public class Chunk_1E : DataChunk
            {
                public Chunk_1E() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0x1E };
                }
            }
            public class Chunk_20 : DataChunk
            {
                public byte bt_0;

                public Chunk_20() { }
                public Chunk_20(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    bt_0 = sr.ReadBE<byte>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x20);
                    outBytes.Add(bt_0);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_28 : DataChunk
            {
                public Chunk_28() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0x28 };
                }
            }
            public class Chunk_2C : DataChunk
            {
                public Chunk_2C() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0x2C };
                }
            }
            public class Chunk_41 : DataChunk
            {
                public byte bt_0;

                public Chunk_41() { }
                public Chunk_41(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    bt_0 = sr.ReadBE<byte>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x41);
                    outBytes.Add(bt_0);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_42 : DataChunk
            {
                public byte bt_0;

                public Chunk_42() { }
                public Chunk_42(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    bt_0 = sr.ReadBE<byte>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x42);
                    outBytes.Add(bt_0);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_43 : DataChunk
            {
                public byte bt_0;

                public Chunk_43() { }
                public Chunk_43(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    bt_0 = sr.ReadBE<byte>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x43);
                    outBytes.Add(bt_0);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_44 : DataChunk
            {
                public byte bt_0;

                public Chunk_44() { }
                public Chunk_44(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    bt_0 = sr.ReadBE<byte>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x44);
                    outBytes.Add(bt_0);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_61 : DataChunk
            {
                public Chunk_61() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0x61 };
                }
            }
            public class Chunk_80 : DataChunk
            {
                public byte bt_0;
                public byte bt_1;
                public byte bt_2;
                public byte bt_3;
                public byte bt_4;

                public Chunk_80() { }
                public Chunk_80(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    bt_0 = sr.ReadBE<byte>();
                    bt_1 = sr.ReadBE<byte>();
                    bt_2 = sr.ReadBE<byte>();
                    bt_3 = sr.ReadBE<byte>();
                    bt_4 = sr.ReadBE<byte>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x80);
                    outBytes.Add(bt_0);
                    outBytes.Add(bt_1);
                    outBytes.Add(bt_2);
                    outBytes.Add(bt_3);
                    outBytes.Add(bt_4);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_81 : DataChunk
            {
                public byte bt_0;
                public byte bt_1;

                public Chunk_81() { }
                public Chunk_81(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    bt_0 = sr.ReadBE<byte>();
                    bt_1 = sr.ReadBE<byte>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x81);
                    outBytes.Add(bt_0);
                    outBytes.Add(bt_1);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_82 : DataChunk
            {
                public byte bt_0;

                public Chunk_82() { }
                public Chunk_82(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    bt_0 = sr.ReadBE<byte>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x82);
                    outBytes.Add(bt_0);

                    return outBytes.ToArray();
                }
            }

            public class Chunk_85 : DataChunk
            {
                public byte bt_0;
                public byte bt_1;
                public byte bt_2;
                public byte bt_3;

                public Chunk_85() { }
                public Chunk_85(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    bt_0 = sr.ReadBE<byte>();
                    bt_1 = sr.ReadBE<byte>();
                    bt_2 = sr.ReadBE<byte>();
                    bt_3 = sr.ReadBE<byte>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x85);
                    outBytes.Add(bt_0);
                    outBytes.Add(bt_1);
                    outBytes.Add(bt_2);
                    outBytes.Add(bt_3);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_86 : DataChunk
            {
                public byte bt_0;
                public byte bt_1;

                public Chunk_86() { }
                public Chunk_86(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    bt_0 = sr.ReadBE<byte>();
                    bt_1 = sr.ReadBE<byte>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x86);
                    outBytes.Add(bt_0);
                    outBytes.Add(bt_1);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_87 : DataChunk
            {
                public byte bt_0;

                public Chunk_87() { }
                public Chunk_87(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    bt_0 = sr.ReadBE<byte>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x87);
                    outBytes.Add(bt_0);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_88 : DataChunk
            {
                public byte bt_0;

                public Chunk_88() { }
                public Chunk_88(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    bt_0 = sr.ReadBE<byte>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x88);
                    outBytes.Add(bt_0);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_89 : DataChunk
            {
                public byte bt_0;
                public float unkFlt_0;
                public float unkFlt_1;

                public Chunk_89() { }
                public Chunk_89(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    bt_0 = sr.ReadBE<byte>();
                    unkFlt_0 = sr.ReadBE<float>();
                    unkFlt_1 = sr.ReadBE<float>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x89);
                    outBytes.Add(bt_0);
                    outBytes.AddValue(unkFlt_0);
                    outBytes.AddValue(unkFlt_1);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_8A : DataChunk
            {
                public byte bt_0;
                public byte bt_1;
                public byte bt_2;
                public byte bt_3;

                public Chunk_8A() { }
                public Chunk_8A(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    bt_0 = sr.ReadBE<byte>();
                    bt_1 = sr.ReadBE<byte>();
                    bt_2 = sr.ReadBE<byte>();
                    bt_3 = sr.ReadBE<byte>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x8A);
                    outBytes.Add(bt_0);
                    outBytes.Add(bt_1);
                    outBytes.Add(bt_2);
                    outBytes.Add(bt_3);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_8B : DataChunk
            {
                public byte bt_0;
                public byte bt_1;
                public byte bt_2;

                public Chunk_8B() { }
                public Chunk_8B(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    bt_0 = sr.ReadBE<byte>();
                    bt_1 = sr.ReadBE<byte>();
                    bt_2 = sr.ReadBE<byte>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x8B);
                    outBytes.Add(bt_0);
                    outBytes.Add(bt_1);
                    outBytes.Add(bt_2);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_8C : DataChunk
            {
                public float unkFlt0;

                public Chunk_8C() { }
                public Chunk_8C(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    unkFlt0 = sr.ReadBE<float>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x8C);
                    outBytes.AddValue(unkFlt0);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_8D : DataChunk
            {
                public float unkFlt0;

                public Chunk_8D() { }
                public Chunk_8D(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    unkFlt0 = sr.ReadBE<float>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x8D);
                    outBytes.AddValue(unkFlt0);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_90 : DataChunk 
            {
                public byte counter;
                public float unkFlt0;

                public Chunk_90() { }
                public Chunk_90(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    counter = sr.ReadBE<byte>();
                    unkFlt0 = sr.ReadBE<float>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x90);
                    outBytes.Add(counter);
                    outBytes.AddValue(unkFlt0);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_91 : DataChunk
            {
                public byte counter;
                public byte unkBt0;
                public float unkFlt0;

                public Chunk_91() { }
                public Chunk_91(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    counter = sr.ReadBE<byte>();
                    unkBt0 = sr.ReadBE<byte>();
                    unkFlt0 = sr.ReadBE<float>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x91);
                    outBytes.Add(counter);
                    outBytes.Add(unkBt0);
                    outBytes.AddValue(unkFlt0);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_92 : DataChunk
            {
                public byte counter;
                public byte unkBt0;
                public float unkFlt0;

                public Chunk_92() { }
                public Chunk_92(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    counter = sr.ReadBE<byte>();
                    unkBt0 = sr.ReadBE<byte>();
                    unkFlt0 = sr.ReadBE<float>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x92);
                    outBytes.Add(counter);
                    outBytes.Add(unkBt0);
                    outBytes.AddValue(unkFlt0);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_93 : DataChunk 
            {
                public byte counter;
                public byte unkBt0;
                public float unkFlt0;
                public float unkFlt1;

                public Chunk_93() { }
                public Chunk_93(BufferedStreamReaderBE<MemoryStream> sr) 
                {
                    counter = sr.ReadBE<byte>();
                    unkBt0 = sr.ReadBE<byte>();
                    unkFlt0 = sr.ReadBE<float>();
                    unkFlt1 = sr.ReadBE<float>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x93);
                    outBytes.Add(counter);
                    outBytes.Add(unkBt0);
                    outBytes.AddValue(unkFlt0);
                    outBytes.AddValue(unkFlt1);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_94 : DataChunk
            {
                public byte counter;
                public float unkFlt0;

                public Chunk_94() { }
                public Chunk_94(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    counter = sr.ReadBE<byte>();
                    unkFlt0 = sr.ReadBE<float>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x94);
                    outBytes.Add(counter);
                    outBytes.AddValue(unkFlt0);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_96 : DataChunk
            {
                public byte bt_1;

                public Chunk_96() { }
                public Chunk_96(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    bt_1 = sr.ReadBE<byte>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x96);
                    outBytes.Add(bt_1);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_98 : DataChunk
            {
                public byte counter;
                public byte bt_1;

                public Chunk_98() { }
                public Chunk_98(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    counter = sr.ReadBE<byte>();
                    bt_1 = sr.ReadBE<byte>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x98);
                    outBytes.Add(counter);
                    outBytes.Add(bt_1);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_99 : DataChunk
            {
                public byte counter;
                public float unkFloat;

                public Chunk_99() { }
                public Chunk_99(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    counter = sr.ReadBE<byte>();
                    unkFloat = sr.ReadBE<float>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x99);
                    outBytes.Add(counter);
                    outBytes.AddValue(unkFloat);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_9A : DataChunk
            {
                public byte counter;
                public byte bt_1;
                public float unkFloat;

                public Chunk_9A() { }
                public Chunk_9A(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    counter = sr.ReadBE<byte>();
                    bt_1 = sr.ReadBE<byte>();
                    unkFloat = sr.ReadBE<float>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0x9A);
                    outBytes.Add(counter);
                    outBytes.Add(bt_1);
                    outBytes.AddValue(unkFloat);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_A2 : DataChunk
            {
                public byte counter;
                public byte unkBt0;
                public float unkFlt0;

                public Chunk_A2() { }
                public Chunk_A2(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    counter = sr.ReadBE<byte>();
                    unkBt0 = sr.ReadBE<byte>();
                    unkFlt0 = sr.ReadBE<float>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0xA2);
                    outBytes.Add(counter);
                    outBytes.Add(unkBt0);
                    outBytes.AddValue(unkFlt0);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_A3 : DataChunk
            {
                public byte counter;
                public byte unkBt0;
                public float unkFlt0;
                public float unkFlt1;

                public Chunk_A3() { }
                public Chunk_A3(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    counter = sr.ReadBE<byte>();
                    unkBt0 = sr.ReadBE<byte>();
                    unkFlt0 = sr.ReadBE<float>();
                    unkFlt1 = sr.ReadBE<float>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0xA3);
                    outBytes.Add(counter);
                    outBytes.Add(unkBt0);
                    outBytes.AddValue(unkFlt0);
                    outBytes.AddValue(unkFlt1);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_C0 : DataChunk
            {
                public Chunk_C0() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0xC0 };
                }
            }
            public class Chunk_C7 : DataChunk
            {
                public Vector3 vec3_00;

                public Chunk_C7() { }
                public Chunk_C7(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    vec3_00 = sr.ReadBEV3();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0xC7);
                    outBytes.AddValue(vec3_00);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_C8 : DataChunk
            {
                public Chunk_C8() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0xC8 };
                }
            }
            public class Chunk_CF : DataChunk
            {
                public Vector3 vec3_00;

                public Chunk_CF() { }
                public Chunk_CF(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    vec3_00 = sr.ReadBEV3();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0xCF);
                    outBytes.AddValue(vec3_00);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_D0 : DataChunk
            {
                public float unkFlt0;

                public Chunk_D0() { }
                public Chunk_D0(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    unkFlt0 = sr.ReadBE<float>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0xD0);
                    outBytes.AddValue(unkFlt0);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_D1 : DataChunk
            {
                public byte unkByte;
                public float unkFlt0;

                public Chunk_D1() { }
                public Chunk_D1(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    unkByte = sr.ReadBE<byte>();
                    unkFlt0 = sr.ReadBE<float>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0xD1);
                    outBytes.Add(unkByte);
                    outBytes.AddValue(unkFlt0);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_D3 : DataChunk
            {
                public byte counter;
                public float unkFlt0;
                public float unkFlt1;

                public Chunk_D3() { }
                public Chunk_D3(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    counter = sr.ReadBE<byte>();
                    unkFlt0 = sr.ReadBE<float>();
                    unkFlt1 = sr.ReadBE<float>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0xD3);
                    outBytes.Add(counter);
                    outBytes.AddValue(unkFlt0);
                    outBytes.AddValue(unkFlt1);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_D4 : DataChunk
            {
                public float unkFlt0;

                public Chunk_D4() { }
                public Chunk_D4(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    unkFlt0 = sr.ReadBE<float>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0xD4);
                    outBytes.AddValue(unkFlt0);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_D5 : DataChunk
            {
                public float unkFlt0;
                public float unkFlt1;

                public Chunk_D5() { }
                public Chunk_D5(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    unkFlt0 = sr.ReadBE<float>();
                    unkFlt1 = sr.ReadBE<float>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0xD5);
                    outBytes.AddValue(unkFlt0);
                    outBytes.AddValue(unkFlt1);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_D6 : DataChunk
            {
                public float unkFlt0;

                public Chunk_D6() { }
                public Chunk_D6(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    unkFlt0 = sr.ReadBE<float>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0xD6);
                    outBytes.AddValue(unkFlt0);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_D7 : DataChunk
            {
                public byte bt_0;
                public Vector3 vec3_01;

                public Chunk_D7() { }
                public Chunk_D7(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    bt_0 = sr.ReadBE<byte>();
                    vec3_01 = sr.ReadBEV3();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0xD7);
                    outBytes.Add(bt_0);
                    outBytes.AddValue(vec3_01);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_DF : DataChunk
            {
                public byte bt_0;
                public Vector3 vec3_01;

                public Chunk_DF() { }
                public Chunk_DF(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    bt_0 = sr.ReadBE<byte>();
                    vec3_01 = sr.ReadBEV3();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0xDF);
                    outBytes.Add(bt_0);
                    outBytes.AddValue(vec3_01);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_E0 : DataChunk
            {
                public Vector3 vec3_00;

                public Chunk_E0() { }
                public Chunk_E0(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    vec3_00 = sr.ReadBEV3();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0xE0);
                    outBytes.AddValue(vec3_00);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_E1 : DataChunk
            {
                public Vector3 vec3_00;
                public Vector3 vec3_01;

                public Chunk_E1() { }
                public Chunk_E1(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    vec3_00 = sr.ReadBEV3();
                    vec3_01 = sr.ReadBEV3();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0xE1);
                    outBytes.AddValue(vec3_00);
                    outBytes.AddValue(vec3_01);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_E2 : DataChunk
            {
                public Vector4 vec4_00;

                public Chunk_E2() { }
                public Chunk_E2(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    vec4_00 = sr.ReadBEV4();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0xE2);
                    outBytes.AddValue(vec4_00);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_E7 : DataChunk
            {
                public byte bt_0;
                public Vector3 vec3_01;

                public Chunk_E7() { }
                public Chunk_E7(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    bt_0 = sr.ReadBE<byte>();
                    vec3_01 = sr.ReadBEV3();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0xE7);
                    outBytes.Add(bt_0);
                    outBytes.AddValue(vec3_01);

                    return outBytes.ToArray();
                }
            }

            public class Chunk_F0 : DataChunk
            {
                public byte bt_0;

                public Chunk_F0() { }
                public Chunk_F0(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    bt_0 = sr.ReadBE<byte>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0xF0);
                    outBytes.Add(bt_0);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_F1 : DataChunk
            {
                public Chunk_F1() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0xF1 };
                }
            }
            public class Chunk_F2 : DataChunk
            {
                public Chunk_F2() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0xF2 };
                }
            }
            public class Chunk_F3 : DataChunk
            {
                public Chunk_F3() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0xF3 };
                }
            }
            public class Chunk_F4 : DataChunk
            {
                public byte bt_0;
                public float unkFlt0;

                public Chunk_F4() { }
                public Chunk_F4(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    bt_0 = sr.ReadBE<byte>();
                    unkFlt0 = sr.ReadBE<float>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0xF4);
                    outBytes.Add(bt_0);
                    outBytes.AddValue(unkFlt0);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_F6 : DataChunk
            {
                public byte bt_0;

                public Chunk_F6() { }
                public Chunk_F6(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    bt_0 = sr.ReadBE<byte>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0xF6);
                    outBytes.Add(bt_0);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_F9 : DataChunk
            {
                public byte bt_0;
                public byte bt_1;
                public byte bt_2;
                public byte bt_3;

                public Chunk_F9() { }
                public Chunk_F9(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    bt_0 = sr.ReadBE<byte>();
                    bt_1 = sr.ReadBE<byte>();
                    bt_2 = sr.ReadBE<byte>();
                    bt_3 = sr.ReadBE<byte>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0xF9);
                    outBytes.Add(bt_0);
                    outBytes.Add(bt_1);
                    outBytes.Add(bt_2);
                    outBytes.Add(bt_3);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_FA : DataChunk
            {
                public byte bt_0;
                public byte bt_1;
                public byte bt_2;
                public byte bt_3;

                public Chunk_FA() { }
                public Chunk_FA(BufferedStreamReaderBE<MemoryStream> sr)
                {
                    bt_0 = sr.ReadBE<byte>();
                    bt_1 = sr.ReadBE<byte>();
                    bt_2 = sr.ReadBE<byte>();
                    bt_3 = sr.ReadBE<byte>();
                }

                public override byte[] GetBytes()
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.Add(0xFA);
                    outBytes.Add(bt_0);
                    outBytes.Add(bt_1);
                    outBytes.Add(bt_2);
                    outBytes.Add(bt_3);

                    return outBytes.ToArray();
                }
            }
            public class Chunk_FF : DataChunk
            {
                public Chunk_FF() { }

                public override byte[] GetBytes()
                {
                    return new byte[] { 0xFF };
                }
            }
        }

        public class DataSet1
        {
            /// <summary>
            /// If 0, offset data is only 8 bytes. If 1, it is 0x30 total.
            /// </summary>
            public int usesExtraData;
            public int offset;

            public DataSet1Inner data = null;
        }

        public class DataSet1Inner
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
            public List<int> unkIntList0 = new List<int>();

            //Offset1
            public List<int> unkIntList1 = new List<int>();

            //MainOffset - This is the only data if useExtraData for its reference is 1
            public List<int> mainInts = new List<int>();
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
