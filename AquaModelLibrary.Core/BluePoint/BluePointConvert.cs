using AquaModelLibrary.Core.General;
using AquaModelLibrary.Data.BluePoint.CANI;
using AquaModelLibrary.Data.BluePoint.CMAT;
using AquaModelLibrary.Data.BluePoint.CMDL;
using AquaModelLibrary.Data.BluePoint.CMSH;
using AquaModelLibrary.Data.BluePoint.CSKL;
using AquaModelLibrary.Data.BluePoint.CTXR;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaNodeData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using AquaModelLibrary.Data.Utility;
using AquaModelLibrary.Helpers.MathHelpers;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Core.BluePoint
{
    public class BluePointConvert
    {
        public static void ReadFileTest(string filePath, out int start)
        {
            using (MemoryStream stream = new MemoryStream(File.ReadAllBytes(filePath)))
            using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                start = streamReader.Read<ushort>();
            }
        }

        public static List<byte> ReadFileTestVertDef(string filePath)
        {
            List<byte> bytes = new List<byte>();
            using (MemoryStream stream = new MemoryStream(File.ReadAllBytes(filePath)))
            using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                bool kill = false;
                while (true && streamReader.Position < stream.Length && kill == false)
                {
                    var posint1 = streamReader.Read<ushort>();
                    var posint2 = streamReader.Read<byte>();
                    if (posint1 == 0x4F53 && posint2 == 0x50)
                    {
                        while (true && streamReader.Position < stream.Length)
                        {
                            var int1 = streamReader.Read<ushort>();
                            var int2 = streamReader.Read<byte>();
                            streamReader.Seek(-3, SeekOrigin.Current);
                            if ((int1 == 0x524D && int2 == 0x4E) || (int1 == 0x4558 && int2 == 0x54))
                            {
                                kill = true;
                                break;
                            }
                            bytes.Add(streamReader.Read<byte>());
                        }
                    }
                    streamReader.Seek(-2, SeekOrigin.Current);
                }
            }

            return bytes;
        }

        /// <summary>
        /// Takes a path for a CMDL or CMSH and does the legwork to convert either and their textures to .fbx and .dds
        /// </summary>
        public static void ConvertCMDLCMSH(string filePath)
        {
            string outPath = "";
            List<string> outNames = new List<string>();
            Dictionary<string, CMAT> materialDict = new Dictionary<string, CMAT>();
            List<string> cmshPaths = new List<string>();
            List<string> uniqueTexNames = new List<string>();

            string rootPath = PSUtility.GetPSRootPath(filePath);
            if (filePath.EndsWith(".cmdl"))
            {
                var cmdl = new CMDL(File.ReadAllBytes(filePath));
                var cmatMap = cmdl.GetCMATMaterialMap();
                foreach (var cmatSet in cmatMap)
                {
                    var cmatPath = Path.Combine(rootPath, cmatSet.Value.Substring(2)).Replace("/", "\\");
                    var cmatPathCmn = cmatPath.Replace("****", "_cmn");
                    var cmatPathPs5 = cmatPath.Replace("****", "_ps5");
                    var cmatPathPs4 = cmatPath.Replace("****", "_ps4");
                    if (File.Exists(cmatPathCmn))
                    {
                        cmatPath = cmatPathCmn;
                    }
                    else if (File.Exists(cmatPathPs5))
                    {
                        cmatPath = cmatPathPs5;
                    }
                    else if (File.Exists(cmatPathPs4))
                    {
                        cmatPath = cmatPathPs4;
                    }

                    if (File.Exists(cmatPath))
                    {
                        var cmat = new CMAT(File.ReadAllBytes(cmatPath));
                        materialDict.Add(cmatSet.Key, cmat);

                        foreach (var texName in cmat.texNames)
                        {
                            if (!uniqueTexNames.Contains(texName))
                            {
                                uniqueTexNames.Add(texName);
                            }
                        }
                    }
                    else
                    {
                        materialDict.Add(cmatSet.Key, null);
                    }
                }
                foreach (var cmshPartialPath in cmdl.GetCMeshReferences())
                {
                    cmshPaths.Add(Path.Combine(rootPath, cmshPartialPath.Substring(2).Replace("****", "_cmn")).Replace("/", "\\"));
                    outNames.Add(Path.GetFileName(cmshPartialPath));
                }

                outPath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath));
                Directory.CreateDirectory(outPath);
            }
            else if (filePath.EndsWith(".cmsh"))
            {
                outPath = Path.GetDirectoryName(filePath);
                outNames.Add(Path.GetFileName(filePath));
                cmshPaths.Add(filePath);
            }
            else
            {
                return;
            }

            //Dump textures
            Parallel.ForEach(uniqueTexNames, texName =>
            {
                var ctxrPath = Path.Combine(rootPath, texName.Substring(2)).Replace("/", "\\");
                var cmatPathCmn = ctxrPath.Replace("****", "_cmn");
                var cmatPathPs5 = ctxrPath.Replace("****", "_ps5");
                var cmatPathPs4 = ctxrPath.Replace("****", "_ps4");
                if (File.Exists(cmatPathCmn))
                {
                    ctxrPath = cmatPathCmn;
                }
                else if (File.Exists(cmatPathPs5))
                {
                    ctxrPath = cmatPathPs5;
                }
                else if (File.Exists(cmatPathPs4))
                {
                    ctxrPath = cmatPathPs4;
                }

                if (File.Exists(ctxrPath))
                {
                    var ctxr = new CTXR(File.ReadAllBytes(ctxrPath));
                    ctxr.WriteToDDS(ctxrPath, Path.Combine(outPath, Path.GetFileName(ctxrPath).Replace(".ctxr", ".dds")));
                }
            });

            //Convert from CMSH paths
            for(int i = 0; i < cmshPaths.Count; i++)
            {
                var mshPath = cmshPaths[i];
                var outName = outNames[i];

                if (File.Exists(mshPath))
                {
                    var aqp = ConvertCMSH(mshPath, materialDict, out var aqn);
                    if (aqp != null)
                    {
                        aqp.ConvertToPSO2Model(true, false, false, true, false, false, false, true);
                        aqp.ConvertToLegacyTypes();
                        aqp.CreateTrueVertWeights();

                        FbxExporterNative.ExportToFile(aqp, aqn, new List<AquaMotion>(), Path.Combine(outPath, Path.ChangeExtension(outName, ".fbx")), new List<string>(), new List<Matrix4x4>(), false, (int)CoordSystem.OpenGL);
                    }
                }
            }

        }

        public static void ConvertCTXR(string ctxrPath)
        {
            if (File.Exists(ctxrPath))
            {
                var ctxr = new CTXR(File.ReadAllBytes(ctxrPath));
                ctxr.WriteToDDS(ctxrPath, Path.Combine(Path.GetDirectoryName(ctxrPath), Path.GetFileName(ctxrPath).Replace(".ctxr", ".dds")));
            }
        }

        /// <summary>
        /// Takes in a CMSH path, a dictionary of CMATs, and outputs an AquaObject and AquaNode
        /// </summary>
        public static AquaObject ConvertCMSH(string filePath, Dictionary<string, CMAT> materialDict, out AquaNode aqn)
        {
            string modelPath = Path.GetDirectoryName(filePath);
            var aboveModelPath = Path.GetDirectoryName(modelPath);
            string cmtlPath = null;
            if (aboveModelPath != null)
            {
                cmtlPath = Path.Combine(aboveModelPath, "materials", "_cmn");
            }
            var cmshBytes = File.ReadAllBytes(filePath);
            var cmsh = new CMSH(cmshBytes);
            //cmsh.vertData.vertDefs.RemoveAt(1);
            //cmsh.vertData.normalTemp.Clear();
            /*
            for (int i = 0; i < cmsh.vertData.qut0List.Count; i++)
            {
                uint qut0, newQut0;
                qut0 = cmsh.vertData.qut0List[i];
                CMSHVertexData.UnpackQUT0(qut0, out var nrm, out var tan, out var bit);
                cmsh.vertData.qut0List[i] = newQut0 = CMSHVertexData.PackQUT0(nrm, tan);
            }
            var cmshOut = cmsh.GetBytes();*/
            //File.WriteAllBytes(filePath, cmshOut);
            return CMDLToAqua(cmsh, materialDict, cmtlPath, modelPath, out aqn);
        }

        /// <summary>
        /// Converts an AquaObject to a CMSH
        /// </summary>
        public static CMSH ConvertToDeSRCMSH(AquaObject aqo, AquaNode aqn, bool isRigid, bool isCloth)
        {
            CMSH cmsh = new();
            cmsh.header = new CMSHHeader();
            cmsh.header.isDeSR = true;
            cmsh.header.meshCount = aqo.meshList.Count;
            cmsh.header.variantFlag2 = 0x2;

            CMSHVertexData vertData = new();
            CMSHFaceData faceData = new();

            cmsh.vertData = vertData;
            cmsh.faceData = faceData;

            List<VertexMagic> magics = new();
            for (int i = 0; i < aqo.vtxlList.Count; i++)
            {
                var vtxl = aqo.vtxlList[i];
                if (vtxl.vertPositions.Count > 0 && !magics.Contains(VertexMagic.POS0))
                {
                    magics.Add(VertexMagic.POS0);
                    vertData.vertDefs.Add(new CMSHVertexDataDefinition() { dataMagic = VertexMagic.POS0});
                }
                if (vtxl.vertNormals.Count > 0 && !magics.Contains(VertexMagic.QUT0))
                {
                    magics.Add(VertexMagic.QUT0);
                    vertData.vertDefs.Add(new CMSHVertexDataDefinition() { dataMagic = VertexMagic.QUT0 });
                }
                if (vtxl.uv1List.Count > 0 && !magics.Contains(VertexMagic.TEX0))
                {
                    magics.Add(VertexMagic.TEX0);
                    vertData.vertDefs.Add(new CMSHVertexDataDefinition() { dataMagic = VertexMagic.TEX0 });
                }
                if (vtxl.uv2List.Count > 0 && !magics.Contains(VertexMagic.TEX1))
                {
                    magics.Add(VertexMagic.TEX1);
                    vertData.vertDefs.Add(new CMSHVertexDataDefinition() { dataMagic = VertexMagic.TEX1 });
                }
                if (vtxl.uv3List.Count > 0 && !magics.Contains(VertexMagic.TEX2))
                {
                    magics.Add(VertexMagic.TEX2);
                    vertData.vertDefs.Add(new CMSHVertexDataDefinition() { dataMagic = VertexMagic.TEX2 });
                }
                if (vtxl.uv4List.Count > 0 && !magics.Contains(VertexMagic.TEX3))
                {
                    magics.Add(VertexMagic.TEX3);
                    vertData.vertDefs.Add(new CMSHVertexDataDefinition() { dataMagic = VertexMagic.TEX3 });
                }
                if (vtxl.vertColors.Count > 0 && !magics.Contains(VertexMagic.COL0))
                {
                    magics.Add(VertexMagic.COL0);
                    vertData.vertDefs.Add(new CMSHVertexDataDefinition() { dataMagic = VertexMagic.COL0 });
                }
                if (vtxl.vertColor2s.Count > 0 && !magics.Contains(VertexMagic.COL1))
                {
                    magics.Add(VertexMagic.COL1);
                    vertData.vertDefs.Add(new CMSHVertexDataDefinition() { dataMagic = VertexMagic.COL1 });
                }
                /*
                if (vtxl.vertColor3s.Count > 0 && !magics.Contains(VertexMagic.COL2))
                {
                    magics.Add(VertexMagic.COL2);
                }
                */

                if (!isRigid)
                {
                    if (vtxl.vertWeightIndices.Count > 0 && !magics.Contains(VertexMagic.BONI))
                    {
                        magics.Add(VertexMagic.BONI);
                    }
                    if (vtxl.vertWeights.Count > 0 && !magics.Contains(VertexMagic.BONW))
                    {
                        magics.Add(VertexMagic.BONW);
                    }
                }
            }

            int nextStartingFaceIndex = 0;
            int nextVertIndex = 0;
            for(int i = 0; i < aqo.meshList.Count; i++)
            {
                var aqoMesh = aqo.meshList[i];
                var vtxl = aqo.vtxlList[aqoMesh.vsetIndex];
                var strips = aqo.strips[aqoMesh.psetIndex];
                var mat = aqo.mateList[aqoMesh.mateIndex];

                var meshRef = new CMSHMeshReference();
                cmsh.header.meshList.Add(meshRef);
                meshRef.matName = mat.matName.GetString();
                meshRef.startingFaceIndex = nextStartingFaceIndex;

                //Get bounding data and face indices
                var triangles = strips.GetTriangles();
                int faceIndexCount = 0;
                for(int j = 0; j < triangles.Count; j++)
                {
                    var face = triangles[j];
                    faceData.faceList.Add(new Data.DataTypes.Vector3Int.Vec3Int((int)face.X + nextVertIndex, (int)face.Y + nextVertIndex, (int)face.Z + nextVertIndex));
                    faceIndexCount += 3;

                    List<Vector3> vertices = new List<Vector3>();
                    vertices.Add(vtxl.vertPositions[(int)face.X]);
                    vertices.Add(vtxl.vertPositions[(int)face.Y]);
                    vertices.Add(vtxl.vertPositions[(int)face.Z]);

                    foreach(var vertex in vertices)
                    {
                        meshRef.minBounding = Vector3.Min(vertex, meshRef.minBounding);
                        meshRef.maxBounding = Vector3.Max(vertex, meshRef.maxBounding);
                    }
                }

                //Get vertex data
                foreach(var magic in magics)
                {
                    switch(magic)
                    {
                        case VertexMagic.POS0:
                            vertData.positionList.AddRange(vtxl.vertPositions);
                            break;
                        case VertexMagic.TEX0:
                            AddUvList(vertData, vtxl, 0);
                            break;
                        case VertexMagic.TEX1:
                            AddUvList(vertData, vtxl, 1);
                            break;
                        case VertexMagic.TEX2:
                            AddUvList(vertData, vtxl, 2);
                            break;
                        case VertexMagic.TEX3:
                            AddUvList(vertData, vtxl, 3);
                            break;
                        case VertexMagic.COL0:
                            AddColList(vertData, vtxl, 0);
                            break;
                        case VertexMagic.COL1:
                            AddColList(vertData, vtxl, 1);
                            break;
                        case VertexMagic.BONI:
                            vertData.vertWeightIndices.AddRange(vtxl.vertWeightIndices.ConvertAll(wt => (int[])wt.Clone()));
                            break;
                        case VertexMagic.BONW:
                            vertData.vertWeights.AddRange(vtxl.vertWeights);
                            break;
                    }
                }
                nextVertIndex = vertData.positionList.Count;
                nextStartingFaceIndex += faceIndexCount;
                meshRef.faceIndexCount = faceIndexCount;
            }

            //Generate QUT0
            CMSHVertexData.GenerateNormalsAndTangents(vertData.positionList, faceData.faceList, vertData.uvDict[VertexMagic.TEX0], out var normals, out var tangents);
            vertData.CreateQUT0List(normals, tangents);

            //Get sizefloat and Surface Area Table data
            cmsh.header.sizeFloat = cmsh.vertData.GetSizeFloat(cmsh.faceData.faceList, out var satValues);

            if(isCloth)
            {
                vertData.satValues = satValues;
                vertData.vertDefs.Add(new CMSHVertexDataDefinition() { dataMagic = VertexMagic.SAT_ });
            }

            cmsh.vertData.uvDict.Remove(VertexMagic.TEX1);
            cmsh.vertData.uvDict.Remove(VertexMagic.TEX2);
            cmsh.vertData.uvDict.Remove(VertexMagic.TEX3);
            cmsh.vertData.colorDict.Remove(VertexMagic.COL0);
            cmsh.vertData.vertDefs.RemoveAt(5);
            cmsh.vertData.vertDefs.RemoveAt(4);
            cmsh.vertData.vertDefs.RemoveAt(3);
            return cmsh;
        }

        private static void AddUvList(CMSHVertexData vertData, VTXL vtxl, int listNum)
        {
            List<Vector2> uvList;
            VertexMagic uvMagic;
            switch(listNum)
            {
                case 0:
                    uvList = vtxl.uv1List;
                    uvMagic = VertexMagic.TEX0;
                    break;
                case 1:
                    uvList = vtxl.uv2List;
                    uvMagic = VertexMagic.TEX1;
                    break;
                case 2:
                    uvList = vtxl.uv3List;
                    uvMagic = VertexMagic.TEX2;
                    break;
                case 3:
                    uvList = vtxl.uv4List;
                    uvMagic = VertexMagic.TEX3;
                    break;
                default:
                    throw new Exception("Unexpected uv list!");
            }
            List<Vector2> listData;
            if (uvList.Count > 0)
            {
                listData = uvList.ToArray().ToList();
            }
            else
            {
                List<Vector2> newUvList = new();
                for (int v = 0; v < vtxl.vertPositions.Count; v++)
                {
                    newUvList.Add(new Vector2());
                }
                listData = newUvList;
            }

            if(vertData.uvDict.ContainsKey(uvMagic))
            {
                vertData.uvDict[uvMagic].AddRange(listData);
            } else
            {
                vertData.uvDict[uvMagic] = listData;
            }
        }

        private static void AddColList(CMSHVertexData vertData, VTXL vtxl, int listNum)
        {
            List<byte[]> colList;
            VertexMagic colMagic;
            switch (listNum)
            {
                case 0:
                    colList = vtxl.vertColors;
                    colMagic = VertexMagic.COL0;
                    break;
                case 1:
                    colList = vtxl.vertColor2s;
                    colMagic = VertexMagic.COL1;
                    break;
                    /*
                case 2:
                    colList = vtxl.uv2List;
                    uvMagic = VertexMagic.COL2;
                    break;
                    */
                default:
                    throw new Exception("Unexpected color list!");
            }

            List<byte[]> listData;
            if (colList.Count > 0)
            {
                listData = colList.ConvertAll(clr => (byte[])clr.Clone()).ToList();
            }
            else
            {
                List<byte[]> newColList = new();
                for (int v = 0; v < vtxl.vertPositions.Count; v++)
                {
                    newColList.Add([0xFF, 0xFF, 0xFF, 0xFF]);
                }
                listData = newColList;
            }

            if (vertData.colorDict.ContainsKey(colMagic))
            {
                vertData.colorDict[colMagic].AddRange(listData);
            }
            else
            {
                vertData.colorDict[colMagic] = listData;
            }
        }

        public static CANI ConvertCANI(string filePath)
        {
            return new CANI(File.ReadAllBytes(filePath));
        }

        public static AquaObject CMDLToAqua(CMSH msh, Dictionary<string, CMAT> materialDict, string cmtlPath, string modelPath, out AquaNode aqn)
        {
            List<string> objList = new List<string>();
            if (msh.header == null || msh.header.variantFlag2 == 0x41)
            {
                aqn = null;
                return null;
            }
            var csklPath = "";
            CSKL cskl = null;
            if (msh.boneData != null && msh.boneData.skeletonPath != null)
            {
                csklPath = Path.Combine(modelPath, Path.GetFileName(msh.boneData.skeletonPath));
                if (File.Exists(csklPath))
                {
                    cskl = new CSKL(File.ReadAllBytes(csklPath));
                }
                else
                {
                    if (modelPath.Contains("-app0"))
                    {
                        string rootPath = PSUtility.GetPSRootPath(modelPath);
                        csklPath = Path.Combine(rootPath, msh.boneData.skeletonPath.Substring(2).Replace("****", "_cmn")).Replace("/", "\\");
                        cskl = new CSKL(File.ReadAllBytes(csklPath));
                    }
                }
            }

            aqn = AquaNode.GenerateBasicAQN();
            AquaObject aqp = new AquaObject();
            if (cskl == null && msh.boneData == null)
            {
                aqp.bonePalette.Add((uint)0);
            }
            else if (cskl == null && msh.boneData != null)
            {
                aqn.nodeList.Clear();
                for (int i = 0; i < msh.boneData.boneNames.Count; i++)
                {
                    aqp.bonePalette.Add((uint)i);

                    //Try to make a skeleton from incomplete bone data. All we really get is a vector4 that seems like it *might* be a rotation? Can't do too much with this, but we'll put it in.
                    NODE aqNode = new NODE();
                    aqNode.boneShort1 = 0x1C0;
                    aqNode.animatedFlag = 1;
                    aqNode.parentId = i - 1;
                    aqNode.unkNode = -1;

                    aqNode.scale = new Vector3(1, 1, 1);

                    var matrix = Matrix4x4.Identity;
                    matrix *= Matrix4x4.CreateScale(1, 1, 1);
                    matrix *= Matrix4x4.CreateTranslation(new Vector3());
                    Matrix4x4.Invert(matrix, out var invMat);

                    aqNode.SetInverseBindPoseMatrix(invMat);
                    aqNode.boneName.SetString(msh.boneData.boneNames[i]);
                    aqn.nodeList.Add(aqNode);
                }
            }
            else
            {
                for (int i = 0; i < cskl.header.boneCount; i++)
                {
                    aqp.bonePalette.Add((uint)i);
                }
                aqn = new AquaNode();

                for (int i = 0; i < cskl.header.boneCount; i++)
                {
                    var metadata = cskl.metadata.familyIds[i];
                    var parentId = metadata.parentId;

                    var invMat = cskl.invWorldTransforms[i];
                    var tfm = cskl.transforms[i];

                    //Create AQN node
                    NODE aqNode = new NODE();
                    aqNode.boneShort1 = 0x1C0;
                    aqNode.animatedFlag = 1;
                    aqNode.parentId = parentId;
                    aqNode.unkNode = -1;

                    aqNode.pos = tfm.position;
                    aqNode.eulRot = MathExtras.QuaternionToEuler(tfm.rotation);
                    aqNode.scale = new Vector3(1, 1, 1);

                    aqNode.m1 = new Vector4(invMat.M11, invMat.M12, invMat.M13, invMat.M14);
                    aqNode.m2 = new Vector4(invMat.M21, invMat.M22, invMat.M23, invMat.M24);
                    aqNode.m3 = new Vector4(invMat.M31, invMat.M32, invMat.M33, invMat.M34);
                    aqNode.m4 = new Vector4(invMat.M41, invMat.M42, invMat.M43, invMat.M44);
                    aqNode.boneName.SetString(cskl.names.primaryNames.names[i].Split('|').Last());
                    aqn.nodeList.Add(aqNode);
                }
            }

            var mesh = msh;

            var nodeMatrix = Matrix4x4.Identity;

            //Vert data
            var vertCount = mesh.vertData.positionList.Count;
            VTXL vtxl = new VTXL();
            if (mesh.vertData.qut0List.Count > 0)
            {
                mesh.vertData.GetQut0Data(out var nrmList, out var tanList, out var bitList);
                vtxl.vertNormals.AddRange(nrmList);
                //vtxl.vertTangentList.AddRange(tanList);
                //vtxl.vertBinormalList.AddRange(bitList);
            }

            for (int v = 0; v < vertCount; v++)
            {
                vtxl.vertPositions.Add(mesh.vertData.positionList[v]);

                //UVs
                if (mesh.vertData.uvDict.ContainsKey(VertexMagic.TEX0))
                {
                    var uv1 = mesh.vertData.uvDict[VertexMagic.TEX0][v];
                    vtxl.uv1List.Add(new Vector2(uv1.X, uv1.Y));
                }
                if (mesh.vertData.uvDict.ContainsKey(VertexMagic.TEX1))
                {
                    var uv2 = mesh.vertData.uvDict[VertexMagic.TEX1][v];
                    vtxl.uv2List.Add(new Vector2(uv2.X, uv2.Y));
                }
                if (mesh.vertData.uvDict.ContainsKey(VertexMagic.TEX2))
                {
                    var uv3 = mesh.vertData.uvDict[VertexMagic.TEX2][v];
                    vtxl.uv3List.Add(new Vector2(uv3.X, uv3.Y));
                }
                if (mesh.vertData.uvDict.ContainsKey(VertexMagic.TEX3))
                {
                    var uv4 = mesh.vertData.uvDict[VertexMagic.TEX3][v];
                    vtxl.uv4List.Add(new Vector2(uv4.X, uv4.Y));
                }
                //Vert Colors
                if (mesh.vertData.colorDict.ContainsKey(VertexMagic.COL0))
                {
                    var colors = mesh.vertData.colorDict[VertexMagic.COL0];
                    vtxl.vertColors.Add(colors[v]);
                } else if (mesh.vertData.colorDict.ContainsKey(VertexMagic.COL1)) //IN theory we should never have COL0; observed models only use COL1 and 2. But just in case...
                {
                    var colors = mesh.vertData.colorDict[VertexMagic.COL1];
                    vtxl.vertColors.Add(colors[v]);
                }
                if (mesh.vertData.colorDict.ContainsKey(VertexMagic.COL2))
                {
                    var colors = mesh.vertData.colorDict[VertexMagic.COL2];
                    vtxl.vertColor2s.Add(colors[v]);
                }

                if (mesh.vertData.vertWeights.Count > 0)
                {
                    vtxl.vertWeights.Add(mesh.vertData.vertWeights[v]);
                    vtxl.vertWeightIndices.Add(mesh.vertData.vertWeightIndices[v]);
                }
                else if (mesh.vertData.vertWeightIndices.Count > 0)
                {
                    vtxl.vertWeights.Add(new Vector4(1, 0, 0, 0));
                    vtxl.vertWeightIndices.Add(new int[] { mesh.vertData.vertWeightIndices[v][0], 0, 0, 0 });
                }
            }

            vtxl.convertToLegacyTypes();
            aqp.vtxeList.Add(VTXE.ConstructFromVTXL(vtxl, out int vc));

            //Face data

            //Do by vertex order if there's no faces
            if (mesh.faceData.faceList.Count == 0)
            {
                int faceCount = mesh.vertData.positionList.Count;
                for (int i = 0; i < faceCount; i += 3)
                {
                    mesh.faceData.faceList.Add(AquaModelLibrary.Data.DataTypes.Vector3Int.Vec3Int.CreateVec3Int(i, i + 1, i + 2));
                }

                //Assume mat face stuff is bad
                mesh.header.meshList[0].startingFaceIndex = 0;
                mesh.header.meshList[0].startingFaceVertIndex = 0;
                mesh.header.meshList[0].faceIndexCount = mesh.faceData.faceList.Count * 6;
                mesh.header.meshList[0].faceVertIndicesUsed = mesh.faceData.faceList.Count * 3;

                for (int i = 1; i < mesh.header.meshList.Count; i++)
                {
                    mesh.header.meshList[i].startingFaceIndex = 0;
                    mesh.header.meshList[i].startingFaceVertIndex = 0;
                    mesh.header.meshList[i].faceIndexCount = 0;
                    mesh.header.meshList[i].faceVertIndicesUsed = 0;
                }
            }

            //Split CMSH by materials. Materials seem to contain a face count after which they split
            int currentFace = 0;
            for (int m = 0; m < mesh.header.meshList.Count; m++)
            {
                objList.Add($"g Mesh_{m}");

                int startFace;
                int faceCount;
                if (mesh.header.isDeSR) //DeSR
                {
                    startFace = mesh.header.meshList[m].startingFaceIndex / 3;
                    faceCount = mesh.header.meshList[m].faceIndexCount / 3;
                }
                else //SOTC
                {
                    startFace = mesh.header.meshList[m].startingFaceVertIndex / 3;
                    faceCount = mesh.header.meshList[m].faceVertIndicesUsed / 3;
                }

                //Sometimes BluePoint's optimization led to degenerate faces, so we skip
                if (faceCount == 0 && mesh.header.meshList[m].faceIndexCount > 1)
                {
                    continue;
                }

                if ((mesh.header.isDeSR && mesh.header.meshList[m].startingFaceIndex <= 0 && mesh.header.meshList[m].faceIndexCount <= 0)
                    || (!mesh.header.isDeSR && mesh.header.meshList[m].startingFaceVertIndex <= 0 && mesh.header.meshList[m].faceVertIndicesUsed <= 0))
                {
                    startFace = currentFace;
                    faceCount = mesh.faceData.faceList.Count - currentFace;
                }
                currentFace = startFace + faceCount;


                var baseMatName = mesh.header.meshList[m];

                string texName = "test_d.dds";

                //This should be the primary way to get the material, but fall back to bruteforcing a bit if that fails
                if (materialDict.ContainsKey(baseMatName.matName))
                {
                    var cmat = materialDict[baseMatName.matName];
                    texName = GetTexNameFromCMAT(texName, cmat);
                }
                else
                {
                    var matFileName = baseMatName.matName.Replace("_mat1", "");
                    matFileName = matFileName.Replace("_mat", "");
                    if (cmtlPath != null)
                    {
                        var matPath = Path.Combine(cmtlPath, matFileName + ".cmat");
                        var backupMatPath = Path.Combine(modelPath, matFileName + ".cmat");
                        if (File.Exists(matPath))
                        {
                            var cmat = new CMAT(File.ReadAllBytes(matPath));
                            texName = GetTexNameFromCMAT(texName, cmat);
                        }
                        else if (File.Exists(backupMatPath))
                        {
                            var cmat = new CMAT(File.ReadAllBytes(backupMatPath));
                            if (cmat.texNames.Count > 0)
                            {
                                texName = cmat.texNames[0];
                            }
                        }
                    }
                }

                texName = Path.GetFileName(texName).Replace(".ctxr", ".dds");

                //Material
                var mat = new GenericMaterial();
                mat.matName = $"{mesh.header.meshList[m].matName}";
                mat.texNames = new List<string>
                {
                    texName
                };
                aqp.tempMats.Add(mat);

                Dictionary<int, int> vertIdDict = new Dictionary<int, int>();
                VTXL matVtxl = new VTXL();
                GenericTriangles genMesh = new GenericTriangles();
                List<Vector3> triList = new List<Vector3>();
                for (int f = startFace; f < (startFace + faceCount); f++)
                {
                    var tri = mesh.faceData.faceList[f];

                    int x;
                    int y;
                    int z;
                    if (vertIdDict.TryGetValue(tri.X, out var value))
                    {
                        x = value;
                    }
                    else
                    {
                        vertIdDict.Add(tri.X, matVtxl.vertPositions.Count);
                        x = matVtxl.vertPositions.Count;
                        VTXL.AppendVertex(vtxl, matVtxl, tri.X);
                    }
                    if (vertIdDict.TryGetValue(tri.Y, out var value2))
                    {
                        y = value2;
                    }
                    else
                    {
                        vertIdDict.Add(tri.Y, matVtxl.vertPositions.Count);
                        y = matVtxl.vertPositions.Count;
                        VTXL.AppendVertex(vtxl, matVtxl, tri.Y);
                    }
                    if (vertIdDict.TryGetValue(tri.Z, out var value3))
                    {
                        z = value3;
                    }
                    else
                    {
                        vertIdDict.Add(tri.Z, matVtxl.vertPositions.Count);
                        z = matVtxl.vertPositions.Count;
                        VTXL.AppendVertex(vtxl, matVtxl, tri.Z);
                    }

                    //Avoid degen tris
                    if (x == y || x == z || y == z)
                    {
                        continue;
                    }
                    triList.Add(new Vector3(x, y, z));
                    objList.Add($"f {x + 1} {y + 1} {z + 1}");
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
                }
            }

            return aqp;
        }

        private static string GetTexNameFromCMAT(string texName, CMAT cmat)
        {
            if (cmat.texNames.Count > 0)
            {
                texName = cmat.texNames[0];
                //If we can find it, make the albedo the texture
                foreach (var tex in cmat.texNames)
                {
                    if (tex.Contains("_col."))
                    {
                        texName = tex;
                        break;
                    }
                }
            }

            return texName;
        }
    }
}
