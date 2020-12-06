using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using static AquaModelLibrary.AquaObject;
using static AquaModelLibrary.AquaNode;
using static AquaModelLibrary.AquaObjectMethods;
using System.Windows;

namespace AquaModelLibrary
{
    public unsafe class VTBFMethods
    {
        public static List<Dictionary<int, object>> ReadVTBFTag(BufferedStreamReader streamReader, out string tagString, out int entryCount)
        {
            List<Dictionary<int, object>> vtbfData = new List<Dictionary<int, object>>();

            string vtc0 = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Read<int>())); //vtc0
            if(vtc0 != "vtc0")
            {
                tagString = null;
                entryCount = 0;
                return null;
            }
            uint bodyLength = streamReader.Read<uint>();
            int mainTagType = streamReader.Read<int>();
            tagString = Encoding.UTF8.GetString(BitConverter.GetBytes(mainTagType));
            #if DEBUG
                        Console.WriteLine($"Start { tagString} around { streamReader.Position().ToString("X")}");
            #endif
            short pointerCount = streamReader.Read<short>(); //Not important for reading. Game assumedly uses this at runtime to know how many pointer ints to prepare for the block.
            entryCount = streamReader.Read<short>();

            Dictionary<int, object> vtbfDict = new Dictionary<int, object>();

            for (int i = 0; i < entryCount; i++)
            {
                byte dataId = streamReader.Read<byte>();
                byte dataType = streamReader.Read<byte>();
                byte subDataType;
                uint subDataAdditions;

                //Check for special ids
                switch (dataId)
                {
                    case 0xFD: //End of sequence and should be end of tag
                        vtbfData.Add(vtbfDict);
                        return vtbfData;
                    case 0xFE:
                        vtbfData.Add(vtbfDict);
                        vtbfDict = new Dictionary<int, object>();
                        break;
                    default: //Nothing needs to be done if this is a normal id. 0xFC, start of sequence, is special, but for reading purposes can also go through here.
                        break;
                }
                object data;
                switch (dataType)
                {
                    case 0x0: //Special Flag. Probably more for parsing purposes than data. 0xFC ID before this means first struct, 0xFE means a later struct
                        data = dataType;
                        break;
                    case 0x1: //Boolean? Single byte
                        data = streamReader.Read<byte>();
                        break;
                    case 0x2: //String
                        byte strLen = streamReader.Read<byte>();
                        if (strLen > 0)
                        {
                            data = streamReader.ReadBytes(streamReader.Position(), strLen);
                        } else
                        {
                            data = new byte[0];
                        }
                        streamReader.Seek(strLen, SeekOrigin.Current);
                        break;
                    case 0x4: //byte
                    case 0x5:
                        data = streamReader.Read<byte>();
                        break;
                    case 0x6: //short 0x6 is signed while 0x7 is unsigned
                    case 0x7:
                        data = streamReader.Read<short>();
                        break;
                    case 0x8: //int. 0x8 is signed while 0x9 is unsigned
                    case 0x9:
                        data = streamReader.Read<int>();
                        break;
                    case 0xA: //float
                        data = streamReader.Read<float>();
                        break;
                    case 0xC: //color, BGRA?
                        data = new byte[4];
                        for (int j = 0; j < 4; j++)
                        {
                            ((byte[])data)[j] = streamReader.Read<byte>();
                        }
                        break;
                    case 0x48: //Vector3 of ints
                    case 0x49:
                        subDataAdditions = streamReader.Read<byte>(); //Presumably the number of these consecutively
                        data = new int[subDataAdditions][];
                        for (int j = 0; j < subDataAdditions; j++)
                        {
                            int[] dataArr = new int[3];
                            dataArr[0] = streamReader.Read<int>();
                            dataArr[1] = streamReader.Read<int>();
                            dataArr[2] = streamReader.Read<int>();

                            ((int[][])data)[j] = dataArr;
                        }
                        break;
                    case 0x4A: //Vector of floats. Observed as Vector3 and Vector4
                        subDataAdditions = streamReader.Read<byte>(); //Amount of floats past the first 2? 0x1 means Vector3, 0x2 means Vector4 total. Other amounts unobserved.

                        switch (subDataAdditions)
                        {
                            case 0x1:
                                data = streamReader.Read<Vector3>();
                                break;
                            case 0x2:
                                data = streamReader.Read<Vector4>();
                                break;
                            default:
                                MessageBox.Show($"Unknown subDataAdditions amount {subDataAdditions} at {streamReader.Position()}");
                                throw new Exception();
                        }

                        break;
                    case 0x84: //Theoretical array of bytes
                    case 0x85:
                        subDataType = streamReader.Read<byte>();      //Next entity type. 0x8 for byte, 0x10 for short
                        switch (subDataType) //The last array entry aka data count - 1.
                        {
                            case 0x8:
                                subDataAdditions = streamReader.Read<byte>() + (uint)1;
                                break;
                            case 0x10:
                                subDataAdditions = streamReader.Read<ushort>() + (uint)1;
                                break;
                            case 0x18:
                                subDataAdditions = streamReader.Read<uint>() + 1;
                                break;
                            default:
                                MessageBox.Show($"Unknown subdataType {subDataType} at {streamReader.Position()}");
                                throw new NotImplementedException();
                        }
                        data = streamReader.ReadBytes(streamReader.Position(), (int)subDataAdditions);

                        streamReader.Seek(subDataAdditions, SeekOrigin.Current);
                        break;
                    case 0x86: //Array of ushorts?
                    case 0x87: //Array of shorts
                        subDataType = streamReader.Read<byte>();      //Next entity type. 0x8 for byte, 0x10 for short
                        switch (subDataType) //The last array entry aka data count - 1.
                        {
                            case 0x8:
                                subDataAdditions = streamReader.Read<byte>() + (uint)1;
                                break;
                            case 0x10:
                                subDataAdditions = streamReader.Read<ushort>() + (uint)1;
                                break;
                            case 0x18:
                                subDataAdditions = streamReader.Read<uint>() + 1;
                                break;
                            default:
                                MessageBox.Show($"Unknown subdataType {subDataType.ToString("X")} at {streamReader.Position()}");
                                throw new NotImplementedException();
                        }

                        if (dataType == 0x86)
                        {
                            data = new ushort[subDataAdditions];
                            for (int j = 0; j < subDataAdditions; j++)
                            {
                                ((ushort[])data)[j] = streamReader.Read<ushort>();
                            }
                        }
                        else
                        {
                            data = new short[subDataAdditions];
                            for (int j = 0; j < subDataAdditions; j++)
                            {
                                ((short[])data)[j] = streamReader.Read<short>();
                            }
                        }
                        break;
                    case 0x88:
                    case 0x89: //Array of ints. Often needs to be processed in various ways in post.
                        subDataType = streamReader.Read<byte>();      //Next entity type. 0x8 for byte, 0x10 for short
                        switch (subDataType) //The last array entry aka data count - 1.
                        {
                            case 0x8:
                                subDataAdditions = streamReader.Read<byte>() + (uint)1;
                                break;
                            case 0x10:
                                subDataAdditions = streamReader.Read<ushort>() + (uint)1;
                                break;
                            case 0x18:
                                subDataAdditions = streamReader.Read<uint>() + 1;
                                break;
                            default:
                                MessageBox.Show($"Unknown subdataType {subDataType.ToString("X")} at {streamReader.Position()}");
                                throw new NotImplementedException();
                        }
                        subDataAdditions *= 4; //The field is stored as some amount of int32s. Therefore, multiplying by 4 gives us the byte buffer length.
                        data = streamReader.ReadBytes(streamReader.Position(), (int)subDataAdditions); //Read the whole vert buffer at once as byte array. We'll handle it later.
                        streamReader.Seek(subDataAdditions, SeekOrigin.Current);
                        break;
                    case 0x8A: //Array of floats
                        subDataType = streamReader.Read<byte>();      //Next entity type. 0x8 for byte, 0x10 for short
                        switch (subDataType) //The last array entry aka data count - 1.
                        {
                            case 0x8:
                                subDataAdditions = streamReader.Read<byte>() + (uint)1;
                                break;
                            case 0x10:
                                subDataAdditions = streamReader.Read<ushort>() + (uint)1;
                                break;
                            case 0x18:
                                subDataAdditions = streamReader.Read<uint>() + 1;
                                break;
                            default:
                                MessageBox.Show($"Unknown subdataType {subDataType.ToString("X")} at {streamReader.Position()}");
                                throw new NotImplementedException();
                        }
                        data = new float[subDataAdditions];
                        for (int j = 0; j < subDataAdditions; j++)
                        {
                            ((float[])data)[j] = streamReader.Read<float>();
                        }
                        break;
                    case 0xC6: //Int16 array? Seen used in .cmx files for storing unicode characters.
                        subDataType = streamReader.Read<byte>();
                        switch (subDataType) //The last array entry aka data count - 1.
                        {
                            case 0x8:
                                subDataAdditions = streamReader.Read<byte>();
                                break;
                            case 0x10:
                                subDataAdditions = streamReader.Read<ushort>();
                                break;
                            case 0x18:
                                subDataAdditions = streamReader.Read<uint>();
                                break;
                            default:
                                MessageBox.Show($"Unknown subdataType {subDataType.ToString("X")} at {streamReader.Position()}");
                                throw new NotImplementedException();
                        }
                        uint actualCount = subDataAdditions * 2 + 2;
                        data = new short[actualCount]; //Yeah something is wrong in the way the og files are written. Should be all 0s after the expected data, but still.
                        for (int j = 0; j < actualCount; j++)
                        {
                            ((short[])data)[j] = streamReader.Read<short>();
                        }
                        break;
                    case 0xCA: //Float Matrix, observed only as 4x4
                        subDataType = streamReader.Read<byte>(); //Expected to always be 0xA for float
                        subDataAdditions = streamReader.Read<byte>() + (uint)1; //last array entry id
                        data = new Vector4[subDataAdditions];

                        for (int j = 0; j < subDataAdditions; j++)
                        {
                            switch (subDataType)
                            {
                                case 0xA:
                                    ((Vector4[])data)[j] = streamReader.Read<Vector4>();
                                    break;
                                default:
                                    MessageBox.Show($"Unknown subDataType {subDataType.ToString("X")}, please report!");
                                    throw new NotImplementedException();
                            }
                        }
                        break;
                    default:
                        MessageBox.Show($"Unknown dataType {dataType.ToString("X")} at {streamReader.Position().ToString("X")}, please report!");
                        throw new NotImplementedException();
                }

                //Really shouldn't happen, but they have this situation.
                if(vtbfDict.ContainsKey(dataId))
                {
                    vtbfData.Add(vtbfDict);
                    vtbfDict = new Dictionary<int, object>();
                    vtbfDict.Add(dataId, data);
                } else
                {
                    vtbfDict.Add(dataId, data);
                }
            #if DEBUG
                //Console.WriteLine($"Processed { dataType.ToString("X")} around { streamReader.Position().ToString("X")}");
            #endif
            }
            //For non-list type tag data and non FD terminated lists (alpha has these)
            vtbfData.Add(vtbfDict);

            #if DEBUG
                //Console.WriteLine($"Processed {tagString} around { streamReader.Position().ToString("X")}");
            #endif

            return vtbfData;
        }

        public static OBJC parseOBJC(List<Dictionary<int, object>> objcRaw)
        {
            OBJC objc = new OBJC();

            objc.type = (int)(objcRaw[0][0x10]);
            objc.size = (int)(objcRaw[0][0x11]);
            objc.unkMeshValue = (int)(objcRaw[0][0x12]);
            objc.largetsVTXL = (int)(objcRaw[0][0x13]);
            objc.totalStripFaces = (int)(objcRaw[0][0x14]);
            objc.totalVTXLCount = (int)(objcRaw[0][0x15]);
            objc.unkMeshCount = (int)(objcRaw[0][0x16]);
            objc.vsetCount = (int)(objcRaw[0][0x24]);
            objc.psetCount = (int)(objcRaw[0][0x25]);
            objc.meshCount = (int)(objcRaw[0][0x17]);
            objc.mateCount = (int)(objcRaw[0][0x18]);
            objc.rendCount = (int)(objcRaw[0][0x19]);
            objc.shadCount = (int)(objcRaw[0][0x1A]);
            objc.tstaCount = (int)(objcRaw[0][0x1B]);
            objc.tsetCount = (int)(objcRaw[0][0x1C]);
            objc.texfCount = (int)(objcRaw[0][0x1D]);

            BoundingVolume bounding = new BoundingVolume();
            bounding.modelCenter = ((Vector3)(objcRaw[0][0x1E]));
            bounding.boundingRadius = (float)(objcRaw[0][0x1F]);
            bounding.modelCenter2 = ((Vector3)(objcRaw[0][0x20]));
            bounding.maxMinXYZDifference = ((Vector3)(objcRaw[0][0x21]));

            objc.bounds = bounding;

            return objc;
        }

        public static byte[] toOBJC(OBJC objc, bool useUNRMs)
        {
            List<byte> outBytes = new List<byte>();

            ushort pointerCount = 0;
            pointerCount += flagCheck(objc.vsetCount);
            pointerCount += flagCheck(objc.psetCount);
            pointerCount += flagCheck(objc.meshCount);
            pointerCount += flagCheck(objc.mateCount);
            pointerCount += flagCheck(objc.rendCount);
            pointerCount += flagCheck(objc.shadCount);
            pointerCount += flagCheck(objc.tstaCount);
            pointerCount += flagCheck(objc.tsetCount);
            pointerCount += flagCheck(objc.texfCount);
            if (useUNRMs)
            {
                pointerCount += 1;
            }

            outBytes.AddRange(Encoding.UTF8.GetBytes("vtc0"));
            outBytes.AddRange(BitConverter.GetBytes(0x9B));          //Data body size is always 0x9B for OBJC
            outBytes.AddRange(Encoding.UTF8.GetBytes("OBJC"));
            outBytes.AddRange(BitConverter.GetBytes(pointerCount));
            outBytes.AddRange(BitConverter.GetBytes((short)0x14)); //Subtag count, always 0x14 for OBJC

            addBytes(outBytes, 0x10, 0x8, BitConverter.GetBytes(objc.type)); //Should just always be 0xC2A. Perhaps some kind of header info?
            addBytes(outBytes, 0x11, 0x8, BitConverter.GetBytes(objc.size)); //Size of the final data struct, always 0xA4. This ends up being the exact size of the NIFL variation of OBJC.
            addBytes(outBytes, 0x12, 0x9, BitConverter.GetBytes(objc.unkMeshValue));
            addBytes(outBytes, 0x13, 0x8, BitConverter.GetBytes(objc.largetsVTXL));
            addBytes(outBytes, 0x14, 0x9, BitConverter.GetBytes(objc.totalStripFaces));
            addBytes(outBytes, 0x15, 0x8, BitConverter.GetBytes(objc.totalVTXLCount));
            addBytes(outBytes, 0x16, 0x8, BitConverter.GetBytes(objc.unkMeshCount));
            addBytes(outBytes, 0x24, 0x9, BitConverter.GetBytes(objc.vsetCount));
            addBytes(outBytes, 0x25, 0x9, BitConverter.GetBytes(objc.psetCount));
            addBytes(outBytes, 0x17, 0x9, BitConverter.GetBytes(objc.meshCount));
            addBytes(outBytes, 0x18, 0x8, BitConverter.GetBytes(objc.mateCount));
            addBytes(outBytes, 0x19, 0x8, BitConverter.GetBytes(objc.rendCount));
            addBytes(outBytes, 0x1A, 0x8, BitConverter.GetBytes(objc.shadCount));
            addBytes(outBytes, 0x1B, 0x8, BitConverter.GetBytes(objc.tstaCount));
            addBytes(outBytes, 0x1C, 0x8, BitConverter.GetBytes(objc.tsetCount));
            addBytes(outBytes, 0x1D, 0x8, BitConverter.GetBytes(objc.texfCount));
            addBytes(outBytes, 0x1E, 0x4A, 0x1, Reloaded.Memory.Struct.GetBytes(objc.bounds.modelCenter));
            addBytes(outBytes, 0x1F, 0xA, BitConverter.GetBytes(objc.bounds.boundingRadius));
            addBytes(outBytes, 0x20, 0x4A, 0x1, Reloaded.Memory.Struct.GetBytes(objc.bounds.modelCenter2));
            addBytes(outBytes, 0x21, 0x4A, 0x1, Reloaded.Memory.Struct.GetBytes(objc.bounds.maxMinXYZDifference));

            return outBytes.ToArray();
        }

        public static List<VSET> parseVSET(List<Dictionary<int, object>> vsetRaw, out List<List<ushort>> bonePalettes, out List<List<ushort>> edgeVertsLists)
        {
            List<VSET> vsetList = new List<VSET>();
            bonePalettes = new List<List<ushort>>();
            edgeVertsLists = new List<List<ushort>>();

            for (int i = 0; i < vsetRaw.Count; i++)
            {
                VSET vset = new VSET();
                vset.vertDataSize = (int)(vsetRaw[i][0xB6]);
                vset.vertTypesCount = (int)(vsetRaw[i][0xBF]);
                vset.vtxlCount = (int)(vsetRaw[i][0xB9]);
                vset.reserve0 = (int)(vsetRaw[i][0xC4]);

                //BonePalette
                vset.bonePaletteCount = (int)(vsetRaw[i][0xBD]);
                if (vsetRaw[i].ContainsKey(0xBE))
                {
                    var rawBP = (vsetRaw[i][0xBE]);
                    if (rawBP is ushort)
                    {
                        bonePalettes.Add(new List<ushort> { (ushort)rawBP });
                    }
                    else if (rawBP is ushort[])
                    {
                        bonePalettes.Add(((ushort[])rawBP).ToList());
                    }
                    else
                    {
                        bonePalettes.Add(null);
                    }
                }

                //Not sure on these, but I don't know that unk0-2 get used normally
                vset.unk0 = (int)(vsetRaw[i][0xC8]);
                vset.unk1 = (int)(vsetRaw[i][0xCC]);

                //EdgeVerts
                vset.edgeVertsCount = (int)(vsetRaw[i][0xC9]);
                if(vsetRaw[i].ContainsKey(0xCA))
                {
                    var rawEV = (vsetRaw[i][0xCA]);
                    if (rawEV is ushort)
                    {
                        edgeVertsLists.Add(new List<ushort> { (ushort)rawEV });
                    }
                    else if (rawEV is short[])
                    {
                        edgeVertsLists.Add(((ushort[])rawEV).ToList());
                    }
                    else
                    {
                        edgeVertsLists.Add(null);
                    }
                }

                vsetList.Add(vset);
            }

            return vsetList;
        }

        public static byte[] toVSETList(List<VSET> vsetList, List<VTXL> vtxlList)
        {
            List<byte> outBytes = new List<byte>();
            int subTagCount = 0;

            for (int i = 0; i < vsetList.Count; i++)
            {
                subTagCount += 0x9; //Each vset substruct adds this many sub tags every time.
                if (i == 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                } else
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }
                addBytes(outBytes, 0xB6, 0x9, BitConverter.GetBytes(vsetList[i].vertDataSize));
                addBytes(outBytes, 0xBF, 0x9, BitConverter.GetBytes(vsetList[i].vertTypesCount));
                addBytes(outBytes, 0xB9, 0x9, BitConverter.GetBytes(vsetList[i].vtxlCount));
                addBytes(outBytes, 0xC4, 0x9, BitConverter.GetBytes(vsetList[i].reserve0));

                if (vtxlList[i].bonePalette != null)
                {
                    if (vtxlList[i].bonePalette.Count > 0)
                    {
                        addBytes(outBytes, 0xBD, 0x9, BitConverter.GetBytes(vtxlList[i].bonePalette.Count));
                        subTagCount++;

                        outBytes.Add(0xBE);
                        outBytes.Add(0x86);
                        outBytes.Add(0x8);
                        outBytes.Add((byte)(vtxlList[i].bonePalette.Count - 1));
                        for (int j = 0; j < vtxlList[i].bonePalette.Count; j++)
                        {
                            outBytes.AddRange(BitConverter.GetBytes(vtxlList[i].bonePalette[j]));
                        }
                    } else
                    {
                        addBytes(outBytes, 0xBD, 0x9, BitConverter.GetBytes((int)0));
                    }

                } else
                {
                    addBytes(outBytes, 0xBD, 0x9, BitConverter.GetBytes((int)0));
                }

                addBytes(outBytes, 0xC8, 0x9, BitConverter.GetBytes(vsetList[i].unk0));
                addBytes(outBytes, 0xCC, 0x9, BitConverter.GetBytes(vsetList[i].unk1));

                if (vtxlList[i].edgeVerts != null)
                {
                    if (vtxlList[i].edgeVerts.Count > 0)
                    {

                        addBytes(outBytes, 0xC9, 0x9, BitConverter.GetBytes(vtxlList[i].edgeVerts.Count));
                        subTagCount++;

                        outBytes.Add(0xCA);
                        outBytes.Add(0x86);
                        outBytes.Add(0x8);
                        outBytes.Add((byte)(vtxlList[i].edgeVerts.Count - 1));
                        for (int j = 0; j < vtxlList[i].edgeVerts.Count; j++)
                        {
                            outBytes.AddRange(BitConverter.GetBytes(vtxlList[i].edgeVerts[j]));
                        }
                    }
                    else
                    {
                        addBytes(outBytes, 0xC9, 0x9, BitConverter.GetBytes((int)0));
                    }

                }
                else
                {
                    addBytes(outBytes, 0xC9, 0x9, BitConverter.GetBytes((int)0));
                }

            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));
            subTagCount++;

            //In VTBF, VSETS are all treated as part of the same struct

            //Pointer count. In this case, 0x2 times the VSET count.
            //Subtag count
            WriteTagHeader(outBytes, "VSET", (ushort)(0x2 * vsetList.Count), (ushort)(subTagCount));

            return outBytes.ToArray();
        }

        public static void parseVTXE_VTXL(List<Dictionary<int, object>> vtxeRaw, List<Dictionary<int, object>> vtxlRaw, out VTXE vtxe, out VTXL vtxl)
        {
            int vertSize = 0;
            vtxe = new VTXE();
            vtxl = new VTXL();
            for (int i = 0; i < vtxeRaw.Count; i++)
            {
                VTXEElement vtxeEle = new VTXEElement();

                vtxeEle.dataType = (int)vtxeRaw[i][0xD0];
                vtxeEle.structVariation = (int)vtxeRaw[i][0xD1];
                vtxeEle.relativeAddress = (int)vtxeRaw[i][0xD2];
                vtxeEle.reserve0 = (int)vtxeRaw[i][0xD3];
                switch (vtxeEle.dataType)
                {
                    case (int)AquaObject.VertFlags.VertPosition:
                        vertSize += 0xC;
                        break;
                    case (int)AquaObject.VertFlags.VertWeight:
                        vertSize += 0x10;
                        break;
                    case (int)AquaObject.VertFlags.VertNormal:
                        vertSize += 0xC;
                        break;
                    case (int)AquaObject.VertFlags.VertColor:
                        vertSize += 0x4;
                        break;
                    case (int)AquaObject.VertFlags.VertColor2:
                        vertSize += 0x4;
                        break;
                    case (int)AquaObject.VertFlags.VertWeightIndex:
                        vertSize += 0x4;
                        break;
                    case (int)AquaObject.VertFlags.VertUV1:
                        vertSize += 0x8;
                        break;
                    case (int)AquaObject.VertFlags.VertUV2:
                        vertSize += 0x8;
                        break;
                    case (int)AquaObject.VertFlags.VertUV3:
                        vertSize += 0x8;
                        break;
                    case (int)AquaObject.VertFlags.VertTangent:
                        vertSize += 0xC;
                        break;
                    case (int)AquaObject.VertFlags.VertBinormal:
                        vertSize += 0xC;
                        break;
                    default:
                        vertSize += 0xC;
                        MessageBox.Show($"Unknown Vert type {vtxeEle.dataType}! Please report!");
                        break;
                }

                vtxe.vertDataTypes.Add(vtxeEle);
            }

            int vertCount = ((byte[])vtxlRaw[0][0xBA]).Length / vertSize;

            using (Stream stream = new MemoryStream((byte[])vtxlRaw[0][0xBA]))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                ReadVTXL(streamReader, vtxe, vtxl, vertCount, vtxe.vertDataTypes.Count);
            }
        }

        public static byte[] toVTXE_VTXL(VTXE vtxe, VTXL vtxl)
        {
            List<byte> outBytes = new List<byte>();

            //VTXE
            for (int i = 0; i < vtxe.vertDataTypes.Count; i++)
            {
                if (i == 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                }
                else
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }

                addBytes(outBytes, 0xD0, 0x9, BitConverter.GetBytes(vtxe.vertDataTypes[i].dataType));
                addBytes(outBytes, 0xD1, 0x9, BitConverter.GetBytes(vtxe.vertDataTypes[i].structVariation));
                addBytes(outBytes, 0xD2, 0x9, BitConverter.GetBytes(vtxe.vertDataTypes[i].relativeAddress));
                addBytes(outBytes, 0xD3, 0x9, BitConverter.GetBytes(vtxe.vertDataTypes[i].reserve0));
            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            //Pointer count. Always 0 on VTXE
            //Subtag count
            WriteTagHeader(outBytes, "VTXE", 0, (ushort)(vtxe.vertDataTypes.Count * 5 + 1));

            int vtxeEnd = outBytes.Count;

            //VTXL
            List<byte> outBytes2 = new List<byte>();
            outBytes2.Add(0xBA);
            outBytes2.Add(0x89);
            int vtxlSizeArea = outBytes2.Count;
            WriteVTXL(vtxe, vtxl, outBytes2);

            //Calc and insert the vert data counts in post due to the way sega does it.
            int vertDataCount = ((outBytes2.Count - vtxlSizeArea) / 4) - 1;
            if (vertDataCount > byte.MaxValue)
            {
                if (vertDataCount - 1 > ushort.MaxValue)
                {
                    outBytes2.Insert(vtxlSizeArea, 0x18);
                    outBytes2.InsertRange(vtxlSizeArea + 0x1, BitConverter.GetBytes(vertDataCount));
                }
                else
                {
                    outBytes2.Insert(vtxlSizeArea, 0x10);
                    outBytes2.InsertRange(vtxlSizeArea + 0x1, BitConverter.GetBytes((short)(vertDataCount)));
                }
            }
            else
            {
                outBytes2.Insert(vtxlSizeArea, 0x8);
                outBytes2.Insert(vtxlSizeArea + 0x1, (byte)vertDataCount);
            }

            //Pointer count. Always 0 on VTXL
            //Subtag count
            WriteTagHeader(outBytes2, "VTXL", 0, 0x1);

            outBytes.AddRange(outBytes2);

            return outBytes.ToArray();
        }

        public static void parsePSET(List<Dictionary<int, object>> psetRaw, out List<PSET> psets, out List<stripData> strips)
        {
            psets = new List<PSET>();
            strips = new List<stripData>();
            for (int i = 0; i < psetRaw.Count; i++)
            {
                PSET pset = new PSET();
                stripData strip = new stripData();

                pset.tag = (int)psetRaw[i][0xC6];
                pset.faceType = (int)psetRaw[i][0xBB];
                pset.psetFaceCount = (int)psetRaw[i][0xBC];
                strip.triCount = (int)psetRaw[i][0xB7];
                strip.triStrips = ((ushort[])psetRaw[i][0xB8]).ToList();
                pset.reserve0 = (int)psetRaw[i][0xC5];

                psets.Add(pset);
                strips.Add(strip);
            }
        }

        public static byte[] toPSET(List<PSET> psets, List<stripData> strips)
        {
            List<byte> outBytes = new List<byte>();
            for (int i = 0; i < psets.Count; i++)
            {
                if (i == 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                }
                else
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }
                addBytes(outBytes, 0xC6, 0x9, BitConverter.GetBytes(psets[i].tag));
                addBytes(outBytes, 0xBB, 0x9, BitConverter.GetBytes(psets[i].faceType));
                addBytes(outBytes, 0xBC, 0x9, BitConverter.GetBytes(psets[i].psetFaceCount));
                addBytes(outBytes, 0xB7, 0x9, BitConverter.GetBytes(strips[i].triCount));

                outBytes.Add(0xB8);
                outBytes.Add(0x86);
                if (strips[i].triCount - 1 > byte.MaxValue)
                {
                    if (strips[i].triCount - 1 > ushort.MaxValue)
                    {
                        outBytes.Add(0x18);
                        outBytes.AddRange(BitConverter.GetBytes(strips[i].triStrips.Count - 1));
                    }
                    else
                    {
                        outBytes.Add(0x10);
                        outBytes.AddRange(BitConverter.GetBytes((short)(strips[i].triStrips.Count - 1)));
                    }
                } else
                {
                    outBytes.Add(0x8);
                    outBytes.Add((byte)(strips[i].triStrips.Count - 1));
                }
                for (int j = 0; j < strips[i].triStrips.Count; j++)
                {
                    outBytes.AddRange(BitConverter.GetBytes(strips[i].triStrips[j]));
                }

                addBytes(outBytes, 0xC5, 0x9, BitConverter.GetBytes(psets[i].reserve0));
            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            //Pointer count. Always 0 on PSET
            //Subtag count. 7 for each PSET + 1 for the end tag, always.
            WriteTagHeader(outBytes, "PSET", 0, (ushort)(psets.Count * 0x7 + 0x1));

            return outBytes.ToArray();
        }

        public static List<MESH> parseMESH(List<Dictionary<int, object>> meshRaw)
        {
            List<MESH> meshList = new List<MESH>();

            for (int i = 0; i < meshRaw.Count; i++)
            {
                MESH mesh = new MESH();

                byte[] c7 = BitConverter.GetBytes((int)meshRaw[i][0xC7]);

                mesh.tag = (short)((int)meshRaw[i][0xB0] % 0x10000);
                mesh.unkShort0 = (short)((int)meshRaw[i][0xB0] / 0x10000);
                mesh.unkByte0 = c7[0];
                mesh.unkByte1 = c7[1];
                mesh.unkShort1 = (short)((int)meshRaw[i][0xC7] / 0x10000);
                mesh.mateIndex = (int)meshRaw[i][0xB1];
                mesh.rendIndex = (int)meshRaw[i][0xB2];
                mesh.shadIndex = (int)meshRaw[i][0xB3];
                mesh.tsetIndex = (int)meshRaw[i][0xB4];
                mesh.baseMeshNodeId = (int)meshRaw[i][0xB5];
                mesh.vsetIndex = (int)meshRaw[i][0xC0];
                mesh.psetIndex = (int)meshRaw[i][0xC1];
                if(meshRaw[i].ContainsKey(0xCD))
                {
                    mesh.unkInt0 = (int)meshRaw[i][0xCD];
                }
                mesh.baseMeshSequenceId = (int)meshRaw[i][0xC2];

                meshList.Add(mesh);
            }

            return meshList;
        }

        public static byte[] toMESH(List<MESH> meshList)
        {
            List<byte> outBytes = new List<byte>();

            for (int i = 0; i < meshList.Count; i++)
            {
                if (i == 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                }
                else
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }
                int shorts = meshList[i].tag + (meshList[i].unkShort0 * 0x10000);
                int bytes = meshList[i].unkByte0 + (meshList[i].unkByte1 * 0x100) + (meshList[i].unkShort1 * 0x10000);
                addBytes(outBytes, 0xB0, 0x9, BitConverter.GetBytes(shorts));
                addBytes(outBytes, 0xC7, 0x9, BitConverter.GetBytes(bytes));
                addBytes(outBytes, 0xB1, 0x8, BitConverter.GetBytes(meshList[i].mateIndex));
                addBytes(outBytes, 0xB2, 0x8, BitConverter.GetBytes(meshList[i].rendIndex));
                addBytes(outBytes, 0xB3, 0x8, BitConverter.GetBytes(meshList[i].shadIndex));
                addBytes(outBytes, 0xB4, 0x8, BitConverter.GetBytes(meshList[i].tsetIndex));
                addBytes(outBytes, 0xB5, 0x8, BitConverter.GetBytes(meshList[i].baseMeshNodeId));
                addBytes(outBytes, 0xC0, 0x8, BitConverter.GetBytes(meshList[i].vsetIndex));
                addBytes(outBytes, 0xC1, 0x8, BitConverter.GetBytes(meshList[i].psetIndex));
                addBytes(outBytes, 0xCD, 0x8, BitConverter.GetBytes(meshList[i].unkInt0));
                addBytes(outBytes, 0xC2, 0x9, BitConverter.GetBytes(meshList[i].baseMeshSequenceId));
            }
            //outBytes.AddRange(BitConverter.GetBytes((short)0xFD)); MESH seemingly doesn't use this for some reason

            //Pointer count. Always 0 on MESH
            //Subtag count. 11 for each MESH + 1 for the end tag, always.
            WriteTagHeader(outBytes, "MESH", 0, (ushort)(meshList.Count * 0xC));

            return outBytes.ToArray();
        }

        public static unsafe List<MATE> parseMATE(List<Dictionary<int, object>> mateRaw)
        {
            List<MATE> mateList = new List<MATE>();

            for (int i = 0; i < mateRaw.Count; i++)
            {
                MATE mate = new MATE();
                mate.diffuseRGBA = (Vector4)mateRaw[i][0x30];
                mate.unkRGBA0 = (Vector4)mateRaw[i][0x31];
                mate._sRGBA = (Vector4)mateRaw[i][0x32];
                mate.unkRGBA1 = (Vector4)mateRaw[i][0x33];
                mate.reserve0 = (int)mateRaw[i][0x34];
                mate.unkFloat0 = (float)mateRaw[i][0x35];
                mate.unkFloat1 = (float)mateRaw[i][0x36];
                mate.unkInt0 = (int)mateRaw[i][0x37];
                mate.unkInt1 = (int)mateRaw[i][0x38];
                mate.alphaType = new AquaCommon.PSO2String();
                mate.alphaType.SetBytes((byte[])mateRaw[i][0x3A]);
                mate.matName = new AquaCommon.PSO2String();
                mate.matName.SetBytes((byte[])mateRaw[i][0x39]);

                mateList.Add(mate);
            }

            return mateList;
        }

        public static unsafe byte[] toMATE(List<MATE> mateList)
        {
            List<byte> outBytes = new List<byte>();

            for (int i = 0; i < mateList.Count; i++)
            {
                //Gotta make a local accessor for fixed arrays
                MATE mate = mateList[i];
                if (i == 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                }
                else
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }

                addBytes(outBytes, 0x30, 0x4A, 0x2, Reloaded.Memory.Struct.GetBytes(mate.diffuseRGBA));
                addBytes(outBytes, 0x31, 0x4A, 0x2, Reloaded.Memory.Struct.GetBytes(mate.unkRGBA0));
                addBytes(outBytes, 0x32, 0x4A, 0x2, Reloaded.Memory.Struct.GetBytes(mate._sRGBA));
                addBytes(outBytes, 0x33, 0x4A, 0x2, Reloaded.Memory.Struct.GetBytes(mate.unkRGBA1));
                addBytes(outBytes, 0x34, 0x9, BitConverter.GetBytes(mate.reserve0));
                addBytes(outBytes, 0x35, 0xA, BitConverter.GetBytes(mate.unkFloat0));
                addBytes(outBytes, 0x36, 0xA, BitConverter.GetBytes(mate.unkFloat1));
                addBytes(outBytes, 0x37, 0x9, BitConverter.GetBytes(mate.unkInt0));
                addBytes(outBytes, 0x38, 0x9, BitConverter.GetBytes(mate.unkInt1));

                //Alpha Type String
                string alphaStr = mate.alphaType.GetString();
                addBytes(outBytes, 0x3A, 0x02, (byte)alphaStr.Length, Encoding.UTF8.GetBytes(alphaStr));

                //Mat Name String. Do it this way in case of names that would break when encoded to utf8 again
                int matLen = mate.matName.GetLength();
                byte[] matBytes = new byte[matLen];
                byte[] tempMatBytes = mate.matName.GetBytes();
                for(int strIndex = 0; strIndex < matLen; strIndex++)
                {
                    matBytes[strIndex] = tempMatBytes[strIndex];
                }
                addBytes(outBytes, 0x39, 0x02, (byte)matLen, matBytes);
            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            //Pointer count. Always 0 on MATE
            //Subtag count. 12 for each MATE + 1 for the end tag, always.
            WriteTagHeader(outBytes, "MATE", 0, (ushort)(mateList.Count * 0xC + 0x1));

            return outBytes.ToArray();
        }

        public static unsafe List<REND> parseREND(List<Dictionary<int, object>> rendRaw)
        {
            List<REND> rendList = new List<REND>();

            for (int i = 0; i < rendRaw.Count; i++)
            {
                REND rend = new REND();
                rend.tag = (int)rendRaw[i][0x40];
                rend.unk0 = (int)rendRaw[i][0x41];
                rend.twosided = (int)rendRaw[i][0x42];
                rend.notOpaque = (int)rendRaw[i][0x43];

                rend.unk1 = (int)rendRaw[i][0x44];
                rend.unk2 = (int)rendRaw[i][0x45];
                rend.unk3 = (int)rendRaw[i][0x46];
                rend.unk4 = (int)rendRaw[i][0x47];

                rend.unk5 = (int)rendRaw[i][0x48];
                rend.unk6 = (int)rendRaw[i][0x49];
                rend.unk7 = (int)rendRaw[i][0x4A];
                rend.unk8 = (int)rendRaw[i][0x4B];

                rend.unk9 = (int)rendRaw[i][0x4C];
                rend.unk10 = (int)rendRaw[i][0x4D];
                rend.unk11 = (int)rendRaw[i][0x4E];
                rend.unk12 = (int)rendRaw[i][0x4F];

                rend.unk13 = (int)rendRaw[i][0x50];
                rendList.Add(rend);
            }

            return rendList;
        }

        public static unsafe byte[] toREND(List<REND> rendList)
        {
            List<byte> outBytes = new List<byte>();

            for (int i = 0; i < rendList.Count; i++)
            {
                if (i == 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                }
                else
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }

                addBytes(outBytes, 0x40, 0x9, BitConverter.GetBytes(rendList[i].tag));
                addBytes(outBytes, 0x41, 0x9, BitConverter.GetBytes(rendList[i].unk0));
                addBytes(outBytes, 0x42, 0x9, BitConverter.GetBytes(rendList[i].twosided));
                addBytes(outBytes, 0x43, 0x9, BitConverter.GetBytes(rendList[i].notOpaque));

                addBytes(outBytes, 0x44, 0x9, BitConverter.GetBytes(rendList[i].unk1));
                addBytes(outBytes, 0x45, 0x9, BitConverter.GetBytes(rendList[i].unk2));
                addBytes(outBytes, 0x46, 0x9, BitConverter.GetBytes(rendList[i].unk3));
                addBytes(outBytes, 0x47, 0x9, BitConverter.GetBytes(rendList[i].unk4));

                addBytes(outBytes, 0x48, 0x9, BitConverter.GetBytes(rendList[i].unk5));
                addBytes(outBytes, 0x49, 0x9, BitConverter.GetBytes(rendList[i].unk6));
                addBytes(outBytes, 0x4A, 0x9, BitConverter.GetBytes(rendList[i].unk7));
                addBytes(outBytes, 0x4B, 0x9, BitConverter.GetBytes(rendList[i].unk8));

                addBytes(outBytes, 0x4C, 0x9, BitConverter.GetBytes(rendList[i].unk9));
                addBytes(outBytes, 0x4D, 0x9, BitConverter.GetBytes(rendList[i].unk10));
                addBytes(outBytes, 0x4E, 0x9, BitConverter.GetBytes(rendList[i].unk11));
                addBytes(outBytes, 0x4F, 0x9, BitConverter.GetBytes(rendList[i].unk12));

                addBytes(outBytes, 0x50, 0x9, BitConverter.GetBytes(rendList[i].unk13));
            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            //Pointer count. Always 0 on REND
            //Subtag count. 18 for each REND + 1 for the end tag, always.
            WriteTagHeader(outBytes, "REND", 0, (ushort)(rendList.Count * 0x12 + 0x1));

            return outBytes.ToArray();
        }

        public static unsafe List<SHAD> parseSHAD(List<Dictionary<int, object>> shadRaw)
        {
            List<SHAD> shadList = new List<SHAD>();

            for (int i = 0; i < shadRaw.Count; i++)
            {
                SHAD shad = new SHAD();

                shad.unk0 = (int)shadRaw[i][0x90];
                shad.pixelShader = new AquaCommon.PSO2String();
                shad.pixelShader.SetBytes((byte[])shadRaw[i][0x91]);
                shad.vertexShader = new AquaCommon.PSO2String();
                shad.vertexShader.SetBytes((byte[])shadRaw[i][0x92]);
                shad.unk1 = (int)shadRaw[i][0x93];

                shadList.Add(shad);
            }

            return shadList;
        }

        public static unsafe byte[] toSHAD(List<SHAD> shadList)
        {
            List<byte> outBytes = new List<byte>();


            for (int i = 0; i < shadList.Count; i++)
            {
                SHAD shad = shadList[i];
                if (i == 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                }
                else
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }

                addBytes(outBytes, 0x90, 0x9, BitConverter.GetBytes(shad.unk0));

                //Pixel Shader String
                string pixelStr = shad.pixelShader.GetString();
                addBytes(outBytes, 0x91, 0x02, (byte)pixelStr.Length, Encoding.UTF8.GetBytes(pixelStr));

                //Vertex Shader String
                string vertStr = shad.vertexShader.GetString();
                addBytes(outBytes, 0x92, 0x02, (byte)vertStr.Length, Encoding.UTF8.GetBytes(vertStr));

                addBytes(outBytes, 0x93, 0x9, BitConverter.GetBytes(shad.unk1));

            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            //Pointer count. SHAD struct count on SHAD.
            //Subtag count. 18 for each SHAD + 1 for the end tag, always.
            WriteTagHeader(outBytes, "SHAD", (ushort)shadList.Count, (ushort)(shadList.Count * 0x5 + 0x1));

            //There's one of these for each SHAD, but they don't seem to have any meaningful contents in observed files
            for (int i = 0; i < shadList.Count; i++)
            {
                outBytes.AddRange(Encoding.UTF8.GetBytes("vtc0"));
                outBytes.AddRange(BitConverter.GetBytes(0xC));
                outBytes.AddRange(Encoding.UTF8.GetBytes("SHAP"));
                outBytes.AddRange(BitConverter.GetBytes((short)0));
                outBytes.AddRange(BitConverter.GetBytes((short)2));
                outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                outBytes.AddRange(BitConverter.GetBytes((short)0xFD));
            }

            return outBytes.ToArray();
        }

        public static unsafe List<TSTA> parseTSTA(List<Dictionary<int, object>> tstaRaw)
        {
            List<TSTA> tstaList = new List<TSTA>();

            //Make sure there are actually textures
            if (tstaRaw[0].Keys.Count > 1)
            {
                for (int i = 0; i < tstaRaw.Count; i++)
                {
                    TSTA tsta = new TSTA();

                    tsta.tag = (int)tstaRaw[i][0x60];
                    tsta.texUsageOrder = (int)tstaRaw[i][0x61];
                    tsta.modelUVSet = (int)tstaRaw[i][0x62];
                    tsta.unkVector0 = (Vector3)tstaRaw[i][0x63];
                    tsta.unkInt0 = (int)tstaRaw[i][0x64];
                    tsta.unkInt1 = (int)tstaRaw[i][0x65];
                    tsta.unkInt2 = (int)tstaRaw[i][0x66];
                    tsta.unkInt3 = (int)tstaRaw[i][0x67];
                    tsta.unkInt4 = (int)tstaRaw[i][0x68];
                    tsta.unkInt5 = (int)tstaRaw[i][0x69];
                    tsta.unkFloat0 = (float)tstaRaw[i][0x6A];
                    tsta.unkFloat1 = (float)tstaRaw[i][0x6B];
                    tsta.texName = new AquaCommon.PSO2String();
                    tsta.texName.SetBytes((byte[])tstaRaw[i][0x6C]);

                    tstaList.Add(tsta);
                }
            }

            return tstaList;
        }

        public static unsafe byte[] toTSTA(List<TSTA> tstaList)
        {
            List<byte> outBytes = new List<byte>();
            
            //Normally the FC tag is included in the count of the rest of these, but when there's no tags we account for it here.
            int emptyArray = 0;
            if (tstaList.Count == 0)
            {
                emptyArray++;
            }

            outBytes.AddRange(BitConverter.GetBytes((short)0xFC)); //Always there, even if there's nothing in the list
            for (int i = 0; i < tstaList.Count; i++)
            {
                TSTA tsta = tstaList[i];
                if (i != 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }

                addBytes(outBytes, 0x60, 0x9, BitConverter.GetBytes(tstaList[i].tag));
                addBytes(outBytes, 0x61, 0x9, BitConverter.GetBytes(tstaList[i].texUsageOrder));
                addBytes(outBytes, 0x62, 0x9, BitConverter.GetBytes(tstaList[i].modelUVSet));
                addBytes(outBytes, 0x63, 0x4A, 0x1, Reloaded.Memory.Struct.GetBytes(tstaList[i].unkVector0));
                addBytes(outBytes, 0x64, 0x9, BitConverter.GetBytes(tstaList[i].unkInt0));
                addBytes(outBytes, 0x65, 0x9, BitConverter.GetBytes(tstaList[i].unkInt1));
                addBytes(outBytes, 0x66, 0x9, BitConverter.GetBytes(tstaList[i].unkInt2));
                addBytes(outBytes, 0x67, 0x9, BitConverter.GetBytes(tstaList[i].unkInt3));
                addBytes(outBytes, 0x68, 0x9, BitConverter.GetBytes(tstaList[i].unkInt4));
                addBytes(outBytes, 0x69, 0x9, BitConverter.GetBytes(tstaList[i].unkInt5));
                addBytes(outBytes, 0x6A, 0xA, BitConverter.GetBytes(tstaList[i].unkFloat0));
                addBytes(outBytes, 0x6B, 0xA, BitConverter.GetBytes(tstaList[i].unkFloat1));

                //TexName String
                string texNameStr = tsta.texName.GetString();
                addBytes(outBytes, 0x6C, 0x02, (byte)texNameStr.Length, Encoding.UTF8.GetBytes(texNameStr));
            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));
            WriteTagHeader(outBytes, "TSTA", 0, (ushort)(tstaList.Count * 0xE + 0x1 + emptyArray));

            return outBytes.ToArray();
        }

        public static unsafe List<TSET> parseTSET(List<Dictionary<int, object>> tsetRaw)
        {
            List<TSET> tsetList = new List<TSET>();

            for (int i = 0; i < tsetRaw.Count; i++)
            {
                TSET tset = new TSET();

                tset.unkInt0 = (int)tsetRaw[i][0x70];
                tset.texCount = (int)tsetRaw[i][0x71];
                tset.unkInt1 = (int)tsetRaw[i][0x72];
                tset.unkInt2 = (int)tsetRaw[i][0x73];
                tset.unkInt3 = (int)tsetRaw[i][0x74];

                //Read tsta texture IDs
                using (Stream stream = new MemoryStream((byte[])tsetRaw[i][0x75]))
                using (var streamReader = new BufferedStreamReader(stream, 8192))
                {
                    for(int j = 0; j < 4; j++)
                    {
                        tset.tstaTexIDs[j] = streamReader.Read<int>();
                    }
                }

                tsetList.Add(tset);
            }

            return tsetList;
        }

        public static unsafe byte[] toTSET(List<TSET> tsetList)
        {
            List<byte> outBytes = new List<byte>();

            for (int i = 0; i < tsetList.Count; i++)
            {
                TSET tset = tsetList[i];
                if (i == 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                }
                else
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }

                addBytes(outBytes, 0x70, 0x9, BitConverter.GetBytes(tset.unkInt0));
                addBytes(outBytes, 0x71, 0x8, BitConverter.GetBytes(tset.texCount));
                addBytes(outBytes, 0x72, 0x9, BitConverter.GetBytes(tset.unkInt1));
                addBytes(outBytes, 0x73, 0x9, BitConverter.GetBytes(tset.unkInt2));
                addBytes(outBytes, 0x74, 0x9, BitConverter.GetBytes(tset.unkInt3));
                addBytes(outBytes, 0x75, 0x88, 0x8, 0x3, BitConverter.GetBytes(tset.tstaTexIDs[0]));
                for(int j = 1; j < 4; j++)
                {
                    outBytes.AddRange(BitConverter.GetBytes(tset.tstaTexIDs[j]));
                }
            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            //Pointer count. Always 0 on TSET
            //Subtag count. 7 for each TSET + 1 for the end tag, always.
            WriteTagHeader(outBytes, "TSET", 0, (ushort)(tsetList.Count * 0x7 + 0x1));

            return outBytes.ToArray();
        }

        public static unsafe List<TEXF> parseTEXF(List<Dictionary<int, object>> texfRaw)
        {
            List<TEXF> texfList = new List<TEXF>();

            //Make sure there are texture refs to get
            if (texfRaw[0].Keys.Count > 1)
            {
                for (int i = 0; i < texfRaw.Count; i++)
                {
                    TEXF texf = new TEXF();

                    texf.texName = new AquaCommon.PSO2String();
                    texf.texName.SetBytes((byte[])texfRaw[i][0x80]);

                    texfList.Add(texf);
                }
            }
            return texfList;
        }

        public static unsafe byte[] toTEXF(List<TEXF> texfList)
        {
            List<byte> outBytes = new List<byte>();

            //Normally the FC tag is included in the count of the rest of these, but when there's no tags we account for it here.
            int emptyArray = 0;
            if (texfList.Count == 0)
            {
                emptyArray++;
            }

            outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
            for (int i = 0; i < texfList.Count; i++)
            {
                TEXF texf = texfList[i];
                if (i != 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }

                //TexName String
                string texNameStr = texf.texName.GetString();
                addBytes(outBytes, 0x80, 0x02, (byte)texNameStr.Length, Encoding.UTF8.GetBytes(texNameStr));
            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            //Pointer count. Always 0 on TEXF
            //Subtag count. 2 for each TEXF + 1 for the end tag, always.
            WriteTagHeader(outBytes, "TEXF", 0, (ushort)(texfList.Count * 0x2 + 0x1 + emptyArray));

            return outBytes.ToArray();
        }

        //Technically, this data is written out as a list, but should only ever have one entry.
        public static unsafe UNRM parseUNRM(List<Dictionary<int, object>> unrmRaw)
        {
            UNRM unrm = new UNRM();

            if(unrmRaw.Count > 1)
            {
                throw new Exception("Unexpected UNRM size! Please report the offending file!");
            }

            for (int i = 0; i < unrmRaw.Count; i++)
            {
                unrm.vertGroupCountCount = (int)unrmRaw[i][0xDA];
                //Read vert group groups
                using (Stream stream = new MemoryStream((byte[])unrmRaw[i][0xDB]))
                using (var streamReader = new BufferedStreamReader(stream, 8192))
                {
                    for (int j = 0; j < unrm.vertGroupCountCount; j++)
                    {
                        unrm.unrmVertGroups.Add(streamReader.Read<int>());
                    }
                }
                unrm.vertCount = (int)unrmRaw[i][0xDC];
                //Read vert mesh ids
                using (Stream stream = new MemoryStream((byte[])unrmRaw[i][0xDD]))
                using (var streamReader = new BufferedStreamReader(stream, 8192))
                {
                    for (int j = 0; j < unrm.vertGroupCountCount; j++)
                    {
                        List<int> meshIds = new List<int>();
                        for(int k = 0; k < unrm.unrmVertGroups[j]; k++)
                        {
                            meshIds.Add(streamReader.Read<int>());
                        }
                        unrm.unrmMeshIds.Add(meshIds);
                    }
                }
                //Read vert ids
                using (Stream stream = new MemoryStream((byte[])unrmRaw[i][0xDE]))
                using (var streamReader = new BufferedStreamReader(stream, 8192))
                {
                    for (int j = 0; j < unrm.vertGroupCountCount; j++)
                    {
                        List<int> vertIds = new List<int>();
                        for (int k = 0; k < unrm.unrmVertGroups[j]; k++)
                        {
                            vertIds.Add(streamReader.Read<int>());
                        }
                        unrm.unrmVertIds.Add(vertIds);
                    }
                }
            }

            return unrm;
        }

        public static unsafe byte[] toUNRM(UNRM unrm)
        {
            List<byte> outBytes = new List<byte>();

            //Technically an array so we put the array start tag
            outBytes.AddRange(BitConverter.GetBytes((short)0xFC));

            addBytes(outBytes, 0xDA, 0x9, BitConverter.GetBytes(unrm.vertGroupCountCount));

            //unrm vert groups
            outBytes.Add(0xDB); outBytes.Add(0x89);
            if (unrm.vertGroupCountCount - 1 > byte.MaxValue)
            {
                if (unrm.vertGroupCountCount - 1 > ushort.MaxValue)
                {
                    outBytes.Add(0x18);
                    outBytes.AddRange(BitConverter.GetBytes(unrm.vertGroupCountCount - 1));
                }
                else
                {
                    outBytes.Add(0x10);
                    outBytes.AddRange(BitConverter.GetBytes((ushort)(unrm.vertGroupCountCount - 1)));
                }
            }
            else
            {
                outBytes.Add(0x08);
                outBytes.Add((byte)(unrm.vertGroupCountCount - 1));

            }
            for (int i = 0; i < unrm.vertGroupCountCount; i++)
            {
                outBytes.AddRange(BitConverter.GetBytes(unrm.unrmVertGroups[i]));
            }

            addBytes(outBytes, 0xDC, 0x9, BitConverter.GetBytes(unrm.vertCount));

            //unrm mesh ids
            outBytes.Add(0xDD); outBytes.Add(0x89);
            int meshIDCount = getListOfListOfIntsIntCount(unrm.unrmMeshIds);
            if (meshIDCount - 1 > byte.MaxValue)
            {
                if (meshIDCount - 1 > ushort.MaxValue)
                {
                    outBytes.Add(0x18);
                    outBytes.AddRange(BitConverter.GetBytes(meshIDCount - 1));
                }
                else
                {
                    outBytes.Add(0x10);
                    outBytes.AddRange(BitConverter.GetBytes((ushort)(meshIDCount - 1)));
                }
            }
            else
            {
                outBytes.Add(0x08);
                outBytes.Add((byte)(meshIDCount - 1));
            }
            for (int i = 0; i < unrm.unrmMeshIds.Count; i++)
            {
                for (int j = 0; j < unrm.unrmMeshIds[i].Count; j++)
                {
                    outBytes.AddRange(BitConverter.GetBytes(unrm.unrmMeshIds[i][j]));
                }
            }

            //unrm vert ids
            outBytes.Add(0xDE); outBytes.Add(0x89);
            int vertIdCount = getListOfListOfIntsIntCount(unrm.unrmVertIds);
            if (vertIdCount - 1 > byte.MaxValue)
            {
                if(vertIdCount - 1 > ushort.MaxValue)
                {
                    outBytes.Add(0x18);
                    outBytes.AddRange(BitConverter.GetBytes(vertIdCount - 1));
                } else
                {
                    outBytes.Add(0x10);
                    outBytes.AddRange(BitConverter.GetBytes((ushort)(vertIdCount - 1)));
                }
            }
            else
            {
                outBytes.Add(0x08);
                outBytes.Add((byte)(vertIdCount - 1));
            }
            for (int i = 0; i < unrm.unrmVertIds.Count; i++)
            {
                for (int j = 0; j < unrm.unrmVertIds[i].Count; j++)
                {
                    outBytes.AddRange(BitConverter.GetBytes(unrm.unrmVertIds[i][j]));
                }
            }

            //Technically an array so we put the array end tag
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            //Pointer count. Always 0 on UNRM
            //Subtag count. In theory, always 7 for UNRM
            WriteTagHeader(outBytes, "UNRM", 0, 7);

            return outBytes.ToArray();
        }

        public static byte[] toROOT(string rootString = "hnd2aqg ver.1.61 Build: Feb 28 2012 18:46:06")
        {
            List<byte> outBytes = new List<byte>();

            addBytes(outBytes, 0x0, 0x2, (byte)rootString.Length, Encoding.UTF8.GetBytes(rootString));
            WriteTagHeader(outBytes, "ROOT", 1, 1);

            return outBytes.ToArray();
        }

        public static byte[] toAQGFVTBF()
        {
            List<byte> outBytes = new List<byte>();

            outBytes.AddRange(new byte[] { 0x56, 0x54, 0x42, 0x46 }); //VTBF
            outBytes.AddRange(new byte[] { 0x10, 0, 0, 0 });
            outBytes.AddRange(new byte[] { 0x41, 0x51, 0x47, 0x46, 0x1, 0, 0, 0x4C }); //AQGF and the constants after

            return outBytes.ToArray();
        }

        public static NDTR parseNDTR(List<Dictionary<int, object>> ndtrRaw)
        {
            NDTR ndtr = new NDTR();

            ndtr.boneCount = (int)ndtrRaw[0][0x01];
            ndtr.unknownCount = (int)ndtrRaw[0][0x02];
            ndtr.effCount = (int)ndtrRaw[0][0xFA];

            return ndtr;
        }

        public static byte[] toNDTR(NDTR ndtr)
        {
            List<byte> outBytes = new List<byte>();

            addBytes(outBytes, 0x1, 0x8, BitConverter.GetBytes(ndtr.boneCount));
            addBytes(outBytes, 0x2, 0x8, BitConverter.GetBytes(ndtr.unknownCount));
            addBytes(outBytes, 0xFA, 0x8, BitConverter.GetBytes(ndtr.effCount));

            WriteTagHeader(outBytes, "NDTR", 0x2, 0x3);

            return outBytes.ToArray();
        }

        public static List<NODE> parseNODE(List<Dictionary<int, object>> nodeRaw)
        {
            List<NODE> nodeList = new List<NODE>();

            for(int i = 0; i < nodeRaw.Count; i++)
            {
                NODE node = new NODE();

                byte[] shorts = BitConverter.GetBytes((int)nodeRaw[i][0x03]);
                node.boneShort1 = (ushort)(shorts[0] * 0x100 + shorts[1]);
                node.boneShort2 = (ushort)(shorts[2] * 0x100 + shorts[3]);
                node.animatedFlag = (int)nodeRaw[i][0x0B];
                node.parentId = (int)nodeRaw[i][0x04];
                node.unkNode = (int)nodeRaw[i][0x0F];
                node.firstChild = (int)nodeRaw[i][0x05];
                node.nextSibling = (int)nodeRaw[i][0x06];
                node.const0_2 = (int)nodeRaw[i][0x0C];
                node.pos = (Vector3)nodeRaw[i][0x07];
                node.eulRot = (Vector3)nodeRaw[i][0x08];
                node.scale = (Vector3)nodeRaw[i][0x09];
                node.m1 = ((Vector4[])nodeRaw[i][0x0A])[0];
                node.m2 = ((Vector4[])nodeRaw[i][0x0A])[1];
                node.m3 = ((Vector4[])nodeRaw[i][0x0A])[2];
                node.m4 = ((Vector4[])nodeRaw[i][0x0A])[3];
                node.boneName = new AquaCommon.PSO2String();
                node.boneName.SetBytes((byte[])nodeRaw[i][0x0D]);

                nodeList.Add(node);
            }

            return nodeList;
        }

        public static byte[] toNODE(List<NODE> nodeList)
        {
            List<byte> outBytes = new List<byte>();

            for(int i = 0; i < nodeList.Count; i++)
            {
                NODE node = nodeList[i];
                if (i == 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                }
                else
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }
                addBytes(outBytes, 0x3, 0x9, BitConverter.GetBytes(node.boneShort1 * 0x10000 + node.boneShort2));
                addBytes(outBytes, 0x4, 0x8, BitConverter.GetBytes(node.parentId));
                addBytes(outBytes, 0xF, 0x8, BitConverter.GetBytes(node.unkNode));
                addBytes(outBytes, 0x5, 0x8, BitConverter.GetBytes(node.firstChild));
                addBytes(outBytes, 0x6, 0x8, BitConverter.GetBytes(node.nextSibling));
                addBytes(outBytes, 0x7, 0x4A, 0x1, Reloaded.Memory.Struct.GetBytes(node.pos));
                addBytes(outBytes, 0x8, 0x4A, 0x1, Reloaded.Memory.Struct.GetBytes(node.eulRot));
                addBytes(outBytes, 0x9, 0x4A, 0x1, Reloaded.Memory.Struct.GetBytes(node.scale));
                addBytes(outBytes, 0xA, 0xCA, 0xA, 0x3, Reloaded.Memory.Struct.GetBytes(node.m1));
                outBytes.AddRange(Reloaded.Memory.Struct.GetBytes(node.m2));
                outBytes.AddRange(Reloaded.Memory.Struct.GetBytes(node.m3));
                outBytes.AddRange(Reloaded.Memory.Struct.GetBytes(node.m4));
                addBytes(outBytes, 0xB, 0x9, BitConverter.GetBytes(node.animatedFlag));
                addBytes(outBytes, 0xC, 0x8, BitConverter.GetBytes(node.const0_2));

                //Bone Name String
                string boneNameStr = node.boneName.GetString();
                addBytes(outBytes, 0x80, 0x02, (byte)boneNameStr.Length, Encoding.UTF8.GetBytes(boneNameStr));
            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            WriteTagHeader(outBytes, "NODE", 0, (ushort)(nodeList.Count * 0xC + 1));

            return outBytes.ToArray();
        }

        public static List<NODO> parseNODO(List<Dictionary<int, object>> nodoRaw)
        {
            List<NODO> nodoList = new List<NODO>();

            if (nodoRaw[0].Keys.Count > 1)
            {
                for (int i = 0; i < nodoRaw.Count; i++)
                {
                    NODO nodo = new NODO();

                    byte[] shorts = BitConverter.GetBytes((int)nodoRaw[i][0x03]);
                    nodo.boneShort1 = (ushort)(shorts[0] * 0x100 + shorts[1]);
                    nodo.boneShort2 = (ushort)(shorts[2] * 0x100 + shorts[3]);
                    nodo.animatedFlag = (int)nodoRaw[i][0x0B];
                    nodo.parentId = (int)nodoRaw[i][0x04];
                    nodo.pos = (Vector3)nodoRaw[i][0x07];
                    nodo.eulRot = (Vector3)nodoRaw[i][0x08];
                    nodo.boneName = new AquaCommon.PSO2String();
                    nodo.boneName.SetBytes((byte[])nodoRaw[i][0x0D]);

                    nodoList.Add(nodo);
                }
            }

            return nodoList;
        }

        public static byte[] toNODO(List<NODO> nodoList)
        {
            List<byte> outBytes = new List<byte>();

            for (int i = 0; i < nodoList.Count; i++)
            {
                NODO nodo = nodoList[i];
                if (i == 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                }
                else
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }
                addBytes(outBytes, 0x3, 0x9, BitConverter.GetBytes(nodo.boneShort1 * 0x10000 + nodo.boneShort2));
                addBytes(outBytes, 0x4, 0x8, BitConverter.GetBytes(nodo.parentId));
                addBytes(outBytes, 0x7, 0x4A, 0x1, Reloaded.Memory.Struct.GetBytes(nodo.pos));
                addBytes(outBytes, 0x8, 0x4A, 0x1, Reloaded.Memory.Struct.GetBytes(nodo.eulRot));
                addBytes(outBytes, 0xB, 0x9, BitConverter.GetBytes(nodo.animatedFlag));

                //Bone Name String
                string boneNameStr = nodo.boneName.GetString();
                addBytes(outBytes, 0x80, 0x02, (byte)boneNameStr.Length, Encoding.UTF8.GetBytes(boneNameStr));
            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            WriteTagHeader(outBytes, "NODO", 0, (ushort)(nodoList.Count * 7 + 1));

            return outBytes.ToArray();
        }

        private static void WriteTagHeader(List<byte> outBytes, string tagString, ushort pointerCount, ushort subtagCount)
        {
            outBytes.InsertRange(0, Encoding.UTF8.GetBytes(tagString));
            outBytes.InsertRange(0x4, BitConverter.GetBytes(pointerCount));  //Pointer count
            outBytes.InsertRange(0x6, BitConverter.GetBytes(subtagCount)); //Subtag count

            outBytes.InsertRange(0, BitConverter.GetBytes(outBytes.Count)); //Data body size
            outBytes.InsertRange(0, Encoding.UTF8.GetBytes("vtc0"));
        }

        public static int getListOfListOfIntsIntCount(List<List<int>> intListlist)
        {
            int count = 0;
            for(int i = 0; i < intListlist.Count; i++)
            {
                count += intListlist[i].Count;
            }

            return count;
        }

        public static void addBytes(List<byte> outBytes, byte id, byte dataType, byte[] data)
        {
            outBytes.Add(id); 
            outBytes.Add(dataType);
            outBytes.AddRange(data);
        }

        public static void addBytes(List<byte> outBytes, byte id, byte dataType, byte vecAmt, byte[] data)
        {
            outBytes.Add(id);
            outBytes.Add(dataType);
            outBytes.Add(vecAmt);
            outBytes.AddRange(data);
        }

        public static void addBytes(List<byte> outBytes, byte id, byte dataType, byte subDataType, byte subDataAdditions, byte[] data)
        {
            outBytes.Add(id);
            outBytes.Add(dataType);
            outBytes.Add(subDataType);
            outBytes.Add(subDataAdditions);
            outBytes.AddRange(data);
        }

        public static ushort flagCheck(int check)
        {
            if(check > 0)
            {
                return 1;
            } else
            {
                return 0;
            }
        }
    }
}
