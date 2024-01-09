using AquaModelLibrary.Helpers.Readers;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public class FLTDPhysics : AquaCommon
    {
        public FltdHeader header = null;
        public UnkStructNGS ngsStruct = null;
        public List<MainPhysicsNode> mainNodes = new List<MainPhysicsNode>();
        public List<unkStruct1> subStructs = new List<unkStruct1>();

        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "fltd"
            };
        }

        public FLTDPhysics() { }

        public FLTDPhysics(byte[] file) : base(file) { }

        public FLTDPhysics(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            header = ReadFLTDHeader(sr);

            //Read main nodes
            sr.Seek(offset + header.mainPhysicsNodeOffset, SeekOrigin.Begin);
            for (int i = 0; i < header.mainPhysicsNodeCount; i++)
            {
                mainNodes.Add(ReadMainNode(sr, offset, header.version));
            }

            //Read unk nodes
            sr.Seek(offset + header.unkStruct1Offset, SeekOrigin.Begin);
            for (int i = 0; i < header.unkStruct1Count; i++)
            {
                subStructs.Add(ReadSubStruct1(sr, offset, header.version));
            }

            //Read unk struct NGS

        }

        public class FltdHeader
        {
            public byte version;         //Apparently there are a number of these and they work differently. This makes things painful
            public byte mainPhysicsNodeCount;
            public byte unkStruct1Count;
            public byte unkByte1;

            public int mainPhysicsNodeOffset;
            public int unkStruct1Offset;

            public int int_18;
            public int unkStructNGSOffset;
        }

        public class UnkStructNGS
        {
            public unkStructNGS ngsStruct;
            //public unkEndStruct
            //public unkEndStruct1
        }

        public struct unkStructNGS
        {
            public int int_00;
            public int unkEndStructPtr; //Struct only observed pointing to null areas at end of file
            public int int_08;
            public int unkEndStructPtr1; //Struct only observed pointing to the 0xFFFFFFFF at 0x10 of most NGS files.
        }

        //Sub of fltdHeader
        public class MainPhysicsNode
        {
            public byte bt_00; //Not the index, but often counts up like that.
            public byte unkByte1; //Usually 0x1
            public byte mainSubNodeCount;
            public byte unkByte3;
            public int namePointerPointer;
            public int mainSubNode;

            public int int_0C;
            public int int_10;

            //
            public string name = null;
            public List<MainSubNode> subNodes = new List<MainSubNode>();
        }

        public class MainSubNode
        {
            public byte unkByte0;
            public byte unkByte1;
            public byte unkByte2;
            public byte unkByte3;

            public float flt_04;
            public float flt_08;
            public float flt_0C;
            public float flt_10;

            public float flt_14;
            public float flt_18;
            public float flt_1C;
            public float flt_20;

            public float flt_24;
            public float flt_28;
            public float flt_2C;

            public Vector3 vec3_30;
            public float flt_3C;

            public float flt_40;
            public int nodePtr;
            public int ptr_48;
            public int ptr_4C;

            public int unkNodePtrPtr;
            public byte bt_54;
            public byte bt_55;
            public byte bt_56;
            public byte bt_57;
            public int ptr_58;

            //
            public string nodeName = null;
            public string ptr48String = null;
            public string ptr4CString = null;
            public string nodeName2 = null;
            public string ptr58String = null;
        }

        //Sub of fltdHeader
        public class unkStruct1
        {
            public byte unkByte0;
            public byte unkByte1;
            public byte unkByte2;
            public byte unkByte3;

            public byte byteListCount;
            public byte unkByte5;
            public byte unkByte6;
            public byte unkByte7;

            public int byteListOffset;
            public int unkStruct2Pointer;
            public int unkStruct1_2Pointer;
            public int unkConst01;

            public int unkPointer4;

            //
            public List<byte> byteList = new List<byte>(); //Seems to be padded with 0s, but dentoed by the byteList count. byteList count includes padding.
            public unkStruct2 unkStr2 = null;
            public unkStruct1_2 unkStr1_2 = null;

        }

        public class unkStruct1_2
        {
            public int int_00;
            public int flt_04;
            public int ptr_08;
        }

        //Sub of unkStruct1
        public class unkStruct2
        {
            public float unkFloat0;
            public float unkFloat1;
            public float unkFloat2;
            public byte unkByte0;
            public byte unkByte1;
            public byte unkByte2;
            public byte unkByte3;

            public byte unkByte4;
            public byte unkStruct4Count;
            public byte unkStruct4_2Count;
            public byte unkStruct5Count;
            public int ptr_14;
            public int int_18;
            public int int_1C;

            public int unkStruct4Pointer;
            public int unkStruct4_2Pointer;
            public int unkStruct5Pointer;
            public int unkStruct7Pointer;

            public int unkStruct8Pointer;
            public int int_34;
            public int unkStruct3Pointer;

            //
            public List<unkStruct4> unkStr4List = new List<unkStruct4>();

            public List<unkStruct4> unkStr4_2List = new List<unkStruct4>();
            public List<float> floatList = new List<float>(); //Seems to be at least 8 and count downwards. Might be pairs of 8? 2 * unkStruct4_2 count of floats?

            public List<unkStruct5> unkStr5List = new List<unkStruct5>();
            public unkStruct3 unkStr3 = null;
        }

        //Sub of unkStruct1
        public class unkStruct3
        {
            public int ptr_00;
            public int ptr_04;
            public int int_08;
            public int ptr_0C;

            public List<unkStruct3_1> unkStr3_1List = new List<unkStruct3_1>();
            public List<unkStruct3_2> unkStr3_2List = new List<unkStruct3_2>();
        }

        public class unkStruct3_1
        {
            public float flt_00;
            public float flt_04;
            public float flt_08;
            public float flt_0C;

            public float flt_10;
            public int int_14;
            public int int_18;
            public int ptr_1C;
        }

        public class unkStruct3_2
        {
            public float flt_00;
            public float flt_04;
            public int int_08;
            public int int_0C;

            public int ptr_10;
        }

        //Sub of unkStruct2
        public class unkStruct4
        {
            public byte index;
            public byte unkByte1;
            public byte unkByte2;
            public byte unkByte3;

            public int int_04;
            public int int_08;
            public int int_0C;
            public float flt_10;
            public int int_14;
        }

        //Sub of unkStruct2
        public class unkStruct5
        {
            public byte unkByte0;
            public byte unkByte1;
            public byte index;
            public byte unkByte3;

            public int int_04;
            public int int_08;
            public int int_0C;
            public float flt_10;
        }

        public static unkStruct1 ReadSubStruct1(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int version)
        {
            var start = streamReader.Position;
            unkStruct1 unkStruct = new unkStruct1();

            unkStruct.unkByte0 = streamReader.Read<byte>();
            unkStruct.unkByte1 = streamReader.Read<byte>();
            unkStruct.unkByte2 = streamReader.Read<byte>();
            unkStruct.unkByte3 = streamReader.Read<byte>();

            unkStruct.byteListCount = streamReader.Read<byte>();
            unkStruct.unkByte5 = streamReader.Read<byte>();
            unkStruct.unkByte6 = streamReader.Read<byte>();
            unkStruct.unkByte7 = streamReader.Read<byte>();

            unkStruct.byteListOffset = streamReader.Read<int>();
            unkStruct.unkStruct2Pointer = streamReader.Read<int>();
            unkStruct.unkStruct1_2Pointer = streamReader.Read<int>();
            unkStruct.unkConst01 = streamReader.Read<int>();
            unkStruct.unkPointer4 = streamReader.Read<int>();

            var bookmark = streamReader.Position;

            if (unkStruct.byteListOffset != 0x10 && unkStruct.byteListOffset != 0)
            {
                streamReader.Seek(offset + unkStruct.byteListOffset, SeekOrigin.Begin);
                for (int i = 0; i < unkStruct.byteListCount; i++)
                {
                    unkStruct.byteList.Add(streamReader.Read<byte>());
                }
            }
            if (unkStruct.unkStruct2Pointer != 0x10 && unkStruct.unkStruct2Pointer != 0)
            {
                streamReader.Seek(offset + unkStruct.unkStruct2Pointer, SeekOrigin.Begin);
                unkStruct.unkStr2 = ReadSubStruct2(streamReader, offset, version);
            }
            if (unkStruct.unkStruct1_2Pointer != 0x10 && unkStruct.unkStruct1_2Pointer != 0)
            {
                streamReader.Seek(offset + unkStruct.unkStruct1_2Pointer, SeekOrigin.Begin);
                unkStruct.unkStr1_2 = ReadSubStruct1_2(streamReader, offset, version);
            }
            if (unkStruct.unkPointer4 != 0x10 && unkStruct.unkPointer4 != 0)
            {
                Debug.WriteLine($"ptr from struct at {start.ToString("X")} to {unkStruct.unkPointer4.ToString("X")}");
            }

            streamReader.Seek(bookmark, SeekOrigin.Begin);

            return unkStruct;
        }

        private static unkStruct2 ReadSubStruct2(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int version)
        {
            unkStruct2 unkStr2 = new unkStruct2();

            unkStr2.unkFloat0 = streamReader.Read<float>();
            unkStr2.unkFloat1 = streamReader.Read<float>();
            unkStr2.unkFloat2 = streamReader.Read<float>();
            unkStr2.unkByte0 = streamReader.Read<byte>();
            unkStr2.unkByte1 = streamReader.Read<byte>();
            unkStr2.unkByte2 = streamReader.Read<byte>();
            unkStr2.unkByte3 = streamReader.Read<byte>();

            unkStr2.unkByte4 = streamReader.Read<byte>();
            unkStr2.unkStruct4Count = streamReader.Read<byte>();
            unkStr2.unkStruct4_2Count = streamReader.Read<byte>();
            unkStr2.unkStruct5Count = streamReader.Read<byte>();
            unkStr2.ptr_14 = streamReader.Read<int>();
            unkStr2.int_18 = streamReader.Read<int>();
            unkStr2.int_1C = streamReader.Read<int>();

            unkStr2.unkStruct4Pointer = streamReader.Read<int>();
            unkStr2.unkStruct4_2Pointer = streamReader.Read<int>();
            unkStr2.unkStruct5Pointer = streamReader.Read<int>();
            unkStr2.unkStruct7Pointer = streamReader.Read<int>();
            unkStr2.unkStruct8Pointer = streamReader.Read<int>();
            unkStr2.int_34 = streamReader.Read<int>();
            unkStr2.unkStruct3Pointer = streamReader.Read<int>();

            if (unkStr2.unkStruct4Pointer != 0x10 && unkStr2.unkStruct4Pointer != 0)
            {
                streamReader.Seek(offset + unkStr2.unkStruct4Pointer, SeekOrigin.Begin);
                for (int i = 0; i < unkStr2.unkStruct4Count; i++)
                {
                    unkStr2.unkStr4List.Add(ReadSubStruct4(streamReader, offset, version));
                }
            }
            if (unkStr2.unkStruct4_2Pointer != 0x10 && unkStr2.unkStruct4_2Pointer != 0)
            {
                streamReader.Seek(offset + unkStr2.unkStruct4_2Pointer, SeekOrigin.Begin);
                for (int i = 0; i < unkStr2.unkStruct4_2Count; i++)
                {
                    unkStr2.unkStr4_2List.Add(ReadSubStruct4(streamReader, offset, version));
                }
            }
            if (unkStr2.unkStruct8Pointer != 0x10 && unkStr2.unkStruct8Pointer != 0)
            {
                streamReader.Seek(offset + unkStr2.unkStruct8Pointer, SeekOrigin.Begin);
                for (int i = 0; i < unkStr2.unkStruct4_2Count * 2; i++)
                {
                    unkStr2.floatList.Add(streamReader.Read<float>());
                }
            }
            if (unkStr2.unkStruct3Pointer != 0x10 && unkStr2.unkStruct3Pointer != 0)
            {
                streamReader.Seek(offset + unkStr2.unkStruct3Pointer, SeekOrigin.Begin);
                unkStr2.unkStr3 = ReadSubStruct3(streamReader, offset, version, unkStr2.unkStruct4_2Count, unkStr2.unkStruct5Count);
            }
            if (unkStr2.unkStruct5Pointer != 0x10 && unkStr2.unkStruct5Pointer != 0)
            {
                streamReader.Seek(offset + unkStr2.unkStruct5Pointer, SeekOrigin.Begin);
                for (int i = 0; i < unkStr2.unkStruct5Count; i++)
                {
                    unkStr2.unkStr5List.Add(ReadSubStruct5(streamReader, offset, version));
                }
            }

            return unkStr2;
        }

        private static unkStruct4 ReadSubStruct4(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int version)
        {
            unkStruct4 unkStr4 = new unkStruct4();

            unkStr4.index = streamReader.Read<byte>();
            unkStr4.unkByte1 = streamReader.Read<byte>();
            unkStr4.unkByte2 = streamReader.Read<byte>();
            unkStr4.unkByte3 = streamReader.Read<byte>();

            unkStr4.int_04 = streamReader.Read<int>();
            unkStr4.int_08 = streamReader.Read<int>();
            unkStr4.int_0C = streamReader.Read<int>();
            unkStr4.flt_10 = streamReader.Read<float>();
            unkStr4.int_14 = streamReader.Read<int>();

            return unkStr4;
        }

        private static unkStruct5 ReadSubStruct5(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int version)
        {
            unkStruct5 unkStr5 = new unkStruct5();

            unkStr5.unkByte0 = streamReader.Read<byte>();
            unkStr5.unkByte1 = streamReader.Read<byte>();
            unkStr5.index = streamReader.Read<byte>();
            unkStr5.unkByte3 = streamReader.Read<byte>();

            unkStr5.int_04 = streamReader.Read<int>();
            unkStr5.int_08 = streamReader.Read<int>();
            unkStr5.int_0C = streamReader.Read<int>();
            unkStr5.flt_10 = streamReader.Read<float>();

            return unkStr5;
        }

        private static unkStruct1_2 ReadSubStruct1_2(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int version)
        {
            unkStruct1_2 unkStr1_2 = new unkStruct1_2();

            unkStr1_2.int_00 = streamReader.Read<int>();
            unkStr1_2.flt_04 = streamReader.Read<int>();
            unkStr1_2.ptr_08 = streamReader.Read<int>();

            return unkStr1_2;
        }

        private static unkStruct3 ReadSubStruct3(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int version, int ptr_00Count, int ptr_04Count)
        {
            unkStruct3 unkStr3 = new unkStruct3();

            unkStr3.ptr_00 = streamReader.Read<int>();
            unkStr3.ptr_04 = streamReader.Read<int>();
            unkStr3.int_08 = streamReader.Read<int>();
            unkStr3.ptr_0C = streamReader.Read<int>();

            var bookmark = streamReader.Position;

            if (unkStr3.ptr_00 != 0x10 && unkStr3.ptr_00 != 0)
            {
                streamReader.Seek(offset + unkStr3.ptr_00, SeekOrigin.Begin);
                for (int i = 0; i < ptr_00Count * 2; i++)
                {
                    unkStr3.unkStr3_1List.Add(ReadUnkStr3_1(streamReader, offset, version));
                }
            }
            if (unkStr3.ptr_04 != 0x10 && unkStr3.ptr_04 != 0)
            {
                streamReader.Seek(offset + unkStr3.ptr_04, SeekOrigin.Begin);
                for (int i = 0; i < ptr_04Count; i++)
                {
                    unkStr3.unkStr3_2List.Add(ReadUnkStr3_2(streamReader, offset, version));
                }
            }

            streamReader.Seek(bookmark, SeekOrigin.Begin);

            return unkStr3;
        }

        public static unkStruct3_1 ReadUnkStr3_1(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int version)
        {
            unkStruct3_1 unkStr31 = new unkStruct3_1();

            unkStr31.flt_00 = streamReader.Read<float>();
            unkStr31.flt_04 = streamReader.Read<float>();
            unkStr31.flt_08 = streamReader.Read<float>();
            unkStr31.flt_0C = streamReader.Read<float>();

            unkStr31.flt_10 = streamReader.Read<float>();
            unkStr31.int_14 = streamReader.Read<int>();
            unkStr31.int_18 = streamReader.Read<int>();
            unkStr31.ptr_1C = streamReader.Read<int>();

            return unkStr31;
        }

        public static unkStruct3_2 ReadUnkStr3_2(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int version)
        {
            unkStruct3_2 unkStr32 = new unkStruct3_2();

            unkStr32.flt_00 = streamReader.Read<float>();
            unkStr32.flt_04 = streamReader.Read<float>();
            unkStr32.int_08 = streamReader.Read<int>();
            unkStr32.int_0C = streamReader.Read<int>();

            unkStr32.ptr_10 = streamReader.Read<int>();

            return unkStr32;
        }

        private static FltdHeader ReadFLTDHeader(BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            FltdHeader header = new FltdHeader();
            header.version = streamReader.Read<byte>();
            header.mainPhysicsNodeCount = streamReader.Read<byte>();
            header.unkStruct1Count = streamReader.Read<byte>();
            header.unkByte1 = streamReader.Read<byte>();

            header.mainPhysicsNodeOffset = streamReader.Read<int>();
            header.unkStruct1Offset = streamReader.Read<int>();

            header.int_18 = streamReader.Read<int>();
            header.unkStructNGSOffset = streamReader.Read<int>();

            return header;
        }

        private static MainPhysicsNode ReadMainNode(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int version)
        {
            MainPhysicsNode physicsNode = new MainPhysicsNode();

            physicsNode.bt_00 = streamReader.Read<byte>();
            physicsNode.unkByte1 = streamReader.Read<byte>();
            physicsNode.mainSubNodeCount = streamReader.Read<byte>();
            physicsNode.unkByte3 = streamReader.Read<byte>();
            physicsNode.namePointerPointer = streamReader.Read<int>();
            physicsNode.mainSubNode = streamReader.Read<int>();
            physicsNode.int_0C = streamReader.Read<int>();
            physicsNode.int_10 = streamReader.Read<int>();

            var bookmark = streamReader.Position;

            if (physicsNode.namePointerPointer != 0x10 && physicsNode.namePointerPointer != 0)
            {
                streamReader.Seek(offset + physicsNode.namePointerPointer, SeekOrigin.Begin);
                var nameAddress = streamReader.Read<int>();

                if (nameAddress != 0x10 && nameAddress != 0)
                {
                    streamReader.Seek(offset + nameAddress, SeekOrigin.Begin);
                    physicsNode.name = streamReader.ReadCString();
                }
            }

            //Read unkPointer node
            streamReader.Seek(offset + physicsNode.mainSubNode, SeekOrigin.Begin);

            for (int i = 0; i < physicsNode.mainSubNodeCount; i++)
            {
                physicsNode.subNodes.Add(ReadMainSubNode(streamReader, offset, version));
            }

            streamReader.Seek(bookmark, SeekOrigin.Begin);
            return physicsNode;
        }

        private static MainSubNode ReadMainSubNode(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int version)
        {
            MainSubNode node = new MainSubNode();

            node.unkByte0 = streamReader.Read<byte>();
            node.unkByte1 = streamReader.Read<byte>();
            node.unkByte2 = streamReader.Read<byte>();
            node.unkByte3 = streamReader.Read<byte>();

            node.flt_04 = streamReader.Read<float>();
            node.flt_08 = streamReader.Read<float>();
            node.flt_0C = streamReader.Read<float>();
            node.flt_10 = streamReader.Read<float>();

            node.flt_14 = streamReader.Read<float>();
            node.flt_18 = streamReader.Read<float>();
            node.flt_1C = streamReader.Read<float>();
            node.flt_10 = streamReader.Read<float>();

            node.flt_24 = streamReader.Read<float>();
            node.flt_28 = streamReader.Read<float>();
            node.flt_2C = streamReader.Read<float>();
            node.vec3_30 = streamReader.Read<Vector3>();

            node.flt_3C = streamReader.Read<float>();

            node.flt_40 = streamReader.Read<float>();
            node.nodePtr = streamReader.Read<int>();
            node.ptr_48 = streamReader.Read<int>();
            node.ptr_4C = streamReader.Read<int>();

            node.unkNodePtrPtr = streamReader.Read<int>();
            node.bt_54 = streamReader.Read<byte>();
            node.bt_55 = streamReader.Read<byte>();
            node.bt_56 = streamReader.Read<byte>();
            node.bt_57 = streamReader.Read<byte>();
            node.ptr_58 = streamReader.Read<int>();

            var bookmark = streamReader.Position;

            if (node.nodePtr != 0x10 && node.nodePtr != 0)
            {
                streamReader.Seek(offset + node.nodePtr, SeekOrigin.Begin);
                node.nodeName = streamReader.ReadCString();
            }
            if (node.ptr_48 != 0x10 && node.ptr_48 != 0)
            {
                Debug.WriteLine("node.ptr_48 = " + node.ptr_48.ToString("X"));
                streamReader.Seek(offset + node.ptr_48, SeekOrigin.Begin);
                node.ptr48String = streamReader.ReadCString();
            }
            if (node.ptr_4C != 0x10 && node.ptr_4C != 0)
            {
                Debug.WriteLine("node.ptr_4C = " + node.ptr_4C.ToString("X"));
                streamReader.Seek(offset + node.ptr_4C, SeekOrigin.Begin);
                node.ptr4CString = streamReader.ReadCString();
            }
            if (node.unkNodePtrPtr != 0x10 && node.unkNodePtrPtr != 0)
            {
                streamReader.Seek(offset + node.unkNodePtrPtr, SeekOrigin.Begin);
                var unkNodePtr = streamReader.Read<int>();

                if (unkNodePtr != 0x10 && unkNodePtr != 0)
                {
                    streamReader.Seek(offset + unkNodePtr, SeekOrigin.Begin);
                    node.nodeName2 = streamReader.ReadCString();
                }
            }
            if (node.ptr_58 != 0x10 && node.ptr_58 != 0)
            {
                Debug.WriteLine("node.ptr_58 = " + node.ptr_58.ToString("X"));
                streamReader.Seek(offset + node.ptr_58, SeekOrigin.Begin);
                node.ptr58String = streamReader.ReadCString();
            }

            streamReader.Seek(bookmark, SeekOrigin.Begin);
            return node;
        }
    }
}
