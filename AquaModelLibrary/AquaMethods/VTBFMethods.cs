using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using static AquaModelLibrary.AquaObject;
using static AquaModelLibrary.AquaMethods.AquaObjectMethods;
using System.Windows;

namespace AquaModelLibrary.AquaMethods
{
    public class VTBFMethods
    {
        public List<Dictionary<int, object>> ReadVTBFTag(BufferedStreamReader streamReader)
        {
            List<Dictionary<int, object>> vtbfData = new List<Dictionary<int, object>>();

            streamReader.Seek(0x4, SeekOrigin.Current); //vtc0
            uint bodyLength = streamReader.Read<uint>();
            int mainTagType = streamReader.Read<int>();
            short pointerCount = streamReader.Read<short>(); //Not important for reading. Game assumedly uses this at runtime to know how many pointer ints to prepare for the block.
            short entryCount = streamReader.Read<short>();

            Dictionary<int, object> vtbfDict = new Dictionary<int, object>();

            for (int i = 0; i < entryCount; i++)
            {
                byte dataId = streamReader.Read<byte>();
                byte dataType = streamReader.Read<byte>();
                byte subDataType;
                int subDataAdditions;

                //Check for special ids
                switch (dataId)
                {
                    case 0xFD: //End of sequence and should be end of tag
                        break;
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
                                MessageBox.Show($"Unknown subDataAdditions amount {subDataAdditions}");
                                throw new Exception();
                        }

                        break;
                    case 0x84: //Theoretical array of bytes
                    case 0x85:
                        subDataType = streamReader.Read<byte>();      //Next entity type. 0x8 for byte, 0x10 for short
                        switch (subDataType) //The last array entry aka data count - 1.
                        {
                            case 0x8:
                                subDataAdditions = streamReader.Read<byte>();
                                break;
                            case 0x10:
                                subDataAdditions = streamReader.Read<ushort>();
                                break;
                            default:
                                throw new Exception($"Unknown subdataType {subDataType}");
                        }
                        data = streamReader.ReadBytes(streamReader.Position(), subDataAdditions);

                        streamReader.Seek(subDataAdditions, SeekOrigin.Current);
                        break;
                    case 0x86: //Array of shorts
                    case 0x87:
                        subDataType = streamReader.Read<byte>();      //Next entity type. 0x8 for byte, 0x10 for short
                        switch (subDataType) //The last array entry aka data count - 1.
                        {
                            case 0x8:
                                subDataAdditions = streamReader.Read<byte>();
                                break;
                            case 0x10:
                                subDataAdditions = streamReader.Read<ushort>();
                                break;
                            default:
                                throw new Exception($"Unknown subdataType {subDataType}");
                        }

                        data = new short[subDataAdditions + 1];
                        for (int j = 0; j < subDataAdditions; j++)
                        {
                            ((short[])data)[j] = streamReader.Read<short>();
                        }
                        break;
                    case 0x88:
                    case 0x89: //Array of ints. Often needs to be processed in various ways in post.
                        subDataType = streamReader.Read<byte>();      //Next entity type. 0x8 for byte, 0x10 for short
                        switch (subDataType) //The last array entry aka data count - 1.
                        {
                            case 0x8:
                                subDataAdditions = streamReader.Read<byte>();
                                break;
                            case 0x10:
                                subDataAdditions = streamReader.Read<ushort>();
                                break;
                            default:
                                throw new Exception($"Unknown subdataType {subDataType}");
                        }
                        subDataAdditions *= 4; //The field is stored as some amount of int32s. Therefore, multiplying by 4 gives us the byte buffer length.

                        data = streamReader.ReadBytes(streamReader.Position(), subDataAdditions); //Read the whole vert buffer at once as byte array. We'll handle it later.

                        streamReader.Seek(subDataAdditions, SeekOrigin.Current);
                        break;
                    case 0xCA: //Float Matrix, observed only as 4x4
                        subDataType = streamReader.Read<byte>(); //Expected to always be 0xA for float
                        subDataAdditions = streamReader.Read<byte>(); //Expected to always be 0x3, maybe for last array entry id
                        data = new Vector4[subDataAdditions + 1];

                        for (int j = 0; j < subDataAdditions; j++)
                        {
                            switch (subDataType)
                            {
                                case 0xA:
                                    ((Vector4[])data)[j] = streamReader.Read<Vector4>();
                                    break;
                                default:
                                    Console.WriteLine($"Unknown subDataType {subDataType}, please report!");
                                    throw new NotImplementedException();
                            }
                        }
                        break;
                    default:
                        Console.WriteLine($"Unknown dataType {dataType}, please report!");
                        throw new NotImplementedException();
                }

                vtbfDict.Add(dataId, data);
            }

            return vtbfData;
        }

        public OBJC parseOBJC(List<Dictionary<int, object>> objcRaw)
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
            bounding.modelCenter = ((Vector3[])(objcRaw[0][0x1E]))[0];
            bounding.boundingRadius = (float)(objcRaw[0][0x1F]);
            bounding.modelCenter2 = ((Vector3[])(objcRaw[0][0x20]))[0];
            bounding.maxMinXYZDifference = ((Vector3[])(objcRaw[0][0x21]))[0];

            objc.bounds = bounding;

            return objc;
        }

        public byte[] toOBJC(OBJC objc, bool useUNRMs)
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

        public List<VSET> ParseVSET(List<Dictionary<int, object>> vsetRaw, out List<List<short>> bonePalettes, out List<List<short>> edgeVertsLists)
        {
            List<VSET> vsetList = new List<VSET>();
            bonePalettes = new List<List<short>>();
            edgeVertsLists = new List<List<short>>();

            for (int i = 0; i < vsetRaw.Count; i++)
            {
                VSET vset = new VSET();
                vset.vertDataSize = (int)(vsetRaw[i][0xB6]);
                vset.vertTypesCount = (int)(vsetRaw[i][0xBF]);
                vset.vtxlCount = (int)(vsetRaw[i][0xB9]);
                vset.reserve0 = (int)(vsetRaw[i][0xC4]);

                vset.bonePaletteCount = (int)(vsetRaw[i][0xBD]);
                var rawBP = (vsetRaw[i][0xBE]);
                if (rawBP is short)
                {
                    bonePalettes.Add(new List<short> { (short)rawBP });
                } else if (rawBP is short[])
                {
                    bonePalettes.Add(((short[])rawBP).ToList());
                }
                //Not sure on these, but I don't know that unk0-2 get used normally
                vset.unk0 = (int)(vsetRaw[i][0xC8]);
                vset.unk1 = (int)(vsetRaw[i][0xCC]);

                vset.edgeVertsCount = (int)(vsetRaw[i][0xC9]);
                var rawEV = (vsetRaw[i][0xCA]);
                if (rawEV is short)
                {
                    edgeVertsLists.Add(new List<short> { (short)rawEV });
                }
                else if (rawEV is short[])
                {
                    edgeVertsLists.Add(((short[])rawEV).ToList());
                }

                vsetList.Add(vset);
            }

            return vsetList;
        }

        public byte[] toVSETList(List<VSET> vsetList, List<VTXL> vtxlList)
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
                    addBytes(outBytes, 0xBD, 0x8, BitConverter.GetBytes(vtxlList[i].bonePalette.Count));
                    if (vtxlList[i].bonePalette.Count > 0)
                    {
                        subTagCount++;

                        outBytes.Add(0xBE);
                        outBytes.Add(0x86);
                        outBytes.Add(0x8);
                        outBytes.Add((byte)vtxlList[i].bonePalette.Count);
                        for (int j = 0; j < vtxlList[i].bonePalette.Count; j++)
                        {
                            outBytes.AddRange(BitConverter.GetBytes(vtxlList[i].bonePalette[j]));
                        }
                    } else
                    {
                        addBytes(outBytes, 0xBD, 0x8, BitConverter.GetBytes((int)0));
                    }

                } else
                {
                    addBytes(outBytes, 0xBD, 0x8, BitConverter.GetBytes((int)0));
                }

                addBytes(outBytes, 0xC8, 0x9, BitConverter.GetBytes(vsetList[i].unk0));
                addBytes(outBytes, 0xCC, 0x9, BitConverter.GetBytes(vsetList[i].unk1));

                if (vtxlList[i].edgeVerts != null)
                {
                    addBytes(outBytes, 0xC9, 0x8, BitConverter.GetBytes(vtxlList[i].edgeVerts.Count));
                    if (vtxlList[i].edgeVerts.Count > 0)
                    {
                        subTagCount++;

                        outBytes.Add(0xCA);
                        outBytes.Add(0x86);
                        outBytes.Add(0x8);
                        outBytes.Add((byte)vtxlList[i].edgeVerts.Count);
                        for (int j = 0; j < vtxlList[i].edgeVerts.Count; j++)
                        {
                            outBytes.AddRange(BitConverter.GetBytes(vtxlList[i].edgeVerts[j]));
                        }
                    }
                    else
                    {
                        addBytes(outBytes, 0xC9, 0x8, BitConverter.GetBytes((int)0));
                    }

                }
                else
                {
                    addBytes(outBytes, 0xC9, 0x8, BitConverter.GetBytes((int)0));
                }

            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));
            subTagCount++;

            //In VTBF, VSETS are all treated as part of the same struct
            outBytes.InsertRange(0, Encoding.UTF8.GetBytes("VSET"));
            outBytes.InsertRange(0x4, BitConverter.GetBytes((short)(0x2 * vsetList.Count)));  //Pointer count. In this case, 0x2 times the VSET count.
            outBytes.InsertRange(0x8, BitConverter.GetBytes((short)subTagCount)); //Subtag count

            outBytes.InsertRange(0, BitConverter.GetBytes(outBytes.Count));          //Data body size
            outBytes.InsertRange(0, Encoding.UTF8.GetBytes("vtc0"));

            return outBytes.ToArray();
        }

        public void parseVTXE_VTXL(List<Dictionary<int, object>> vtxeRaw, List<Dictionary<int, object>> vtxlRaw, out VTXE vtxe, out VTXL vtxl)
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

            int vertCount = ((byte[])vtxlRaw[0][0x89]).Length / vertSize;

            using (Stream stream = new MemoryStream((byte[])vtxlRaw[0][0x89]))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                ReadVTXL(streamReader, vtxe, vtxl, vertCount, vtxe.vertDataTypes.Count);
            }
        }

        public byte[] toVTXE_VTXL(VTXE vtxe, VTXL vtxl)
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

            outBytes.InsertRange(0, Encoding.UTF8.GetBytes("VTXE"));
            outBytes.InsertRange(0x4, BitConverter.GetBytes((short)(0)));  //Pointer count. Always 0 on VTXE
            outBytes.InsertRange(0x8, BitConverter.GetBytes((short)vtxe.vertDataTypes.Count * 5 + 1)); //Subtag count

            outBytes.InsertRange(0, BitConverter.GetBytes(outBytes.Count));          //Data body size
            outBytes.InsertRange(0, Encoding.UTF8.GetBytes("vtc0"));

            int vtxeEnd = outBytes.Count;

            //VTXL
            outBytes.Add(0xBA);
            outBytes.Add(0x89);
            int vtxlSizeArea = outBytes.Count;
            for (int i = 0; i < vtxl.vertPositions.Count; i++)
            {
                for (int j = 0; j < vtxe.vertDataTypes.Count; j++)
                {
                    switch (vtxe.vertDataTypes[j].dataType)
                    {
                        case (int)AquaObject.VertFlags.VertPosition:
                            outBytes.AddRange(Reloaded.Memory.Struct.GetBytes(vtxl.vertPositions[i]));
                            break;
                        case (int)AquaObject.VertFlags.VertWeight:
                            outBytes.AddRange(Reloaded.Memory.Struct.GetBytes(vtxl.vertWeights[i]));
                            break;
                        case (int)AquaObject.VertFlags.VertNormal:
                            outBytes.AddRange(Reloaded.Memory.Struct.GetBytes(vtxl.vertNormals[i]));
                            break;
                        case (int)AquaObject.VertFlags.VertColor:
                            for (int color = 0; color < 4; color++)
                            {
                                outBytes.Add(vtxl.vertColors[i][color]);
                            }
                            break;
                        case (int)AquaObject.VertFlags.VertColor2:
                            for (int color = 0; color < 4; color++)
                            {
                                outBytes.Add(vtxl.vertColor2s[i][color]);
                            }
                            break;
                        case (int)AquaObject.VertFlags.VertWeightIndex:
                            for (int weight = 0; weight < 4; weight++)
                            {
                                outBytes.Add(vtxl.vertWeightIndices[i][weight]);
                            }
                            break;
                        case (int)AquaObject.VertFlags.VertUV1:
                            outBytes.AddRange(Reloaded.Memory.Struct.GetBytes(vtxl.uv1List[i]));
                            break;
                        case (int)AquaObject.VertFlags.VertUV2:
                            outBytes.AddRange(Reloaded.Memory.Struct.GetBytes(vtxl.uv2List[i]));
                            break;
                        case (int)AquaObject.VertFlags.VertUV3:
                            outBytes.AddRange(Reloaded.Memory.Struct.GetBytes(vtxl.uv3List[i]));
                            break;
                        case (int)AquaObject.VertFlags.VertTangent:
                            outBytes.AddRange(Reloaded.Memory.Struct.GetBytes(vtxl.vertTangentList[i]));
                            break;
                        case (int)AquaObject.VertFlags.VertBinormal:
                            outBytes.AddRange(Reloaded.Memory.Struct.GetBytes(vtxl.vertBinormalList[i]));
                            break;
                        default:
                            MessageBox.Show($"Unknown Vert type {vtxe.vertDataTypes[j].dataType}! Please report!");
                            throw new Exception("Not implemented!");
                    }


                }
            }

            //Calc and insert the vert data counts in post due to the way sega does it.
            int vertDataCount = ((outBytes.Count - vtxlSizeArea) / 4) - 1;
            if (vertDataCount > byte.MaxValue)
            {
                outBytes.Insert(vtxlSizeArea, 0x10);
                outBytes.InsertRange(vtxlSizeArea + 0x4, BitConverter.GetBytes((short)(vertDataCount)));
            }
            else
            {
                outBytes.Insert(vtxlSizeArea, 0x8);
                outBytes.Insert(vtxlSizeArea + 0x4, (byte)vertDataCount);
            }

            outBytes.InsertRange(vtxeEnd, Encoding.UTF8.GetBytes("VTXL"));
            outBytes.InsertRange(vtxeEnd + 0x4, BitConverter.GetBytes((short)(0)));  //Pointer count. Always 0 on VTXL
            outBytes.InsertRange(vtxeEnd + 0x8, BitConverter.GetBytes((short)1)); //Subtag count

            outBytes.InsertRange(vtxeEnd, BitConverter.GetBytes(outBytes.Count - vtxeEnd));          //Data body size
            outBytes.InsertRange(vtxeEnd, Encoding.UTF8.GetBytes("vtc0"));

            return outBytes.ToArray();
        }

        public void ParsePSET(List<Dictionary<int, object>> psetRaw, out List<PSET> psets, out List<stripData> strips)
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
                strip.triStrips = ((short[])psetRaw[i][0xB8]).ToList();
                pset.reserve0 = (int)psetRaw[i][0xC5];

                psets.Add(pset);
                strips.Add(strip);
            }
        }

        public byte[] toPSET(List<PSET> psets, List<stripData> strips)
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
                    outBytes.Add(0x10);
                    outBytes.AddRange(BitConverter.GetBytes((short)(strips[i].triStrips.Count - 1)));
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

            outBytes.InsertRange(0, Encoding.UTF8.GetBytes("PSET"));
            outBytes.InsertRange(0x4, BitConverter.GetBytes((short)(0)));  //Pointer count. Always 0 on PSET
            outBytes.InsertRange(0x8, BitConverter.GetBytes((short)psets.Count * 0x7 + 0x1)); //Subtag count. 7 for each PSET + 1 for the end tag, always.

            outBytes.InsertRange(0, BitConverter.GetBytes(outBytes.Count));          //Data body size
            outBytes.InsertRange(0, Encoding.UTF8.GetBytes("vtc0"));

            return outBytes.ToArray();
        }

        public List<MESH> ParseMESH(List<Dictionary<int, object>> meshRaw)
        {
            List<MESH> meshList = new List<MESH>();

            for (int i = 0; i < meshRaw.Count; i++)
            {
                MESH mesh = new MESH();

                mesh.unkShort0 = (short)((int)meshRaw[i][0xB0] % 0x10000);
                mesh.unkByte0 = (byte)((int)meshRaw[i][0xC7] % 0x100);
                mesh.unkByte1 = (byte)(((int)meshRaw[i][0xC7] / 100) % 0x100);
                mesh.unkShort1 = (short)((int)meshRaw[i][0xB0] / 0x10000);
                mesh.mateIndex = (int)meshRaw[i][0xB1];
                mesh.rendIndex = (int)meshRaw[i][0xB2];
                mesh.shadIndex = (int)meshRaw[i][0xB3];
                mesh.tsetIndex = (int)meshRaw[i][0xB4];
                mesh.baseMeshNodeId = (int)meshRaw[i][0xB5];
                mesh.vsetIndex = (int)meshRaw[i][0xC0];
                mesh.psetIndex = (int)meshRaw[i][0xC1];
                mesh.baseMeshSequenceId = (int)meshRaw[i][0xC2];

                meshList.Add(mesh);
            }

            return meshList;
        }

        public byte[] toMESH(List<MESH> meshList)
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
                int shorts = meshList[i].unkShort0 + (meshList[i].unkShort1 << 4);
                int bytes = meshList[i].unkByte0 + (meshList[i].unkByte1 << 2);
                addBytes(outBytes, 0xB0, 0x9, BitConverter.GetBytes(shorts));
                addBytes(outBytes, 0xC7, 0x9, BitConverter.GetBytes(bytes));
                addBytes(outBytes, 0xB1, 0x8, BitConverter.GetBytes(meshList[i].mateIndex));
                addBytes(outBytes, 0xB2, 0x8, BitConverter.GetBytes(meshList[i].rendIndex));
                addBytes(outBytes, 0xB3, 0x8, BitConverter.GetBytes(meshList[i].shadIndex));
                addBytes(outBytes, 0xB4, 0x8, BitConverter.GetBytes(meshList[i].tsetIndex));
                addBytes(outBytes, 0xB5, 0x8, BitConverter.GetBytes(meshList[i].baseMeshNodeId));
                addBytes(outBytes, 0xC0, 0x8, BitConverter.GetBytes(meshList[i].vsetIndex));
                addBytes(outBytes, 0xC1, 0x8, BitConverter.GetBytes(meshList[i].psetIndex));
                addBytes(outBytes, 0xC2, 0x9, BitConverter.GetBytes(meshList[i].baseMeshSequenceId));
            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            outBytes.InsertRange(0, Encoding.UTF8.GetBytes("MESH"));
            outBytes.InsertRange(0x4, BitConverter.GetBytes((short)(0)));  //Pointer count. Always 0 on MESH
            outBytes.InsertRange(0x8, BitConverter.GetBytes((short)meshList.Count * 0xB + 0x1)); //Subtag count. 11 for each MESH + 1 for the end tag, always.

            outBytes.InsertRange(0, BitConverter.GetBytes(outBytes.Count));          //Data body size
            outBytes.InsertRange(0, Encoding.UTF8.GetBytes("vtc0"));

            return outBytes.ToArray();
        }

        public unsafe List<MATE> parseMATE(List<Dictionary<int, object>> mateRaw)
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

                byte[] alphaArr = (byte[])mateRaw[i][0x3A];
                int alphaCount = alphaArr.Count() < 0x20 ? alphaArr.Count() : 0x20;
                for (int j = 0; j < alphaCount; j++)
                {
                    mate.alphaType[j] = alphaArr[j];
                }
                byte[] matNameArr = (byte[])mateRaw[i][0x39];
                int matCount = matNameArr.Count() < 0x20 ? matNameArr.Count() : 0x20;
                for (int j = 0; j < matCount; j++)
                {
                    mate.matName[j] = matNameArr[j];
                }

                mateList.Add(mate);
            }

            return mateList;
        }

        public unsafe byte[] toMATE(List<MATE> mateList)
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
                string alphaStr = GetPSO2String(mate.alphaType);
                addBytes(outBytes, 0x3A, 0x02, (byte)alphaStr.Length, Encoding.UTF8.GetBytes(alphaStr));

                //Mat Name String
                string matName = GetPSO2String(mate.matName);
                addBytes(outBytes, 0x39, 0x02, (byte)matName.Length, Encoding.UTF8.GetBytes(matName));
            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            outBytes.InsertRange(0, Encoding.UTF8.GetBytes("MATE"));
            outBytes.InsertRange(0x4, BitConverter.GetBytes((short)(0)));  //Pointer count. Always 0 on MATE
            outBytes.InsertRange(0x8, BitConverter.GetBytes((short)mateList.Count * 0xB + 0x1)); //Subtag count. 11 for each MATE + 1 for the end tag, always.

            outBytes.InsertRange(0, BitConverter.GetBytes(outBytes.Count));          //Data body size
            outBytes.InsertRange(0, Encoding.UTF8.GetBytes("vtc0"));

            return outBytes.ToArray();
        }

        public unsafe List<REND> parseREND(List<Dictionary<int, object>> rendRaw)
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

        public unsafe byte[] toREND(List<REND> rendList)
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

            outBytes.InsertRange(0, Encoding.UTF8.GetBytes("REND"));
            outBytes.InsertRange(0x4, BitConverter.GetBytes((short)(0)));  //Pointer count. Always 0 on REND
            outBytes.InsertRange(0x8, BitConverter.GetBytes((short)rendList.Count * 0x12 + 0x1)); //Subtag count. 18 for each REND + 1 for the end tag, always.

            outBytes.InsertRange(0, BitConverter.GetBytes(outBytes.Count));          //Data body size
            outBytes.InsertRange(0, Encoding.UTF8.GetBytes("vtc0"));

            return outBytes.ToArray();
        }

        public unsafe List<SHAD> parseSHAD(List<Dictionary<int, object>> shadRaw)
        {
            List<SHAD> shadList = new List<SHAD>();

            for (int i = 0; i < shadRaw.Count; i++)
            {
                SHAD shad = new SHAD();

                shad.unk0 = (int)shadRaw[i][0x90];

                //Pixel Shader String
                byte[] pixelArr = (byte[])shadRaw[i][0x91];
                int pixCount = pixelArr.Count() < 0x20 ? pixelArr.Count() : 0x20;
                for (int j = 0; j < pixCount; j++)
                {
                    shad.pixelShader[j] = pixelArr[j];
                }

                //Vertex Shader String
                byte[] vertArr = (byte[])shadRaw[i][0x92];
                int vertCount = vertArr.Count() < 0x20 ? vertArr.Count() : 0x20;
                for (int j = 0; j < vertCount; j++)
                {
                    shad.vertexShader[j] = vertArr[j];
                }

                shad.unk1 = (int)shadRaw[i][0x93];

                shadList.Add(shad);
            }

            return shadList;
        }

        public unsafe byte[] toSHAD(List<SHAD> shadList)
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
                string pixelStr = GetPSO2String(shad.pixelShader);
                addBytes(outBytes, 0x91, 0x02, (byte)pixelStr.Length, Encoding.UTF8.GetBytes(pixelStr));

                //Vertex Shader String
                string vertStr = GetPSO2String(shad.vertexShader);
                addBytes(outBytes, 0x92, 0x02, (byte)vertStr.Length, Encoding.UTF8.GetBytes(vertStr));

                addBytes(outBytes, 0x93, 0x9, BitConverter.GetBytes(shad.unk1));

            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            outBytes.InsertRange(0, Encoding.UTF8.GetBytes("SHAD"));
            outBytes.InsertRange(0x4, BitConverter.GetBytes((short)(shadList.Count)));  //Pointer count. SHAD struct count on SHAD.
            outBytes.InsertRange(0x8, BitConverter.GetBytes((short)shadList.Count * 0x5 + 0x1)); //Subtag count. 18 for each SHAD + 1 for the end tag, always.

            outBytes.InsertRange(0, BitConverter.GetBytes(outBytes.Count));          //Data body size
            outBytes.InsertRange(0, Encoding.UTF8.GetBytes("vtc0"));

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

        public unsafe List<TSTA> parseTSTA(List<Dictionary<int, object>> tstaRaw)
        {
            List<TSTA> tstaList = new List<TSTA>();

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

                //TexName String
                byte[] textNameArr = (byte[])tstaRaw[i][0x6C];
                int nameCount = textNameArr.Count() < 0x20 ? textNameArr.Count() : 0x20;
                for (int j = 0; j < nameCount; j++)
                {
                    tsta.texName[j] = textNameArr[j];
                }

                tstaList.Add(tsta);
            }

            return tstaList;
        }

        public unsafe byte[] toTSTA(List<TSTA> tstaList)
        {
            List<byte> outBytes = new List<byte>();

            for (int i = 0; i < tstaList.Count; i++)
            {
                TSTA tsta = tstaList[i];
                if (i == 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                }
                else
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
                addBytes(outBytes, 0x6A, 0x9, BitConverter.GetBytes(tstaList[i].unkFloat0));
                addBytes(outBytes, 0x6B, 0x9, BitConverter.GetBytes(tstaList[i].unkFloat1));

                //TexName String
                string texNameStr = GetPSO2String(tsta.texName);
                addBytes(outBytes, 0x6C, 0x02, (byte)texNameStr.Length, Encoding.UTF8.GetBytes(texNameStr));
            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            outBytes.InsertRange(0, Encoding.UTF8.GetBytes("TSTA"));
            outBytes.InsertRange(0x4, BitConverter.GetBytes((short)0));  //Pointer count. Always 0 on TSTA
            outBytes.InsertRange(0x8, BitConverter.GetBytes((short)tstaList.Count * 0xE + 0x1)); //Subtag count. 14 for each TSTA + 1 for the end tag, always.

            outBytes.InsertRange(0, BitConverter.GetBytes(outBytes.Count));          //Data body size
            outBytes.InsertRange(0, Encoding.UTF8.GetBytes("vtc0"));
            return outBytes.ToArray();
        }

        public unsafe List<TSET> parseTSET(List<Dictionary<int, object>> tsetRaw)
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

        public unsafe byte[] toTSET(List<TSET> tsetList)
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

            outBytes.InsertRange(0, Encoding.UTF8.GetBytes("TSET"));
            outBytes.InsertRange(0x4, BitConverter.GetBytes((short)0));  //Pointer count. Always 0 on TSET
            outBytes.InsertRange(0x8, BitConverter.GetBytes((short)tsetList.Count * 0x7 + 0x1)); //Subtag count. 7 for each TSET + 1 for the end tag, always.

            outBytes.InsertRange(0, BitConverter.GetBytes(outBytes.Count));          //Data body size
            outBytes.InsertRange(0, Encoding.UTF8.GetBytes("vtc0"));

            return outBytes.ToArray();
        }

        public unsafe List<TEXF> parseTEXF(List<Dictionary<int, object>> texfRaw)
        {
            List<TEXF> texfList = new List<TEXF>();

            for (int i = 0; i < texfRaw.Count; i++)
            {
                TEXF texf = new TEXF();

                byte[] textNameArr = (byte[])texfRaw[i][0x80];
                int nameCount = textNameArr.Count() < 0x20 ? textNameArr.Count() : 0x20;
                for (int j = 0; j < nameCount; j++)
                {
                    texf.texName[j] = textNameArr[j];
                }

                texfList.Add(texf);
            }

            return texfList;
        }

        public unsafe byte[] toTEXF(List<TEXF> texfList)
        {
            List<byte> outBytes = new List<byte>();

            for (int i = 0; i < texfList.Count; i++)
            {
                TEXF texf = texfList[i];
                if (i == 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                }
                else
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }

                //TexName String
                string texNameStr = GetPSO2String(texf.texName);
                addBytes(outBytes, 0x80, 0x02, (byte)texNameStr.Length, Encoding.UTF8.GetBytes(texNameStr));
            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            outBytes.InsertRange(0, Encoding.UTF8.GetBytes("TEXF"));
            outBytes.InsertRange(0x4, BitConverter.GetBytes((short)0));  //Pointer count. Always 0 on TEXF
            outBytes.InsertRange(0x8, BitConverter.GetBytes((short)texfList.Count * 0x2 + 0x1)); //Subtag count. 2 for each TEXF + 1 for the end tag, always.

            outBytes.InsertRange(0, BitConverter.GetBytes(outBytes.Count));          //Data body size
            outBytes.InsertRange(0, Encoding.UTF8.GetBytes("vtc0"));

            return outBytes.ToArray();
        }

        //Technically, this data is written out as a list, but should only ever have one entry.
        public unsafe UNRM parseUNRM(List<Dictionary<int, object>> unrmRaw)
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
                        unrm.unrmMeshIds.Add(vertIds);
                    }
                }
            }

            return unrm;
        }

        public unsafe byte[] toUNRM(UNRM unrm)
        {
            List<byte> outBytes = new List<byte>();

            //Technically an array so we put the array start tag
            outBytes.AddRange(BitConverter.GetBytes((short)0xFC));

            addBytes(outBytes, 0xDA, 0x9, BitConverter.GetBytes(unrm.vertGroupCountCount));

            //unrm vert groups
            outBytes.Add(0xDB); outBytes.Add(0x89);
            if(unrm.vertGroupCountCount - 1 > byte.MaxValue)
            {
                outBytes.Add(0x10);
                outBytes.AddRange(BitConverter.GetBytes((ushort)unrm.vertGroupCountCount - 1));
            } else
            {
                outBytes.Add(0x08);
                outBytes.Add((byte)(unrm.vertGroupCountCount - 1));

            }
            for(int i = 0; i < unrm.vertGroupCountCount; i++)
            {
                outBytes.AddRange(BitConverter.GetBytes(unrm.unrmVertGroups[i]));
            }

            addBytes(outBytes, 0xDC, 0x9, BitConverter.GetBytes(unrm.vertCount));

            //unrm mesh ids
            outBytes.Add(0xDD); outBytes.Add(0x89);
            int meshIDCount = getListOfListOfIntsIntCount(unrm.unrmMeshIds);
            if (meshIDCount - 1 > byte.MaxValue)
            {
                outBytes.Add(0x10);
                outBytes.AddRange(BitConverter.GetBytes((ushort)meshIDCount - 1));
            }
            else
            {
                outBytes.Add(0x08);
                outBytes.Add((byte)(meshIDCount - 1));
            }
            for(int i = 0; i < unrm.unrmMeshIds.Count; i++)
            {
                for(int j = 0; j < unrm.unrmMeshIds[i].Count; i++)
                {
                    outBytes.AddRange(BitConverter.GetBytes(unrm.unrmMeshIds[i][j]));
                }
            }

            //unrm vert ids
            outBytes.Add(0xDE); outBytes.Add(0x89);
            int vertIdCount = getListOfListOfIntsIntCount(unrm.unrmVertIds);
            if (vertIdCount - 1 > byte.MaxValue)
            {
                outBytes.Add(0x10);
                outBytes.AddRange(BitConverter.GetBytes((ushort)vertIdCount - 1));
            }
            else
            {
                outBytes.Add(0x08);
                outBytes.Add((byte)(vertIdCount - 1));
            }
            for (int i = 0; i < unrm.unrmVertIds.Count; i++)
            {
                for (int j = 0; j < unrm.unrmVertIds[i].Count; i++)
                {
                    outBytes.AddRange(BitConverter.GetBytes(unrm.unrmVertIds[i][j]));
                }
            }

            //Technically an array so we put the array end tag
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            outBytes.InsertRange(0, Encoding.UTF8.GetBytes("UNRM"));
            outBytes.InsertRange(0x4, BitConverter.GetBytes((short)0));  //Pointer count. Always 0 on UNRM
            outBytes.InsertRange(0x8, BitConverter.GetBytes((short)0x7)); //Subtag count. In theory, always 7.

            outBytes.InsertRange(0, BitConverter.GetBytes(outBytes.Count));          //Data body size
            outBytes.InsertRange(0, Encoding.UTF8.GetBytes("vtc0"));

            return outBytes.ToArray();
        }

        public int getListOfListOfIntsIntCount(List<List<int>> intListlist)
        {
            int count = 0;
            for(int i = 0; i < intListlist.Count; i++)
            {
                count += intListlist[i].Count;
            }

            return count;
        }

        public void addBytes(List<byte> outBytes, byte id, byte dataType, byte[] data)
        {
            outBytes.Add(id); 
            outBytes.Add(dataType);
            outBytes.AddRange(data);
        }

        public void addBytes(List<byte> outBytes, byte id, byte dataType, byte vecAmt, byte[] data)
        {
            outBytes.Add(id);
            outBytes.Add(dataType);
            outBytes.Add(vecAmt);
            outBytes.AddRange(data);
        }

        public void addBytes(List<byte> outBytes, byte id, byte dataType, byte subDataType, byte subDataAdditions, byte[] data)
        {
            outBytes.Add(id);
            outBytes.Add(dataType);
            outBytes.Add(subDataType);
            outBytes.Add(subDataAdditions);
            outBytes.AddRange(data);
        }

        public ushort flagCheck(int check)
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
