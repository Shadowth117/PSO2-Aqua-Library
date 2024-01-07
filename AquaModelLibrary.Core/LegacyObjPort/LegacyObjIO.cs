//Based on process from Aqp2obj (Name is a misnomer, program did partial conversions both ways via use of intermediary files)
//Mostly reimplemented based on the Aqua Model Library codebase

using AquaModelLibrary.Data.LegacyObj;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace AquaModelLibrary.Core.LegacyObjPort
{
    public static class LegacyObjIO
    {
        public static Regex RE_ObjName = new Regex(@"^.*_(\d+)_(\d+)_(\d+)_(\d+)$");

        public struct SkinData
        {
            public Vector3 pos;
            public int[] indices;
            public Vector4 weights;
        }

        //Import obj data, gather and reconstruct weighting data, reconstruct model with new geometry data, delete LOD models loaded
        public static AquaObject ImportObj(string fileName, AquaObject aqo)
        {
            bool doBitangent = false;
            List<SkinData> skinData = new List<SkinData>();
            aqo.OptimizeBonePalettes();

            //Add a local version of the global bonepalette. We're reworking things so we need to do this.
            for (int i = 0; i < aqo.vtxlList.Count; i++)
            {
                aqo.vtxlList[i].bonePalette = new List<ushort>();
                for (int b = 0; b < aqo.bonePalette.Count; b++)
                {
                    aqo.vtxlList[i].bonePalette.Add((ushort)aqo.bonePalette[b]);
                }
            }

            //Assign skin data for comparison later and make it relativitize it to the GlobalBonePalette
            if (aqo.vtxlList[0].vertWeights.Count > 0)
            {
                foreach (var vtxl in aqo.vtxlList)
                {
                    for (int i = 0; i < vtxl.vertPositions.Count; i++)
                    {
                        SkinData skin = new SkinData();
                        skin.pos = vtxl.vertPositions[i];
                        List<int> indices = new List<int>(new int[4]);
                        for (int id = 0; id < 4; id++)
                        {
                            var temp = aqo.bonePalette.IndexOf(vtxl.bonePalette[vtxl.vertWeightIndices[i][id]]);
                            if (indices.Contains(temp)) //Repeats should only occur for index 0
                            {
                                temp = 0;
                            }
                            indices[id] = temp;
                        }
                        skin.indices = indices.ToArray();
                        skin.weights = VTXL.SumWeightsTo1(vtxl.vertWeights[i]);
                        skinData.Add(skin);
                    }
                }
            }

            if (aqo.vtxlList[0].vertBinormalList.Count > 0)
            {
                doBitangent = true;
            }

            //House cleaning since we can't really redo these this way
            aqo.tempTris.Clear();
            aqo.strips3.Clear();
            aqo.strips2.Clear();
            aqo.strips.Clear();
            aqo.pset2List.Clear();
            aqo.psetList.Clear();
            aqo.mesh2List.Clear();
            aqo.strips3Lengths.Clear();
            aqo.objc.pset2Count = 0;
            aqo.objc.mesh2Count = 0;

            var obj = ObjFile.FromFile(fileName);
            var subMeshes = obj.Meshes.SelectMany(i => i.SubMeshes).ToArray();
            var oldMESHList = aqo.meshList;
            aqo.meshList = new List<MESH>();
            int totalStripsShorts = 0;
            int totalVerts = 0;
            int boneLimit;
            AquaObject tempModel;

            if (aqo.objc.type >= 0xC32)
            {
                tempModel = new AquaObject();
                boneLimit = 255;
            }
            else
            {
                tempModel = new AquaObject();
                boneLimit = 16;
            }

            //Assemble face and vertex data. Vert data is stored with faces so that it can be split as needed for proper UV mapping etc.
            foreach (var mesh in subMeshes)
            {
                var tempMesh = new GenericTriangles();
                tempMesh.name = mesh.Name;
                tempMesh.bonePalette = aqo.bonePalette;
                List<int> vertIds = new List<int>();
                Dictionary<int, int> vertIdRemap = new Dictionary<int, int>();
                int greatestId = 0;

                for (int f = 0; f < mesh.PositionFaces.Count; ++f)
                {
                    var pf = mesh.PositionFaces[f];
                    var nf = mesh.NormalFaces[f];
                    var tf = mesh.TexCoordFaces[f];

                    tempMesh.triList.Add(new Vector3(pf.A, pf.B, pf.C)); //faces are 1 based
                    if (!vertIds.Contains(pf.A))
                    {
                        greatestId = pf.A > greatestId ? pf.A : greatestId;
                        vertIds.Add(pf.A);
                    }
                    if (!vertIds.Contains(pf.B))
                    {
                        greatestId = pf.B > greatestId ? pf.B : greatestId;
                        vertIds.Add(pf.B);
                    }
                    if (!vertIds.Contains(pf.C))
                    {
                        greatestId = pf.C > greatestId ? pf.C : greatestId;
                        vertIds.Add(pf.C);
                    }
                    tempMesh.matIdList.Add(0);

                    var vtxl = new VTXL();
                    vtxl.rawVertId = new List<int>() { pf.A, pf.B, pf.C };
                    vtxl.rawFaceId = new List<int>() { f, f, f };

                    //Undo scaling and rotate to Y up
                    vtxl.vertPositions = new List<Vector3>() { new Vector3(obj.Positions[pf.A].X / 100, obj.Positions[pf.A].Z / 100, -obj.Positions[pf.A].Y / 100),
                         new Vector3(obj.Positions[pf.B].X / 100, obj.Positions[pf.B].Z / 100, -obj.Positions[pf.B].Y / 100),
                         new Vector3(obj.Positions[pf.C].X / 100, obj.Positions[pf.C].Z / 100, -obj.Positions[pf.C].Y / 100) };
                    vtxl.vertNormals = new List<Vector3>() { new Vector3(obj.Normals[nf.A].X, obj.Normals[nf.A].Z, -obj.Normals[nf.A].Y),
                        new Vector3(obj.Normals[nf.B].X, obj.Normals[nf.B].Z, -obj.Normals[nf.B].Y),
                        new Vector3(obj.Normals[nf.C].X, obj.Normals[nf.C].Z, -obj.Normals[nf.C].Y)};

                    vtxl.uv1List = new List<Vector2>() { new Vector2(obj.TexCoords[tf.A].X, -obj.TexCoords[tf.A].Y), new Vector2(obj.TexCoords[tf.B].X, -obj.TexCoords[tf.B].Y),
                        new Vector2(obj.TexCoords[tf.C].X, -obj.TexCoords[tf.C].Y),};

                    if (aqo.vtxlList[0].vertWeights.Count > 0)
                    {
                        //Autoskin magic - Essentially, iterate through and get weight values from the closest match to the original 
                        for (int vt = 0; vt < vtxl.vertPositions.Count; vt++)
                        {
                            var maxDistance = float.MaxValue;
                            vtxl.vertWeights.Add(new Vector4());
                            vtxl.vertWeightIndices.Add(null);

                            SkinData tempWeight = new SkinData();
                            //Go through each original vertex and compare distances. Adjust references if it's closer
                            foreach (var skin in skinData)
                            {
                                var distance = (vtxl.vertPositions[vt] - skin.pos).LengthSquared(); //Not a full true distance, just to be quicker

                                if (distance < maxDistance)
                                {
                                    maxDistance = distance;
                                    tempWeight = skin;
                                    if (distance == 0)
                                    {
                                        break;
                                    }
                                }
                            }
                            vtxl.vertWeights[vt] = tempWeight.weights;
                            vtxl.vertWeightIndices[vt] = tempWeight.indices;

                            if (aqo.vtxlList[0].vertWeightsNGS.Count > 0)
                            {
                                ushort[] shortWeights = new ushort[] { (ushort)(tempWeight.weights.X * ushort.MaxValue), (ushort)(tempWeight.weights.Y * ushort.MaxValue),
                                    (ushort)(tempWeight.weights.Z * ushort.MaxValue), (ushort)(tempWeight.weights.W * ushort.MaxValue), };
                                vtxl.vertWeightsNGS.Add(shortWeights);
                            }
                        }
                    }
                    tempMesh.faceVerts.Add(vtxl);
                }

                //Set up remapp ids
                for (int i = 0; i < vertIds.Count; i++)
                {
                    var id = vertIds[i];
                    vertIdRemap.Add(id, i);
                }

                //Remap Ids to verts
                for (int f = 0; f < tempMesh.faceVerts.Count; f++)
                {
                    for (int v = 0; v < tempMesh.faceVerts[f].rawVertId.Count; v++)
                    {
                        tempMesh.faceVerts[f].rawVertId[v] = vertIdRemap[tempMesh.faceVerts[f].rawVertId[v]];
                    }
                }

                //Remap Ids to face ids
                for (int f = 0; f < tempMesh.triList.Count; f++)
                {
                    var tri = tempMesh.triList[f];
                    if (vertIdRemap.ContainsKey((int)tri.X))
                    {
                        tri.X = vertIdRemap[(int)tri.X];
                    }
                    if (vertIdRemap.ContainsKey((int)tri.Y))
                    {
                        tri.Y = vertIdRemap[(int)tri.Y];
                    }
                    if (vertIdRemap.ContainsKey((int)tri.Z))
                    {
                        tri.Z = vertIdRemap[(int)tri.Z];
                    }

                    tempMesh.triList[f] = tri;
                }

                tempMesh.vertCount = vertIds.Count;
                totalVerts += vertIds.Count;

                aqo.tempTris.Add(tempMesh);
            }
            aqo.VTXLFromFaceVerts();
            if (aqo.objc.type < 0xC32)
            {
                aqo.BatchSplitByBoneCountTempData(boneLimit);
                aqo.tempTris = tempModel.tempTris;
                aqo.vtxlList = tempModel.vtxlList;
                tempModel = null;

                aqo.OptimizeBonePalettes();
            }

            //AquaObjectMethods.CalcUNRMs(aqo, aqo.applyNormalAveraging, aqo.objc.unrmOffset != 0);

            //Set up PSETs and strips, and other per mesh data
            for (int i = 0; i < aqo.tempTris.Count; i++)
            {
                //strips
                StripData strips;
                if (aqo.objc.type >= 0xC31)
                {
                    strips = new StripData();
                    strips.format0xC31 = true;
                    strips.triStrips = new List<ushort>(aqo.tempTris[i].toUshortArray());
                    strips.triIdCount = strips.triStrips.Count;
                    strips.faceGroups.Add(strips.triStrips.Count);
                }
                else
                {
                    strips = new StripData(aqo.tempTris[i].toUshortArray());
                }
                aqo.strips.Add(strips);

                //PSET
                var pset = new PSET();
                pset.faceGroupCount = 0x1;
                pset.psetFaceCount = strips.triIdCount;
                if (aqo.objc.type >= 0xC31)
                {
                    pset.tag = 0x1000;
                    pset.stripStartCount = totalStripsShorts;
                }
                else
                {
                    pset.tag = 0x2100;
                }
                aqo.psetList.Add(pset);
                totalStripsShorts += strips.triIdCount; //Update this *after* setting the strip start count so that we don't direct to bad data.

                //MESH
                Match m;
                int idx_MATE = 0;
                int idx_REND = 0;
                int idx_SHAD = 0;
                int idx_TSET = 0;
                if (null == aqo.tempTris[i].name)
                {
                    m = null;
                }
                else
                {
                    m = RE_ObjName.Match(aqo.tempTris[i].name);
                    if (!m.Success)
                    {
                        m = null;
                    }
                    else
                    {
                        idx_MATE = int.Parse(m.Groups[1].Value);
                        idx_REND = int.Parse(m.Groups[2].Value);
                        idx_SHAD = int.Parse(m.Groups[3].Value);
                        idx_TSET = int.Parse(m.Groups[4].Value);
                    }
                }
                if (m == null)
                {
                    idx_MATE = oldMESHList[0].mateIndex;
                    idx_REND = oldMESHList[0].rendIndex;
                    idx_SHAD = oldMESHList[0].shadIndex;
                    idx_TSET = oldMESHList[0].tsetIndex;
                }

                var mesh = new MESH();
                MESH oldMesh = new MESH();
                bool oldMeshFound = false;

                //Compare
                for (int msh = 0; msh < oldMESHList.Count; msh++)
                {
                    var tempMesh = oldMESHList[msh];
                    if (tempMesh.mateIndex == idx_MATE && tempMesh.rendIndex == idx_REND && tempMesh.shadIndex == idx_SHAD && tempMesh.tsetIndex == idx_TSET)
                    {
                        oldMesh = tempMesh;
                        oldMeshFound = true;
                        break;
                    }
                }

                if (oldMeshFound == false)
                {
                    mesh.flags = 0x17; //No idea what this really does. Seems to vary a lot, but also not matter a lot.
                    mesh.unkShort0 = 0x0;
                    mesh.unkByte0 = 0x80;
                    mesh.unkByte1 = 0x64;
                    mesh.unkShort1 = 0;
                    mesh.mateIndex = idx_MATE;
                    mesh.rendIndex = idx_REND;
                    mesh.shadIndex = idx_SHAD;
                    mesh.tsetIndex = idx_TSET;
                    mesh.baseMeshNodeId = 0;
                    mesh.baseMeshDummyId = 0;
                    mesh.unkInt0 = 0;
                }
                else
                {
                    mesh.flags = oldMesh.flags;
                    mesh.unkShort0 = oldMesh.unkShort0;
                    mesh.unkByte0 = oldMesh.unkByte0;
                    mesh.unkByte1 = oldMesh.unkByte1;
                    mesh.unkShort1 = oldMesh.unkShort1;
                    mesh.mateIndex = idx_MATE;
                    mesh.rendIndex = idx_REND;
                    mesh.shadIndex = idx_SHAD;
                    mesh.tsetIndex = idx_TSET;
                    mesh.baseMeshNodeId = oldMesh.baseMeshNodeId;
                    mesh.baseMeshDummyId = oldMesh.baseMeshDummyId;
                    mesh.unkInt0 = oldMesh.unkInt0;
                }
                mesh.vsetIndex = i;
                mesh.psetIndex = i;
                mesh.reserve0 = 0;
                aqo.meshList.Add(mesh);
            }

            //Generate VTXEs and VSETs
            int largestVertSize = 0;
            int vertCounter = 0;
            totalVerts = 0;
            aqo.vsetList.Clear();
            aqo.vtxeList.Clear();
            for (int i = 0; i < aqo.vtxlList.Count; i++)
            {
                totalVerts += aqo.vtxlList[i].vertPositions.Count;
                VTXE vtxe = VTXE.ConstructFromVTXL(aqo.vtxlList[i], out int size);
                aqo.vtxeList.Add(vtxe);

                //Track this for objc
                if (size > largestVertSize)
                {
                    largestVertSize = size;
                }

                VSET vset = new VSET();
                vset.vertDataSize = size;
                vset.vtxlCount = aqo.vtxlList[i].vertPositions.Count;
                vset.edgeVertsCount = aqo.vtxlList[i].edgeVerts.Count;

                if (aqo.objc.type >= 0xC32)
                {
                    vset.vtxeCount = aqo.vtxeList.Count - 1;
                    vset.vtxlStartVert = vertCounter;
                    vertCounter += vset.vtxlCount;

                    vset.bonePaletteCount = -1;
                }
                else
                {
                    vset.vtxeCount = vtxe.vertDataTypes.Count;
                    vset.bonePaletteCount = aqo.vtxlList[i].bonePalette.Count;
                }

                aqo.vsetList.Add(vset);
            }

            //Update OBJC
            aqo.objc.largetsVtxl = largestVertSize;
            aqo.objc.totalStripFaces = totalStripsShorts;
            aqo.objc.totalVTXLCount = totalVerts;
            aqo.objc.unkStructCount = aqo.vtxlList.Count;
            aqo.objc.vsetCount = aqo.vsetList.Count;
            aqo.objc.psetCount = aqo.psetList.Count;
            aqo.objc.meshCount = aqo.meshList.Count;
            aqo.objc.mateCount = aqo.mateList.Count;
            aqo.objc.rendCount = aqo.rendList.Count;
            aqo.objc.shadCount = aqo.shadList.Count;
            aqo.objc.tstaCount = aqo.tstaList.Count;
            aqo.objc.tsetCount = aqo.tsetList.Count;
            aqo.objc.texfCount = aqo.texfList.Count;
            aqo.objc.vtxeCount = aqo.vtxeList.Count;
            aqo.objc.fBlock0 = -1;
            aqo.objc.fBlock1 = -1;
            aqo.objc.fBlock2 = -1;
            aqo.objc.fBlock3 = -1;
            aqo.objc.globalStrip3LengthCount = 1;
            aqo.objc.unkCount3 = 1;
            aqo.objc.bounds = new BoundingVolume(aqo.vtxlList);
            if (doBitangent)
            {
                aqo.ComputeTangentSpace(false, true);
            }

            return aqo;
        }

        public static void ExportObj(string fileName, AquaObject aqo)
        {
            //We have to split these if this is an NGS model because obj only supports one material per mesh
            if (aqo.objc.type >= 0xC32)
            {
                aqo.splitVSETPerMesh();
            }
            //Running this this way ensures we have normals to grab.
            aqo.ComputeTangentSpace(false, false);

            //Ensure there's UV data, even if it's not actually used.
            for (int i = 0; i < aqo.vtxlList.Count; i++)
            {
                if (aqo.vtxlList[i].uv1List == null || aqo.vtxlList[i].uv1List.Count == 0)
                {
                    aqo.vtxlList[i].uv1List = new List<Vector2>(new Vector2[aqo.vtxlList[i].vertPositions.Count]);
                }
            }

            var mtlfile = Path.ChangeExtension(fileName, ".mtl");
            var mtls = ExportMtl(mtlfile, aqo);
            mtlfile = Path.GetFileName(mtlfile);

            using (var w = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                w.WriteLine("# {0}", Path.GetFileName(fileName));
                w.WriteLine("mtllib {0}", Path.GetFileName(mtlfile));
                w.WriteLine("");

                StringBuilder pos = new StringBuilder();
                StringBuilder nrm = new StringBuilder();
                StringBuilder tex = new StringBuilder();
                StringBuilder faces = new StringBuilder();

                var offpos = 1;
                for (int mshId = 0; mshId < aqo.meshList.Count; mshId++)
                {
                    var mesh = aqo.meshList[mshId];
                    foreach (var i in aqo.vtxlList[mesh.vsetIndex].vertPositions)
                        pos.AppendLine(String.Format("v  {0:F8} {1:F8} {2:F8}", i.X * 100, -i.Z * 100, i.Y * 100));

                    foreach (var i in aqo.vtxlList[mesh.vsetIndex].vertNormals)
                        nrm.AppendLine(String.Format("vn  {0:F8} {1:F8} {2:F8}", i.X, -i.Z, i.Y));

                    foreach (var i in aqo.vtxlList[mesh.vsetIndex].uv1List)
                        tex.AppendLine(String.Format("vt  {0:F8} {1:F8} {2:F8}", i.X, -i.Y, 0));

                    var meshName = string.Format("mesh_{0}_{1}_{2}_{3}", mesh.mateIndex, mesh.rendIndex, mesh.shadIndex, mesh.tsetIndex);
                    faces.AppendLine("");
                    faces.AppendLine(String.Format("o {0}", meshName));
                    faces.AppendLine(String.Format("g {0}", meshName));
                    faces.AppendLine(String.Format("usemtl {0}", mtls[mshId]));
                    faces.AppendLine(String.Format("s {0}", 0));

                    //Write faces
                    var tris = aqo.strips[mesh.psetIndex].GetTriangles();
                    foreach (var tri in tris)
                    {
                        faces.AppendLine(String.Format("f {0}/{2}/{1} {3}/{5}/{4} {6}/{8}/{7}",
                            tri.X + offpos, tri.X + offpos, tri.X + offpos,
                            tri.Y + offpos, tri.Y + offpos, tri.Y + offpos,
                            tri.Z + offpos, tri.Z + offpos, tri.Z + offpos));
                    }

                    offpos += aqo.vtxlList[mesh.vsetIndex].vertPositions.Count;
                }
                pos.AppendLine();
                nrm.AppendLine();
                tex.AppendLine();

                w.Write(pos);
                w.Write(nrm);
                w.Write(tex);

                w.Write(faces);

                w.Flush();
                w.BaseStream.SetLength(w.BaseStream.Position);
            }
        }

        //Writes a material per mesh. Metadata values after the material name are the material index and texture set index in that order
        public static List<string> ExportMtl(string fileName, AquaObject aqo)
        {
            List<string> mtlNames = new List<string>();

            using (var w = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                w.WriteLine("# {0}", Path.GetFileName(fileName));

                foreach (var mesh in aqo.meshList)
                {
                    var mate = aqo.mateList[mesh.mateIndex];
                    var texNames = aqo.GetTexListNamesUnicode(mesh.tsetIndex);

                    var mtlname = mate.matName.GetString() + string.Format("_mtl_{0}_{1}", mesh.mateIndex, mesh.tsetIndex); //Added the actual name because that's useful
                    mtlNames.Add(mtlname);

                    w.WriteLine("newmtl {0}", mtlname);
                    w.WriteLine("	Ka 0.00000000 0.00000000 0.00000000");
                    w.WriteLine($"	Kd {mate.diffuseRGBA.X:F8} {mate.diffuseRGBA.Y:F8} {mate.diffuseRGBA.Z:F8}"); //Not originally based on actual data, but why not
                    w.WriteLine("	Ks 0.50000000 0.50000000 0.50000000");
                    w.WriteLine("	Ke 0.00000000 0.00000000 0.00000000");

                    if (texNames != null && texNames.Count > 0) w.WriteLine("	map_Kd   {0}", texNames[0]);

                    //Texture usage order based albedo/diffuse is wildly inconsistent, and therefore not written out as in the original.
                    //if(0xFFFFFFFF != (tidx= tset._8875[1])) w.WriteLine("	map_Ks   {0}", GetTexFile(TSTA.Data[tidx]._026C, dir));
                    //if(0xFFFFFFFF != (tidx= tset._8875[2])) w.WriteLine("	map_refl {0}", GetTexFile(TSTA.Data[tidx]._026C, dir));
                    //if (0xFFFFFFFF != (tidx = tset._8875[3])) w.WriteLine("	map_Bump {0}", GetTexFile(TSTA.Data[tidx]._026C, dir));
                }

                w.Flush();
                w.BaseStream.SetLength(w.BaseStream.Position);
            }

            return mtlNames;
        }
    }
}
