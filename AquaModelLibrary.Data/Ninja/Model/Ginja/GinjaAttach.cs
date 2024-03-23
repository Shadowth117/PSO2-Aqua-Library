using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.Ninja.Model.Ginja
{
    public class GinjaAttach : Attach
    {
        public GinjaVertexData vertData = null;
        public GinjaSkinVertexData skinVertData = null;
        public List<GinjaMesh> opaqueFaceData = new List<GinjaMesh>();
        public List<GinjaMesh> transparentFaceData = new List<GinjaMesh>();
        public NinjaBoundingVolume bounding = new NinjaBoundingVolume();

        public GinjaAttach() { }

        public GinjaAttach(byte[] file, bool be = true, int offset = 0)
        {
            Read(file, be, offset);
        }

        public GinjaAttach(BufferedStreamReaderBE<MemoryStream> sr, bool be = true, int offset = 0)
        {
            Read(sr, be, offset);
        }

        public void GetFaceData(int nodeId, VTXL vtxl, AquaObject aqo)
        {
            //Parameters are set with the first mesh and tweaked with each subsequent one so we want to cache these for the next mesh.
            Dictionary<ParameterType, GCParameter> gcParams = new Dictionary<ParameterType, GCParameter>();
            int m = 0;
            string meshSet = "opaque";
            foreach (var gcMesh in opaqueFaceData)
            {
                ProcessMeshData(nodeId, vtxl, aqo, gcParams, m, meshSet, gcMesh);
                m++;
            }

            m = 0;
            meshSet = "transparent";
            gcParams = new Dictionary<ParameterType, GCParameter>();
            foreach (var gcMesh in transparentFaceData)
            {
                ProcessMeshData(nodeId, vtxl, aqo, gcParams, m, meshSet, gcMesh);
                m++;
            }
        }

        private void ProcessMeshData(int nodeId, VTXL vtxl, AquaObject aqo, Dictionary<ParameterType, GCParameter> gcParams, int m, string meshSet, GinjaMesh gcMesh)
        {
            //Refresh gcParams
            foreach (var param in gcMesh.parameters)
            {
                gcParams[param.type] = param;
            }

            GenericMaterial mat = new GenericMaterial();
            mat.matName = "Mat_";

            var textureSetting = gcParams.ContainsKey(ParameterType.Texture) ? ((TextureParameter)gcParams[ParameterType.Texture]) : new TextureParameter();
            var blendAlpha = ((BlendAlphaParameter)gcParams[ParameterType.BlendAlpha]);
            var indexParam = (IndexAttributeParameter)gcParams[ParameterType.IndexAttributeFlags];

            mat.texNames = new List<string>() { $"{textureSetting.TextureID}" };

            foreach (var flag in Enum.GetValues(typeof(GCTileMode)))
            {
                if ((textureSetting.Tile & (GCTileMode)flag) > 0)
                {
                    mat.matName += $"#{flag}";
                }
            }

            mat.matName += $"#S_{blendAlpha.SourceAlpha}";
            mat.matName += $"#D_{blendAlpha.DestAlpha}";

            //Check for existing equivalent materials
            int matIndex = -1;
            for (int i = 0; i < aqo.tempMats.Count; i++)
            {
                var genMat = aqo.tempMats[i];
                if (genMat.matName == mat.matName && genMat.texNames[0] == mat.texNames[0])
                {
                    matIndex = i;
                    break;
                }
            }
            if (matIndex == -1)
            {
                matIndex = aqo.tempMats.Count;
                aqo.tempMats.Add(mat);
            }

            //Set up mesh
            GenericTriangles mesh = new GenericTriangles();
            Dictionary<string, int> vertTracker = new Dictionary<string, int>();
            mesh.triList = new List<Vector3>();
            mesh.name = $"Mesh_{nodeId}_{meshSet}_{m}";
            aqo.meshNames.Add(mesh.name);
            int f = 0;
            foreach (var triData in gcMesh.primitives)
            {
                var tris = triData.ToTriangles();
                for (int index = 0; index < tris.Count - 2; index += 3)
                {
                    VTXL faceVtxl = new VTXL();
                    faceVtxl.rawFaceId.Add(f);
                    faceVtxl.rawFaceId.Add(f);
                    faceVtxl.rawFaceId.Add(f++);

                    var x = AddGinjaVert(vtxl, tris[index + 2], faceVtxl, indexParam.IndexAttributes, mesh, vertTracker);
                    var y = AddGinjaVert(vtxl, tris[index + 1], faceVtxl, indexParam.IndexAttributes, mesh, vertTracker);
                    var z = AddGinjaVert(vtxl, tris[index + 0], faceVtxl, indexParam.IndexAttributes, mesh, vertTracker);
                    mesh.triList.Add(new Vector3(x, y, z));
                    faceVtxl.rawVertId.Add(x);
                    faceVtxl.rawVertId.Add(y);
                    faceVtxl.rawVertId.Add(z);
                    mesh.matIdList.Add(matIndex);

                    mesh.faceVerts.Add(faceVtxl);
                }
            }

            aqo.tempTris.Add(mesh);
        }

        public int AddGinjaVert(VTXL fullVtxl, Loop loop, VTXL vtxl, GCIndexAttributeFlags flags, GenericTriangles genMesh, Dictionary<string, int> vertTracker)
        {
            string vertId = "";
            if (((flags & GCIndexAttributeFlags.HasPosition) > 0) || ((flags & GCIndexAttributeFlags.Position16BitIndex) > 0))
            {
                vtxl.vertPositions.Add(fullVtxl.vertPositions[loop.PositionIndex]);
                vertId += ((int)GCIndexAttributeFlags.HasPosition).ToString() + loop.PositionIndex;

                vtxl.vertWeightIndices.Add((int[])fullVtxl.vertWeightIndices[loop.PositionIndex].Clone());
                vtxl.vertWeights.Add(fullVtxl.vertWeights[loop.PositionIndex]);
            }
            if (((flags & GCIndexAttributeFlags.HasNormal) > 0) || ((flags & GCIndexAttributeFlags.Normal16BitIndex) > 0))
            {
                vtxl.vertNormals.Add(fullVtxl.vertNormals[loop.NormalIndex]);
                vertId += ((int)GCIndexAttributeFlags.HasNormal).ToString() + loop.NormalIndex;
            }
            if (((flags & GCIndexAttributeFlags.HasColor) > 0) || ((flags & GCIndexAttributeFlags.Color16BitIndex) > 0))
            {
                vtxl.vertColors.Add((byte[])fullVtxl.vertColors[loop.Color0Index].Clone());
                vertId += ((int)GCIndexAttributeFlags.HasColor).ToString() + loop.Color0Index;
            }
            if (((flags & GCIndexAttributeFlags.HasUV) > 0) || ((flags & GCIndexAttributeFlags.UV16BitIndex) > 0))
            {
                vtxl.uv1List.Add(fullVtxl.uv1List[loop.UV0Index]);
                vertId += ((int)GCIndexAttributeFlags.HasUV).ToString() + loop.UV0Index;
            }

            if (vertTracker.ContainsKey(vertId))
            {
                return vertTracker[vertId];
            }
            else
            {
                vertTracker.Add(vertId, genMesh.vertCount);
                return genMesh.vertCount++;
            }
        }

        public void GetVertexData(int nodeId, VTXL vtxl, Matrix4x4 transform)
        {
            if (skinVertData != null)
            {
                foreach (var element in skinVertData.elements)
                {
                    switch (element.elementType)
                    {
                        case GCSkinAttribute.StaticWeight:
                            foreach (var posNrm in element.posNrms)
                            {
                                vtxl.vertPositions.Add(Vector3.Transform(new Vector3((float)(posNrm.posX / 255.0), (float)(posNrm.posY / 255.0), (float)(posNrm.posZ / 255.0)), transform));
                                vtxl.vertNormals.Add(Vector3.TransformNormal(new Vector3((float)(posNrm.nrmX / 255.0), (float)(posNrm.nrmY / 255.0), (float)(posNrm.nrmZ / 255.0)), transform));
                                vtxl.rawVertWeights.Add(new List<float>() { 1 });
                                vtxl.rawVertWeightIds.Add(new List<int>() { nodeId });
                            }
                            break;
                        case GCSkinAttribute.PartialWeightStart:
                            for (int i = 0; i < element.weightData.Count; i++)
                            {
                                var weightSet = element.weightData[i];
                                var posNrm = element.posNrms[i];
                                var weight = (float)(weightSet.weight / 255.0);
                                vtxl.vertPositions.Add(Vector3.Transform(new Vector3((float)(posNrm.posX / 255.0), (float)(posNrm.posY / 255.0), (float)(posNrm.posZ / 255.0)), transform) * weight);
                                vtxl.vertNormals.Add(Vector3.TransformNormal(new Vector3((float)(posNrm.nrmX / 255.0), (float)(posNrm.nrmY / 255.0), (float)(posNrm.nrmZ / 255.0)), transform) * weight);
                                vtxl.rawVertWeights.Add(new List<float>() { weight });
                                vtxl.rawVertWeightIds.Add(new List<int>() { nodeId });
                            }
                            break;
                        case GCSkinAttribute.PartialWeight:
                            for (int i = 0; i < element.weightData.Count; i++)
                            {
                                var weightSet = element.weightData[i];
                                var vId = weightSet.vertIndex;
                                var posNrm = element.posNrms[i];
                                var weight = (float)(weightSet.weight / 255.0);
                                vtxl.vertPositions[vId] += Vector3.Transform(new Vector3((float)(posNrm.posX / 255.0), (float)(posNrm.posY / 255.0), (float)(posNrm.posZ / 255.0)), transform) * weight;
                                vtxl.vertNormals[vId] += Vector3.TransformNormal(new Vector3((float)(posNrm.nrmX / 255.0), (float)(posNrm.nrmY / 255.0), (float)(posNrm.nrmZ / 255.0)), transform) * weight;
                                vtxl.rawVertWeights[vId].Add(weight);
                                vtxl.rawVertWeightIds[vId].Add(nodeId);
                            }
                            break;
                        case GCSkinAttribute.WeightEnd: //More of a marker than anything; No data here.
                            break;
                        default:
                            throw new Exception($"Unexpected GCSkinAttribute {element.elementType}");
                    }
                }
            }
            if (vertData != null)
            {
                if (vertData.posList.Count > 0)
                {
                    vtxl.vertPositions.AddRange(vertData.posList);
                    for(int i = 0; i < vertData.posList.Count; i++)
                    {
                        vtxl.rawVertWeights.Add(new List<float>() { 1 });
                        vtxl.rawVertWeightIds.Add(new List<int>() { nodeId });
                    }
                }
                if (vertData.nrmList.Count > 0)
                {
                    vtxl.vertNormals.AddRange(vertData.nrmList);
                }
                for(int i = 0; i < vtxl.vertPositions.Count; i++)
                {
                    vtxl.vertPositions[i] = Vector3.Transform(vtxl.vertPositions[i], transform);
                }
                for (int i = 0; i < vtxl.vertNormals.Count; i++)
                {
                    vtxl.vertNormals[i] = Vector3.TransformNormal(vtxl.vertNormals[i], transform);
                }

                //Only color set 0 is probably used
                if (vertData.colorsArray[0]?.Count > 0)
                {
                    vtxl.vertColors.AddRange(vertData.colorsArray[0]);
                }
                if (vertData.colorsArray[1]?.Count > 0)
                {
                    vtxl.vertColor2s.AddRange(vertData.colorsArray[1]);
                }

                //Only UV set 0 is probably used in existing ninja games
                if (vertData.uvsArray[0]?.Count > 0)
                {
                    vtxl.uv1List.AddRange(vertData.uvsArray[0]);
                }
                if (vertData.uvsArray[1]?.Count > 0)
                {
                    vtxl.uv2List.AddRange(vertData.uvsArray[1]);
                }
                if (vertData.uvsArray[2]?.Count > 0)
                {
                    vtxl.uv3List.AddRange(vertData.uvsArray[2]);
                }
                if (vertData.uvsArray[3]?.Count > 0)
                {
                    vtxl.uv4List.AddRange(vertData.uvsArray[3]);
                }
            }
        }

        public bool HasWeights()
        {
            return skinVertData != null;
        }

        public void Read(byte[] file, bool be = true, int offset = 0)
        {
            using (var ms = new MemoryStream(file))
            using (var sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr, be);
            }
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr, bool be = true, int offset = 0)
        {
            sr._BEReadActive = be;
            int vertexAddress = sr.ReadBE<int>();
            int skinnedVertexAddress = sr.ReadBE<int>();
            int opaquePolyAddress = sr.ReadBE<int>();
            int transparentPolyAddress = sr.ReadBE<int>();

            ushort opaqueCount = sr.ReadBE<ushort>();
            ushort transparentCount = sr.ReadBE<ushort>();
            bounding.center = sr.ReadBEV3();
            bounding.radius = sr.ReadBE<float>();

            if (vertexAddress > 0)
            {
                sr.Seek(vertexAddress + offset, SeekOrigin.Begin);
                vertData = new GinjaVertexData(sr, be, offset);
            }

            if (skinnedVertexAddress > 0)
            {
                sr.Seek(skinnedVertexAddress + offset, SeekOrigin.Begin);
                skinVertData = new GinjaSkinVertexData(sr, be, offset);
            }

            if (opaquePolyAddress > 0)
            {
                sr.Seek(opaquePolyAddress + offset, SeekOrigin.Begin);
                var indexFlags = GCIndexAttributeFlags.HasPosition;
                for (int i = 0; i < opaqueCount; i++)
                {
                    GinjaMesh mesh = new GinjaMesh(sr, indexFlags, be, offset);

                    GCIndexAttributeFlags? t = mesh.IndexFlags;
                    if (t.HasValue) indexFlags = t.Value;

                    opaqueFaceData.Add(mesh);
                }
            }

            if (transparentPolyAddress > 0)
            {
                sr.Seek(transparentPolyAddress + offset, SeekOrigin.Begin);
                var indexFlags = GCIndexAttributeFlags.HasPosition;
                for (int i = 0; i < transparentCount; i++)
                {
                    GinjaMesh mesh = new GinjaMesh(sr, indexFlags, be, offset);

                    GCIndexAttributeFlags? t = mesh.IndexFlags;
                    if (t.HasValue) indexFlags = t.Value;

                    transparentFaceData.Add(mesh);
                }
            }
        }

        public void Write(List<byte> outBytes, List<int> POF0Offsets)
        {
            string attachAddress = outBytes.Count.ToString();
            if (vertData?.elements?.Count > 0)
            {
                POF0Offsets.Add(outBytes.Count);
            }
            outBytes.ReserveInt($"{attachAddress}_vertex");
            if (skinVertData?.elements?.Count > 0)
            {
                POF0Offsets.Add(outBytes.Count);
            }
            outBytes.ReserveInt($"{attachAddress}_skinVertex");
            if (opaqueFaceData?.Count > 0)
            {
                POF0Offsets.Add(outBytes.Count);
            }
            outBytes.ReserveInt($"{attachAddress}_opaque");
            if (transparentFaceData?.Count > 0)
            {
                POF0Offsets.Add(outBytes.Count);
            }
            outBytes.ReserveInt($"{attachAddress}_transparent");

            outBytes.AddValue((ushort)(opaqueFaceData == null ? 0 : opaqueFaceData.Count));
            outBytes.AddValue((ushort)(transparentFaceData == null ? 0 : transparentFaceData.Count));
            outBytes.AddValue(bounding.center.X);
            outBytes.AddValue(bounding.center.Y);
            outBytes.AddValue(bounding.center.Z);
            outBytes.AddValue(bounding.radius);

            if(vertData?.elements?.Count > 0)
            {
                outBytes.FillInt($"{attachAddress}_vertex", outBytes.Count);
                vertData.Write(outBytes, POF0Offsets);
            }

            if (skinVertData?.elements?.Count > 0)
            {
                outBytes.FillInt($"{attachAddress}_skinVertex", outBytes.Count);
                skinVertData.Write(outBytes, POF0Offsets);
            }

            if (opaqueFaceData?.Count > 0)
            {
                outBytes.FillInt($"{attachAddress}_opaque", outBytes.Count);
                WriteMesh(outBytes, POF0Offsets, opaqueFaceData, "o");
            }

            if (transparentFaceData?.Count > 0)
            {
                outBytes.FillInt($"{attachAddress}_transparent", outBytes.Count);
                WriteMesh(outBytes, POF0Offsets, transparentFaceData, "a");
            }

            if (opaqueFaceData?.Count != null)
            {
                WriteMeshDataParameters(outBytes, POF0Offsets, opaqueFaceData, "o");
            }

            if (transparentFaceData?.Count != null)
            {
                WriteMeshDataParameters(outBytes, POF0Offsets, transparentFaceData, "a");
            }

            outBytes.AlignWriter(0x20);
            if (opaqueFaceData?.Count != null)
            {
                WriteMeshDataPrimitives(outBytes, POF0Offsets, opaqueFaceData, "o");
            }

            if (transparentFaceData?.Count != null)
            {
                WriteMeshDataPrimitives(outBytes, POF0Offsets, transparentFaceData, "a");
            }
        }

        public void WriteMesh(List<byte> outBytes, List<int> POF0Offsets, List<GinjaMesh> meshList, string type)
        {
            for (int i = 0; i < meshList.Count; i++)
            {
                var mesh = meshList[i];
                if(mesh.parameters.Count > 0)
                {
                    POF0Offsets.Add(outBytes.Count);
                }
                outBytes.ReserveInt($"meshParameter{type}_{i}");
                outBytes.AddValue(mesh.parameters.Count);

                if (mesh.primitives.Count > 0)
                {
                    POF0Offsets.Add(outBytes.Count);
                }
                outBytes.ReserveInt($"meshPrimitive{type}_{i}");
                outBytes.ReserveInt($"meshPrimitiveSize{type}_{i}");
            }
        }

        public void WriteMeshDataParameters(List<byte> outBytes, List<int> POF0Offsets, List<GinjaMesh> meshList, string type)
        {
            //Parameters
            for (int i = 0; i < meshList.Count; i++)
            {
                if (meshList[i].parameters?.Count > 0)
                {
                    outBytes.FillInt($"meshParameter{type}_{i}", outBytes.Count);
                    foreach (var prm in meshList[i].parameters)
                    {
                        outBytes.AddRange(prm.GetBytes());
                    }
                }
            }
        }
        public void WriteMeshDataPrimitives(List<byte> outBytes, List<int> POF0Offsets, List<GinjaMesh> meshList, string type)
        {
            GCIndexAttributeFlags indexFlags = GCIndexAttributeFlags.Position16BitIndex;
            //Primitives
            for (int i = 0; i < meshList.Count; i++)
            {
                var mesh = meshList[i];

                var meshFlags = mesh.IndexFlags;
                if (meshFlags != null)
                {
                    indexFlags = (GCIndexAttributeFlags)meshFlags;
                }
                if (mesh.primitives.Count > 0)
                {
                    List<byte> primitives = new List<byte>();
                    foreach (var prim in mesh.primitives)
                    {
                        primitives.AddRange(prim.GetBytes(indexFlags));
                    }
                    outBytes.FillInt($"meshPrimitive{type}_{i}", outBytes.Count);
                    outBytes.AddRange(primitives.ToArray());
                    var addition = outBytes.AlignWriter(0x20);
                    outBytes.FillInt($"meshPrimitiveSize{type}_{i}", primitives.Count + addition);
                }
            }
        }
    }
}
