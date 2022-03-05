﻿using AquaModelLibrary.Nova.Structures;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static AquaModelLibrary.Nova.AXSConstants;

namespace AquaModelLibrary.Nova
{
    public static class AXSMethods
    {
        //We're not realistically going to fully convert everything, but we can get vertex data and bones if nothing else
        //Returns an aqp ready for the ConvertToNGSPSO2Mesh method
        public static AquaObject ReadAXS(string filePath, out AquaNode aqn)
        {
            AquaObject aqp = new NGSAquaObject();
            aqn = new AquaNode();

            using (Stream stream = (Stream)new FileStream(filePath, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                Debug.WriteLine(Path.GetFileName(filePath));
                List<ffubStruct> ffubList = new List<ffubStruct>();
                List<XgmiStruct> xgmiList = new List<XgmiStruct>();
                List<MeshDefinitions> meshDefList = new List<MeshDefinitions>();
                Dictionary<string, rddaStruct> rddaList = new Dictionary<string, rddaStruct>();
                Dictionary<string, rddaStruct> imgRddaList = new Dictionary<string, rddaStruct>();
                Dictionary<string, rddaStruct> vertRddaList = new Dictionary<string, rddaStruct>();
                Dictionary<string, rddaStruct> faceRddaList = new Dictionary<string, rddaStruct>();
                ffubStruct imgFfub = new ffubStruct();
                ffubStruct vertFfub = new ffubStruct();
                ffubStruct faceFfub = new ffubStruct(); 

                var fType = streamReader.Read<int>();
                if (fType != FSA)
                {
                    return null;
                }

                var fsaLen = streamReader.Read<int>();
                streamReader.Seek(0x8, SeekOrigin.Current);
                //Go to Vert definition, node, material, and misc data
                while (streamReader.Position() < fsaLen)
                {
                    var tag = streamReader.Peek<int>();
                    switch (tag)
                    {
                        case __oa:
                            streamReader.Seek(0xD0, SeekOrigin.Current);
                            break;
                        case FIA:
                            streamReader.Seek(0x10, SeekOrigin.Current);
                            break;
                        case __bm:
                            streamReader.ReadBM(meshDefList);
                            break;
                        case eert:
                            streamReader.SkipBasicAXSStruct(); //Replace when understood
                            break;
                        case ssem:
                            streamReader.SkipBasicAXSStruct(); //Maybe use for material data later. Remember to store ordered id for _bm mesh entries for this
                            break;
                        case Xgmi:
                            xgmiList.Add(streamReader.ReadXgmi());
                            break;
                        default:
                            streamReader.SkipBasicAXSStruct();
                            break;
                    }

                }

                //Go to mesh buffers
                streamReader.Seek(fsaLen, SeekOrigin.Begin);
                if(streamReader.Position() >= stream.Length)
                {
                    return null;
                }

                var fType2 = streamReader.Read<int>();
                //Read mesh data
                if (fType2 != FMA)
                {
                    MessageBox.Show("Warning, this is NOT an FMA struct!");
                    return null;
                }
                streamReader.Seek(0xC, SeekOrigin.Current);

                //Skip daeh
                int meshSettingLen = streamReader.ReadDAEH();

                //Read ffub and rdda
                //Count mesh count here for now and store starts and ends of data
                long meshSettingStart = streamReader.Position();
                while (streamReader.Position() < meshSettingStart + meshSettingLen)
                {
                    streamReader.ReadFFUBorRDDA(ffubList, rddaList, imgRddaList, vertRddaList, faceRddaList, ref imgFfub, ref vertFfub, ref faceFfub);
                }

                int meshCount = meshDefList.Count;

                //Read image data
                /*for (int i = 0; i < imgRddaList.Count; i++)
                {
                    Debug.WriteLine($"Image set {i}: " + vertRddaList[i].dataStartOffset.ToString("X") + " " + vertRddaList[i].toTagStruct.ToString("X") + " " + (meshSettingStart + vertFfub.dataStartOffset + vertRddaList[i].dataStartOffset).ToString("X"));
                }*/

                //Read model data - Since ffubs are initialized, they default to 0. 
                int vertFfubPadding = imgFfub.structSize;
                int faceFfubPadding = imgFfub.structSize + vertFfub.structSize;

                for(int i = 0; i < meshCount; i++)
                {
                    var mesh = meshDefList[i];
                    var vertBufferInfo = vertRddaList[$"{mesh.salvStr.md5_1.ToString("X")}{mesh.salvStr.md5_2.ToString("X")}"];
                    var faceBufferInfo = faceRddaList[$"{mesh.lxdiStr.md5_1.ToString("X")}{mesh.lxdiStr.md5_2.ToString("X")}"];
                    Debug.WriteLine($"Vert set {i}: " + vertBufferInfo.md5_1.ToString("X") + " " + vertBufferInfo.dataStartOffset.ToString("X") + " " + vertBufferInfo.toTagStruct.ToString("X") + " " + (meshSettingStart + vertFfubPadding  + vertFfub.dataStartOffset + vertBufferInfo.dataStartOffset).ToString("X"));
                    Debug.WriteLine($"Face set {i}: " + faceBufferInfo.md5_1.ToString("X") + " " + faceBufferInfo.dataStartOffset.ToString("X") + " " + faceBufferInfo.toTagStruct.ToString("X") + " " + (meshSettingStart + faceFfubPadding + faceFfub.dataStartOffset + faceBufferInfo.dataStartOffset).ToString("X"));

                    //Vert data
                    var vertCount = vertBufferInfo.dataSize / mesh.salvStr.vertLen;
                    AquaObject.VTXL vtxl = new AquaObject.VTXL();

                    streamReader.Seek((meshSettingStart + vertFfubPadding + vertFfub.dataStartOffset + vertBufferInfo.dataStartOffset), SeekOrigin.Begin);
                    AquaObjectMethods.ReadVTXL(streamReader, mesh.vtxe, vtxl, vertCount, mesh.vtxe.vertDataTypes.Count);
                    vtxl.convertToLegacyTypes();

                    aqp.vtxlList.Add(vtxl);

                    //Face data
                    AquaObject.GenericTriangles genMesh = new AquaObject.GenericTriangles();

                    int faceIndexCount = faceBufferInfo.dataSize / 2;
                    List<int> faceList = new List<int>();

                    streamReader.Seek((meshSettingStart + faceFfubPadding + faceFfub.dataStartOffset + faceBufferInfo.dataStartOffset), SeekOrigin.Begin);
                    int maxStep = streamReader.Read<ushort>();
                    for (int fId = 0; fId < faceIndexCount - 1; fId++)
                    {
                        faceList.Add(streamReader.Read<ushort>());
                    }

                    //Convert the data to something usable with this algorithm and then destripify it.
                    List<ushort> triList = unpackInds(inverseWatermarkTransform(faceList, maxStep)).ConvertAll(delegate (int num) {
                        return (ushort)num;
                    });
                    var tempFaceData = new AquaObject.stripData() { triStrips = triList, format0xC33 = true, triIdCount = triList.Count };
                    genMesh.triList = tempFaceData.GetTriangles();

                    //Extra
                    genMesh.vertCount = vertCount;
                    if (mesh.ipnbStr != null && mesh.ipnbStr.shortList.Count > 0)
                    {
                        genMesh.bonePalette = (mesh.ipnbStr.shortList.ConvertAll(delegate (short num) {
                            return (uint)num;
                        }));
                    }
                    genMesh.matIdList = new List<int>(new int[genMesh.triList.Count]);

                    aqp.tempTris.Add(genMesh);
                }

                aqp.tempMats.Add(new AquaObject.GenericMaterial() { texNames = new List<string>() { "noTexture.dds" } });

                return aqp;
            }
        }

        public static AquaObject.VTXE GenerateGenericPSO2VTXE(byte vertData0, byte vertData1, byte vertData2, byte vertData3, int trueLength)
        {
            AquaObject.VTXE vtxe = new AquaObject.VTXE();
            int curLength = 0;

            //Vert positions
            if ((vertData0 & 0x1) > 0)
            {
                vtxe.vertDataTypes.Add(AquaObjectMethods.vtxeElementGenerator(0x0, 0x3, curLength));
                curLength += 0xC;
            } else if ((vertData0 & 0x8) > 0)
            {
                vtxe.vertDataTypes.Add(AquaObjectMethods.vtxeElementGenerator(0x0, 0x99, curLength));
                curLength += 0x8;
            }

            //Vert normals
            if((vertData0 & 0x10) > 0)
            {
                if((vertData0 & 0x80) > 0)
                {
                    vtxe.vertDataTypes.Add(AquaObjectMethods.vtxeElementGenerator(0x2, 0xF, curLength));
                    curLength += 0x8;
                } else
                {
                    vtxe.vertDataTypes.Add(AquaObjectMethods.vtxeElementGenerator(0x2, 0x3, curLength));
                    curLength += 0xC;
                }
            }

            //Vert colors
            if ((vertData3 & 0x10) > 0)
            {
                vtxe.vertDataTypes.Add(AquaObjectMethods.vtxeElementGenerator(0x3, 0x5, curLength));
                curLength += 0x4;
            }

            //Weights and Weight Indices
            if((vertData3 & 0x40) > 0)
            {
                vtxe.vertDataTypes.Add(AquaObjectMethods.vtxeElementGenerator(0x1, 0x11, curLength));
                curLength += 0x8;
                vtxe.vertDataTypes.Add(AquaObjectMethods.vtxeElementGenerator(0xb, 0x7, curLength));
                curLength += 0x4;
            }

            //Vert UV1
            int addition = 0;
            if ((vertData1 & 0x1) > 0)
            {
                if((vertData1 & 0x8) > 0)
                {
                    vtxe.vertDataTypes.Add(AquaObjectMethods.vtxeElementGenerator(0x10, 0xE, curLength));
                    curLength += 0x4;
                } else
                {
                    vtxe.vertDataTypes.Add(AquaObjectMethods.vtxeElementGenerator(0x10, 0x2, curLength));
                    curLength += 0x8;
                }
                addition++;
            }

            //Vert UV1 and 2?
            if ((vertData1 & 0x2) > 0) 
            {
                if ((vertData1 & 0x1) > 0)
                {
                    Debug.WriteLine("Warning, vertData1 has 0x1 AND 0x2 defined!");
                }
                if((vertData1 & 0x8) > 0)
                {
                    vtxe.vertDataTypes.Add(AquaObjectMethods.vtxeElementGenerator(0x10 + addition, 0xE, curLength));
                    curLength += 0x4;
                    vtxe.vertDataTypes.Add(AquaObjectMethods.vtxeElementGenerator(0x11 + addition, 0xE, curLength));
                    curLength += 0x4;
                } else
                {
                    vtxe.vertDataTypes.Add(AquaObjectMethods.vtxeElementGenerator(0x10 + addition, 0x2, curLength));
                    curLength += 0x8;
                    vtxe.vertDataTypes.Add(AquaObjectMethods.vtxeElementGenerator(0x11 + addition, 0x2, curLength));
                    curLength += 0x8;
                }

                addition += 2;
            }

            //MOAR UV
            if ((vertData1 & 0x10) > 0)
            {
                vtxe.vertDataTypes.Add(AquaObjectMethods.vtxeElementGenerator(0x10 + addition, 0x2, curLength));
                curLength += 0x8;
                vtxe.vertDataTypes.Add(AquaObjectMethods.vtxeElementGenerator(0x11 + addition, 0x2, curLength));
                curLength += 0x8;

                addition += 2;
            }

            //Some kind of uv info? Idefk
            if ((vertData1 & 0x20) > 0)
            {
                vtxe.vertDataTypes.Add(AquaObjectMethods.vtxeElementGenerator(0x10 + addition, 0x2, curLength));
                curLength += 0x8;

                addition += 1;
            }

            //Some other kind of uv info? Idefk
            if ((vertData1 & 0x40) > 0) 
            {
                vtxe.vertDataTypes.Add(AquaObjectMethods.vtxeElementGenerator(0x10 + addition, 0xE, curLength));
                curLength += 0x4;
                vtxe.vertDataTypes.Add(AquaObjectMethods.vtxeElementGenerator(0x11 + addition, 0xE, curLength));
                curLength += 0x4;

                addition += 2;
            }

            //More uv stuff??
            if((vertData2 & 0x40) > 0)
            {
                vtxe.vertDataTypes.Add(AquaObjectMethods.vtxeElementGenerator(0x22, 0xC, curLength));
                curLength += 0x4;
                vtxe.vertDataTypes.Add(AquaObjectMethods.vtxeElementGenerator(0x23, 0xC, curLength));
                curLength += 0x4;
            }

            if(curLength != trueLength)
            {
                Debug.WriteLine(curLength + " != " + trueLength + " " + vertData0.ToString("X") + " " + vertData1.ToString("X") + " " + vertData2.ToString("X") + " " + vertData3.ToString("X"));
            }

            return vtxe;
        }

        //Returns DAEH's length for the section - its own length
        public static int ReadDAEH(this BufferedStreamReader streamReader)
        {
            streamReader.Seek(0x4, SeekOrigin.Current);
            var len = streamReader.Read<int>();
            streamReader.Seek(len - 0x10, SeekOrigin.Current);
            var meshSettingLen = streamReader.Read<int>();
            streamReader.Seek(0x4, SeekOrigin.Current);

            return meshSettingLen - len;
        }

        public static void SkipBasicAXSStruct(this BufferedStreamReader streamReader)
        {
            long bookmark = streamReader.Position();

            streamReader.Read<int>();
            var trueLen = streamReader.Read<int>(); //Doesn't include padding so shouldn't be used
            streamReader.Read<int>();
            var len = streamReader.Read<int>();
            streamReader.Seek(bookmark, SeekOrigin.Begin);

            if(len == 0)
            {
                len = trueLen;
            }
            streamReader.Seek(len, SeekOrigin.Current);
        }

        public static void ReadBM(this BufferedStreamReader streamReader, List<MeshDefinitions> defs)
        {
            MeshDefinitions mesh = null;
            var bmStart = streamReader.Position();
            streamReader.Read<int>();
            var bmEnd = streamReader.Read<int>() + bmStart;

            streamReader.Seek(0x8, SeekOrigin.Current);
            while (streamReader.Position() < bmEnd)
            {
                var tag = streamReader.Peek<int>();
                switch (tag)
                {
                    case ydbm:
                        if(mesh != null)
                        {
                            Debug.WriteLine(defs.Count);
                            defs.Add(mesh);
                        }
                        mesh = new MeshDefinitions();
                        mesh.ydbmStr = streamReader.ReadYdbm();
                        break;
                    case lxdi:
                        mesh.lxdiStr = streamReader.ReadLxdi();
                        break;
                    case salv:
                        mesh.salvStr = streamReader.ReadSalv();
                        mesh.vtxe = GenerateGenericPSO2VTXE(mesh.salvStr.vertDef0, mesh.salvStr.vertDef1, mesh.salvStr.vertDef2, mesh.salvStr.vertDef3, mesh.salvStr.vertLen);
                        break;
                    case ipnb:
                        mesh.ipnbStr = streamReader.ReadIpnb();
                        break;
                    default:
                        streamReader.SkipBasicAXSStruct();
                        break;
                }
            }
            if (mesh != null)
            {
                Debug.WriteLine(defs.Count);
                defs.Add(mesh);
            }
        }

        public static ydbmStruct ReadYdbm(this BufferedStreamReader streamReader)
        {
            ydbmStruct ydbmStr = new ydbmStruct();

            ydbmStr.magic = streamReader.Read<int>();
            ydbmStr.len = streamReader.Read<int>();
            ydbmStr.int_08 = streamReader.Read<int>();
            ydbmStr.paddedLen = streamReader.Read<int>();

            ydbmStr.int_10 = streamReader.Read<int>();
            ydbmStr.int_14 = streamReader.Read<int>();
            ydbmStr.int_18 = streamReader.Read<int>();
            ydbmStr.int_1C = streamReader.Read<int>();

            return ydbmStr;
        }

        public static lxdiStruct ReadLxdi(this BufferedStreamReader streamReader)
        {
            lxdiStruct lxdiStr = new lxdiStruct();

            lxdiStr.magic = streamReader.Read<int>();
            lxdiStr.len = streamReader.Read<int>();
            lxdiStr.int_08 = streamReader.Read<int>();
            lxdiStr.paddedLen = streamReader.Read<int>();

            lxdiStr.int_10 = streamReader.Read<int>();
            lxdiStr.int_14 = streamReader.Read<int>();
            lxdiStr.int_18 = streamReader.Read<int>();
            lxdiStr.int_1C = streamReader.Read<int>();

            lxdiStr.md5_1 = streamReader.Read<long>();
            lxdiStr.md5_2 = streamReader.Read<long>();

            lxdiStr.md5_2_1 = streamReader.Read<long>();
            lxdiStr.md5_2_2 = streamReader.Read<long>();

            return lxdiStr;
        }

        public static salvStruct ReadSalv(this BufferedStreamReader streamReader)
        {
            salvStruct salvStr = new salvStruct();

            salvStr.magic = streamReader.Read<int>();
            salvStr.len = streamReader.Read<int>();
            salvStr.int_08 = streamReader.Read<int>();
            salvStr.paddedLen = streamReader.Read<int>();

            salvStr.vertDef0 = streamReader.Read<byte>();
            salvStr.vertDef1 = streamReader.Read<byte>();
            salvStr.vertDef2 = streamReader.Read<byte>();
            salvStr.vertDef3 = streamReader.Read<byte>();
            salvStr.int_14 = streamReader.Read<int>();
            salvStr.vertLen = streamReader.Read<int>();
            salvStr.int_1C = streamReader.Read<int>();

            salvStr.md5_1 = streamReader.Read<long>();
            salvStr.md5_2 = streamReader.Read<long>();

            salvStr.int_30 = streamReader.Read<int>();
            salvStr.int_34 = streamReader.Read<int>();
            salvStr.int_38 = streamReader.Read<int>();
            salvStr.int_3C = streamReader.Read<int>();

            salvStr.md5_2_1 = streamReader.Read<long>();
            salvStr.md5_2_2 = streamReader.Read<long>();

#if DEBUG

            if((salvStr.vertDef0 & 0x2) > 0)
            {
                Debug.WriteLine("vertDef0 & 0x2 == true");
            }
            if ((salvStr.vertDef0 & 0x4) > 0)
            {
                Debug.WriteLine("vertDef0 & 0x4 == true");
            }
            if ((salvStr.vertDef0 & 0x20) > 0)
            {
                Debug.WriteLine("vertDef0 & 0x20 == true");
            }
            if ((salvStr.vertDef0 & 0x40) > 0)
            {
                Debug.WriteLine("vertDef0 & 0x40 == true");
            }


            if ((salvStr.vertDef1 & 0x4) > 0)
            {
                Debug.WriteLine("vertDef1 & 0x4 == true");
            }
            if ((salvStr.vertDef1 & 0x10) > 0)
            {
                Debug.WriteLine("vertDef1 & 0x10 == true");
            }
            if ((salvStr.vertDef1 & 0x80) > 0)
            {
                Debug.WriteLine("vertDef1 & 0x80 == true");
            }

            if ((salvStr.vertDef2 & 0x1) > 0)
            {
                Debug.WriteLine("vertDef2 & 0x1 == true");
            }
            if ((salvStr.vertDef2 & 0x2) > 0)
            {
                Debug.WriteLine("vertDef2 & 0x2 == true");
            }
            if ((salvStr.vertDef2 & 0x4) > 0)
            {
                Debug.WriteLine("vertDef2 & 0x4 == true");
            }
            if ((salvStr.vertDef2 & 0x8) > 0)
            {
                Debug.WriteLine("vertDef2 & 0x8 == true");
            }
            if ((salvStr.vertDef2 & 0x10) > 0)
            {
                Debug.WriteLine("vertDef2 & 0x10 == true");
            }
            if ((salvStr.vertDef2 & 0x20) > 0)
            {
                Debug.WriteLine("vertDef2 & 0x20 == true");
            }
            if ((salvStr.vertDef2 & 0x40) > 0)
            {
                Debug.WriteLine("vertDef2 & 0x40 == true");
            }
            if ((salvStr.vertDef2 & 0x80) > 0)
            {
                Debug.WriteLine("vertDef2 & 0x80 == true");
            }

            if ((salvStr.vertDef3 & 0x1) > 0)
            {
                Debug.WriteLine("vertDef3 & 0x1 == true");
            }
            if ((salvStr.vertDef3 & 0x2) > 0)
            {
                Debug.WriteLine("vertDef3 & 0x2 == true");
            }
            if ((salvStr.vertDef3 & 0x4) > 0)
            {
                Debug.WriteLine("vertDef3 & 0x4 == true");
            }
            if ((salvStr.vertDef3 & 0x8) > 0)
            {
                Debug.WriteLine("vertDef3 & 0x8 == true");
            }
            if ((salvStr.vertDef3 & 0x20) > 0)
            {
                Debug.WriteLine("vertDef3 & 0x20 == true");
            }
            if ((salvStr.vertDef3 & 0x80) > 0)
            {
                Debug.WriteLine("vertDef3 & 0x80 == true");
            }
#endif

            return salvStr;
        }

        public static ipnbStruct ReadIpnb(this BufferedStreamReader streamReader)
        {
            long bookmark = streamReader.Position();
            ipnbStruct ipbnStr = new ipnbStruct();
            
            ipbnStr.magic = streamReader.Read<int>();
            ipbnStr.len = streamReader.Read<int>();
            ipbnStr.int_08 = streamReader.Read<int>();
            ipbnStr.paddedLen = streamReader.Read<int>();

            int count = (ipbnStr.len - 0x10) / 2;
            for(int i = 0; i < count; i++)
            {
                ipbnStr.shortList.Add(streamReader.Read<short>());
            }

            streamReader.Seek(bookmark + ipbnStr.paddedLen, SeekOrigin.Begin);
            return ipbnStr;
        }

        public static void ReadFFUBorRDDA(this BufferedStreamReader streamReader, List<ffubStruct> ffubStructs, Dictionary<string, rddaStruct> rddaStructs,
            Dictionary<string, rddaStruct> imgRddaStructs, Dictionary<string, rddaStruct> vertRddaStructs, Dictionary<string, rddaStruct> faceRddaStructs, ref ffubStruct imgFfub, ref ffubStruct vertFfub, ref ffubStruct faceFfub)
        {
            long bookmark = streamReader.Position();

            int magic = streamReader.Read<int>();
            int len;
            switch (magic)
            {
                case ffub:
                    var ffubData = streamReader.ReadFFUB(magic, out len);
                    switch(ffubData.dataType)
                    {
                        case 0x2:
                            imgFfub = ffubData;
                            break;
                        case 0x3:
                            vertFfub = ffubData;
                            break;
                        case 0x9:
                            faceFfub = ffubData;
                            break;
                        default:
                            ffubStructs.Add(ffubData);
                            Debug.WriteLine($"Unknown ffub type: {ffubData.dataType.ToString("X")}");
                            break;
                    }
                    break;
                case rdda:
                    var rddaData = streamReader.ReadRDDA(magic, out len);
                    string md5 = $"{rddaData.md5_1.ToString("X")}{rddaData.md5_2.ToString("X")}";
                    switch (rddaData.dataType)
                    {
                        case 0x2:
                            imgRddaStructs.Add(md5, rddaData);
                            break;
                        case 0x3:
                            vertRddaStructs.Add(md5, rddaData);
                            break;
                        case 0x9:
                            faceRddaStructs.Add(md5, rddaData);
                            break;
                        default:
                            rddaStructs.Add(md5, rddaData);
                            Debug.WriteLine($"Unknown rdda type: {rddaData.dataType.ToString("X")}");
                            break;
                    }
                    break;
                default:
                    throw new Exception(magic.ToString("X"));
            }
            streamReader.Seek(bookmark, SeekOrigin.Begin);
            streamReader.Seek(len, SeekOrigin.Current);
        }

        public static ffubStruct ReadFFUB(this BufferedStreamReader streamReader, int magic, out int len)
        {
            ffubStruct ffubStr = new ffubStruct();
            ffubStr.magic = magic;
            ffubStr.structSize = streamReader.Read<int>();
            ffubStr.int_08 = streamReader.Read<int>();
            ffubStr.toTagStruct = streamReader.Read<int>();

            ffubStr.dataSize = streamReader.Read<int>();
            ffubStr.int_14 = streamReader.Read<int>();
            ffubStr.dataStartOffset = streamReader.Read<int>();
            ffubStr.int_1C = streamReader.Read<int>();
            
            ffubStr.int_20 = streamReader.Read<int>();
            ffubStr.int_24 = streamReader.Read<int>();
            ffubStr.int_28 = streamReader.Read<int>();
            ffubStr.dataType = streamReader.Read<int>();
            
            len = ffubStr.structSize;

            return ffubStr;
        }

        public static rddaStruct ReadRDDA(this BufferedStreamReader streamReader, int magic, out int len)
        {
            rddaStruct rddaStr = new rddaStruct();
            rddaStr.magic = magic;
            rddaStr.structSize = streamReader.Read<int>();
            rddaStr.int_08 = streamReader.Read<int>();
            rddaStr.toTagStruct = streamReader.Read<int>();

            rddaStr.md5_1 = streamReader.Read<long>();
            rddaStr.md5_2 = streamReader.Read<long>();

            rddaStr.dataSize = streamReader.Read<int>();
            rddaStr.int_24 = streamReader.Read<int>();
            rddaStr.dataStartOffset = streamReader.Read<int>();
            rddaStr.int_2C = streamReader.Read<int>();

            rddaStr.int_30 = streamReader.Read<int>();
            rddaStr.int_34 = streamReader.Read<int>();
            rddaStr.int_38 = streamReader.Read<int>();
            rddaStr.dataType = streamReader.Read<short>();
            rddaStr.short_3E = streamReader.Read<short>();

            len = rddaStr.structSize;

            return rddaStr;
        }

        //Face decoding functions from: https://forum.xentax.com/viewtopic.php?p=143356&sid=c54ef316ad86c051345fec2ef63a0bf7#p143356
        public static List<int> inverseWatermarkTransform(List<int> in_inds, int max_step)
        {
            List<int> out_inds = new List<int>();

            int hi = max_step - 1;
            foreach (int v in in_inds)
            {
                int decV = hi - v;
                out_inds.Add(decV);
                hi = Math.Max(hi, decV + max_step);
            }

            return out_inds;
        }
        public static List<int> unpackInds(List<int> inds, bool invertNormals = true)
        {
            List<int> out_inds = new List<int>();
            
            //Nova normals seem to require this. Maybe Star Ocean faces worked different.
            if(invertNormals)
            {
                for (int i = 0; i < inds.Count;)
                {
                    int a = inds[i++];
                    int b = inds[i++];
                    int c = inds[i++];
                    out_inds.Add(b); out_inds.Add(a); out_inds.Add(c);

                    if (a < b)
                    {
                        int d = inds[i++];
                        out_inds.Add(d); out_inds.Add(a); out_inds.Add(b);
                    }
                }
            }
            else
            {
                for (int i = 0; i < inds.Count;)
                {
                    int a = inds[i++];
                    int b = inds[i++];
                    int c = inds[i++];
                    out_inds.Add(a); out_inds.Add(b); out_inds.Add(c);

                    if (a < b)
                    {
                        int d = inds[i++];
                        out_inds.Add(a); out_inds.Add(d); out_inds.Add(b);
                    }
                }
            }


            return out_inds;
        }

        public static void boxTest()
        {
            List<System.Numerics.Vector3> verts = new List<System.Numerics.Vector3>();
            List<int> stripIndices = new List<int>();
            using (Stream stream = (Stream)new FileStream(@"C:\boxVertData", FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                for(int i = 0; i < 8; i++)
                {
                    var half0 = SystemHalf.HalfHelper.HalfToSingle(streamReader.Read<SystemHalf.Half>());
                    var half1 = SystemHalf.HalfHelper.HalfToSingle(streamReader.Read<SystemHalf.Half>());
                    var half2 = SystemHalf.HalfHelper.HalfToSingle(streamReader.Read<SystemHalf.Half>());
                    streamReader.Read<SystemHalf.Half>();

                    verts.Add(new System.Numerics.Vector3(half0, half1, half2));
                }
            }
            int maxStep;
            using (Stream stream = (Stream)new FileStream(@"C:\boxFaces", FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                maxStep = streamReader.Read<ushort>();
                for (int i = 0; i < 26; i++)
                {
                    stripIndices.Add(streamReader.Read<ushort>());
                }
            }
            List<int> indices = inverseWatermarkTransform(stripIndices, maxStep);
            List<int> indicesTest = unpackInds(indices);
            List<byte> indicesOut = new List<byte>();
            List<byte> indicesOut2 = new List<byte>();
            foreach(var id in indices)
            {
                indicesOut.AddRange(BitConverter.GetBytes((ushort)id));
            }
            foreach (var id in indicesTest)
            {
                indicesOut2.AddRange(BitConverter.GetBytes((ushort)id));
            }

            File.WriteAllBytes(@"C:\boxFacesDecrypted", indicesOut.ToArray());
            File.WriteAllBytes(@"C:\boxFacesDecryptedTest", indicesOut2.ToArray());

            StringBuilder outStr = new StringBuilder();
            foreach(var vert in verts)
            {
                outStr.AppendLine($"v {vert.X} {vert.Y} {vert.Z}");
            }
            outStr.AppendLine();
            for (int i = 0; i < indicesTest.Count - 2; i += 3)
            {
                outStr.AppendLine($"f {indicesTest[i] + 1} {indicesTest[i + 1] + 1} {indicesTest[i + 2] + 1}");
            }

            File.WriteAllText(@"C:\boxbad.obj", outStr.ToString());
        }
    }
}