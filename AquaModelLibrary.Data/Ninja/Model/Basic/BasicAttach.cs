using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;
using AquaModelLibrary.Helpers.Extensions;

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
            }
            if (vertNrmAddress > 0)
            {
                sr.Seek(vertNrmAddress + offset, SeekOrigin.Begin);
            }
            if (meshAddress > 0)
            {
                sr.Seek(meshAddress + offset, SeekOrigin.Begin);
            }
            if (matAddress > 0)
            {
                sr.Seek(matAddress + offset, SeekOrigin.Begin);
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

        public void Write(List<byte> outBytes, List<int> POF0Offsets)
        {
            string attachAddress = outBytes.Count.ToString();
            if(vertPositions.Count > 0)
            {
                outBytes.ReserveInt($"{attachAddress}_vertPositions");
            }
            if (vertNormals.Count > 0)
            {
                outBytes.ReserveInt($"{attachAddress}_vertNormals");
            }
            outBytes.AddValue(vertPositions.Count);
            if (meshSetList.Count > 0)
            {
                outBytes.ReserveInt($"{attachAddress}_meshSetList");
            }
            if (matList.Count > 0)
            {
                outBytes.ReserveInt($"{attachAddress}_matList");
            }
            outBytes.AddValue(bounding.center);
            outBytes.AddValue(bounding.radius);
            if(DXValue != null)
            {
                outBytes.AddValue(DXValue.Value);
            }
        }
    }
}
