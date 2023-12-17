using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace AquaModelLibrary.Extra.Ninja.BillyHatcher
{
    public class MC2Convert
    {

        public static NGSAquaObject ConvertMC2(byte[] file, out AquaNode aqn)
        {
            using (Stream stream = (Stream)new MemoryStream(file))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                return MC2ToAqua(new MC2(streamReader), out aqn);
            }
        }

        public static NGSAquaObject MC2ToAqua(MC2 mc2, out AquaNode aqn)
        {
            NGSAquaObject aqp = new NGSAquaObject();
            aqn = AquaNode.GenerateBasicAQN();

            aqn.ndtr.boneCount = aqn.nodeList.Count;
            aqp.objc.bonePaletteOffset = 1;


            //Separate out to meshes by flag combos
            int i = 0;
            Dictionary<string, List<MC2.MC2FaceData>> meshDict = new Dictionary<string, List<MC2.MC2FaceData>>();
            for (int triId = 0; triId < mc2.faceData.Count; triId++)
            {
                var tri = mc2.faceData[triId];
                string name = $"mat";

                //Flags
                foreach (var flag in Enum.GetValues(typeof(MC2.FlagSet0)))
                {
                    if ((tri.flagSet0 & (MC2.FlagSet0)flag) > 0)
                    {
                        name += $"#{flag}";
                    }
                }
                foreach (var flag in Enum.GetValues(typeof(MC2.FlagSet1)))
                {
                    if ((tri.flagSet1 & (MC2.FlagSet1)flag) > 0)
                    {
                        name += $"#{flag}";
                    }
                }
                foreach (var flag in Enum.GetValues(typeof(MC2.FlagSet2)))
                {
                    if ((tri.flagSet2 & (MC2.FlagSet2)flag) > 0)
                    {
                        name += $"#{flag}";
                    }
                }

                if (!meshDict.ContainsKey(name))
                {
                    meshDict.Add(name, new List<MC2.MC2FaceData>());
                }
                meshDict[name].Add(tri);

                i++;
            }

            //Assemble Meshes
            int m = 0;
            foreach (var pair in meshDict)
            {
                Dictionary<int, int> vertIndexRemap = new Dictionary<int, int>();
                AquaObject.GenericTriangles genMesh = new AquaObject.GenericTriangles();
                var genMat = new AquaObject.GenericMaterial();
                genMesh.triList = new List<Vector3>();
                genMat.matName = pair.Key;
                genMat.diffuseRGBA = new Vector4(1, 1, 1, 1);
                int f = 0;

                foreach (var tri in pair.Value)
                {
                    AquaObject.VTXL faceVtxl = new AquaObject.VTXL();
                    faceVtxl.rawFaceId.Add(f);
                    faceVtxl.rawFaceId.Add(f);
                    faceVtxl.rawFaceId.Add(f++);

                    if (!vertIndexRemap.ContainsKey(tri.vert0))
                    {
                        vertIndexRemap.Add(tri.vert0, genMesh.vertCount++);
                    }
                    if (!vertIndexRemap.ContainsKey(tri.vert1))
                    {
                        vertIndexRemap.Add(tri.vert1, genMesh.vertCount++);
                    }
                    if (!vertIndexRemap.ContainsKey(tri.vert2))
                    {
                        vertIndexRemap.Add(tri.vert2, genMesh.vertCount++);
                    }
                    faceVtxl.vertPositions.Add(mc2.vertPositions[tri.vert0]);
                    faceVtxl.vertPositions.Add(mc2.vertPositions[tri.vert1]);
                    faceVtxl.vertPositions.Add(mc2.vertPositions[tri.vert2]);
                    genMesh.matIdList.Add(aqp.tempMats.Count);
                    genMesh.triList.Add(new Vector3(vertIndexRemap[tri.vert0], vertIndexRemap[tri.vert1], vertIndexRemap[tri.vert2]));
                    faceVtxl.rawVertId.Add(vertIndexRemap[tri.vert0]);
                    faceVtxl.rawVertId.Add(vertIndexRemap[tri.vert1]);
                    faceVtxl.rawVertId.Add(vertIndexRemap[tri.vert2]);

                    genMesh.faceVerts.Add(faceVtxl);
                }

                aqp.tempMats.Add(genMat);
                aqp.tempTris.Add(genMesh);

                m++;
            }

            return aqp;
        }

        /// <summary>
        /// Facecount limited by shorts or ushorts (needs testing).
        /// </summary>
        public static MC2 ConvertToMC2(string initialFilePath)
        {
            MC2 mc2 = new MC2();
            var scene = ModelImporter.GetAssimpScene(initialFilePath, Assimp.PostProcessSteps.Triangulate | Assimp.PostProcessSteps.JoinIdenticalVertices | Assimp.PostProcessSteps.FlipUVs);
            var baseScale = ModelImporter.SetAssimpScale(scene);

            Vector3 rootBoxMinExtents = new Vector3(scene.Meshes[0].Vertices[0].X, scene.Meshes[0].Vertices[0].Y, scene.Meshes[0].Vertices[0].Z) * baseScale;
            Vector3 rootBoxMaxExtents = rootBoxMinExtents;
            for (int i = 0; i < scene.MeshCount; i++)
            {
                var mesh = scene.Meshes[i];
                Dictionary<int, int> vertIndexRemapper = new Dictionary<int, int>(); //For reassigning vertex ids in faces after they've been combined.

                for (int v = 0; v < mesh.VertexCount; v++)
                {
                    var vert = mesh.Vertices[v] * baseScale;

                    //Min extents
                    if (rootBoxMinExtents.X > vert.X)
                    {
                        rootBoxMinExtents.X = vert.X;
                    }
                    if (rootBoxMinExtents.Z > vert.Z)
                    {
                        rootBoxMinExtents.Z = vert.Z;
                    }

                    //Max extents
                    if (rootBoxMaxExtents.X < vert.X)
                    {
                        rootBoxMaxExtents.X = vert.X;
                    }
                    if (rootBoxMaxExtents.Z < vert.Z)
                    {
                        rootBoxMaxExtents.Z = vert.Z;
                    }

                    //Combine and remap repeated vertices as needed.
                    //Assimp splits things by material by necessity and so we need to recombine them.
                    var vertData = new Vector3(vert.X, vert.Y, vert.Z);
                    bool foundDuplicateVert = false;
                    for (int vt = 0; vt < mc2.vertPositions.Count; vt++)
                    {
                        if (vertData == mc2.vertPositions[vt])
                        {
                            foundDuplicateVert = true;
                            vertIndexRemapper.Add(v, vt);
                            break;
                        }
                    }
                    if (!foundDuplicateVert)
                    {
                        vertIndexRemapper.Add(v, mc2.vertPositions.Count);
                        mc2.vertPositions.Add(vertData);
                    }
                }

                var flagsSplit = scene.Materials[mesh.MaterialIndex].Name.Split('#');
                MC2.FlagSet0 flags0 = new MC2.FlagSet0();
                MC2.FlagSet1 flags1 = new MC2.FlagSet1();
                MC2.FlagSet2 flags2 = new MC2.FlagSet2();

                if (flagsSplit.Length > 1)
                {
                    for (int f = 1; f < flagsSplit.Length; f++)
                    {
                        switch (flagsSplit[f].ToLower())
                        {
                            case "defaultground":
                                flags1 |= MC2.FlagSet1.DefaultGround;
                                break;
                            case "unk1_0x2":
                                flags1 |= MC2.FlagSet1.Unk1_0x2;
                                break;
                            case "drown":
                                flags1 |= MC2.FlagSet1.Drown;
                                break;
                            case "quicksand":
                                flags1 |= MC2.FlagSet1.Quicksand;
                                break;
                            case "unk1_0x10":
                                flags1 |= MC2.FlagSet1.Unk1_0x10;
                                break;
                            case "unk1_0x20":
                                flags1 |= MC2.FlagSet1.Unk1_0x20;
                                break;
                            case "unk1_0x40":
                                flags1 |= MC2.FlagSet1.Unk1_0x40;
                                break;
                            case "snow":
                                flags1 |= MC2.FlagSet1.Snow;
                                break;
                            case "lava":
                                flags0 |= MC2.FlagSet0.Lava;
                                break;
                            case "unk0_0x2":
                                flags0 |= MC2.FlagSet0.Unk0_0x2;
                                break;
                            case "slide":
                                flags0 |= MC2.FlagSet0.Slide;
                                break;
                            case "unk0_0x8":
                                flags0 |= MC2.FlagSet0.Unk0_0x8;
                                break;
                            case "unk0_0x10":
                                flags0 |= MC2.FlagSet0.Unk0_0x10;
                                break;
                            case "nobillyandeggcollision":
                                flags0 |= MC2.FlagSet0.NoBillyAndEggCollision;
                                break;
                            case "unk0_0x40":
                                flags0 |= MC2.FlagSet0.Unk0_0x40;
                                break;
                            case "death":
                                flags0 |= MC2.FlagSet0.Death;
                                break;
                            case "unk2_0x1":
                                flags2 |= MC2.FlagSet2.Unk2_0x1;
                                break;
                            case "unk2_0x2":
                                flags2 |= MC2.FlagSet2.Unk2_0x2;
                                break;
                            case "unk2_0x4":
                                flags2 |= MC2.FlagSet2.Unk2_0x4;
                                break;
                            case "unk2_0x8":
                                flags2 |= MC2.FlagSet2.Unk2_0x8;
                                break;
                            case "unk2_0x10":
                                flags2 |= MC2.FlagSet2.Unk2_0x10;
                                break;
                            case "unk2_0x20":
                                flags2 |= MC2.FlagSet2.Unk2_0x20;
                                break;
                            case "unk2_0x40":
                                flags2 |= MC2.FlagSet2.Unk2_0x40;
                                break;
                            case "unk2_0x80":
                                flags2 |= MC2.FlagSet2.Unk2_0x80;
                                break;
                        }
                    }
                }

                //Default ground if no flags
                if (flags0 == 0 && flags1 == 0 && flags2 == 0)
                {
                    flags1 = MC2.FlagSet1.DefaultGround;
                }

                for (int f = 0; f < mesh.FaceCount; f++)
                {
                    var face = mesh.Faces[f];
                    var tri = new MC2.MC2FaceData();
                    tri.flagSet0 = flags0;
                    tri.flagSet1 = flags1;
                    tri.flagSet2 = flags2;

                    //Ensure we remap vert indices to their new, combined ids as needed
                    tri.vert0 = (ushort)(vertIndexRemapper.ContainsKey(face.Indices[0]) ? vertIndexRemapper[face.Indices[0]] : face.Indices[0]);
                    tri.vert1 = (ushort)(vertIndexRemapper.ContainsKey(face.Indices[1]) ? vertIndexRemapper[face.Indices[1]] : face.Indices[1]);
                    tri.vert2 = (ushort)(vertIndexRemapper.ContainsKey(face.Indices[2]) ? vertIndexRemapper[face.Indices[2]] : face.Indices[2]);
                    tri.vert0Value = mc2.vertPositions[tri.vert0];
                    tri.vert1Value = mc2.vertPositions[tri.vert1];
                    tri.vert2Value = mc2.vertPositions[tri.vert2];

                    //Calculate face normal
                    Vector3 u = mc2.vertPositions[tri.vert1] - mc2.vertPositions[tri.vert0];
                    Vector3 v = mc2.vertPositions[tri.vert2] - mc2.vertPositions[tri.vert0];
                    tri.faceNormal = Vector3.Normalize(Vector3.Cross(u, v));

                    mc2.faceData.Add(tri);
                }
            }

            List<ushort> faceIndices = new List<ushort>();
            for (int i = 0; i < mc2.faceData.Count; i++)
            {
                faceIndices.Add((ushort)i);
            }

            mc2.PopulateFaceBounds();
            mc2.rootSector = mc2.SubdivideSector(new Vector2(rootBoxMinExtents.X, rootBoxMaxExtents.X), new Vector2(rootBoxMinExtents.Z, rootBoxMaxExtents.Z), faceIndices, 0);
            mc2.header.maxDepth = (ushort)MC2.maxDepth;
            mc2.header.ushort3 = 0x14;

            return mc2;
        }

    }
}
