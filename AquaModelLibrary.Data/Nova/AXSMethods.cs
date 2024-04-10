using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Data.Nova.Structures;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaNodeData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using AquaModelLibrary.Helpers.Readers;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using static AquaModelLibrary.Data.Nova.AXSConstants;
using static AquaModelLibrary.Helpers.MathHelpers.MathExtras;

namespace AquaModelLibrary.Data.Nova
{
    public static class AXSMethods
    {
        //We're not realistically going to fully convert everything, but we can get vertex data and bones if nothing else
        //Returns an aqp ready for the ConvertToNGSPSO2Mesh method
        public static AquaObject ReadAXS(string filePath, bool writeTextures, out AquaNode aqn)
        {
            AquaObject aqp = new AquaObject();
            var ext = Path.GetExtension(filePath);
            aqp.objc.type = 0xC33;
            aqn = new AquaNode();

            using (MemoryStream stream = new MemoryStream(File.ReadAllBytes(filePath)))
            using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                Debug.WriteLine(Path.GetFileName(filePath));
                long last__oaPos = 0;
                eertStruct eertNodes = null;
                ipnbStruct tempLpnbList = null;
                List<ffubStruct> ffubList = new List<ffubStruct>();
                List<XgmiStruct> xgmiList = new List<XgmiStruct>();
                List<string> texNames = new List<string>();
                List<MeshDefinitions> meshDefList = new List<MeshDefinitions>();
                List<stamData> stamList = new List<stamData>();
                Dictionary<string, rddaStruct> rddaList = new Dictionary<string, rddaStruct>();
                Dictionary<string, rddaStruct> imgRddaList = new Dictionary<string, rddaStruct>();
                Dictionary<string, rddaStruct> vertRddaList = new Dictionary<string, rddaStruct>();
                Dictionary<string, rddaStruct> faceRddaList = new Dictionary<string, rddaStruct>();
                Dictionary<string, int> xgmiIdByCombined = new Dictionary<string, int>();
                Dictionary<string, int> xgmiIdByUnique = new Dictionary<string, int>();
                ffubStruct imgFfub = new ffubStruct();
                ffubStruct vertFfub = new ffubStruct();
                ffubStruct faceFfub = new ffubStruct();

                var fType = streamReader.Read<int>();

                var fsaLen = streamReader.Read<int>();
                streamReader.Seek(0x8, SeekOrigin.Current);
                //Go to Vert definition, node, material, and misc data
                while (streamReader.Position < fsaLen)
                {
                    var tag = streamReader.Peek<int>();
                    var test = Encoding.UTF8.GetString(BitConverter.GetBytes(tag));
                    //Debug.WriteLine(streamReader.Position.ToString("X"));
                    //Debug.WriteLine(test);
                    switch (tag)
                    {
                        case __oa:
                            last__oaPos = streamReader.Position;
                            streamReader.Seek(0xD0, SeekOrigin.Current);
                            break;
                        case FIA:
                            streamReader.Seek(0x10, SeekOrigin.Current);
                            break;
                        case __lm:
                            var stam = streamReader.ReadLM();
                            if (stam != null && stam.Count > 0)
                            {
                                stamList = stam;
                            }
                            break;
                        case __bm:
                            streamReader.ReadBM(meshDefList, tempLpnbList, stamList, last__oaPos);
                            break;
                        case lpnb:
                            tempLpnbList = streamReader.ReadIpnb();
                            break;
                        case eert:
                            eertNodes = streamReader.ReadEert();
                            break;
                        //case ssem:
                        //  streamReader.SkipBasicAXSStruct(); //Maybe use for material data later. Remember to store ordered id for _bm mesh entries for this
                        // break;
                        case Xgmi:
                            var xgmiData = streamReader.ReadXgmi();
                            if (!xgmiIdByCombined.ContainsKey(xgmiData.stamCombinedId))
                            {
                                xgmiIdByCombined.Add(xgmiData.stamCombinedId, xgmiIdByCombined.Count);
                            }
                            xgmiIdByUnique.Add(xgmiData.stamUniqueId, xgmiList.Count);
                            xgmiList.Add(xgmiData);
                            break;
                        default:
                            streamReader.SkipBasicAXSStruct();
                            break;
                    }

                }

                //Assemble aqn from eert
                if (eertNodes != null)
                {
                    for (int i = 0; i < eertNodes.boneCount; i++)
                    {
                        var rttaNode = eertNodes.rttaList[i];
                        Matrix4x4 mat = Matrix4x4.Identity;

                        mat *= Matrix4x4.CreateScale(rttaNode.scale);

                        var rotation = Matrix4x4.CreateFromQuaternion(rttaNode.quatRot);

                        mat *= rotation;

                        mat *= Matrix4x4.CreateTranslation(rttaNode.pos);

                        var parentId = rttaNode.parentNodeId;

                        //If there's a parent, multiply by it
                        if (i != 0)
                        {
                            if (rttaNode.parentNodeId < 0)
                            {
                                parentId = 0;
                            }
                            var pn = aqn.nodeList[parentId];
                            var parentInvTfm = new Matrix4x4(pn.m1.X, pn.m1.Y, pn.m1.Z, pn.m1.W,
                                                          pn.m2.X, pn.m2.Y, pn.m2.Z, pn.m2.W,
                                                          pn.m3.X, pn.m3.Y, pn.m3.Z, pn.m3.W,
                                                          pn.m4.X, pn.m4.Y, pn.m4.Z, pn.m4.W);
                            Matrix4x4.Invert(parentInvTfm, out var invParentInvTfm);
                            mat = mat * invParentInvTfm;

                        }
                        else
                        {
                            parentId = -1;
                        }
                        rttaNode.nodeMatrix = mat;

                        //Create AQN node
                        NODE aqNode = new NODE();
                        aqNode.animatedFlag = 1;
                        aqNode.parentId = parentId;
                        aqNode.unkNode = -1;
                        aqNode.pos = rttaNode.pos;
                        aqNode.eulRot = QuaternionToEuler(rttaNode.quatRot);

                        if (Math.Abs(aqNode.eulRot.Y) > 120)
                        {
                            aqNode.scale = new Vector3(-1, -1, -1);
                        }
                        else
                        {
                            aqNode.scale = new Vector3(1, 1, 1);
                        }
                        Matrix4x4.Invert(mat, out var invMat);
                        aqNode.m1 = new Vector4(invMat.M11, invMat.M12, invMat.M13, invMat.M14);
                        aqNode.m2 = new Vector4(invMat.M21, invMat.M22, invMat.M23, invMat.M24);
                        aqNode.m3 = new Vector4(invMat.M31, invMat.M32, invMat.M33, invMat.M34);
                        aqNode.m4 = new Vector4(invMat.M41, invMat.M42, invMat.M43, invMat.M44);
                        aqNode.boneName = rttaNode.nodeName;
                        //Debug.WriteLine($"{i} " + aqNode.boneName.GetString());
                        aqn.nodeList.Add(aqNode);
                    }
                }


                //Go to mesh buffers
                streamReader.Seek(fsaLen, SeekOrigin.Begin);
                if (streamReader.Position >= stream.Length)
                {
                    return null;
                }

                var fType2 = streamReader.Read<int>();
                //Read mesh data
                if (fType2 != FMA)
                {
                    Debug.WriteLine("Unexpected struct in location of FMA!");
                    return null;
                }
                streamReader.Seek(0xC, SeekOrigin.Current);

                //Skip daeh
                int meshSettingLen = streamReader.ReadDAEH();

                //Read ffub and rdda
                //Count mesh count here for now and store starts and ends of data
                long meshSettingStart = streamReader.Position;
                while (streamReader.Position < meshSettingStart + meshSettingLen)
                {
                    streamReader.ReadFFUBorRDDA(ffubList, rddaList, imgRddaList, vertRddaList, faceRddaList, ref imgFfub, ref vertFfub, ref faceFfub);
                }

                int meshCount = meshDefList.Count;

                //Read image data
                Dictionary<string, List<XgmiStruct>> xgmiDict = new Dictionary<string, List<XgmiStruct>>();
                foreach(var xgmiMip in xgmiList)
                {
                    string id = xgmiMip.stamCombinedId;
                    if(!xgmiDict.ContainsKey(id))
                    {
                        xgmiDict.Add(id, new List<XgmiStruct>() { xgmiMip });
                    } else
                    {
                        xgmiDict[id].Add(xgmiMip);
                    }
                }

                //Sort mips in case they're out of order
                //In theory, they're probably in order and just work and we could do this more simply by just reading them in order and splitting when a new combined id is detected.
                //In practice there's no reason it would have to be like that from the file structure and so we take the safe road.
                foreach(var set in xgmiDict)
                {
                    set.Value.Sort((x, y) => x.mipIdByte > y.mipIdByte ? 1 : x.mipIdByte == y.mipIdByte ? 0 : -1);
                }

                //Write out as single dds
                foreach (var set in xgmiDict)
                {
                    var xgmiData = set.Value[0];
                    bool isCubemap = (xgmiData.int_50 & 0x81FF) > 0;

                    byte mipCount;
                    if(isCubemap)
                    {
                        mipCount = (byte)(set.Value.Count / 6);
                    } else
                    {
                        mipCount = (byte)set.Value.Count;
                    }

                    List<byte> ddsBytes = new List<byte>();
                    ddsBytes.AddRange(AIFMethods.GetDDSHeaderBytes(xgmiData.width, xgmiData.height, xgmiData, mipCount, isCubemap));

                    if(isCubemap)
                    {
                        for(int i = 0; i < 6; i++)
                        {
                            for(int j = 0; j < mipCount; j++)
                            {
                                var xgmiMip = set.Value[i + j * 6];
                                var imgBufferInfo = imgRddaList[$"{xgmiMip.md5_1.ToString("X")}{xgmiMip.md5_2.ToString("X")}"];
                                var position = meshSettingStart + imgFfub.dataStartOffset + imgBufferInfo.dataStartOffset;
                                ddsBytes.AddRange(AIFMethods.GetMipImage(xgmiMip, streamReader.ReadBytes(position, imgBufferInfo.dataSize)));
                            }
                        }
                    } else
                    {
                        foreach (var xgmiMip in set.Value)
                        {
                            var imgBufferInfo = imgRddaList[$"{xgmiMip.md5_1.ToString("X")}{xgmiMip.md5_2.ToString("X")}"];
                            var position = meshSettingStart + imgFfub.dataStartOffset + imgBufferInfo.dataStartOffset;
                            ddsBytes.AddRange(AIFMethods.GetMipImage(xgmiMip, streamReader.ReadBytes(position, imgBufferInfo.dataSize)));
                        }
                    }

                    var outImagePath = filePath.Replace(ext, $"_tex_{xgmiData.stamCombinedId}" + ".dds");
                    texNames.Add(Path.GetFileName(outImagePath));
                    try
                    {
                        if (writeTextures)
                        {
                            File.WriteAllBytes(outImagePath, ddsBytes.ToArray());
                        }
                    }
                    catch (Exception exc)
                    {
#if DEBUG
                        string name = Path.GetFileName(filePath);
                        Debug.WriteLine($"Extract tex {xgmiData.stamCombinedId} failed.");
                        /*
                        File.WriteAllBytes($"C:\\{name}_xgmiHeader_{i}.bin", xgmiData.GetBytes());
                        File.WriteAllBytes($"C:\\{name}_xgmiBuffer_{i}.bin", buffer);
                        */
                        Debug.WriteLine(exc.Message);
#endif
                    }
                }

                //Read model data - Since ffubs are initialized, they default to 0. 
                int vertFfubPadding = imgFfub.structSize;
                int faceFfubPadding = imgFfub.structSize + vertFfub.structSize;

                for (int i = 0; i < meshCount; i++)
                {
                    var mesh = meshDefList[i];
                    var nodeMatrix = Matrix4x4.Identity;
                    for (int bn = 0; bn < eertNodes.boneCount; bn++)
                    {
                        var node = eertNodes.rttaList[bn];
                        if (node.meshNodePtr == mesh.oaPos)
                        {
                            nodeMatrix = node.nodeMatrix;
                            break;
                        }
                    }
                    var vertBufferInfo = vertRddaList[$"{mesh.salvStr.md5_1.ToString("X")}{mesh.salvStr.md5_2.ToString("X")}"];
                    var faceBufferInfo = faceRddaList[$"{mesh.lxdiStr.md5_1.ToString("X")}{mesh.lxdiStr.md5_2.ToString("X")}"];
                    //Debug.WriteLine($"Vert set {i}: " + vertBufferInfo.md5_1.ToString("X") + " " + vertBufferInfo.dataStartOffset.ToString("X") + " " + vertBufferInfo.toTagStruct.ToString("X") + " " + (meshSettingStart + vertFfubPadding + vertFfub.dataStartOffset + vertBufferInfo.dataStartOffset).ToString("X"));
                    //Debug.WriteLine($"Face set {i}: " + faceBufferInfo.md5_1.ToString("X") + " " + faceBufferInfo.dataStartOffset.ToString("X") + " " + faceBufferInfo.toTagStruct.ToString("X") + " " + (meshSettingStart + faceFfubPadding + faceFfub.dataStartOffset + faceBufferInfo.dataStartOffset).ToString("X"));

                    //Vert data
                    var vertCount = vertBufferInfo.dataSize / mesh.salvStr.vertLen;

                    streamReader.Seek((meshSettingStart + vertFfubPadding + vertFfub.dataStartOffset + vertBufferInfo.dataStartOffset), SeekOrigin.Begin);
                    //Debug.WriteLine(streamReader.Position.ToString("X"));
                    VTXL vtxl = new VTXL(streamReader, mesh.vtxe, vertCount);

                    //Account for indices without weights
                    /*if(vtxl.vertWeightIndices.Count > 0 && vtxl.vertWeights.Count == 0)
                    {
                        for(int v = 0; v < vtxl.vertWeightIndices.Count; v++)
                        {
                            vtxl.vertWeights.Add(new Vector4(1, 0, 0, 0));
                        }
                    }*/
                    vtxl.convertToLegacyTypes();

                    //Fix vert transforms
                    for (int p = 0; p < vtxl.vertPositions.Count; p++)
                    {
                        vtxl.vertPositions[p] = Vector3.Transform(vtxl.vertPositions[p], nodeMatrix);
                        if (vtxl.vertNormals.Count > 0)
                        {
                            vtxl.vertNormals[p] = Vector3.TransformNormal(vtxl.vertNormals[p], nodeMatrix);
                        }
                    }


                    //Handle bone indices
                    if (mesh.ipnbStr != null && mesh.ipnbStr.shortList.Count > 0)
                    {
                        vtxl.bonePalette = (mesh.ipnbStr.shortList.ConvertAll(delegate (short num)
                        {
                            return (ushort)num;
                        }));

                        //Convert the indices based on the global bone list as pso2 will expect
                        for (int bn = 0; bn < vtxl.bonePalette.Count; bn++)
                        {
                            vtxl.bonePalette[bn] = (ushort)mesh.lpnbStr.shortList[vtxl.bonePalette[bn]];
                        }
                    }

                    aqp.vtxlList.Add(vtxl);

                    //Face data
                    GenericTriangles genMesh = new GenericTriangles();

                    int faceIndexCount = faceBufferInfo.dataSize / 2;
                    List<int> faceList = new List<int>();

                    streamReader.Seek((meshSettingStart + faceFfubPadding + faceFfub.dataStartOffset + faceBufferInfo.dataStartOffset), SeekOrigin.Begin);
                    int maxStep = streamReader.Read<ushort>();
                    for (int fId = 0; fId < faceIndexCount - 1; fId++)
                    {
                        faceList.Add(streamReader.Read<ushort>());
                    }

                    //Convert the data to something usable with this algorithm and then destripify it.
                    List<ushort> triList = unpackInds(inverseWatermarkTransform(faceList, maxStep)).ConvertAll(delegate (int num)
                    {
                        return (ushort)num;
                    });
                    var tempFaceData = new StripData() { triStrips = triList, format0xC31 = true, triIdCount = triList.Count };
                    genMesh.triList = tempFaceData.GetTriangles();

                    //Extra
                    genMesh.vertCount = vertCount;
                    genMesh.matIdList = new List<int>(new int[genMesh.triList.Count]);
                    for (int j = 0; j < genMesh.matIdList.Count; j++)
                    {
                        genMesh.matIdList[j] = aqp.tempMats.Count;
                    }
                    aqp.tempTris.Add(genMesh);

                    //Material
                    var mat = new GenericMaterial();
                    mat.matName = $"NovaMaterial_{i}";
                    mat.texNames = GetTexNames(mesh, xgmiIdByCombined, xgmiIdByUnique, texNames);
                    aqp.tempMats.Add(mat);
                }

                return aqp;
            }
        }

        public static List<string> GetTexNames(MeshDefinitions mesh, Dictionary<string, int> xgmiIdByCombined, Dictionary<string, int> xgmiIdByUnique, List<string> cachedTexNames)
        {
            var texNames = new List<string>();
            if (mesh.stam != null)
            {
                foreach (var xgmiKey in mesh.stam.texIds)
                {
                    if (xgmiIdByCombined.ContainsKey(xgmiKey))
                    {
                        var id = xgmiIdByCombined[xgmiKey];
                        texNames.Add(cachedTexNames[id]);
                    }
                    else if (xgmiIdByUnique.ContainsKey(xgmiKey))
                    {
                        texNames.Add(cachedTexNames[xgmiIdByUnique[xgmiKey]]);
                    }
                }
            }

            if (texNames.Count == 0)
            {
                texNames.Add("dummyTex.dds");
            }

            return texNames;
        }

        public static VTXE GenerateGenericPSO2VTXE(byte vertData0, byte vertData1, byte vertData2, byte vertData3, byte vertData4, byte vertData5, byte vertData6, byte vertData7, int trueLength)
        {
            VTXE vtxe = new VTXE();
            int curLength = 0;

            //Vert positions
            if ((vertData0 & 0x1) > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0x0, 0x3, curLength));
                curLength += 0xC;
            }
            else if ((vertData0 & 0x8) > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0x0, 0x99, curLength));
                curLength += 0x8;
            }

            //Vert normals
            if ((vertData0 & 0x10) > 0)
            {
                if ((vertData0 & 0x80) > 0)
                {
                    vtxe.vertDataTypes.Add(new VTXEElement(0x2, 0xF, curLength));
                    curLength += 0x8;
                }
                else
                {
                    vtxe.vertDataTypes.Add(new VTXEElement(0x2, 0x3, curLength));
                    curLength += 0xC;
                }
            }

            //Vert colors
            if ((vertData3 & 0x10) > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0x3, 0x5, curLength));
                curLength += 0x4;
            }

            //Yet another UV thing?
            if ((vertData4 & 0x40) > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0x25, 0xC, curLength));
                curLength += 0x4;
            }

            //Vert UV1
            int addition = 0;
            if ((vertData1 & 0x1) > 0)
            {
                if ((vertData1 & 0x8) > 0)
                {
                    vtxe.vertDataTypes.Add(new VTXEElement(0x10, 0x99, curLength));
                    curLength += 0x4;
                }
                else
                {
                    vtxe.vertDataTypes.Add(new VTXEElement(0x10, 0x2, curLength));
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
                if ((vertData1 & 0x8) > 0)
                {
                    vtxe.vertDataTypes.Add(new VTXEElement(0x10 + addition, 0x99, curLength));
                    curLength += 0x4;
                    vtxe.vertDataTypes.Add(new VTXEElement(0x11 + addition, 0x99, curLength));
                    curLength += 0x4;
                }
                else
                {
                    vtxe.vertDataTypes.Add(new VTXEElement(0x10 + addition, 0x2, curLength));
                    curLength += 0x8;
                    vtxe.vertDataTypes.Add(new VTXEElement(0x11 + addition, 0x2, curLength));
                    curLength += 0x8;
                }

                addition += 2;
            }

            //MOAR UV
            if ((vertData1 & 0x10) > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0x10 + addition, 0x2, curLength));
                curLength += 0x8;
                vtxe.vertDataTypes.Add(new VTXEElement(0x11 + addition, 0x2, curLength));
                curLength += 0x8;

                addition += 2;
            }

            //Some kind of uv info? Idefk
            if ((vertData1 & 0x20) > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0x10 + addition, 0x2, curLength));
                curLength += 0x8;

                addition += 1;
            }

            //Some other kind of uv info? Idefk
            if ((vertData1 & 0x40) > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0x10 + addition, 0x99, curLength));
                curLength += 0x4;
                vtxe.vertDataTypes.Add(new VTXEElement(0x11 + addition, 0x99, curLength));
                curLength += 0x4;

                addition += 2;
            }

            //Weights and Weight Indices
            if ((vertData3 & 0x40) > 0)
            {
                if ((vertData5 & 0x1) > 0)
                {
                    vtxe.vertDataTypes.Add(new VTXEElement(0x1, 0x11, curLength));
                    curLength += 0x8;
                }
                vtxe.vertDataTypes.Add(new VTXEElement(0xb, 0x7, curLength));
                curLength += 0x4;
            }

            //More uv stuff??
            if ((vertData2 & 0x40) > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0x22, 0xC, curLength));
                curLength += 0x4;
                vtxe.vertDataTypes.Add(new VTXEElement(0x23, 0xC, curLength));
                curLength += 0x4;
            }

            if (curLength != trueLength || vertData6 > 0 || vertData7 > 0)
            {
                Debug.WriteLine(curLength + " != " + trueLength + " " + vertData0.ToString("X") + " " + vertData1.ToString("X") + " " + vertData2.ToString("X") + " " + vertData3.ToString("X")
                    + " " + vertData4.ToString("X") + " " + vertData5.ToString("X") + " " + vertData6.ToString("X") + " " + vertData7.ToString("X"));
            }

            return vtxe;
        }

        public static eertStruct ReadEert(this BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            eertStruct boneList = new eertStruct();
            long bookmark = streamReader.Position;

            streamReader.Seek(0x4, SeekOrigin.Current);
            var len = streamReader.Read<int>();
            streamReader.Seek(0x4, SeekOrigin.Current);
            var trueLen = streamReader.Read<int>();

            boneList.boneCount = streamReader.Read<int>();

            streamReader.Seek(bookmark + 0x100, SeekOrigin.Begin);
            for (int i = 0; i < boneList.boneCount; i++)
            {
                rttaStruct bone = new rttaStruct();
                bookmark = streamReader.Position;
                bone.magic = streamReader.Read<int>();
                bone.len = streamReader.Read<int>();
                bone.int_08 = streamReader.Read<int>();
                bone.trueLen = streamReader.Read<int>();

                bone.nodeName = streamReader.Read<PSO2String>();
                bone.int_30 = streamReader.Read<int>();
                bone.meshNodePtr = streamReader.Read<int>();
                bone.int_38 = streamReader.Read<int>();
                bone.parentNodeId = streamReader.Read<int>();

                bone.childNodeCount = streamReader.Read<short>();
                streamReader.Seek(0x3E, SeekOrigin.Current);
                bone.pos = streamReader.Read<Vector3>(); streamReader.Seek(0x4, SeekOrigin.Current);
                bone.quatRot = streamReader.Read<Quaternion>();
                bone.scale = streamReader.Read<Vector3>(); streamReader.Seek(0x4, SeekOrigin.Current);

                boneList.rttaList.Add(bone);
                streamReader.Seek(bookmark + bone.trueLen, SeekOrigin.Begin);
            }

            return boneList;
        }

        //Returns DAEH's length for the section - its own length
        public static int ReadDAEH(this BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            streamReader.Seek(0x4, SeekOrigin.Current);
            var len = streamReader.Read<int>();
            streamReader.Seek(len - 0x10, SeekOrigin.Current);
            var meshSettingLen = streamReader.Read<int>();
            streamReader.Seek(0x4, SeekOrigin.Current);

            return meshSettingLen - len;
        }

        public static void SkipBasicAXSStruct(this BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            long bookmark = streamReader.Position;

            streamReader.Read<int>();
            var trueLen = streamReader.Read<int>(); //Doesn't include padding so shouldn't be used
            streamReader.Read<int>();
            var len = streamReader.Read<int>();
            streamReader.Seek(bookmark, SeekOrigin.Begin);

            if (len == 0)
            {
                len = trueLen;
            }
            streamReader.Seek(len, SeekOrigin.Current);
        }

        public static List<stamData> ReadLM(this BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            var stamList = new List<stamData>();
            var lmStart = streamReader.Position;
            streamReader.Read<int>();
            var lmEnd = streamReader.Read<int>() + lmStart;

            streamReader.Seek(0x8, SeekOrigin.Current);
            while (streamReader.Position < lmEnd)
            {
                var tag = streamReader.Peek<int>();
                switch (tag)
                {
                    case stam:
                        stamList.Add(streamReader.ReadStam());
                        break;
                    default:
                        streamReader.SkipBasicAXSStruct();
                        break;
                }

                //Make sure to stop the loop if needed
                if (stamList[stamList.Count - 1].lastStam == true)
                {
                    break;
                }
            }

            streamReader.Seek(lmEnd, SeekOrigin.Begin);

            return stamList;
        }

        public static stamData ReadStam(this BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            var stamDataObj = new stamData();
            var stamStart = streamReader.Position;
            streamReader.Read<int>();
            streamReader.Seek(0x8, SeekOrigin.Current);
            var stamSize = streamReader.Read<int>();
            streamReader.Seek(0x8, SeekOrigin.Current);
            var stamTexCount = streamReader.Read<ushort>();
            streamReader.Seek(0x12, SeekOrigin.Current);

            int texOffset = streamReader.Read<int>();
            if (texOffset == 0)
            {
                stamDataObj.lastStam = true;
                return stamDataObj;
            }
            streamReader.Seek(stamStart + texOffset, SeekOrigin.Begin);
            for (int i = 0; i < stamTexCount; i++)
            {
                var key0 = BitConverter.GetBytes(streamReader.Read<int>());
                int key1 = streamReader.Read<int>();
                stamDataObj.texIds.Add("00" + key0[1].ToString("X2") + key0[2].ToString("X2") + key0[3].ToString("X2") + key1.ToString("X")); //The first key digit can be an LOD sometimes, but we only want the highest quality.
                                                                                                                                              //Mips don't seem to be specially distinct and so this should be fine.
                //stamDataObj.texIds.Add(key0[0].ToString("X2") + key0[1].ToString("X2") + key0[2].ToString("X2") + key0[3].ToString("X2") + key1.ToString("X"));
                streamReader.Seek(0x18, SeekOrigin.Current);
            }

            //streamReader.Seek(stamEnd, SeekOrigin.Begin);

            return stamDataObj;
        }

        public static void ReadBM(this BufferedStreamReaderBE<MemoryStream> streamReader, List<MeshDefinitions> defs, ipnbStruct tempLpnbList, List<stamData> stamList, long last__oaPos)
        {
            int counter = 0;
            MeshDefinitions mesh = null;
            var bmStart = streamReader.Position;
            streamReader.Read<int>();
            var bmEnd = streamReader.Read<int>() + bmStart;

            streamReader.Seek(0x8, SeekOrigin.Current);
            while (streamReader.Position < bmEnd)
            {
                var tag = streamReader.Peek<int>();
                switch (tag)
                {
                    case ydbm:
                        if (mesh != null)
                        {
                            //Debug.WriteLine(defs.Count);
                            defs.Add(mesh);
                        }
                        mesh = new MeshDefinitions();
                        mesh.oaPos = last__oaPos;
                        mesh.lpnbStr = tempLpnbList;
                        mesh.ydbmStr = streamReader.ReadYdbm();
                        if (stamList.Count > counter)
                        {
                            mesh.stam = stamList[counter];
                        }
                        else if (stamList.Count > 0)
                        {
                            mesh.stam = stamList[stamList.Count - 1];
                        }
                        else
                        {
                            mesh.stam = null;
                        }
                        counter++;
                        break;
                    case lxdi:
                        mesh.lxdiStr = streamReader.ReadLxdi();
                        break;
                    case salv:
                        mesh.salvStr = streamReader.ReadSalv();
                        mesh.vtxe = GenerateGenericPSO2VTXE(mesh.salvStr.vertDef0, mesh.salvStr.vertDef1, mesh.salvStr.vertDef2, mesh.salvStr.vertDef3, mesh.salvStr.vertDef4, mesh.salvStr.vertDef5,
                            mesh.salvStr.vertDef6, mesh.salvStr.vertDef7, mesh.salvStr.vertLen);
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
                //Debug.WriteLine(defs.Count);
                defs.Add(mesh);
            }
        }

        public static ydbmStruct ReadYdbm(this BufferedStreamReaderBE<MemoryStream> streamReader)
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

        public static lxdiStruct ReadLxdi(this BufferedStreamReaderBE<MemoryStream> streamReader)
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

        public static salvStruct ReadSalv(this BufferedStreamReaderBE<MemoryStream> streamReader)
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
            salvStr.vertDef4 = streamReader.Read<byte>();
            salvStr.vertDef5 = streamReader.Read<byte>();
            salvStr.vertDef6 = streamReader.Read<byte>();
            salvStr.vertDef7 = streamReader.Read<byte>();
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
            if (salvStr.vertDef4 > 0)
            {
                Debug.WriteLine($"vertDef4 == {salvStr.vertDef4}");
            }
            if ((salvStr.vertDef0 & 0x2) > 0)
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

        public static ipnbStruct ReadIpnb(this BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            long bookmark = streamReader.Position;
            ipnbStruct ipbnStr = new ipnbStruct();

            ipbnStr.magic = streamReader.Read<int>();
            ipbnStr.len = streamReader.Read<int>();
            ipbnStr.int_08 = streamReader.Read<int>();
            ipbnStr.paddedLen = streamReader.Read<int>();

            int count = (ipbnStr.len - 0x10) / 2;
            for (int i = 0; i < count; i++)
            {
                ipbnStr.shortList.Add(streamReader.Read<short>());
            }

            streamReader.Seek(bookmark + ipbnStr.paddedLen, SeekOrigin.Begin);
            return ipbnStr;
        }

        public static void ReadFFUBorRDDA(this BufferedStreamReaderBE<MemoryStream> streamReader, List<ffubStruct> ffubStructs, Dictionary<string, rddaStruct> rddaStructs,
            Dictionary<string, rddaStruct> imgRddaStructs, Dictionary<string, rddaStruct> vertRddaStructs, Dictionary<string, rddaStruct> faceRddaStructs, ref ffubStruct imgFfub, ref ffubStruct vertFfub, ref ffubStruct faceFfub)
        {
            long bookmark = streamReader.Position;

            int magic = streamReader.Read<int>();
            int len;
            switch (magic)
            {
                case ffub:
                    var ffubData = streamReader.ReadFFUB(magic, out len);
                    switch (ffubData.dataType)
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

        public static ffubStruct ReadFFUB(this BufferedStreamReaderBE<MemoryStream> streamReader, int magic, out int len)
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

        public static rddaStruct ReadRDDA(this BufferedStreamReaderBE<MemoryStream> streamReader, int magic, out int len)
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

        //Thanks to Silent for this find
        //Face decoding functions from: https://forum.xentax.com/viewtopic.php?p=143356&sid=c54ef316ad86c051345fec2ef63a0bf7#p143356
        public static List<int> inverseWatermarkTransform(List<int> in_inds, int max_step)
        {
            if (max_step == 0x0000ffff)
            {
                return in_inds;
            }

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
            if (invertNormals)
            {
                for (int i = 0; i < inds.Count;)
                {
                    int a = inds[i++];
                    int b = inds[i++];
                    int c = inds[i++];

                    bool isDegen = a == b || b == c || a == c;
                    if (!isDegen)
                    {
                        out_inds.Add(b); out_inds.Add(a); out_inds.Add(c);
                    }

                    if (a < b && i < inds.Count)
                    {
                        int d = inds[i++];

                        isDegen = a == b || b == d || a == d;
                        if (!isDegen)
                        {
                            out_inds.Add(d); out_inds.Add(a); out_inds.Add(b);
                        }
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

                    bool isDegen = a == b || b == c || a == c;
                    if (!isDegen)
                    {
                        out_inds.Add(a); out_inds.Add(b); out_inds.Add(c);
                    }

                    if (a < b && i < inds.Count)
                    {
                        int d = inds[i++];
                        isDegen = a == b || b == d || a == d;
                        if (!isDegen)
                        {
                            out_inds.Add(a); out_inds.Add(d); out_inds.Add(b);
                        }
                    }
                }
            }


            return out_inds;
        }
    }
}
