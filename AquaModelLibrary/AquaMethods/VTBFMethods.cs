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
                byte subDataAdditions;

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
                
                switch(dataType)
                {
                    case 0x0: //Special Flag. Probably more for parsing purposes than data. 0xFC ID before this means first struct, 0xFE means a later struct
                        data = dataType;
                        break;
                    case 0x1: //Boolean? Single byte
                        data = streamReader.Read<byte>();
                        break;
                    case 0x2: //String
                        byte strLen = streamReader.Read<byte>();
                        if(strLen > 0)
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
                        for(int j = 0; j < 4; j++)
                        {
                            ((byte[])data)[j] = streamReader.Read<byte>();
                        }
                        break;
                    case 0x48: //Vector3 of ints
                        subDataAdditions = streamReader.Read<byte>(); //Presumably the number of these consecutively
                        data = new int[subDataAdditions][];
                        for(int j = 0; j < subDataAdditions; j++)
                        {
                            int[] dataArr = new int[3];
                            dataArr[0] = streamReader.Read<int>();
                            dataArr[1] = streamReader.Read<int>();
                            dataArr[2] = streamReader.Read<int>();

                            ((int[][])data)[j] = dataArr;
                        }
                        break;
                    case 0x86: //Array of shorts
                        subDataType = streamReader.Read<byte>();      //Somewhat unsure. Observed as 0x8 or 0x6
                        subDataAdditions = streamReader.Read<byte>(); //Presumably the number of these consecutively

                        data = new short[subDataAdditions + 1];
                        for(int j = 0; j < subDataAdditions; j++)
                        {
                            ((short[])data)[j] = streamReader.Read<short>();
                        }
                        break;
                    case 0x89: //Vertices
                        streamReader.Read<byte>(); // 0x10 probably
                        data = streamReader.ReadBytes(streamReader.Position(), (int)(bodyLength) - 0xB); //Read the whole vert buffer at once. We'll handle it later.
                        streamReader.Seek((int)(bodyLength) - 0xB, SeekOrigin.Current);
                        break;
                    case 0x4A: //Vector3 of floats
                        subDataAdditions = streamReader.Read<byte>(); //Presumably the number of these consecutively
                        data = new Vector3[subDataAdditions];

                        for (int j = 0; j < subDataAdditions; j++)
                        {
                            Vector3 vec3 = streamReader.Read<Vector3>();

                            ((Vector3[])data)[j] = vec3;
                        }
                        break;
                    case 0xCA: //Float Matrix, observed only as 4x4
                        subDataType = streamReader.Read<byte>(); //Expected to always be 0xA for float
                        subDataAdditions = streamReader.Read<byte>(); //Expected to always be 0x3, maybe for last array entry id
                        data = new Vector4[subDataAdditions + 1];

                        for(int j = 0; j < subDataAdditions; j++)
                        {
                            switch(subDataType)
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
            if(useUNRMs)
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
                if(i == 0)
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

                if(vtxlList[i].bonePalette != null)
                {
                    addBytes(outBytes, 0xBD, 0x8, BitConverter.GetBytes(vtxlList[i].bonePalette.Count));
                    if(vtxlList[i].bonePalette.Count > 0)
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
            for(int i = 0; i < vtxe.vertDataTypes.Count; i++)
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
            outBytes.Add(0x10);
            for(int i = 0; i < vtxl.vertPositions.Count; i++)
            {
                for(int j = 0; j < vtxe.vertDataTypes.Count; j++)
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
                            for(int color = 0; color < 4; color++)
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
                            break;
                    }

                    
                }
            }

            outBytes.InsertRange(vtxeEnd, Encoding.UTF8.GetBytes("VTXL"));
            outBytes.InsertRange(vtxeEnd + 0x4, BitConverter.GetBytes((short)(0)));  //Pointer count. Always 0 on VTXL
            outBytes.InsertRange(vtxeEnd + 0x8, BitConverter.GetBytes((short)1)); //Subtag count

            outBytes.InsertRange(vtxeEnd, BitConverter.GetBytes(outBytes.Count - vtxeEnd));          //Data body size
            outBytes.InsertRange(vtxeEnd, Encoding.UTF8.GetBytes("vtc0"));

            return outBytes.ToArray();
        }

        public List<PSET> ParsePSET(List<Dictionary<int, object>> psetRaw)
        {
            List<PSET> psets = new List<PSET>();

            return psets;
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
