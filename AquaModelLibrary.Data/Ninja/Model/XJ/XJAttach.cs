using AquaModelLibrary.Data.DataTypes;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.Ninja.Model.XJ
{
	/// <summary>
	/// An attach/mesh using the Xbox/PC format
	/// </summary>
	public class XJAttach : Attach
	{
		/// <summary>
		/// Flags for the XJ Attach
		/// </summary>
		public uint flags;

		/// <summary>
		/// The seperate sets of vertex data in this attach
		/// </summary>
		public readonly List<XJVertexSet> vertexData = new List<XJVertexSet>();

		/// <summary>
		/// The meshes with opaque rendering properties
		/// </summary>
		public readonly List<XJMesh> opaqueMeshes = new List<XJMesh>();

		/// <summary>
		/// The meshes with translucent rendering properties
		/// </summary>
		public readonly List<XJMesh> translucentMeshes = new List<XJMesh>();

		/// <summary>
		/// Attach Bounding
		/// </summary>
        public NinjaBoundingVolume bounding = new NinjaBoundingVolume();

        /// <summary>
        /// Create a new empty XJ attach
        /// </summary>
        public XJAttach()
		{
			flags = 0;
			vertexData = new List<XJVertexSet>();
			opaqueMeshes = new List<XJMesh>();
			translucentMeshes = new List<XJMesh>();
		}

        public XJAttach(byte[] file, bool be = true, int offset = 0)
        {
            Read(file, be, offset);
        }

        public XJAttach(BufferedStreamReaderBE<MemoryStream> sr, bool be = true, int offset = 0)
        {
            Read(sr, be, offset);
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
            flags = sr.ReadBE<uint>();
			int vertexSetOffset = sr.ReadBE<int>();
            int vertexSetCount = sr.ReadBE<int>();
            int opaqueMeshesOffset = sr.ReadBE<int>();
            int opaqueMeshesCount = sr.ReadBE<int>();
            int translucentMeshesOffset = sr.ReadBE<int>();
            int translucentMeshesCount = sr.ReadBE<int>();
            bounding.center = sr.ReadBEV3();
            bounding.radius = sr.ReadBE<float>();

            for (int i = 0; i < vertexSetCount; i++)
			{
				sr.Seek(vertexSetOffset + (0x10 * i) + offset, SeekOrigin.Begin);
				vertexData.Add(new XJVertexSet(sr, be, offset));
			}

			for (int i = 0; i < opaqueMeshesCount; i++)
            {
                sr.Seek(opaqueMeshesOffset + (0x14 * i) + offset, SeekOrigin.Begin);
                opaqueMeshes.Add(new XJMesh(sr, be, offset));
			}

			for (int i = 0; i < translucentMeshesCount; i++)
            {
                sr.Seek(translucentMeshesOffset + (0x14 * i) + offset, SeekOrigin.Begin);
                translucentMeshes.Add(new XJMesh(sr, be, offset));
			}
		}

		public void Write(List<byte> outBytes, List<int> POF0Offsets)
		{
            var vdataAddress = outBytes.Count;
			foreach(var vdata in vertexData)
			{
                vdata.Write(outBytes, POF0Offsets);
			}
            outBytes.AlignWriter(0x20);
			int opaqueAddress = outBytes.Count;
            XJMesh.Write(outBytes, POF0Offsets, opaqueMeshes);
            outBytes.AlignWriter(0x20);
			int translucentAddress = outBytes.Count;
            XJMesh.Write(outBytes, POF0Offsets, translucentMeshes);
			outBytes.AlignWriter(0x20);

            POF0Offsets.Add((outBytes.Count + 0x4));
            POF0Offsets.Add((outBytes.Count + 0xC));
            POF0Offsets.Add((outBytes.Count + 0x14));
            outBytes.AddValue(flags);
            outBytes.AddValue(vdataAddress);
            outBytes.AddValue(vertexData.Count);
            outBytes.AddValue(opaqueAddress);
            outBytes.AddValue(opaqueMeshes.Count);
            outBytes.AddValue(translucentAddress);
            outBytes.AddValue(translucentMeshes.Count);
            outBytes.AddValue(bounding.center);
            outBytes.AddValue(bounding.radius);
            outBytes.AlignWriter(0x20);
		}

		public void GetVertexData(int nodeId, VTXL vtxl, Matrix4x4 transform)
		{
            if (vertexData != null)
            {
                for (int vd = 0; vd < vertexData.Count; vd++) //There should only be 1 of these, but in case there's not...
                {
                    var vertData = vertexData[vd];

					for(int i = 0; i < vertData.Positions.Count; i++)
					{
						vtxl.vertPositions.Add(Vector3.Transform(vertData.Positions[i], transform));
						if(vertData.Normals.Count > i)
						{
							vtxl.vertNormals.Add(Vector3.TransformNormal(vertData.Normals[i], transform));
                        }
                        if (vertData.Colors.Count > i)
                        {
                            vtxl.vertColors.Add(vertData.Colors[i]);
                        }
                        if (vertData.UVs.Count > i)
                        {
                            vtxl.uv1List.Add(vertData.UVs[i]);
                        }
                    }
                }
            }
        }

		public void GetFaceData(int nodeId, VTXL vtxl, AquaObject aqo)
		{
			for(int m = 0; m < opaqueMeshes.Count; m++)
			{
				ProcessMeshData(vtxl, aqo, $"Mesh_{nodeId}_opaque_{m}", opaqueMeshes[m]);
            }

			for(int m = 0; m < translucentMeshes.Count; m++)
            {
                ProcessMeshData(vtxl, aqo, $"Mesh_{nodeId}_transparent_{m}", translucentMeshes[m]);
            }
		}

		public void ProcessMeshData(VTXL vtxl, AquaObject aqo, string name, XJMesh xjMesh)
		{
			//Set up material
            GenericMaterial mat = new GenericMaterial();
            mat.texNames = new List<string>() { $"{xjMesh.TextureId}" };
            mat.matName = "Mat_" + $"#S_{xjMesh.SourceAlpha}#D_{xjMesh.DestinationAlpha}";
			mat.diffuseRGBA = new Vector4((float)(xjMesh.DiffuseColor[0] / 255.0), (float)(xjMesh.DiffuseColor[1] / 255.0), (float)(xjMesh.DiffuseColor[2] / 255.0), (float)(xjMesh.DiffuseColor[3] / 255.0));

            //Check for existing equivalent materials
            int matIndex = -1;
            for (int i = 0; i < aqo.tempMats.Count; i++)
            {
                var genMat = aqo.tempMats[i];
                if (genMat.matName == mat.matName && genMat.texNames[0] == mat.texNames[0] && genMat.diffuseRGBA == mat.diffuseRGBA)
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
			mesh.name = name;
            aqo.meshNames.Add(mesh.name);
            var tris = xjMesh.GetTriangles();
            int f = 0;
            foreach (var tri in tris)
			{
				VTXL faceVtxl = new VTXL();
                faceVtxl.rawFaceId.Add(f);
                faceVtxl.rawFaceId.Add(f);
                faceVtxl.rawFaceId.Add(f++);

				var x = AddXJVert(vtxl, tri.X, faceVtxl, mesh, vertTracker);
				var y = AddXJVert(vtxl, tri.Y, faceVtxl, mesh, vertTracker);
				var z = AddXJVert(vtxl, tri.Z, faceVtxl, mesh, vertTracker);
				
				mesh.triList.Add(new Vector3(x, y, z));
				faceVtxl.rawVertId.Add(x);
				faceVtxl.rawVertId.Add(y);
				faceVtxl.rawVertId.Add(z);
                mesh.matIdList.Add(matIndex);

                mesh.faceVerts.Add(faceVtxl);
            }

			aqo.tempTris.Add(mesh);
        }

		public int AddXJVert(VTXL fullVtxl, int faceVertIndex, VTXL vtxl, GenericTriangles mesh, Dictionary<string, int> vertTracker)
        {
            string vertId = "";
            if (fullVtxl.vertPositions.Count > faceVertIndex)
			{
				vtxl.vertPositions.Add(fullVtxl.vertPositions[faceVertIndex]);
				vertId += $"p{faceVertIndex}";
            }
            if (fullVtxl.vertNormals.Count > faceVertIndex)
            {
                vtxl.vertNormals.Add(fullVtxl.vertNormals[faceVertIndex]);
                vertId += $"n{faceVertIndex}";
            }
            if (fullVtxl.vertColors.Count > faceVertIndex)
            {
                vtxl.vertColors.Add(fullVtxl.vertColors[faceVertIndex]);
                vertId += $"c{faceVertIndex}";
            }
            if (fullVtxl.uv1List.Count > faceVertIndex)
            {
                vtxl.uv1List.Add(fullVtxl.uv1List[faceVertIndex]);
                vertId += $"u{faceVertIndex}";
            }

            if (vertTracker.ContainsKey(vertId))
            {
                return vertTracker[vertId];
            }
            else
            {
                vertTracker.Add(vertId, mesh.vertCount);
                return mesh.vertCount++;
            }
        }

		public bool HasWeights()
		{
			return false;
		}
	}
}
