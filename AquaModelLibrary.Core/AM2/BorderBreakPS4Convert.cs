using AquaModelLibrary.Data.AM2.BorderBreakPS4;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaNodeData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Core.General;
using System.Numerics;

namespace AquaModelLibrary.Core.AM2
{
    public class BorderBreakPS4Convert
    {
        public static List<AquaNode> MotBonesToAQN(MOT_BONE bones)
        {
            List<AquaNode> aquaNodes = new List<AquaNode>();

            for (int i = 0; i < bones.skeletonList.Count; i++)
            {
                var skeleton = bones.skeletonList[i];
                var aqn = new AquaNode();
                var nodeArr = new NODE[skeleton.Count];

                for (int b = skeleton.Count - 1; b >= 0; b--)
                {
                    var bone = skeleton[b];
                    //Debug.WriteLine($"{bone.boneStruct.usht_08} {bone.boneStruct.usht_0A} {bone.name}");
                    var node = new NODE();
                    node.boneName.SetString(bone.name);
                    node.animatedFlag = 1;
                    node.boneShort1 = 0x1C0;
                    node.pos = bone.boneStruct.position;
                    node.eulRot = bone.boneStruct.eulerRotation * (float)(180 / Math.PI);
                    node.scale = new Vector3(1, 1, 1);
                    node.parentId = -1;
                    node.nextSibling = -1;
                    if (bone.childrenIds.Count > 0)
                    {
                        node.firstChild = bone.childrenIds[0];
                    }
                    else
                    {
                        node.firstChild = -1;
                    }

                    //Set parent and nextSibling for children. Only child data is stored
                    for (int c = 0; c < bone.childrenIds.Count; c++)
                    {
                        var childId = bone.childrenIds[c];
                        var cbone = nodeArr[childId];
                        cbone.parentId = b;

                        if (c != bone.childrenIds.Count - 1)
                        {
                            cbone.nextSibling = bone.childrenIds[c + 1];
                        }
                        nodeArr[childId] = cbone;
                    }
                    nodeArr[b] = node;
                }
                aqn.nodeList.AddRange(nodeArr);

                //Set inverse world matrices
                for (int b = 0; b < aqn.nodeList.Count; b++)
                {
                    var bone = skeleton[b];
                    var node = aqn.nodeList[b];

                    Matrix4x4 mat = Matrix4x4.Identity;
                    mat *= Matrix4x4.CreateScale(node.scale);
                    Matrix4x4 rotation = Matrix4x4.Identity;


                    rotation = Matrix4x4.CreateRotationX(bone.boneStruct.eulerRotation.X) *
                        Matrix4x4.CreateRotationY(bone.boneStruct.eulerRotation.Y) *
                        Matrix4x4.CreateRotationZ(bone.boneStruct.eulerRotation.Z);

                    mat *= rotation;
                    mat *= Matrix4x4.CreateTranslation(node.pos);

                    //If there's a parent, multiply by it
                    if (node.parentId > -1)
                    {
                        var pn = aqn.nodeList[node.parentId];
                        var parentInvTfm = new Matrix4x4(pn.m1.X, pn.m1.Y, pn.m1.Z, pn.m1.W,
                                                      pn.m2.X, pn.m2.Y, pn.m2.Z, pn.m2.W,
                                                      pn.m3.X, pn.m3.Y, pn.m3.Z, pn.m3.W,
                                                      pn.m4.X, pn.m4.Y, pn.m4.Z, pn.m4.W);
                        Matrix4x4.Invert(parentInvTfm, out var invParentInvTfm);
                        mat = mat * invParentInvTfm;
                    }

                    Matrix4x4.Invert(mat, out var invMat);
                    node.SetInverseBindPoseMatrix(invMat);

                    aqn.nodeList[b] = node;
                }

                aquaNodes.Add(aqn);
            }

            return aquaNodes;
        }

        public static void STGLayoutDump(string stgFileName)
        {
            var rootFolder = Path.GetDirectoryName(Path.GetDirectoryName(stgFileName));
            var objFolderName = Path.GetFileName(Path.GetDirectoryName(stgFileName)).Replace("stg", "objset");
            var baseFilename = Path.GetFileNameWithoutExtension(stgFileName).Split('_')[1];
            var objFileName = baseFilename + "_obj.bin";
            var texFileName = baseFilename + "_tex.bin";
            var fullObjFileName = Path.Combine(rootFolder, objFolderName, objFileName);
            var fullTexFileName = Path.Combine(rootFolder, objFolderName, texFileName);
            var outFolder = stgFileName + "_";

            STG stg;
            E_OBJ eobj;
            List<AquaObject> aqpList;
            List<AquaNode> exportAqnList = new List<AquaNode>();
            List<AquaObject> exportAqpList = new List<AquaObject>();
            List<string> modelNames = new List<string>();
            Dictionary<string, AquaObject> aqpDict = new Dictionary<string, AquaObject>();
            List<List<Matrix4x4>> instanceTransformListList = new List<List<Matrix4x4>>();

            using (MemoryStream stream = new MemoryStream(File.ReadAllBytes(stgFileName)))
            using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                stg = new STG(streamReader);
            }
            using (MemoryStream stream = new MemoryStream(File.ReadAllBytes(fullObjFileName)))
            using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                eobj = new E_OBJ(streamReader);
            }
            ExtractTXP(fullTexFileName, File.ReadAllBytes(fullTexFileName), outFolder);
            aqpList = EOBJToAqua(eobj);

            for (int i = 0; i < eobj.names.Count; i++)
            {
                var aqp = aqpList[i];
                aqp.ConvertToPSO2Model(true, false, false, true, false, false, false, true);
                aqp.ConvertToLegacyTypes();
                aqp.CreateTrueVertWeights();
                aqpDict.Add(eobj.names[i], aqp);
            }
            foreach (var obj in stg.objList)
            {
                if (obj.modelName != null && aqpDict.ContainsKey(obj.modelName))
                {
                    modelNames.Add(obj.modelName);
                    exportAqnList.Add(new AquaNode(obj.modelName));
                    exportAqpList.Add(aqpDict[obj.modelName]);
                    List<Matrix4x4> tfm = new List<Matrix4x4>() { obj.stgObj.transform };
                    instanceTransformListList.Add(tfm);
                }
            }

            string exportName = Path.Combine(outFolder, Path.GetFileName(stgFileName) + ".fbx");
            FbxExporterNative.ExportToFileSets(exportAqpList, exportAqnList, modelNames, exportName, instanceTransformListList, false);
        }

        public static void ExtractTXP(string txpArchive, byte[] txpRaw, string outPath = null)
        {
            using (MemoryStream stream = new MemoryStream(txpRaw))
            using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                if (Path.GetFileName(txpArchive).StartsWith("spr_"))
                {
                    var int_00 = streamReader.Read<int>();
                    var txpOffset = streamReader.Read<int>();
                    streamReader.Seek(txpOffset, SeekOrigin.Begin);
                }

                //Standard TXP archive
                var TXP = new TXP(streamReader);
                var path = outPath ?? txpArchive + "_out";
                Directory.CreateDirectory(path);
                for (int i = 0; i < TXP.txp3.txp4List.Count; i++)
                {
                    string baseFname;
                    if (TXP.txp3.txp4Names.Count > 0)
                    {
                        baseFname = TXP.txp3.txp4Names[i];
                    }
                    else
                    {
                        baseFname = Path.GetFileName(txpArchive) + $"_{i}";
                    }
                    var fname = Path.Combine(path, baseFname + ".dds");
                    File.WriteAllBytes(fname, TXP.txp3.txp4List[i].GetDDS());
                }
            }
        }

        public static List<AquaObject> EOBJToAqua(E_OBJ mdl)
        {
            int matCounter = 0;
            List<AquaObject> aqpList = new List<AquaObject>();

            for (int m = 0; m < mdl.models.Count; m++)
            {
                AquaObject aqp = new AquaObject();
                var model = mdl.models[m];
                for (int i = 0; i <= model.highestBone; i++)
                {
                    aqp.bonePalette.Add((uint)i);
                }
                aqp.objc.bonePaletteOffset = 1;

                for (int vertSet = 0; vertSet < model.meshes.Count; vertSet++)
                {
                    var eObjMesh = model.meshes[vertSet];

                    VTXL vtxl = new VTXL();

                    vtxl.vertPositions.AddRange(eObjMesh.vertPositions);
                    vtxl.vertNormals.AddRange(eObjMesh.vertNormals);
                    vtxl.uv1List.AddRange(eObjMesh.vertUvs);
                    vtxl.uv2List.AddRange(eObjMesh.vertUv2s);

                    if (eObjMesh.vertRigidWeightIndices.Count > 0)
                    {
                        foreach (var index in eObjMesh.vertRigidWeightIndices)
                        {
                            vtxl.vertWeightIndices.Add(new int[] { (int)index, 0, 0, 0 });
                            vtxl.vertWeights.Add(new Vector4(1, 0, 0, 0));
                        }
                    }
                    else
                    {
                        vtxl.vertWeightIndices.AddRange(eObjMesh.vertWeightIndices);
                        vtxl.vertWeights.AddRange(eObjMesh.vertWeights);
                    }

                    int bpCounter = 0;
                    foreach (var faceSet in eObjMesh.faceLists)
                    {
                        //Material
                        var mat = new GenericMaterial();
                        mat.matName = $"Material_{matCounter++}";
                        mat.texNames = new List<string>() { "test_d.dds" };
                        aqp.tempMats.Add(mat);

                        Dictionary<int, int> vertIdDict = new Dictionary<int, int>();
                        VTXL matVtxl = new VTXL();
                        GenericTriangles genMesh = new GenericTriangles();
                        List<Vector3> triList = new List<Vector3>();
                        for (int vertIndex = 0; vertIndex < faceSet.Count - 2; vertIndex++)
                        {
                            Vector3 tri;
                            //When index is odd, flip
                            if ((vertIndex & 1) > 0)
                            {
                                tri = new Vector3(faceSet[vertIndex + 2], faceSet[vertIndex + 1], faceSet[vertIndex]);
                            }
                            else
                            {
                                tri = new Vector3(faceSet[vertIndex], faceSet[vertIndex + 1], faceSet[vertIndex + 2]);
                            }

                            VTXL.AddVertices(vtxl, vertIdDict, matVtxl, tri, out int x, out int y, out int z);

                            //Avoid degen tris
                            if (x == y || x == z || y == z)
                            {
                                continue;
                            }
                            triList.Add(new Vector3(x, y, z));
                        }
                        genMesh.triList = triList;
                        //matVtxl.bonePalette.AddRange(eObjMesh.bonePalettes[bpCounter++]);

                        //Extra
                        genMesh.vertCount = matVtxl.vertPositions.Count;
                        genMesh.matIdList = new List<int>(new int[genMesh.triList.Count]);
                        for (int j = 0; j < genMesh.matIdList.Count; j++)
                        {
                            genMesh.matIdList[j] = aqp.tempMats.Count - 1;
                        }

                        if (genMesh.vertCount > 0)
                        {
                            aqp.tempTris.Add(genMesh);
                            aqp.vtxlList.Add(matVtxl);
                            aqp.vtxeList.Add(VTXE.ConstructFromVTXL(matVtxl, out int vc));
                        }
                    }
                }

                aqpList.Add(aqp);
            }

            return aqpList;
        }

        public static List<AquaObject> FLDToAqua(FLD mdl)
        {
            int matCounter = 0;
            List<AquaObject> aqpList = new List<AquaObject>();

            for (int m = 0; m < mdl.fldModels.Count; m++)
            {
                AquaObject aqp = new AquaObject();
                var model = mdl.fldModels[m];
                aqp.bonePalette.Add(0);
                aqp.objc.bonePaletteOffset = 1;

                VTXL vtxl = new VTXL();

                vtxl.vertPositions.AddRange(model.vertPositions);
                //vtxl.vertNormals.AddRange(model.vertNormals);

                //Material
                var mat = new GenericMaterial();
                mat.matName = $"Material_{matCounter++}";
                mat.texNames = new List<string>() { "test_d.dds" };
                aqp.tempMats.Add(mat);

                Dictionary<int, int> vertIdDict = new Dictionary<int, int>();
                VTXL matVtxl = new VTXL();
                GenericTriangles genMesh = new GenericTriangles();
                List<Vector3> triList = new List<Vector3>();
                foreach (var polygon in model.polygons)
                {
                    Vector3 tri0 = new Vector3(polygon.vertId0, polygon.vertId1, polygon.vertId2);
                    Vector3 tri1 = new Vector3(polygon.vertId0, polygon.vertId2, polygon.vertId3);

                    int x, y, z;
                    VTXL.AddVertices(vtxl, vertIdDict, matVtxl, tri0, out x, out y, out z);
                    triList.Add(new Vector3(x, y, z));
                    VTXL.AddVertices(vtxl, vertIdDict, matVtxl, tri1, out x, out y, out z);
                    triList.Add(new Vector3(x, y, z));

                }
                genMesh.triList = triList;

                //Extra
                genMesh.vertCount = matVtxl.vertPositions.Count;
                genMesh.matIdList = new List<int>(new int[genMesh.triList.Count]);
                for (int j = 0; j < genMesh.matIdList.Count; j++)
                {
                    genMesh.matIdList[j] = aqp.tempMats.Count - 1;
                }

                if (genMesh.vertCount > 0)
                {
                    aqp.tempTris.Add(genMesh);
                    aqp.vtxlList.Add(matVtxl);
                    aqp.vtxeList.Add(VTXE.ConstructFromVTXL(matVtxl, out int vc));
                }

                aqpList.Add(aqp);
            }

            return aqpList;
        }
    }
}
