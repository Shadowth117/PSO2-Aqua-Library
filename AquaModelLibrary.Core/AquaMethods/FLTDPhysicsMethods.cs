using Reloaded.Memory.Streams;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using static AquaModelLibrary.AquaMethods.AquaGeneralMethods;

namespace AquaModelLibrary.AquaMethods
{
    public static class FLTDPhysicsMethods
    {
        public static void LoadFLTD(string inFilename)
        {
            string ext = Path.GetExtension(inFilename);
            string variant = "";
            int offset;
            if (ext.Length > 5)
            {
                ext = ext.Substring(0, 5);
            }
            using (Stream stream = new FileStream(inFilename, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                variant = ReadAquaHeader(streamReader, ext, out offset);

                if (variant == "NIFL")
                {
                    FLTDPhysics fltd = new FLTDPhysics();
                    var nifl = streamReader.Read<AquaCommon.NIFL>();
                    var rel = streamReader.Read<AquaCommon.REL0>();
                    streamReader.Seek(offset + rel.REL0DataStart, SeekOrigin.Begin);
                    fltd.header = ReadFLTDHeader(streamReader);

                    //Read main nodes
                    streamReader.Seek(offset + fltd.header.mainPhysicsNodeOffset, SeekOrigin.Begin);
                    for (int i = 0; i < fltd.header.mainPhysicsNodeCount; i++)
                    {
                        fltd.mainNodes.Add(ReadMainNode(streamReader, offset, fltd.header.version));
                    }

                    //Read unk nodes
                    streamReader.Seek(offset + fltd.header.unkStruct1Offset, SeekOrigin.Begin);
                    for (int i = 0; i < fltd.header.unkStruct1Count; i++)
                    {
                        fltd.subStructs.Add(ReadSubStruct1(streamReader, offset, fltd.header.version));
                    }

                    //Read unk struct NGS

                }
            }
        }

        public static FLTDPhysics.unkStruct1 ReadSubStruct1(BufferedStreamReader streamReader, int offset, int version)
        {
            var start = streamReader.Position();
            FLTDPhysics.unkStruct1 unkStruct = new FLTDPhysics.unkStruct1();

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

            var bookmark = streamReader.Position();

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

        private static FLTDPhysics.unkStruct2 ReadSubStruct2(BufferedStreamReader streamReader, int offset, int version)
        {
            FLTDPhysics.unkStruct2 unkStr2 = new FLTDPhysics.unkStruct2();

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

        private static FLTDPhysics.unkStruct4 ReadSubStruct4(BufferedStreamReader streamReader, int offset, int version)
        {
            FLTDPhysics.unkStruct4 unkStr4 = new FLTDPhysics.unkStruct4();

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

        private static FLTDPhysics.unkStruct5 ReadSubStruct5(BufferedStreamReader streamReader, int offset, int version)
        {
            FLTDPhysics.unkStruct5 unkStr5 = new FLTDPhysics.unkStruct5();

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

        private static FLTDPhysics.unkStruct1_2 ReadSubStruct1_2(BufferedStreamReader streamReader, int offset, int version)
        {
            FLTDPhysics.unkStruct1_2 unkStr1_2 = new FLTDPhysics.unkStruct1_2();

            unkStr1_2.int_00 = streamReader.Read<int>();
            unkStr1_2.flt_04 = streamReader.Read<int>();
            unkStr1_2.ptr_08 = streamReader.Read<int>();

            return unkStr1_2;
        }

        private static FLTDPhysics.unkStruct3 ReadSubStruct3(BufferedStreamReader streamReader, int offset, int version, int ptr_00Count, int ptr_04Count)
        {
            FLTDPhysics.unkStruct3 unkStr3 = new FLTDPhysics.unkStruct3();

            unkStr3.ptr_00 = streamReader.Read<int>();
            unkStr3.ptr_04 = streamReader.Read<int>();
            unkStr3.int_08 = streamReader.Read<int>();
            unkStr3.ptr_0C = streamReader.Read<int>();

            var bookmark = streamReader.Position();

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

        public static FLTDPhysics.unkStruct3_1 ReadUnkStr3_1(BufferedStreamReader streamReader, int offset, int version)
        {
            FLTDPhysics.unkStruct3_1 unkStr31 = new FLTDPhysics.unkStruct3_1();

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

        public static FLTDPhysics.unkStruct3_2 ReadUnkStr3_2(BufferedStreamReader streamReader, int offset, int version)
        {
            FLTDPhysics.unkStruct3_2 unkStr32 = new FLTDPhysics.unkStruct3_2();

            unkStr32.flt_00 = streamReader.Read<float>();
            unkStr32.flt_04 = streamReader.Read<float>();
            unkStr32.int_08 = streamReader.Read<int>();
            unkStr32.int_0C = streamReader.Read<int>();

            unkStr32.ptr_10 = streamReader.Read<int>();

            return unkStr32;
        }

        private static FLTDPhysics.FltdHeader ReadFLTDHeader(BufferedStreamReader streamReader)
        {
            FLTDPhysics.FltdHeader header = new FLTDPhysics.FltdHeader();
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

        private static FLTDPhysics.MainPhysicsNode ReadMainNode(BufferedStreamReader streamReader, int offset, int version)
        {
            FLTDPhysics.MainPhysicsNode physicsNode = new FLTDPhysics.MainPhysicsNode();

            physicsNode.bt_00 = streamReader.Read<byte>();
            physicsNode.unkByte1 = streamReader.Read<byte>();
            physicsNode.mainSubNodeCount = streamReader.Read<byte>();
            physicsNode.unkByte3 = streamReader.Read<byte>();
            physicsNode.namePointerPointer = streamReader.Read<int>();
            physicsNode.mainSubNode = streamReader.Read<int>();
            physicsNode.int_0C = streamReader.Read<int>();
            physicsNode.int_10 = streamReader.Read<int>();

            var bookmark = streamReader.Position();

            if (physicsNode.namePointerPointer != 0x10 && physicsNode.namePointerPointer != 0)
            {
                streamReader.Seek(offset + physicsNode.namePointerPointer, SeekOrigin.Begin);
                var nameAddress = streamReader.Read<int>();

                if (nameAddress != 0x10 && nameAddress != 0)
                {
                    streamReader.Seek(offset + nameAddress, SeekOrigin.Begin);
                    physicsNode.name = ReadCString(streamReader);
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

        private static FLTDPhysics.MainSubNode ReadMainSubNode(BufferedStreamReader streamReader, int offset, int version)
        {
            FLTDPhysics.MainSubNode node = new FLTDPhysics.MainSubNode();

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

            var bookmark = streamReader.Position();

            if (node.nodePtr != 0x10 && node.nodePtr != 0)
            {
                streamReader.Seek(offset + node.nodePtr, SeekOrigin.Begin);
                node.nodeName = ReadCString(streamReader);
            }
            if (node.ptr_48 != 0x10 && node.ptr_48 != 0)
            {
                Debug.WriteLine("node.ptr_48 = " + node.ptr_48.ToString("X"));
                streamReader.Seek(offset + node.ptr_48, SeekOrigin.Begin);
                node.ptr48String = ReadCString(streamReader);
            }
            if (node.ptr_4C != 0x10 && node.ptr_4C != 0)
            {
                Debug.WriteLine("node.ptr_4C = " + node.ptr_4C.ToString("X"));
                streamReader.Seek(offset + node.ptr_4C, SeekOrigin.Begin);
                node.ptr4CString = ReadCString(streamReader);
            }
            if (node.unkNodePtrPtr != 0x10 && node.unkNodePtrPtr != 0)
            {
                streamReader.Seek(offset + node.unkNodePtrPtr, SeekOrigin.Begin);
                var unkNodePtr = streamReader.Read<int>();

                if (unkNodePtr != 0x10 && unkNodePtr != 0)
                {
                    streamReader.Seek(offset + unkNodePtr, SeekOrigin.Begin);
                    node.nodeName2 = ReadCString(streamReader);
                }
            }
            if (node.ptr_58 != 0x10 && node.ptr_58 != 0)
            {
                Debug.WriteLine("node.ptr_58 = " + node.ptr_58.ToString("X"));
                streamReader.Seek(offset + node.ptr_58, SeekOrigin.Begin);
                node.ptr58String = ReadCString(streamReader);
            }

            streamReader.Seek(bookmark, SeekOrigin.Begin);
            return node;
        }
    }
}
