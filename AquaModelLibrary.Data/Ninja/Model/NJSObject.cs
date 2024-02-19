using AquaModelLibrary.Data.Ninja.Model.Ginja;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.Ninja.Model
{
    public class NJSObject
    {
        public NinjaVariant variant;
        public int flags;
        public Attach mesh = null;
        public Vector3 pos;
        public Vector3 rot;
        public Vector3 scale;
        public NJSObject childObject = null;
        public NJSObject siblingObject = null;
        public int unkInt;

        public NJSObject() { variant = NinjaVariant.Basic; }

        public NJSObject(byte[] file, NinjaVariant ninjaVariant, bool be = true, int offset = 0)
        {
            Read(file, ninjaVariant, be, offset);
        }

        public NJSObject(BufferedStreamReaderBE<MemoryStream> sr, NinjaVariant ninjaVariant, bool be = true, int offset = 0)
        {
            Read(sr, ninjaVariant, be, offset);
        }

        public bool HasWeights()
        {
            if(mesh?.HasWeights() == true || childObject?.HasWeights() == true|| siblingObject?.HasWeights() == true)
            {
                return true;
            }
            return false;
        }

        public void GetFaceData(int nodeId, VTXL vtxl, AquaObject aqo)
        {
            if (mesh != null)
            {
                mesh.GetFaceData(nodeId, vtxl, aqo);
            }
        }

        public void GetVertexData(int nodeId, VTXL vtxl, Matrix4x4 transform)
        {
            if(mesh != null)
            {
                mesh.GetVertexData(nodeId, vtxl, transform);
            }
        }

        public void Read(byte[] file, NinjaVariant ninjaVariant, bool be = true, int offset = 0)
        {
            using (var ms = new MemoryStream(file))
            using (var sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr, ninjaVariant, be, offset);
            }
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr, NinjaVariant ninjaVariant, bool be = true, int offset = 0)
        {
            sr._BEReadActive = be;
            flags = sr.ReadBE<int>();
            int attachOffset = sr.ReadBE<int>();
            pos = sr.ReadBEV3();
            int rotX = sr.ReadBE<int>();
            int rotY = sr.ReadBE<int>();
            int rotZ = sr.ReadBE<int>();
            rot = new Vector3((float)(rotX * NinjaConstants.FromBAMSvalue), (float)(rotY * NinjaConstants.FromBAMSvalue), (float)(rotZ * NinjaConstants.FromBAMSvalue));
            scale = sr.ReadBEV3();
            int childOffset = sr.ReadBE<int>();
            int siblingOffset = sr.ReadBE<int>();

            sr.Seek(offset + attachOffset, SeekOrigin.Begin);
            switch (ninjaVariant)
            {
                case NinjaVariant.Basic:
                    throw new NotImplementedException();
                    break;
                case NinjaVariant.Chunk:
                    throw new NotImplementedException();
                    break;
                case NinjaVariant.Ginja:
                    mesh = attachOffset > 0 ? new GinjaAttach(sr, be, offset) : null;
                    break;
                case NinjaVariant.XJ:
                    throw new NotImplementedException();
                    break;
            }

            sr.Seek(offset + childOffset, SeekOrigin.Begin);
            childObject = childOffset > 0 ? new NJSObject(sr, ninjaVariant, be, offset) : null;

            sr.Seek(offset + siblingOffset, SeekOrigin.Begin);
            siblingObject = siblingOffset > 0 ? new NJSObject(sr, ninjaVariant, be, offset) : null;

            if(variant == NinjaVariant.Ginja)
            {
                unkInt = sr.ReadBE<int>();
            }
        }
    }
}
