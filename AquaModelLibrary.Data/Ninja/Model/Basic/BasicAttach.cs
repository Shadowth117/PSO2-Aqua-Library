using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;
using AquaModelLibrary.Helpers.Writers;

namespace AquaModelLibrary.Data.Ninja.Model.Basic
{
    /// <summary>
    /// Handling based on SA Tools implementation https://github.com/X-Hax/sa_tools
    /// </summary>
    public class BasicAttach : Attach
    {
        public List<Vector3> vertPositions = new();
        public List<Vector3> vertNormals = new();
        public List<NJSMeshSet> meshSetList = new();
        public List<NJSMaterial> matList = new();
        public NinjaBoundingVolume bounding = new NinjaBoundingVolume();
        public int? DXValue = null;

        public BasicAttach()
        {
            matList = new List<NJSMaterial>();
            meshSetList = new List<NJSMeshSet>();
        }

        public BasicAttach(byte[] file, bool be = false, int offset = 0, bool DX = false)
            : this()
        {
            Read(file, be, offset, DX);
        }

        public BasicAttach(BufferedStreamReaderBE<MemoryStream> sr, bool be = false, int offset = 0, bool DX = false)
            : this()
        {
            Read(sr, be, offset, DX);
        }

        public void Read(byte[] file, bool be = false, int offset = 0, bool DX = false)
        {
            using (var ms = new MemoryStream(file))
            using (var sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr, be, offset, DX);
            }
        }
        public void Read(BufferedStreamReaderBE<MemoryStream> sr, bool be = false, int offset = 0, bool DX = false)
        {
            sr._BEReadActive = be;
            var vertPosAddress = sr.ReadBE<int>();
            var vertNrmAddress = sr.ReadBE<int>();
            var vertCount = sr.ReadBE<int>();
            var meshAddress = sr.ReadBE<int>();
            var matAddress = sr.ReadBE<int>();
            var meshCount = sr.ReadBE<int>();
            var matCount = sr.ReadBE<int>();
            bounding = new NinjaBoundingVolume()
            {
                center = sr.ReadBEV3(),
                radius = sr.ReadBE<float>()
            };
            if(DX) //Only used in Sonic Adventure DX basic models, a stub?
            {
                DXValue = sr.ReadBE<int>();
            }

            if(vertPosAddress > 0)
            {
                sr.Seek(vertPosAddress + offset, SeekOrigin.Begin);
                for (int i = 0; i < vertCount; i++)
                {
                    vertPositions.Add(sr.ReadBEV3());
                }
            }
            if (vertNrmAddress > 0)
            {
                sr.Seek(vertNrmAddress + offset, SeekOrigin.Begin);
                for(int i = 0; i < vertCount; i++)
                {
                    vertNormals.Add(sr.ReadBEV3());
                }
            }
            if (meshAddress > 0)
            {
                sr.Seek(meshAddress + offset, SeekOrigin.Begin);
                for(int i = 0; i < meshCount; i++)
                {
                    meshSetList.Add(new NJSMeshSet(sr, be, offset, DX));
                }
            }
            if (matAddress > 0)
            {
                sr.Seek(matAddress + offset, SeekOrigin.Begin);
                for (int i = 0; i < matCount; i++)
                {
                    matList.Add(new NJSMaterial(sr, be, offset, DX));
                }
            }
        }

        public bool HasWeights()
        {
            return false;
        }

        public void GetVertexData(int nodeId, VTXL vtxl, Matrix4x4 transform)
        {
            throw new NotImplementedException();
        }

        public void GetFaceData(int nodeId, VTXL vtxl, AquaObject aqo)
        {
            throw new NotImplementedException();
        }

        public void Write(ByteListWriter outBytes, List<int> POF0Offsets)
        {
            string attachAddress = outBytes.Count.ToString();
            if(vertPositions.Count > 0)
            {
                POF0Offsets.Add(outBytes.ReserveInt($"{attachAddress}_vertPositions"));
            }
            if (vertNormals.Count > 0)
            {
                POF0Offsets.Add(outBytes.ReserveInt($"{attachAddress}_vertNormals"));
            }
            outBytes.AddValue(vertPositions.Count);
            if (meshSetList.Count > 0)
            {
                POF0Offsets.Add(outBytes.ReserveInt($"{attachAddress}_meshSetList"));
            }
            if (matList.Count > 0)
            {
                POF0Offsets.Add(outBytes.ReserveInt($"{attachAddress}_matList"));
            }
            outBytes.AddValue(bounding.center);
            outBytes.AddValue(bounding.radius);
            if(DXValue != null)
            {
                outBytes.AddValue(DXValue.Value);
            }
            if(vertPositions.Count > 0)
            {
                outBytes.FillInt($"{attachAddress}_vertPositions", outBytes.Count);
                for (int i = 0; i < vertPositions.Count; i++)
                {
                    outBytes.AddValue(vertPositions[i]);
                }
            }
            if (vertNormals.Count > 0)
            {
                outBytes.FillInt($"{attachAddress}_vertNormals", outBytes.Count);
                for (int i = 0; i < vertNormals.Count; i++)
                {
                    outBytes.AddValue(vertNormals[i]);
                }
            }
            if (meshSetList.Count > 0)
            {
                outBytes.FillInt($"{attachAddress}_meshSetList", outBytes.Count);
                for (int i = 0; i < meshSetList.Count; i++)
                {
                    NJSMeshSet.Write(outBytes, meshSetList, POF0Offsets);
                }
            }
            if (matList.Count > 0)
            {
                outBytes.FillInt($"{attachAddress}_matList", outBytes.Count);
                for (int i = 0; i < matList.Count; i++)
                {
                    matList[i].Write(outBytes);
                }
            }
        }
    }
}
