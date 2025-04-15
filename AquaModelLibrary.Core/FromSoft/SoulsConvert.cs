﻿using AquaModelLibrary.Core.General;
using AquaModelLibrary.Core.Morpheme;
using AquaModelLibrary.Data.FromSoft;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaNodeData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using AquaModelLibrary.Data.Utility;
using AquaModelLibrary.Data.Utility.Model;
using AquaModelLibrary.Helpers.MathHelpers;
using SoulsFormats;
using SoulsFormats.Formats.Morpheme;
using SoulsFormats.Formats.Morpheme.NSA;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Quaternion = System.Numerics.Quaternion;

namespace AquaModelLibrary.Core.FromSoft
{
    public static class SoulsConvert
    {
        public static bool useMetaData = false;
        public static bool applyMaterialNamesToMesh = false;
        public static bool transformMesh = false;
        public static bool extractUnreferencedMapData = false;
        public static bool separateMSBDumpByModel = false;
        public static bool doNotAdjustRootRotation = false;
        public static bool addFBXRootNode = true;
        public static bool addFlverDummies = false;
        public static bool parentDummiesToAttachNodes = true;

        public static SoulsGame game = SoulsGame.None;
        public static ExportFormat exportFormat = ExportFormat.Fbx;
        public static MirrorType mirrorType = MirrorType.Z;
        public static CoordSystem coordSystem = CoordSystem.Max;

        public class GameFile
        {
            public byte[] data = null;
            public string fileName = "";
        }

        public static SoulsGame GetGameEnum(string gameExePath)
        {
            var gameExe = Path.GetFileName(gameExePath).ToLower();
            if (gameExe == "eboot.bin")
            {
                var path = Path.GetDirectoryName(gameExePath);
                if (Directory.Exists($@"{path}\dvdroot_ps4"))
                {
                    return SoulsGame.Bloodborne;
                }
                else
                {
                    return SoulsGame.DemonsSouls;
                }
            }
            switch (gameExe)
            {
                case "darksoulsremastered.exe":
                    return SoulsGame.DarkSouls1Remastered;
                case "darksouls.exe":
                    return SoulsGame.DarkSouls1;
                case "darksoulsii.exe":
                    return SoulsGame.DarkSouls2;
                case "darksoulsiii.exe":
                    return SoulsGame.DarkSouls3;
                case "sekiro.exe":
                    return SoulsGame.Sekiro;
                case "eldenring.exe":
                    return SoulsGame.EldenRing;
                case "armoredcore6.exe":
                    return SoulsGame.ArmoredCore6;
            }

            return SoulsGame.None;
        }

        public static Matrix4x4 mirrorMatX = new Matrix4x4(-1, 0, 0, 0,
                                                            0, 1, 0, 0,
                                                            0, 0, 1, 0,
                                                            0, 0, 0, 1);

        public static Matrix4x4 mirrorMatY = new Matrix4x4(1, 0, 0, 0,
                                                            0, -1, 0, 0,
                                                            0, 0, 1, 0,
                                                            0, 0, 0, 1);

        public static Matrix4x4 mirrorMatZ = new Matrix4x4(1, 0, 0, 0,
                                                            0, 1, 0, 0,
                                                            0, 0, -1, 0,
                                                            0, 0, 0, 1);
        public static Matrix4x4 mirrorMat = mirrorMatZ;

        /// <summary>
        /// Purely for testing purposes
        /// </summary>
        public static void ReadSoulsFile(string filePath)
        {
            JsonSerializerOptions jss = new JsonSerializerOptions() { WriteIndented = true };
            var raw = File.ReadAllBytes(filePath);
            if (filePath.EndsWith(".mcg"))
            {
                var mcg = SoulsFormats.SoulsFile<SoulsFormats.MCG>.Read(raw);
                string mcgData = JsonSerializer.Serialize(mcg, jss);
                File.WriteAllText(filePath + ".json", mcgData);
                int maxA = 0;
                int maxB = 0;
                Debug.WriteLine($"mcg edge count == {mcg.Edges.Count}");
                Debug.WriteLine($"mcg node count == {mcg.Nodes.Count}");
                string edgesText = "";
                for (int i = 0; i < mcg.Edges.Count; i++)
                {
                    edgesText += $"Edge {i}\n";
                    edgesText += $"  Unk Indices A:\n    ";
                    for (int j = 0; j < mcg.Edges[i].UnkIndicesA.Count; j++)
                    {
                        edgesText += $"{mcg.Edges[i].UnkIndicesA[j]}, ";
                        maxA = Math.Max(maxA, mcg.Edges[i].UnkIndicesA[j]);
                    }
                    edgesText += "\n";
                    edgesText += $"  Unk Indices B:\n    ";
                    for (int j = 0; j < mcg.Edges[i].UnkIndicesB.Count; j++)
                    {
                        edgesText += $"{mcg.Edges[i].UnkIndicesB[j]}, ";
                        maxB = Math.Max(maxB, mcg.Edges[i].UnkIndicesB[j]);
                    }
                    edgesText += "\n";
                }
                Debug.Write(edgesText);
                Debug.WriteLine($"Max A: {maxA}\nMax B: {maxB}");
                Debug.WriteLine($"Unk04 {mcg.Unk04} {mcg.Unk04:X}");
                Debug.WriteLine($"Unk18 {mcg.Unk18} {mcg.Unk18:X}");
                Debug.WriteLine($"Unk1C {mcg.Unk1C} {mcg.Unk1C:X}");
            }
            else if (filePath.EndsWith(".mcp"))
            {
                var mcp = SoulsFormats.SoulsFile<SoulsFormats.MCP>.Read(raw);
                string mcpData = JsonSerializer.Serialize(mcp, jss);
                File.WriteAllText(filePath + ".json", mcpData);
                Dictionary<uint, List<MCP.Room>> rooms = new Dictionary<uint, List<MCP.Room>>();
                foreach (var room in mcp.Rooms)
                {
                    if (!rooms.ContainsKey(room.MapID))
                    {
                        rooms[room.MapID] = new List<MCP.Room>();
                    }
                    else
                    {
                        rooms[room.MapID].Add(room);
                    }
                }
                StringBuilder sb = new StringBuilder();
                var keys = rooms.Keys.ToList();
                keys.Sort();
                foreach (var key in keys)
                {
                    sb.AppendLine(key.ToString("X"));
                    var roomList = rooms[key];
                    for (int i = 0; i < roomList.Count; i++)
                    {
                        sb.AppendLine(roomList[i].LocalIndex.ToString());
                    }
                }
                Debug.Write(sb.ToString());
            }
            else if (filePath.EndsWith(".msb"))
            {
                if (BitConverter.ToUInt32(raw, 4) == 0xFFFFFFFF)
                {
                    var msb = SoulsFormats.SoulsFile<MSBDR>.Read(raw);
                }
                else
                {
                    var msb = SoulsFormats.SoulsFile<MSBD>.Read(raw);
                }
            }
            else if (filePath.EndsWith(".drb"))
            {
                var drb = DRB.Read(raw, DRB.DRBVersion.DarkSouls);

                /*
                DRB.Control.Static control = new DRB.Control.Static();
                control.Unk00 = 0;
                drb.Textures.Add(new DRB.Texture() {Name = "MapShot_03", Path = "N:\\DemonsSoul\\data\\Menu\\Texture\\MapShot\\MapShot_03.tga" });

                short TexBottomEdge = 512;
                short TexRightEdge = 1024;
                short TexTopEdge = 256;
                short TexLeftEdge = 512;
                short textureId = 2;
                for (int i = 0; i < 8; i++)
                {
                    //Check if we need to go to the next column
                    if(TexBottomEdge > 1024)
                    {
                        TexBottomEdge = 256;
                        TexTopEdge = 0;
                        //Check if we need to go to a new texture sheet
                        if (TexRightEdge == 1024)
                        {
                            TexRightEdge = 512;
                            TexLeftEdge = 0;
                            textureId += 1;
                        } else //If not, go to the next column
                        {
                            TexRightEdge += 512;
                            TexLeftEdge += 512;
                        }
                    }
                    var dlgo70 = new DRB.Dlgo();
                    dlgo70.Control = control;
                    dlgo70.Name = $"mapshot_7_{i}";
                    DRB.Shape.Sprite dlgo70Shape = new DRB.Shape.Sprite();
                    dlgo70Shape.BlendMode = DRB.Shape.BlendingMode.Alpha;
                    dlgo70Shape.CustomColor = System.Drawing.Color.White;
                    dlgo70Shape.Orientation = DRB.Shape.SpriteOrientation.None;
                    dlgo70Shape.PaletteColor = 0;
                    dlgo70Shape.ScalingOriginX = -1;
                    dlgo70Shape.ScalingOriginY = -1;
                    dlgo70Shape.ScalingMode = 0;
                    dlgo70Shape.TextureIndex = textureId;

                    dlgo70Shape.BottomEdge = 256;
                    dlgo70Shape.RightEdge = 512;
                    dlgo70Shape.TopEdge = 0;
                    dlgo70Shape.LeftEdge = 0;

                    dlgo70Shape.TexBottomEdge = TexBottomEdge;
                    dlgo70Shape.TexRightEdge = TexRightEdge;
                    dlgo70Shape.TexTopEdge = TexTopEdge;
                    dlgo70Shape.TexLeftEdge = TexLeftEdge;
                    dlgo70.Shape = dlgo70Shape;

                    drb.Dlgs[0].Dlgos.Add(dlgo70);

                    //Advance the row
                    TexBottomEdge += 256;
                    TexTopEdge += 256;
                }
                */
                //drb.Write(filePath);
            }
            else if (filePath.EndsWith(".grass"))
            {
                var grass = GRASS.Read(raw);
            }
        }

        public static void NullUnkIndices(string filePath)
        {
            var raw = File.ReadAllBytes(filePath);
            if (filePath.EndsWith(".mcg"))
            {
                var mcg = SoulsFormats.SoulsFile<SoulsFormats.MCG>.Read(raw);
                foreach (var edge in mcg.Edges)
                {
                    edge.UnkIndicesA.Clear();
                    edge.UnkIndicesB.Clear();
                }
                foreach (var node in mcg.Nodes)
                {
                    node.Unk18 = 0;
                    node.Unk1C = 0;
                }
                mcg.Write(filePath);
            }
        }

        public static void ConvertFile(string filePath, bool extractAllFiles = false)
        {
            ConvertFile(Path.GetDirectoryName(filePath), filePath, extractAllFiles);
        }

        public static void ConvertFile(string outDir, string filePath, bool extractAllFiles = false)
        {
            ConvertFile(outDir, filePath, File.ReadAllBytes(filePath), extractAllFiles);
        }

        public static void ConvertFile(string outDir, string filePath, byte[] file, bool extractAllFiles = false)
        {
            byte[] newFile = file;

            if (SoulsFormats.DCX.Is(newFile))
            {
                newFile = DCX.Decompress(newFile);
            }

            IBinder files = null;
            if (SoulsFile<SoulsFormats.BND3>.Is(newFile))
            {
                files = SoulsFile<SoulsFormats.BND3>.Read(newFile);
            }
            else if (SoulsFile<SoulsFormats.BND4>.Is(newFile))
            {
                files = SoulsFile<SoulsFormats.BND4>.Read(newFile);
            }

            if (files == null)
            {
                files = new BND3();
                files.Files.Add(new BinderFile() { Bytes = newFile, Name = null });
            }
            else
            {
                outDir = Path.Combine(outDir, Path.GetFileName(filePath) + "_");
            }

            foreach (var bndFile in files.Files)
            {
                var name = bndFile.Name ?? filePath;
                var fileName = Path.GetFileNameWithoutExtension(name);
                string outPath = outDir;
                string extension;
                if (bndFile.Name != null)
                {
                    extension = Path.GetExtension(bndFile.Name);
                    if (!extractAllFiles)
                    {
                        outPath = Path.Combine(outDir, fileName);
                    }
                }
                else
                {
                    extension = Path.GetExtension(filePath);
                }

                if (extractAllFiles)
                {
                    var path = Path.Combine(outPath, Path.GetFileName(name));
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllBytes(path, bndFile.Bytes);
                    continue;
                }

#if !DEBUG
            try
            {
#endif
                if (SoulsFormats.SoulsFile<SoulsFormats.FLVER0>.Is(bndFile.Bytes) || SoulsFormats.SoulsFile<SoulsFormats.FLVER2>.Is(bndFile.Bytes) || SoulsFormats.SoulsFile<SoulsFormats.Other.MDL4>.Is(bndFile.Bytes))
                {
                    Directory.CreateDirectory(outPath);

                    string modelPath = Path.Combine(outPath, Path.ChangeExtension(fileName, extension));

                    var aqp = ReadFlver(modelPath, bndFile.Bytes, out AquaNode aqn);

                    string finalPath;
                    var outName = Path.ChangeExtension(fileName, ".aqp");
                    if (aqp != null && aqp.vtxlList.Count > 0)
                    {
                        aqp.ConvertToPSO2Model(true, false, false, true, false, false, false, true, false, aqp.bonePalette.Count == 0);
                        aqp.ConvertToLegacyTypes();
                        aqp.CreateTrueVertWeights();

                        switch(mirrorType)
                        {
                            //Default
                            case MirrorType.Z:
                                HandednessUtility.FlipHandednessAqpAndAqnZ(aqp, aqn);
                                break;
                            case MirrorType.Y:
                                HandednessUtility.FlipHandednessAqpAndAqnY(aqp, aqn);
                                break;
                            case MirrorType.X:
                                HandednessUtility.FlipHandednessAqpAndAqnX(aqp, aqn);
                                break;
                            case MirrorType.None:
                                break;
                        }

                        switch(exportFormat)
                        {
                            case ExportFormat.Fbx:
                                finalPath = Path.Combine(outPath, Path.ChangeExtension(fileName, ".fbx"));
                                FbxExporterNative.ExportToFile(aqp, aqn, new List<AquaMotion>(), finalPath, new List<string>(), new List<Matrix4x4>(), false, (int)coordSystem);
                                break;
                            case ExportFormat.Smd:
                                finalPath = Path.Combine(outPath, Path.ChangeExtension(fileName, ".smd"));
                                SmdExporter.ExportToFile(aqp, aqn, finalPath, null);
                                break;
                        }
                    }
                }
                else if (TPF.Is(bndFile.Bytes))
                {
                    Directory.CreateDirectory(outPath);
                    var tpf = TPF.Read(bndFile.Bytes);
                    foreach (var tex in tpf.Textures)
                    {
                        File.WriteAllBytes(Path.Combine(outPath, Path.GetFileName(tex.Name) + ".dds"), tex.Headerize());
                    }
                }
#if !DEBUG
            }
                catch
                {
                    //Debug.WriteLine(Path.Combine(outPath, fileName));
                }
#endif
            }

        }

        public static void ConvertMorphemeAnims(List<string> nsaPaths, string modelPath, string nmbPath)
        {
            var nmb = SoulsConvert.ReadNMB(nmbPath, File.ReadAllBytes(nmbPath));
            //Temp, delete after testing and fix rotations up to match
            var temp = mirrorMat;
            mirrorMat = Matrix4x4.Identity;

            var flver = SoulsFormats.SoulsFile<SoulsFormats.FLVER2>.Read(File.ReadAllBytes(modelPath));
            List<AquaMotion> motions = new List<AquaMotion>();
            List<string> nsaNames = new List<string>();
            foreach (var path in nsaPaths)
            {
                nsaNames.Add(Path.GetFileName(path));
                motions.Add(NSAConvert.GetAquaMotionFromNSA(ReadNSA(path, File.ReadAllBytes(path)), flver, nmb));
            }

            string finalPath = Path.ChangeExtension(modelPath, ".fbx");
            var aqp = FlverToAqua(flver, out var aqn);
            aqp.ConvertToPSO2Model(true, false, false, true, false, false, false, true, false);
            aqp.ConvertToLegacyTypes();
            aqp.CreateTrueVertWeights();

            FbxExporterNative.ExportToFile(aqp, aqn, motions, finalPath, nsaNames, new List<Matrix4x4>(), false, (int)coordSystem);

            //Temp, delete after testing
            mirrorMat = temp;
        }

        public static NSA ReadNSA(string filePath, byte[] raw)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, raw);
            NSA.Set64BitAndEndianness(br);
            return new NSA(br);
        }

        public static NMB ReadNMB(string filePath, byte[] raw)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, raw);
            return new NMB(br);
        }

        public static AquaObject ReadFlver(string filePath, byte[] raw, out AquaNode aqn)
        {
            SoulsFormats.IFlver flver = null;

            JsonSerializerOptions jss = new JsonSerializerOptions() { WriteIndented = true };

            aqn = AquaNode.GenerateBasicAQN();
            if (SoulsFormats.SoulsFile<SoulsFormats.FLVER0>.Is(raw))
            {
                flver = SoulsFormats.SoulsFile<SoulsFormats.FLVER0>.Read(raw);
            }
            else if (SoulsFormats.SoulsFile<SoulsFormats.FLVER2>.Is(raw))
            {
                flver = SoulsFormats.SoulsFile<SoulsFormats.FLVER2>.Read(raw);
            }
            else if (SoulsFormats.SoulsFile<SoulsFormats.Other.MDL4>.Is(raw))
            {
                var mdl4 = SoulsFormats.SoulsFile<SoulsFormats.Other.MDL4>.Read(raw);

                if (useMetaData)
                {
                    string materialData = JsonSerializer.Serialize(mdl4.Materials, jss);
                    string dummyData = JsonSerializer.Serialize(mdl4.Dummies, jss);
                    string boneData = JsonSerializer.Serialize(mdl4.Nodes, jss);
                    File.WriteAllText(filePath + ".matData.json", materialData);
                    File.WriteAllText(filePath + ".dummyData.json", dummyData);
                    File.WriteAllText(filePath + ".boneData.json", boneData);
                }

                aqn = null;
                return MDL4ToAqua(Path.GetFileNameWithoutExtension(filePath) + "_skeleton", mdl4, out aqn, useMetaData);
            }

            //Dump metadata
            if (useMetaData)
            {
                string materialData = null;
                string dummyData = null;
                string boneData = null;
                if (flver is FLVER0)
                {
                    materialData = JsonSerializer.Serialize((List<FLVER0.Material>)flver.Materials, jss);
                    dummyData = JsonSerializer.Serialize(((FLVER0)flver).Dummies, jss);
                    boneData = JsonSerializer.Serialize(((FLVER0)flver).Nodes, jss);
                }
                else if (flver is FLVER2)
                {
                    materialData = JsonSerializer.Serialize((List<FLVER2.Material>)flver.Materials, jss);
                    dummyData = JsonSerializer.Serialize(((FLVER2)flver).Dummies, jss);
                    boneData = JsonSerializer.Serialize(((FLVER2)flver).Nodes, jss);
                }
                if(materialData != null)
                {
                    File.WriteAllText(filePath + ".matData.json", materialData);
                }
                if (materialData != null)
                {
                    File.WriteAllText(filePath + ".dummyData.json", dummyData);
                }
                if (materialData != null)
                {
                    File.WriteAllText(filePath + ".boneData.json", boneData);
                }
                
                
            }

            //We can't handle speedtree models right now
            if(flver == null || flver.IsSpeedtree())
            {
                return null;
            }

            return FlverToAqua(Path.GetFileNameWithoutExtension(filePath) + "_skeleton", flver, out aqn, useMetaData);
        }
        public static AquaObject MDL4ToAqua(SoulsFormats.Other.MDL4 mdl4, out AquaNode aqn, bool useMetaData = false)
        {
            return MDL4ToAqua("RootNode", mdl4, out aqn, useMetaData);
        }
        public static AquaObject MDL4ToAqua(string newRootName, SoulsFormats.Other.MDL4 mdl4, out AquaNode aqn, bool useMetaData = false)
        {
            //Preprocessing
            if(addFBXRootNode && exportFormat == ExportFormat.Fbx)
            {
                mdl4.Nodes.Insert(0, new SoulsFormats.Other.MDL4.Node() { Name = newRootName, FirstChildIndex = 1});
                for(int i = 0; i < mdl4.Nodes.Count; i++)
                {
                    if(i == 0)
                    {
                        continue;
                    }
                    var node = mdl4.Nodes[i];
                    node.ParentIndex++;
                    if (node.FirstChildIndex != -1)
                    {
                        node.FirstChildIndex++;
                    }
                    if(node.NextSiblingIndex != -1)
                    {
                        node.NextSiblingIndex++;
                    }
                    if(node.PreviousSiblingIndex != -1)
                    {
                        node.PreviousSiblingIndex++;
                    }
                }
                foreach(var mesh in mdl4.Meshes)
                {
                    for(int i = 0; i < mesh.BoneIndices.Length; i++)
                    {
                        if (mesh.BoneIndices[i] != -1)
                        {
                            mesh.BoneIndices[i] = (short)(mesh.BoneIndices[i] + 1);
                        }
                    }
                }
            }

            AquaObject aqp = new AquaObject();
            List<Matrix4x4> BoneTransforms = new List<Matrix4x4>();
            aqn = new AquaNode();
            for (int i = 0; i < mdl4.Nodes.Count; i++)
            {
                var flverBone = mdl4.Nodes[i];
                var parentId = flverBone.ParentIndex;

                var tfmMat = Matrix4x4.Identity;

                Matrix4x4 mat = flverBone.ComputeLocalTransform();
                mat *= tfmMat;

                //If there's a parent, multiply by it
                if (parentId != -1)
                {
                    var pn = aqn.nodeList[parentId];
                    var parentInvTfm = new Matrix4x4(pn.m1.X, pn.m1.Y, pn.m1.Z, pn.m1.W,
                                                  pn.m2.X, pn.m2.Y, pn.m2.Z, pn.m2.W,
                                                  pn.m3.X, pn.m3.Y, pn.m3.Z, pn.m3.W,
                                                  pn.m4.X, pn.m4.Y, pn.m4.Z, pn.m4.W);

                    Matrix4x4.Invert(parentInvTfm, out var invParentInvTfm);
                    mat = mat * invParentInvTfm;
                }
                if (exportFormat == ExportFormat.Fbx && parentId == -1 && i != 0)
                {
                    parentId = 0;
                }

                //Create AQN node
                NODE aqNode = new NODE();
                aqNode.boneShort1 = 0x1C0;
                aqNode.animatedFlag = 1;
                aqNode.parentId = parentId;
                aqNode.firstChild = flverBone.FirstChildIndex;
                aqNode.nextSibling = flverBone.NextSiblingIndex;
                aqNode.unkNode = -1;
                aqNode.pos = flverBone.Translation;
                aqNode.eulRot = flverBone.Rotation;
                aqNode.scale = new Vector3(1, 1, 1);

                BoneTransforms.Add(mat);
                Matrix4x4.Invert(mat, out var invMat);
                aqNode.m1 = new Vector4(invMat.M11, invMat.M12, invMat.M13, invMat.M14);
                aqNode.m2 = new Vector4(invMat.M21, invMat.M22, invMat.M23, invMat.M24);
                aqNode.m3 = new Vector4(invMat.M31, invMat.M32, invMat.M33, invMat.M34);
                aqNode.m4 = new Vector4(invMat.M41, invMat.M42, invMat.M43, invMat.M44);
                aqNode.boneName.SetString(flverBone.Name);
                aqn.nodeList.Add(aqNode);
            }

            for (int i = 0; i < mdl4.Meshes.Count; i++)
            {
                var mesh = mdl4.Meshes[i];

                var nodeMatrix = Matrix4x4.Identity;

                //Vert data
                var vertCount = mesh.Vertices.Count;
                VTXL vtxl = new VTXL();
                SoulsFormats.Other.MDL4.Mesh mesh0 = mesh;
                vtxl.bonePalette = new List<ushort>();
                for (int b = 0; b < mesh0.BoneIndices.Length; b++)
                {
                    if (mesh0.BoneIndices[b] == -1)
                    {
                        break;
                    }
                    vtxl.bonePalette.Add((ushort)mesh0.BoneIndices[b]);
                }
                var indices = mesh0.Triangulate(true, false);

                for (int v = 0; v < vertCount; v++)
                {
                    var vert = mesh.Vertices[v];
                    Vector3 vertPos = vert.Position;
                    Vector3 vertNorm = new Vector3(vert.Normal.X, vert.Normal.Y, vert.Normal.Z);

                    if (transformMesh && mesh.BoneIndices?[0] != -1)
                    {
                        int boneTransformationIndex = -1;
                        if (vert.BoneIndices[0] == vert.BoneIndices[1] && vert.BoneIndices[0] == vert.BoneIndices[2] && vert.BoneIndices[0] == vert.BoneIndices[3])
                        {
                            boneTransformationIndex = mesh.BoneIndices[vert.BoneIndices[0]];
                        }
                        else if (vert.Normal.Length() > 0) //Check that there IS a normal to read
                        {
                            boneTransformationIndex = mesh.BoneIndices[(int)vert.Normal.W];
                        }

                        if (boneTransformationIndex > -1 && BoneTransforms.Count > boneTransformationIndex)
                        {
                            var boneTfm = BoneTransforms[boneTransformationIndex];

                            vertPos = Vector3.Transform(vertPos, boneTfm);
                            vertNorm = Vector3.TransformNormal(vertNorm, boneTfm);
                            vertNorm *= -1;
                        }
                    }

                    vtxl.vertPositions.Add(vertPos);
                    vtxl.vertNormals.Add(vertNorm);

                    if (vert.UVs.Count > 0)
                    {
                        var uv1 = vert.UVs[0];
                        vtxl.uv1List.Add(new Vector2(uv1.X, uv1.Y));
                    }
                    if (vert.UVs.Count > 1)
                    {
                        var uv2 = vert.UVs[1];
                        vtxl.uv2List.Add(new Vector2(uv2.X, uv2.Y));
                    }
                    if (vert.UVs.Count > 2)
                    {
                        var uv3 = vert.UVs[2];
                        vtxl.uv3List.Add(new Vector2(uv3.X, uv3.Y));
                    }
                    if (vert.UVs.Count > 3)
                    {
                        var uv4 = vert.UVs[3];
                        vtxl.uv4List.Add(new Vector2(uv4.X, uv4.Y));
                    }
                    var color = vert.Color;
                    vtxl.vertColors.Add(new byte[] { (color.B), (color.G), (color.R), (color.A) });

                    if ((vert.BoneWeights[0] + vert.BoneWeights[1] + vert.BoneWeights[2] + vert.BoneWeights[3]) > 0)
                    {
                        vtxl.vertWeights.Add(new Vector4(vert.BoneWeights[0], vert.BoneWeights[1], vert.BoneWeights[2], vert.BoneWeights[3]));
                        vtxl.vertWeightIndices.Add(new int[] { vert.BoneIndices[0], vert.BoneIndices[1], vert.BoneIndices[2], vert.BoneIndices[3] });
                    }
                    else if (vert.Normal.W < 65535)
                    {
                        vtxl.vertWeights.Add(new Vector4(1, 0, 0, 0));
                        vtxl.vertWeightIndices.Add(new int[] { (int)vert.Normal.W, 0, 0, 0 });
                    }
                    else if (vert.BoneIndices.Length > 0)
                    {
                        vtxl.vertWeights.Add(new Vector4(1, 0, 0, 0));
                        vtxl.vertWeightIndices.Add(new int[] { vert.BoneIndices[0], 0, 0, 0 });
                    }
                }

                vtxl.convertToLegacyTypes();
                aqp.vtxeList.Add(VTXE.ConstructFromVTXL(vtxl, out int vc));
                aqp.vtxlList.Add(vtxl);

                //Face data
                GenericTriangles genMesh = new GenericTriangles();

                List<Vector3> triList = new List<Vector3>();
                for (int id = 0; id < indices.Count - 2; id += 3)
                {
                    triList.Add(new Vector3(indices[id], indices[id + 1], indices[id + 2]));
                }

                genMesh.triList = triList;

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
                var flverMat = mdl4.Materials[mesh.MaterialIndex];
                mat.matName = $"{flverMat.Name}|{mesh.MaterialIndex}";
                mat.texNames = flverMat.GetTexList();
                aqp.tempMats.Add(mat);
            }

            return aqp;
        }

        public static AquaObject FlverToAqua(IFlver flver, out AquaNode aqn, bool useMetaData = false)
        {
            return FlverToAqua("RootNode", flver, out aqn, useMetaData); 
        }

        public static AquaObject FlverToAqua(string newRootName, IFlver flver, out AquaNode aqn, bool useMetaData = false)
        {
            //Preprocessing
            if (addFBXRootNode && exportFormat == ExportFormat.Fbx)
            {
                if (flver is FLVER2 flv2)
                {
                    flv2.Nodes.Insert(0, new FLVER.Node() { Name = newRootName, FirstChildIndex = 1 });
                    for(int i = 0; i < flv2.Nodes.Count; i++)
                    {
                        if (i == 0)
                        {
                            continue;
                        }
                        var node = flv2.Nodes[i];
                        UpdateNodeForNewRoot(node);
                    }
                    foreach(var mesh in flv2.Meshes)
                    {
                        bool localIndicesUsed = false;
                        for(int j = 0; j < mesh.BoneIndices.Count; j++)
                        {
                            if (mesh.BoneIndices[j] != -1)
                            {
                                mesh.BoneIndices[j] = mesh.BoneIndices[j] + 1;
                                localIndicesUsed = true;
                            }
                        }
                        if (localIndicesUsed == false)
                        {
                            foreach (var vert in mesh.Vertices)
                            {
                                if (vert.NormalW > 0 && vert.NormalW < 65535)
                                {
                                    vert.NormalW++;
                                }
                                vert.BoneIndices[0] = vert.BoneIndices[0] + 1;
                                vert.BoneIndices[1] = vert.BoneIndices[1] + 1;
                                vert.BoneIndices[2] = vert.BoneIndices[2] + 1;
                                vert.BoneIndices[3] = vert.BoneIndices[3] + 1;
                            }
                        }
                    }
                } else if (flver is FLVER0 flv0)
                {
                    flv0.Nodes.Insert(0, new FLVER.Node() { Name = newRootName });
                    for (int i = 0; i < flv0.Nodes.Count; i++)
                    {
                        if (i == 0)
                        {
                            continue;
                        }
                        var node = flv0.Nodes[i];
                        UpdateNodeForNewRoot(node);
                    }
                    foreach (var mesh in flv0.Meshes)
                    {
                        bool localIndicesUsed = false;
                        for (int j = 0; j < mesh.BoneIndices.Length; j++)
                        {
                            if (mesh.BoneIndices[j] != -1)
                            {
                                mesh.BoneIndices[j] = (short)(mesh.BoneIndices[j] + 1);
                                localIndicesUsed = true;
                            }
                        }
                        if (localIndicesUsed == false)
                        {
                            foreach (var vert in mesh.Vertices)
                            {
                                if (vert.NormalW > 0 && vert.NormalW < 65535)
                                {
                                    vert.NormalW++;
                                }
                                vert.BoneIndices[0] = vert.BoneIndices[0] + 1;
                                vert.BoneIndices[1] = vert.BoneIndices[1] + 1;
                                vert.BoneIndices[2] = vert.BoneIndices[2] + 1;
                                vert.BoneIndices[3] = vert.BoneIndices[3] + 1;
                            }
                        }
                    }
                }
            }

            AquaObject aqp = new AquaObject();
            List<Matrix4x4> BoneTransforms = new List<Matrix4x4>();

            if (flver is FLVER2 flver2)
            {
                if (flver2.Header.Version > 0x20010)
                {
                    for (int i = 0; i < flver2.Nodes.Count; i++)
                    {
                        aqp.bonePalette.Add((uint)i);
                    }
                }
            }
            aqn = new AquaNode();
            Vector3 maxTest = new Vector3();
            Vector3 minTest = new Vector3();
            for (int i = 0; i < flver.Nodes.Count; i++)
            {
                var flverBone = flver.Nodes[i];
                var parentId = flverBone.ParentIndex;

                var tfmMat = Matrix4x4.Identity;

                //Debug.WriteLine($"{i} {flverBone.Rotation.X} {flverBone.Rotation.Y} {flverBone.Rotation.Z}");
                if (flverBone.Rotation.X > maxTest.X)
                {
                    maxTest.X = flverBone.Rotation.X;
                }
                else if (flverBone.Rotation.X < minTest.X)
                {
                    minTest.X = flverBone.Rotation.X;
                }
                if (flverBone.Rotation.Y > maxTest.Y)
                {
                    maxTest.Y = flverBone.Rotation.Y;
                }
                else if (flverBone.Rotation.Y < minTest.Y)
                {
                    minTest.Y = flverBone.Rotation.Y;
                }
                if (flverBone.Rotation.Z > maxTest.Z)
                {
                    maxTest.Z = flverBone.Rotation.Z;
                }
                else if (flverBone.Rotation.Z < minTest.Z)
                {
                    minTest.Z = flverBone.Rotation.Z;
                }
                Matrix4x4 mat = flverBone.ComputeLocalTransform();

                Matrix4x4.Decompose(mat, out Vector3 scale, out System.Numerics.Quaternion quatrot, out Vector3 translation);

                //Debug.WriteLine($"{i} Quat {quatrot.X} {quatrot.Y} {quatrot.Z} {quatrot.W}");
                mat *= tfmMat;

                //If there's a parent, multiply by it
                //Some of these are malformed and specify a bone greater than what exists. Thanks, From
                if (parentId != -1 && parentId < aqn.nodeList.Count)
                {
                    var pn = aqn.nodeList[parentId];
                    var parentInvTfm = new Matrix4x4(pn.m1.X, pn.m1.Y, pn.m1.Z, pn.m1.W,
                                                  pn.m2.X, pn.m2.Y, pn.m2.Z, pn.m2.W,
                                                  pn.m3.X, pn.m3.Y, pn.m3.Z, pn.m3.W,
                                                  pn.m4.X, pn.m4.Y, pn.m4.Z, pn.m4.W);

                    Matrix4x4.Invert(parentInvTfm, out var invParentInvTfm);
                    mat = mat * invParentInvTfm;
                }
                if (exportFormat == ExportFormat.Fbx && parentId == -1 && i != 0)
                {
                    parentId = 0;
                }

                //Create AQN node
                NODE aqNode = new NODE();
                aqNode.boneShort1 = 0x1C0;
                aqNode.animatedFlag = 1;
                aqNode.parentId = parentId;
                aqNode.firstChild = flverBone.FirstChildIndex;
                aqNode.nextSibling = flverBone.NextSiblingIndex;
                aqNode.unkNode = -1;
                aqNode.pos = flverBone.Translation;
                aqNode.eulRot = flverBone.Rotation * (float)(180 / Math.PI);
                aqNode.scale = new Vector3(1, 1, 1);

#if DEBUG
                var xyz = MathExtras.EulerToQuaternionRadian(flverBone.Rotation, RotationOrder.XYZ);
                var xzy = MathExtras.EulerToQuaternionRadian(flverBone.Rotation, RotationOrder.XZY);
                var yxz = MathExtras.EulerToQuaternionRadian(flverBone.Rotation, RotationOrder.YXZ);
                var yzx = MathExtras.EulerToQuaternionRadian(flverBone.Rotation, RotationOrder.YZX);
                var zxy = MathExtras.EulerToQuaternionRadian(flverBone.Rotation, RotationOrder.ZXY);
                var zyx = MathExtras.EulerToQuaternionRadian(flverBone.Rotation, RotationOrder.ZYX);
#endif
                BoneTransforms.Add(mat);
                Matrix4x4.Invert(mat, out var invMat);
                aqNode.m1 = new Vector4(invMat.M11, invMat.M12, invMat.M13, invMat.M14);
                aqNode.m2 = new Vector4(invMat.M21, invMat.M22, invMat.M23, invMat.M24);
                aqNode.m3 = new Vector4(invMat.M31, invMat.M32, invMat.M33, invMat.M34);
                aqNode.m4 = new Vector4(invMat.M41, invMat.M42, invMat.M43, invMat.M44);
                aqNode.boneName.SetString(flverBone.Name);
                aqn.nodeUnicodeNames.Add(flverBone.Name);
                aqn.nodeList.Add(aqNode);
            }

            if(addFlverDummies)
            {
                for (int i = 0; i < flver.Dummies.Count; i++)
                {
                    var dummy = flver.Dummies[i];
                    var transform = Matrix4x4.CreateWorld(dummy.Position, dummy.Forward, dummy.Upward);

                    var ogTransform = transform;
                    NODO nodo = new NODO();
                    if (dummy.AttachBoneIndex > -1)
                    {
                        var realParent = flver.Nodes[dummy.AttachBoneIndex];
                        if (parentDummiesToAttachNodes)
                        {
                            var realParentAqn = aqn.nodeList[dummy.AttachBoneIndex];
                            var pseudoParentAqn = aqn.nodeList[dummy.ParentBoneIndex];
                            transform *= pseudoParentAqn.GetInverseBindPoseMatrixInverted();
                            transform *= realParentAqn.GetInverseBindPoseMatrix();
                            nodo.parentId = dummy.AttachBoneIndex;
                        } else
                        {
                            nodo.parentId = dummy.ParentBoneIndex;
                        }
                        var name = string.Format("Dummy#{0}#{1}#{2}#{3}#{4}#{5}#{6}", dummy.ReferenceID, realParent.Name, flver.Nodes[dummy.ParentBoneIndex].Name, dummy.Flag1, dummy.Unk30, dummy.Unk34, $"{dummy.Color.R:X}{dummy.Color.G:X}{dummy.Color.B:X}{dummy.Color.A:X}");
                        nodo.boneName.SetString(name);
                        aqn.nodoUnicodeNames.Add(name);
                    }
                    else
                    {
                        var name = string.Format("Dummy#{0}#None#{1}#{2}#{3}#{4}#{5}", dummy.ReferenceID, flver.Nodes[dummy.ParentBoneIndex].Name, dummy.Flag1, dummy.Unk30, dummy.Unk34, $"{dummy.Color.R:X}{dummy.Color.G:X}{dummy.Color.B:X}{dummy.Color.A:X}");
                        nodo.boneName.SetString(name);
                        aqn.nodoUnicodeNames.Add(name);
                        nodo.parentId = dummy.ParentBoneIndex;
                    }
                    Matrix4x4.Decompose(transform, out var scale, out var rot, out var pos);
                    Matrix4x4.Decompose(ogTransform, out var ogscale, out var ogrot, out var ogpos);
                    nodo.eulRot = MathExtras.GetFlverEulerFromQuaternion_Bone(ogrot) * (float)(180 / Math.PI);
                    nodo.pos = pos;
                    aqn.nodoList.Add(nodo);
                }
            }
            Vector3? maxBounding = null;
            Vector3? minBounding = null;
            Vector3? maxBounding2 = null;
            Vector3? minBounding2 = null;

            for (int i = 0; i < flver.Meshes.Count; i++)
            {
                var mesh = flver.Meshes[i];

                var nodeMatrix = Matrix4x4.Identity;

                //Vert data
                var vertCount = mesh.Vertices.Count;
                VTXL vtxl = new VTXL();

                if (mesh.Dynamic > 0)
                {
                    if (mesh is FLVER0.Mesh flv)
                    {

                        for (int b = 0; b < flv.BoneIndices.Length; b++)
                        {
                            if (flv.BoneIndices[b] == -1)
                            {
                                break;
                            }
                            vtxl.bonePalette.Add((ushort)flv.BoneIndices[b]);
                        }
                    }
                    else if (mesh is FLVER2.Mesh flv2)
                    {
                        for (int b = 0; b < flv2.BoneIndices.Count; b++)
                        {
                            if (flv2.BoneIndices[b] == -1)
                            {
                                break;
                            }
                            vtxl.bonePalette.Add((ushort)flv2.BoneIndices[b]);
                        }
                    }
                }

                bool useNormalWTransform = false;
                bool useIndexNoWeightTransform = false;
                bool useDefaultBoneTransform = false;
                bool foundBoneIndices = false;
                bool foundBoneWeights = false;
                List<int> indices = new List<int>();
                if (flver is FLVER0)
                {
                    FLVER0.Mesh mesh0 = (FLVER0.Mesh)mesh;

                    vtxl.bonePalette = new List<ushort>();
                    for (int b = 0; b < mesh0.BoneIndices.Length; b++)
                    {
                        if (mesh0.BoneIndices[b] == -1)
                        {
                            break;
                        }
                        vtxl.bonePalette.Add((ushort)mesh0.BoneIndices[b]);
                    }
                    indices = mesh0.Triangulate(((FLVER0)flver).Header.Version);

                    var material = (FLVER0.Material)flver.Materials[mesh0.MaterialIndex];
                    foreach (var layoutType in material.Layouts[0])
                    {
                        switch (layoutType.Semantic)
                        {
                            case FLVER.LayoutSemantic.Normal:
                                if (layoutType.Type == FLVER.LayoutType.Byte4B || layoutType.Type == FLVER.LayoutType.Byte4E)
                                {
                                    useNormalWTransform = true;
                                }
                                break;
                            case FLVER.LayoutSemantic.BoneIndices:
                                foundBoneIndices = true;
                                break;
                            case FLVER.LayoutSemantic.BoneWeights:
                                foundBoneWeights = true;
                                break;
                        }
                    }
                }
                else if (flver is FLVER2)
                {
                    FLVER2.Mesh mesh2 = (FLVER2.Mesh)mesh;
                    if (mesh2.VertexBuffers?.Count > 0)
                    {
                        var layouts = ((FLVER2)flver).BufferLayouts[mesh2.VertexBuffers[0].LayoutIndex];
                        foreach (var layoutType in layouts)
                        {
                            switch (layoutType.Semantic)
                            {
                                case FLVER.LayoutSemantic.Normal:
                                    if (layoutType.Type == FLVER.LayoutType.Byte4B || layoutType.Type == FLVER.LayoutType.Byte4E)
                                    {
                                        useNormalWTransform = true;
                                    }
                                    break;
                                case FLVER.LayoutSemantic.BoneIndices:
                                    foundBoneIndices = true;
                                    break;
                                case FLVER.LayoutSemantic.BoneWeights:
                                    foundBoneWeights = true;
                                    break;
                            }
                        }
                    }
                    if (useNormalWTransform == false)
                    {
                        useDefaultBoneTransform = mesh2.NodeIndex != -1 && mesh2.NodeIndex < flver.Nodes.Count;
                    }

                    //Dark souls 3+ (Maybe bloodborne too) use direct bone id references instead of a bone palette
                    vtxl.bonePalette = new List<ushort>();
                    for (int b = 0; b < mesh2.BoneIndices.Count; b++)
                    {
                        if (mesh2.BoneIndices[b] == -1)
                        {
                            break;
                        }
                        vtxl.bonePalette.Add((ushort)mesh2.BoneIndices[b]);
                    }

                    FLVER2.FaceSet faceSet = mesh2.FaceSets.Count > 0 ? mesh2.FaceSets[0] : new FLVER2.FaceSet();
                    //By order?
                    if (faceSet.Indices.Count == 0)
                    {
                        for (int f = 0; f < mesh2.Vertices.Count; f += 1)
                        {
                            indices.Add(f);
                        }
                    }
                    else
                    {
                        indices = faceSet.Triangulate(mesh2.Vertices.Count < ushort.MaxValue);
                    }
                }
                else
                {
                    throw new Exception("Unexpected flver variant");
                }

                //Transformation condition for DeS models
                if (foundBoneIndices && !foundBoneWeights)
                {
                    useIndexNoWeightTransform = true;
                }

                bool wasTransformed = false;
                for (int v = 0; v < vertCount; v++)
                {
                    var vert = mesh.Vertices[v];
                    Vector3 vertPos = vert.Position;
                    Vector3 vertNorm = new Vector3(vert.Normal.X, vert.Normal.Y, vert.Normal.Z);
                    Vector3 vertPosNorm = vertPos + vertNorm;

                    int boneTransformationIndex = -1;
                    int normalWOverride = -1;

                    if (mesh is FLVER0.Mesh)
                    {
                        var f0Mesh = (FLVER0.Mesh)mesh;
                        if (useNormalWTransform && f0Mesh.Dynamic == 0)
                        {
                            boneTransformationIndex = vert.NormalW;
                        }
                        else if (useIndexNoWeightTransform && f0Mesh.BoneIndices?[0] != -1 && f0Mesh.Dynamic == 0)
                        {
                            boneTransformationIndex = f0Mesh.BoneIndices[vert.BoneIndices[0]];
                        }
                    }
                    else
                    {
                        var f2Mesh = (FLVER2.Mesh)mesh;
                        bool useDefaultBoneIndex = false;
                        if (f2Mesh.Dynamic == 0 && vert.NormalW < flver.Nodes.Count)
                        {
                            boneTransformationIndex = vert.NormalW;
                        }
                        else if (useIndexNoWeightTransform && vert.BoneIndices[0] != -1 && f2Mesh.Dynamic == 0)
                        {
                            if (f2Mesh.BoneIndices?.Count > 0 && f2Mesh.BoneIndices[0] != -1)
                            {
                                boneTransformationIndex = vert.BoneIndices[0];
                            }
                            else
                            {
                                boneTransformationIndex = vert.BoneIndices[0];
                            }
                        }
                        else if (useDefaultBoneTransform && flver.Nodes.Count > f2Mesh.NodeIndex)
                        {
                            boneTransformationIndex = f2Mesh.NodeIndex;
                            useDefaultBoneIndex = true;
                        }

                        if (f2Mesh.BoneIndices.Count > 0 && useDefaultBoneIndex == false && boneTransformationIndex >= 0)
                        {
                            if (f2Mesh.BoneIndices.Count > boneTransformationIndex)
                            {
                                boneTransformationIndex = f2Mesh.BoneIndices[boneTransformationIndex];
                            }
                            else
                            {
                                boneTransformationIndex = 0;
                                normalWOverride = boneTransformationIndex;
                            }
                        }
                    }
                    if (transformMesh)
                    {
                        if (boneTransformationIndex > -1 && BoneTransforms.Count > boneTransformationIndex)
                        {
                            var boneTfm = BoneTransforms[boneTransformationIndex];

                            vertPos = Vector3.Transform(vertPos, boneTfm);
                            vertNorm = Vector3.Normalize(Vector3.Transform(vertPosNorm, boneTfm) - vertPos);
                            wasTransformed = true;
                        }
                    }

                    var alter = Vector3.Transform(vertPos, flver.Nodes[0].ComputeLocalTransform());
                    //Recalc model bounding
                    if (maxBounding == null)
                    {
                        maxBounding = vertPos;
                        minBounding = vertPos;
                        maxBounding2 = alter;
                        minBounding2 = alter;
                    }
                    else
                    {
                        maxBounding = MathExtras.GetMaximumBounding(vertPos, (Vector3)maxBounding);
                        minBounding = MathExtras.GetMinimumBounding(vertPos, (Vector3)minBounding);
                        maxBounding2 = MathExtras.GetMaximumBounding(alter, (Vector3)maxBounding);
                        minBounding2 = MathExtras.GetMinimumBounding(alter, (Vector3)minBounding);
                    }

                    vtxl.vertPositions.Add(vertPos);
                    vtxl.vertNormals.Add(vertNorm);

                    if (vert.UVs.Count > 0)
                    {
                        var uv1 = vert.UVs[0];
                        vtxl.uv1List.Add(new Vector2(uv1.X, uv1.Y));
                    }
                    if (vert.UVs.Count > 1)
                    {
                        var uv2 = vert.UVs[1];
                        vtxl.uv2List.Add(new Vector2(uv2.X, uv2.Y));
                    }
                    if (vert.UVs.Count > 2)
                    {
                        var uv3 = vert.UVs[2];
                        vtxl.uv3List.Add(new Vector2(uv3.X, uv3.Y));
                    }
                    if (vert.UVs.Count > 3)
                    {
                        var uv4 = vert.UVs[3];
                        vtxl.uv4List.Add(new Vector2(uv4.X, uv4.Y));
                    }

                    if (vert.Colors.Count > 0)
                    {
                        var color = vert.Colors[0];
                        vtxl.vertColors.Add(new byte[] { (byte)(color.B * 255), (byte)(color.G * 255), (byte)(color.R * 255), (byte)(color.A * 255) });
                    }
                    if (vert.Colors.Count > 1)
                    {
                        var color2 = vert.Colors[1];
                        vtxl.vertColor2s.Add(new byte[] { (byte)(color2.B * 255), (byte)(color2.G * 255), (byte)(color2.R * 255), (byte)(color2.A * 255) });
                    }

                    if ((vert.BoneWeights[0] + vert.BoneWeights[1] + vert.BoneWeights[2] + vert.BoneWeights[3]) > 0)
                    {
                        vtxl.vertWeights.Add(new Vector4(vert.BoneWeights[0], vert.BoneWeights[1], vert.BoneWeights[2], vert.BoneWeights[3]));
                        vtxl.vertWeightIndices.Add(new int[] { vert.BoneIndices[0], vert.BoneIndices[1], vert.BoneIndices[2], vert.BoneIndices[3] });
                    }
                    else if (normalWOverride > -1)
                    {
                        vtxl.vertWeights.Add(new Vector4(1, 0, 0, 0));
                        vtxl.vertWeightIndices.Add(new int[] { normalWOverride, 0, 0, 0 });
                    }
                    else if (vert.NormalW < 65535 && vert.NormalW < flver.Nodes.Count)
                    {
                        vtxl.vertWeights.Add(new Vector4(1, 0, 0, 0));
                        vtxl.vertWeightIndices.Add(new int[] { vert.NormalW, 0, 0, 0 });
                    }
                    else if (vert.BoneIndices.Length > 0)
                    {
                        vtxl.vertWeights.Add(new Vector4(1, 0, 0, 0));
                        vtxl.vertWeightIndices.Add(new int[] { vert.BoneIndices[0], 0, 0, 0 });
                    }
                }

                vtxl.convertToLegacyTypes();
                aqp.vtxeList.Add(VTXE.ConstructFromVTXL(vtxl, out int vc));
                aqp.vtxlList.Add(vtxl);

                //Face data
                GenericTriangles genMesh = new GenericTriangles();

                List<Vector3> triList = new List<Vector3>();
                for (int id = 0; id < indices.Count - 2; id += 3)
                {
                    if (mirrorType != MirrorType.None)
                    {
                        triList.Add(new Vector3(indices[id], indices[id + 1], indices[id + 2]));
                    }
                    else
                    {
                        triList.Add(new Vector3(indices[id + 2], indices[id + 1], indices[id]));
                    }
                }

                genMesh.triList = triList;

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
                string matName;
                var flverMat = flver.Materials[mesh.MaterialIndex];
                if (applyMaterialNamesToMesh)
                {
                    aqp.meshNames.Add($"{flverMat.Name}|{Path.GetFileName(flverMat.MTD)}|mesh{i}");
                }
                if (useMetaData)
                {
                    matName = $"{flverMat.Name}|{Path.GetFileName(flverMat.MTD)}|{mesh.MaterialIndex}";
                    mat.matName = matName;
                }
                else
                {
                    matName = $"{flverMat.Name}";
                    mat.matName = matName;
                }
                mat.texNames = new List<string>();
                foreach (var tex in flverMat.Textures)
                {
                    var texName = Path.GetFileName(tex.Path);
                    if (texName != null && Path.HasExtension(texName))
                    {
                        texName = Path.ChangeExtension(texName, ".dds");
                    }
                    mat.texNames.Add(texName);
                }
                aqp.tempMats.Add(mat);
                aqp.matUnicodeNames.Add(matName);
            }

            return aqp;
        }

        private static void UpdateNodeForNewRoot(FLVER.Node node)
        {

            node.ParentIndex++;
            if (node.FirstChildIndex != -1)
            {
                node.FirstChildIndex++;
            }
            if (node.NextSiblingIndex != -1)
            {
                node.NextSiblingIndex++;
            }
            if (node.PreviousSiblingIndex != -1)
            {
                node.PreviousSiblingIndex++;
            }
        }

        public static void ConvertModelToFlverAndWrite(string initialFilePath, string outPath, float scaleFactor, bool preAssignNodeIds, bool isNGS, SoulsGame game)
        {
            ((FLVER0)ConvertModelToFlver(initialFilePath, scaleFactor, preAssignNodeIds, isNGS, game, null)).Write(outPath);
        }

        public static IFlver ConvertModelToFlver(string initialFilePath, float scaleFactor, bool preAssignNodeIds, bool isNGS, SoulsGame game, IFlver referenceFlver = null)
        {
            var aqp = AssimpModelImporter.AssimpAquaConvertFull(initialFilePath, scaleFactor, preAssignNodeIds, isNGS, out AquaNode aqn, false);

            //Demon's Souls has a limit of 28 bones per mesh.
            aqp.BatchSplitByBoneCount(28, true);
            return AquaToFlver(initialFilePath, aqp, aqn, game, referenceFlver);
        }

        public static IFlver AquaToFlver(string initialFilePath, AquaObject aqp, AquaNode aqn, SoulsGame game, IFlver referenceFlver = null)
        {
            switch (game)
            {
                case SoulsGame.DemonsSouls:
                    break;
                default:
                    return null;
            }

            var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var mtdDict = ReadMTDLayoutData(Path.Combine(exePath, "DeSMtdLayoutData.bin"));
            FLVER0 flver = new FLVER0();
            flver.Header = new FLVER0Header();
            flver.Header.BigEndian = true;
            flver.Header.Unicode = true;
            flver.Header.Version = 0x15;
            flver.Header.Unk4A = 0x1;
            flver.Header.Unk4B = 0;
            flver.Header.Unk4C = 0xFFFF;
            flver.Header.Unk5C = 0;
            flver.Header.VertexIndexSize = 16; //Probably needs to be 32 if we go over 65535 faces in a mesh, but DeS PS3 probably never does that.

            List<string> usedMaterials = new List<string>();
            flver.Materials = new List<FLVER0.Material>();

            var dummyPath = Path.ChangeExtension(initialFilePath, "flver.dummyData.json");
            var matPath = Path.ChangeExtension(initialFilePath, "flver.matData.json");
            var bonePath = Path.ChangeExtension(initialFilePath, "FLVER.NodeData.json");
            var dummyDataText = File.Exists(dummyPath) ? File.ReadAllText(dummyPath) : null;
            var materialDataText = File.Exists(matPath) ? File.ReadAllText(matPath) : null;
            var boneDataText = File.Exists(bonePath) ? File.ReadAllText(bonePath) : null;
            //Dummies - Deserialize from JSON and apply if there's not a reference flver selected. Use reference flver if available
            if (referenceFlver != null)
            {
                flver.Dummies = (List<FLVER.Dummy>)referenceFlver.Dummies;
            }
            else if (dummyDataText != null)
            {
                List<FLVER.Dummy> metaDummies = null;
                metaDummies = JsonSerializer.Deserialize<List<FLVER.Dummy>>(dummyDataText);
                flver.Dummies = metaDummies;
            }
            else
            {
                flver.Dummies = new List<FLVER.Dummy>();
            }

            //Materials - Deserialize tex lists from JSON and apply. Use reference flver for tex names if available
            List<FLVER0.Material> metaMats = null;
            if (referenceFlver != null)
            {
                metaMats = (List<FLVER0.Material>)referenceFlver.Materials;
                flver.Materials.AddRange(metaMats);
            }
            else if (materialDataText != null)
            {
                metaMats = JsonSerializer.Deserialize<List<FLVER0.Material>>(materialDataText);
                flver.Materials.AddRange(metaMats);
            }

            List<int> matIds = new List<int>();
            for (int i = 0; i < aqp.meshList.Count; i++)
            {
                int pso2MatId = aqp.meshList[i].mateIndex;
                var texList = aqp.GetTexListNamesUnicode(aqp.meshList[i].tsetIndex);
                var pso2Mat = aqp.mateList[pso2MatId];
                string rawName;
                if (aqp.matUnicodeNames.Count > pso2MatId)
                {
                    rawName = aqp.matUnicodeNames[pso2MatId];
                }
                else
                {
                    rawName = pso2Mat.matName.GetString();
                }
                var nameSplit = rawName.Split('|');
                string name = nameSplit[0];
                var matIndex = usedMaterials.IndexOf(name);
                string mtd = null;
                int ogMatIndex = -1;

                //Use stored mtd name if possible 
                if (nameSplit.Length > 1)
                {
                    mtd = nameSplit[1];

                    //Grab original material index if possible
                    if (nameSplit.Length > 2)
                    {
                        var index = nameSplit[2];
                        index = index.Split('.')[0];
                        ogMatIndex = Int32.Parse(index);
                    }
                }

                FLVER0.Material flvMat;

                //If we loaded maps externally and it's in range of that list, use it
                if (metaMats != null && ogMatIndex < metaMats.Count && ogMatIndex != -1)
                {
                    matIds.Add(ogMatIndex);
                    continue;
                }

                //If matIndex isn't -1, we're good and can leave. Otherwise, on with the defaults
                if (matIndex != -1)
                {
                    matIds.Add(matIndex);
                    continue;
                }
                else
                {
                    usedMaterials.Add(name);
                    flvMat = new FLVER0.Material();
                    flvMat.Name = name;
                    mtd = mtd ?? "n:\\orthern\\limit\\p_metal[dsb]_skin.mtd";
                    flvMat.MTD = mtd;
                    if (texList.Count > 0)
                    {
                        FLVER0.Texture tex = new FLVER0.Texture();
                        flvMat.Textures = new List<FLVER0.Texture>();
                        tex.Path = texList[0];
                        tex.Type = "g_Diffuse";
                        flvMat.Textures.Add(tex);
                    }
                }
                flvMat.Layouts = new List<FLVER0.BufferLayout>();
                flvMat.Name = name;
                var mtdShortName = Path.GetFileName(mtd).ToLower();
                if (mtdDict.ContainsKey(mtdShortName))
                {
                    flvMat.Layouts.Add(mtdDict[mtdShortName]);
                    flvMat.MTD = mtd;
                }
                else
                {
                    flvMat.Layouts.Add(mtdDict["p_metal[dsb]_skin.mtd"]);
                    flvMat.MTD = "p_metal[dsb]_skin.mtd";
                }

                matIds.Add(flver.Materials.Count);
                flver.Materials.Add(flvMat);
            }

            //Bones store bounding which encompass the extents of all vertices onto which they are weighted.
            //When no vertices are weighted to them, this bounding is -3.402823e+38 for all min bound values and 3.402823e+38 for all max bound values
            Dictionary<int, Vector3> MaxBoundingBoxByBone = new Dictionary<int, Vector3>();
            Dictionary<int, Vector3> MinBoundingBoxByBone = new Dictionary<int, Vector3>();
            var defaultMaxBound = new Vector3(3.402823e+38f, 3.402823e+38f, 3.402823e+38f);
            var defaultMinBound = new Vector3(-3.402823e+38f, -3.402823e+38f, -3.402823e+38f);
            Vector3? maxBounding = null;
            Vector3? minBounding = null;
            flver.Meshes = new List<FLVER0.Mesh>();
            for (int i = 0; i < aqp.meshList.Count; i++)
            {
                var mesh = aqp.meshList[i];
                var vtxl = aqp.vtxlList[mesh.vsetIndex];
                var faces = aqp.strips[mesh.psetIndex];
                var shader = aqp.shadList[mesh.shadIndex];
                var render = aqp.rendList[mesh.rendIndex];
                var flvMat = flver.Materials[matIds[i]];

                FLVER0.Mesh flvMesh = new FLVER0.Mesh();
                var flverMaterial = flver.Materials[matIds[i]];
                flvMesh.MaterialIndex = (byte)matIds[i];
                flvMesh.BackfaceCulling = false;

                bool normalWBoneTransform = false;
                bool foundBoneIndices = false;
                bool foundBoneWeights = false;
                if (flvMat.Layouts?.Count > 0)
                {
                    foreach (var layoutType in flvMat.Layouts[0])
                    {
                        switch (layoutType.Semantic)
                        {
                            case FLVER.LayoutSemantic.Normal:
                                if (layoutType.Type == FLVER.LayoutType.Byte4B || layoutType.Type == FLVER.LayoutType.Byte4E)
                                {
                                    normalWBoneTransform = true;
                                }
                                break;
                            case FLVER.LayoutSemantic.BoneIndices:
                                foundBoneIndices = true;
                                break;
                            case FLVER.LayoutSemantic.BoneWeights:
                                foundBoneWeights = true;
                                break;
                        }
                    }
                }
                bool isPart = Path.GetFileName(flverMaterial.MTD).ToLower().StartsWith("p"); //Bit of a hack, but if we find this, we should NOT enable dynamic
                bool hasIndexNoWeightTransform = foundBoneIndices && !foundBoneWeights;

                flvMesh.Dynamic = (byte)((hasIndexNoWeightTransform || normalWBoneTransform) && !isPart ? 0 : 1);

                //TODO DECIDE IF DYNAMIC BASED ON LAYOUT AND ALTER VERTEX WEIGHTS, NORMAL W, BONE INDICES APPROPRIATELY
                flvMesh.Vertices = new List<FLVER.Vertex>();
                flvMesh.VertexIndices = new List<int>();
                flvMesh.DefaultBoneIndex = 0; //Maybe set properly later from the aqp version if important
                flvMesh.BoneIndices = new short[28];
                for (int b = 0; b < 28; b++)
                {
                    if (vtxl.bonePalette.Count > 0)
                    {

                        if (vtxl.bonePalette.Count > b)
                        {
                            flvMesh.BoneIndices[b] = (short)vtxl.bonePalette[b];
                        }
                        else
                        {
                            flvMesh.BoneIndices[b] = -1;
                        }
                    }
                    else
                    {
                        if (aqp.bonePalette.Count > b)
                        {
                            flvMesh.BoneIndices[b] = (short)aqp.bonePalette[b];
                        }
                        else
                        {
                            flvMesh.BoneIndices[b] = -1;
                        }
                    }
                }

                //Handle faces
                //Possibly implement tristripping? 
                flvMesh.UseTristrips = false;
                foreach (var ind in faces.triStrips)
                {
                    flvMesh.VertexIndices.Add(ind);
                }

                //Saves time so we don't have to dynamically find the greatest weight for each set, but only if the mesh needs it
                if (normalWBoneTransform || hasIndexNoWeightTransform)
                {
                    vtxl.SortBoneIndexWeightOrderByWeight();
                }

                //Handle vertices
                int tfmBone = -1;
                for (int v = 0; v < vtxl.vertPositions.Count; v++)
                {
                    var vert = new FLVER.Vertex();

                    //Get the bone for transforms if needed
                    if ((normalWBoneTransform || hasIndexNoWeightTransform) && !isPart)
                    {
                        if (vtxl.vertWeightIndices?.Count > 0 && vtxl.vertWeightIndices[v]?.Length > 0)
                        {
                            tfmBone = vtxl.vertWeightIndices[v][0];
                            if (!aqp.IsNGS)
                            {
                                tfmBone = vtxl.bonePalette[tfmBone];
                            }
                        }
                        else
                        {
                            tfmBone = 0;
                        }
                    }

                    for (int l = 0; l < flvMat.Layouts[0].Count; l++)
                    {
                        switch (flvMat.Layouts[0][l].Semantic)
                        {
                            case FLVER.LayoutSemantic.Position:
                                var pos = vtxl.vertPositions[v];

                                //transform to inverse position if transforming
                                if ((normalWBoneTransform || hasIndexNoWeightTransform) && !isPart)
                                {
                                    var tfm = aqn.nodeList[tfmBone].GetInverseBindPoseMatrix();
                                    pos = Vector3.Transform(pos, tfm);
                                }

                                pos = Vector3.Transform(pos, mirrorMat);
                                vert.Position = pos;

                                //Calc model bounding
                                if (maxBounding == null)
                                {
                                    maxBounding = pos;
                                    minBounding = pos;
                                }
                                else
                                {
                                    maxBounding = MathExtras.GetMaximumBounding(pos, (Vector3)maxBounding);
                                    minBounding = MathExtras.GetMinimumBounding(pos, (Vector3)minBounding);
                                }
                                break;
                            case FLVER.LayoutSemantic.UV:
                                AddUV(vert, vtxl, v);
                                if (flvMat.Layouts[0][l].Type == FLVER.LayoutType.UVPair)
                                {
                                    AddUV(vert, vtxl, v);
                                }
                                break;
                            case FLVER.LayoutSemantic.Normal:
                                if (vtxl.vertNormals.Count > 0)
                                {
                                    var nrm = vtxl.vertNormals[v];
                                    if ((normalWBoneTransform || hasIndexNoWeightTransform) && !isPart)
                                    {
                                        var tfm = aqn.nodeList[tfmBone].GetInverseBindPoseMatrix();
                                        nrm = Vector3.TransformNormal(nrm, tfm);
                                    }
                                    vert.Normal = Vector3.TransformNormal(nrm, mirrorMat);
                                }
                                else
                                {
                                    vert.Normal = Vector3.One;
                                }
                                vert.NormalW = normalWBoneTransform ? tfmBone : 127;
                                break;
                            case FLVER.LayoutSemantic.Tangent:
                                if (vtxl.vertTangentList.Count > 0)
                                {
                                    vert.Tangents.Add(new Vector4(Vector3.Transform(vtxl.vertTangentList[v], mirrorMat), 0));
                                }
                                else
                                {
                                    vert.Tangents.Add(Vector4.One);
                                }
                                break;
                            case FLVER.LayoutSemantic.Bitangent:
                                if (vtxl.vertBinormalList.Count > 0)
                                {
                                    vert.Bitangent = new Vector4(Vector3.Transform(vtxl.vertBinormalList[v], mirrorMat), 0);
                                }
                                else
                                {
                                    vert.Bitangent = Vector4.One;
                                }
                                break;
                            case FLVER.LayoutSemantic.VertexColor:
                                if (vert.Colors.Count > 0)
                                {
                                    if (vtxl.vertColor2s.Count > 0)
                                    {
                                        vert.Colors.Add(new FLVER.VertexColor(vtxl.vertColor2s[v][3], vtxl.vertColor2s[v][2], vtxl.vertColor2s[v][1], vtxl.vertColor2s[v][0]));
                                    }
                                    else
                                    {
                                        vert.Colors.Add(new FLVER.VertexColor(1.0f, 1.0f, 1.0f, 1.0f));
                                    }
                                }
                                else
                                {
                                    if (vtxl.vertColors.Count > 0)
                                    {
                                        vert.Colors.Add(new FLVER.VertexColor(vtxl.vertColors[v][3], vtxl.vertColors[v][2], vtxl.vertColors[v][1], vtxl.vertColors[v][0]));
                                    }
                                    else
                                    {
                                        vert.Colors.Add(new FLVER.VertexColor(1.0f, 1.0f, 1.0f, 1.0f));
                                    }
                                }
                                break;
                            case FLVER.LayoutSemantic.BoneIndices:
                                int[] indices;
                                if (hasIndexNoWeightTransform && tfmBone != -1)
                                {
                                    indices = new int[] { tfmBone, tfmBone, tfmBone, tfmBone };
                                }
                                else if (vtxl.vertWeightIndices.Count == 0)
                                {
                                    indices = new int[4];
                                }
                                else
                                {
                                    indices = vtxl.vertWeightIndices[v];
                                }
                                vert.BoneIndices = new FLVER.VertexBoneIndices() { };
                                vert.BoneIndices[0] = indices[0];
                                vert.BoneIndices[1] = indices[1];
                                vert.BoneIndices[2] = indices[2];
                                vert.BoneIndices[3] = indices[3];

                                int bone0 = indices[0];
                                int bone1 = indices[1];
                                int bone2 = indices[2];
                                int bone3 = indices[3];
                                if (!aqp.IsNGS)
                                {
                                    bone0 = vtxl.bonePalette[bone0];
                                    bone1 = vtxl.bonePalette[bone1];
                                    bone2 = vtxl.bonePalette[bone2];
                                    bone3 = vtxl.bonePalette[bone3];
                                }

                                List<int> boneCheckList = new List<int>
                                {
                                    bone0
                                };
                                if (boneCheckList.Contains(bone1))
                                {
                                    bone1 = -1;
                                }
                                if (boneCheckList.Contains(bone2))
                                {
                                    bone2 = -1;
                                }
                                if (boneCheckList.Contains(bone3))
                                {
                                    bone3 = -1;
                                }

                                //Calc bone bounding. Bone bounding is made up of extents from each vertex with it assigned. 
                                CheckBounds(MaxBoundingBoxByBone, MinBoundingBoxByBone, vert.Position, bone0);
                                CheckBounds(MaxBoundingBoxByBone, MinBoundingBoxByBone, vert.Position, bone1);
                                CheckBounds(MaxBoundingBoxByBone, MinBoundingBoxByBone, vert.Position, bone2);
                                CheckBounds(MaxBoundingBoxByBone, MinBoundingBoxByBone, vert.Position, bone3);
                                break;
                            case FLVER.LayoutSemantic.BoneWeights:
                                Vector4 weights;
                                if (vtxl.vertWeights.Count == 0)
                                {
                                    weights = new Vector4();
                                    weights.X = 1.0f;
                                }
                                else
                                {
                                    weights = vtxl.vertWeights[v];
                                }

                                vert.BoneWeights = new FLVER.VertexBoneWeights() { };
                                vert.BoneWeights[0] = weights.X;
                                vert.BoneWeights[1] = weights.Y;
                                vert.BoneWeights[2] = weights.Z;
                                vert.BoneWeights[3] = weights.W;
                                break;
                        }
                    }

                    flvMesh.Vertices.Add(vert);
                }

                TangentSolver.SolveTangentsDemonsSouls(flvMesh, flver.Header.Version);
                flver.Meshes.Add(flvMesh);
            }

            List<int> rootSiblings = new List<int>();
            flver.Nodes = new List<FLVER.Node>();

            if (boneDataText == null)
            {
                //Set up bones
                for (int i = 0; i < aqn.nodeList.Count; i++)
                {
                    var aqBone = aqn.nodeList[i];
                    Matrix4x4.Invert(aqBone.GetInverseBindPoseMatrix(), out Matrix4x4 aqBoneWorldTfm);
                    aqBoneWorldTfm *= mirrorMat;
                    //aqBoneWorldTfm = MathExtras.SetMatrixScale(aqBoneWorldTfm, new Vector3(1, 1, 1));

                    //Set the inverted transform so when we read it back we can use it for parent calls
                    Matrix4x4.Invert(aqBoneWorldTfm, out Matrix4x4 aqBoneInvWorldTfm);
                    aqBone.SetInverseBindPoseMatrix(aqBoneInvWorldTfm);
                    aqn.nodeList[i] = aqBone;

                    FLVER.Node bone = new FLVER.Node();
                    var name = bone.Name = aqBone.boneName.GetString();
                    bone.BoundingBoxMax = MaxBoundingBoxByBone.ContainsKey(i) ? MaxBoundingBoxByBone[i] : defaultMaxBound;
                    bone.BoundingBoxMin = MinBoundingBoxByBone.ContainsKey(i) ? MinBoundingBoxByBone[i] : defaultMinBound;
                    bone.Flags = bone.Name.ToUpper().EndsWith("NUB") ? (FLVER.Node.NodeFlags)1 : (FLVER.Node.NodeFlags)0;
                    bone.ParentIndex = (short)aqBone.parentId;
                    bone.FirstChildIndex = (short)aqBone.firstChild;
                    bone.PreviousSiblingIndex = (short)GetPreviousSibling(aqn.nodeList, i, rootSiblings);
                    bone.NextSiblingIndex = (short)aqn.nodeList[i].nextSibling;

                    Matrix4x4 localTfm;
                    if (aqBone.parentId == -1)
                    {
                        rootSiblings.Add(i);
                        localTfm = aqBoneWorldTfm;
                    }
                    else
                    {
                        //Calc local transforms
                        //Parent is already mirrored from earlier processing
                        var parBoneInvTfm = aqn.nodeList[aqBone.parentId].GetInverseBindPoseMatrix();
                        localTfm = Matrix4x4.Multiply(aqBoneWorldTfm, parBoneInvTfm);
                    }
                    Matrix4x4.Decompose(localTfm, out var scale, out var rotation, out var translation);

                    bone.Translation = translation;

                    Debug.WriteLine($"{i} Quat {rotation.X} {rotation.Y} {rotation.Z} {rotation.W}");
                    var eulerAngles = MathExtras.GetFlverEulerFromQuaternion_Bone(rotation);
                    var eulerAnglesTest = MathExtras.QuaternionToEulerRadiansTest(rotation);
                    var eulerAnglesMirrorMaybe = MathExtras.GetFlverEulerFromQuaternion_Bone(new Quaternion(-rotation.X, rotation.Y, rotation.Z, -rotation.W));

                    var eulerAnglesold = MathExtras.QuaternionToEulerRadians(rotation, RotationOrder.XZY);
#if DEBUG
                    var eulerAngles2 = MathExtras.QuaternionToEulerRadians(rotation, RotationOrder.XYZ);
                    var eulerAngles3 = MathExtras.QuaternionToEulerRadians(rotation, RotationOrder.YXZ);
                    var eulerAngles4 = MathExtras.QuaternionToEulerRadians(rotation, RotationOrder.YZX);
                    var eulerAngles5 = MathExtras.QuaternionToEulerRadians(rotation, RotationOrder.ZXY);
                    var eulerAngles6 = MathExtras.QuaternionToEulerRadians(rotation, RotationOrder.ZYX);
#endif


                    bone.Rotation = eulerAnglesold;
                    bone.Scale = new Vector3(1, 1, 1);
                    //Debug.WriteLine($"{i} {bone.Rotation.X} {bone.Rotation.Y} {bone.Rotation.Z}");
                    var mat = bone.ComputeLocalTransform();
                    flver.Nodes.Add(bone);
                }
            }
            else
            {
                flver.Nodes = JsonSerializer.Deserialize<List<FLVER.Node>>(boneDataText);
            }

            flver.Header.BoundingBoxMax = (Vector3)maxBounding;
            flver.Header.BoundingBoxMin = (Vector3)minBounding;

            return flver;
        }

        private static int GetPreviousSibling(List<NODE> nodeList, int boneIndex, List<int> rootSiblings)
        {
            //Nothing CAN be before this
            if (boneIndex == 0)
            {
                return -1;
            }

            //Handle root specially; loop through the root siblings
            var curBone = nodeList[boneIndex];
            int currentPreviousSibling = -1;
            if (curBone.parentId == -1)
            {
                for (int i = 0; i < rootSiblings.Count; i++)
                {
                    if (rootSiblings[i] < boneIndex && rootSiblings[i] > currentPreviousSibling)
                    {
                        currentPreviousSibling = rootSiblings[i];
                    }
                }

                return currentPreviousSibling;
            }

            var parBone = nodeList[curBone.parentId];

            //Return early if this is the first child
            if (parBone.firstChild == boneIndex)
            {
                return -1;
            }

            var curSiblingBone = nodeList[parBone.firstChild];
            currentPreviousSibling = parBone.firstChild;

            //Loop through each next sibling since we've set these already
            while (curSiblingBone.nextSibling != -1)
            {
                if (curSiblingBone.nextSibling >= boneIndex)
                {
                    return currentPreviousSibling;
                }
                currentPreviousSibling = curSiblingBone.nextSibling;
                curSiblingBone = nodeList[curSiblingBone.nextSibling];
            }

            return currentPreviousSibling;
        }

        private static void AddUV(FLVER.Vertex vert, VTXL vtxl, int vertId)
        {
            switch (vert.UVs.Count)
            {
                case 0:
                    if (vtxl.uv1List.Count > vertId)
                    {
                        vert.UVs.Add(new Vector3(vtxl.uv1List[vertId], 0));
                    }
                    else
                    {
                        vert.UVs.Add(new Vector3(0, 0, 0));
                    }
                    break;
                case 1:
                    if (vtxl.uv2List.Count > vertId)
                    {
                        vert.UVs.Add(new Vector3(vtxl.uv2List[vertId], 0));
                    }
                    else
                    {
                        vert.UVs.Add(new Vector3(0, 0, 0));
                    }
                    break;
                case 2:
                    if (vtxl.uv3List.Count > vertId)
                    {
                        vert.UVs.Add(new Vector3(vtxl.uv3List[vertId], 0));
                    }
                    else
                    {
                        vert.UVs.Add(new Vector3(0, 0, 0));
                    }
                    break;
                case 3:
                    if (vtxl.uv4List.Count > vertId)
                    {
                        vert.UVs.Add(new Vector3(vtxl.uv4List[vertId], 0));
                    }
                    else
                    {
                        vert.UVs.Add(new Vector3(0, 0, 0));
                    }
                    break;
            }

        }

        private static void CheckBounds(Dictionary<int, Vector3> MaxBoundingBoxByBone, Dictionary<int, Vector3> MinBoundingBoxByBone, Vector3 vec3, int boneId)
        {
            if (boneId != -1 && !MaxBoundingBoxByBone.ContainsKey(boneId))
            {
                MaxBoundingBoxByBone[boneId] = vec3;
                MinBoundingBoxByBone[boneId] = vec3;
            }
            else if (boneId != -1)
            {
                MaxBoundingBoxByBone[boneId] = MathExtras.GetMaximumBounding(MaxBoundingBoxByBone[boneId], vec3);
                MinBoundingBoxByBone[boneId] = MathExtras.GetMinimumBounding(MinBoundingBoxByBone[boneId], vec3);
            }
        }

        public static void GetDeSLayoutMTDInfo(string desPath)
        {
            Dictionary<string, FLVER0.BufferLayout> mtdLayoutsRawDict = new Dictionary<string, FLVER0.BufferLayout>();
            var files = Directory.EnumerateFiles(desPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var loadedFiles = ReadFilesNullable<FLVER0>(file);
                if (loadedFiles != null && loadedFiles.Count > 0)
                {
                    foreach (var fileSet in loadedFiles)
                    {
                        var flver = fileSet.File;

                        foreach (var mat in flver.Materials)
                        {
                            var layout = mat.Layouts[0];
                            if (!mtdLayoutsRawDict.ContainsKey(mat.MTD))
                            {
                                mtdLayoutsRawDict.Add(mat.MTD, layout);
                            }
                        }
                    }
                }

                List<byte> mtdLayoutBytes = new List<byte>();
                mtdLayoutBytes.AddRange(BitConverter.GetBytes(mtdLayoutsRawDict.Count));
                foreach (var entry in mtdLayoutsRawDict)
                {
                    mtdLayoutBytes.AddRange(BitConverter.GetBytes(entry.Key.Length * 2));
                    mtdLayoutBytes.AddRange(UnicodeEncoding.Unicode.GetBytes(entry.Key));
                    mtdLayoutBytes.AddRange(BitConverter.GetBytes(entry.Value.Count));
                    foreach (var layoutEntry in entry.Value)
                    {
                        mtdLayoutBytes.Add((byte)layoutEntry.Type);
                        mtdLayoutBytes.Add((byte)layoutEntry.Semantic);
                    }
                }

                var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                File.WriteAllBytes(Path.Combine(exePath, "DeSMtdLayoutData.bin"), mtdLayoutBytes.ToArray());
            }
        }

        public static Dictionary<string, FLVER0.BufferLayout> ReadMTDLayoutData(string dataPath)
        {
            var layoutsRaw = File.ReadAllBytes(dataPath);
            int offset = 0;
            int entryCount = BitConverter.ToInt32(layoutsRaw, 0);
            offset += 4;
            Dictionary<string, FLVER0.BufferLayout> layouts = new Dictionary<string, FLVER0.BufferLayout>();
            for (int i = 0; i < entryCount; i++)
            {
                int strByteLen = BitConverter.ToInt32(layoutsRaw, offset);
                offset += 4;
                string mtd = Encoding.Unicode.GetString(layoutsRaw, offset, strByteLen).ToLower();
                offset += strByteLen;

                int layoutLen = BitConverter.ToInt32(layoutsRaw, offset);
                offset += 4;
                FLVER0.BufferLayout layout = new FLVER0.BufferLayout();
                for (int j = 0; j < layoutLen; j++)
                {
                    byte type = layoutsRaw[offset];
                    offset += 1;
                    byte semantic = layoutsRaw[offset];
                    offset += 1;

                    layout.Add(new FLVER.LayoutMember((FLVER.LayoutType)type, (FLVER.LayoutSemantic)semantic, j, 0));
                }
                if (!layouts.ContainsKey(Path.GetFileName(mtd)))
                {
                    layouts.Add(Path.GetFileName(mtd), layout);
                }
            }

            return layouts;
        }

        public class URIFlver0Pair
        {
            public string Uri { get; set; }
            public FLVER0 File { get; set; }
        }

        public static List<URIFlver0Pair> ReadFilesNullable<TFormat>(string path)
    where TFormat : SoulsFile<TFormat>, new()
        {
            if (BND3.Is(path))
            {
                var bnd3 = BND3.Read(path);
                var selected = bnd3.Files.Where(f => Path.GetExtension(f.Name) == ".flver");
                List<URIFlver0Pair> Files = new List<URIFlver0Pair>();
                foreach (var file in bnd3.Files)
                {
                    if (Path.GetExtension(file.Name) == ".flver")
                    {
                        Files.Add(new URIFlver0Pair() { Uri = file.Name, File = SoulsFile<FLVER0>.Read(file.Bytes) });
                    }
                }
                return Files;
            }
            else if (BND4.Is(path))
            {
                var bnd4 = BND4.Read(path);
                var selected = bnd4.Files.Where(f => Path.GetExtension(f.Name) == ".flver");
                List<URIFlver0Pair> Files = new List<URIFlver0Pair>();
                foreach (var file in bnd4.Files)
                {
                    if (Path.GetExtension(file.Name) == ".flver")
                    {
                        Files.Add(new URIFlver0Pair() { Uri = file.Name, File = SoulsFile<FLVER0>.Read(file.Bytes) });
                    }
                }
                return Files;
            }
            else
            {
                var file = File.ReadAllBytes(path);
                if (FLVER0.Is(file))
                {
                    return new List<URIFlver0Pair>() { new URIFlver0Pair() { Uri = path, File = SoulsFile<FLVER0>.Read(file) } };
                }
                return null;
            }

        }

        public static bool IsMaterialUsingSkinning(FLVER0.Material mat)
        {
            for (int i = 0; i < mat.Layouts.Count; i++)
            {
                //HOPEFULLY this should always be 0. If not, pain
                if (mat.Layouts[0][i].Semantic == FLVER.LayoutSemantic.BoneWeights)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
