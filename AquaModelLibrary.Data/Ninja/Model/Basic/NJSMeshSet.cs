using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Drawing;
using System.Numerics;

namespace AquaModelLibrary.Data.Ninja.Model.Basic
{
    /// <summary>
    /// Handling based on SA Tools implementation https://github.com/X-Hax/sa_tools
    /// </summary>
    public class NJSMeshSet
    {
        /// <summary>
        /// Ideally manipulate this with its accessors instead
        /// </summary>
        public ushort polyInfo;

        public ushort MaterialId
        {
            get { return (ushort)(polyInfo & 0x3FFF); }
            set { polyInfo = (ushort)((polyInfo & ~0x3FFF) | (value & 0x3FFF)); }
        }
        public BasicPolyType PolyType
        {
            get { return (BasicPolyType)((polyInfo & ~0x3FFF) >> 0xE); }
            set { polyInfo = (ushort)((polyInfo & 0x3FFF) | (((int)value << 0xE) & ~0x3FFF)); }
        }
        public List<Poly> faceList = new();
        public int polyAttribute;
        public List<Vector3> polyNrmList = new();
        public List<Color> polyClrList = new();
        public List<Vector2> polyUvList = new();
        public int? DXValue = null;

        public NJSMeshSet() { }
        public NJSMeshSet(BufferedStreamReaderBE<MemoryStream> sr, bool bigEndian = false, int offset = 0, bool DX = false)
        {
            bool GCColorReverse = sr.streamChecks.ContainsKey("GCColorReverse") ? sr.streamChecks["GCColorReverse"] : false;
            polyInfo = sr.ReadBE<ushort>();
            var faceCount = sr.ReadBE<ushort>();
            var polyAddress = sr.ReadBE<int>();
            polyAttribute = sr.ReadBE<int>();
            var polyNrmAddress = sr.ReadBE<int>();
            var polyClrAddress = sr.ReadBE<int>();
            var polyUvAddress = sr.ReadBE<int>();
            if(DX)
            {
                DXValue = sr.ReadBE<int>();
            }
            var bookmark = sr.Position;

            int indexTotal = 0;
            if(polyAddress != 0)
            {
                sr.Seek(polyAddress + offset, SeekOrigin.Begin);
                for(int i = 0; i < faceCount; i++)
                {
                    switch(PolyType)
                    {
                        case BasicPolyType.Triangles:
                            faceList.Add(new Triangle(sr));
                            break;
                        case BasicPolyType.Quads:
                            faceList.Add(new Quad(sr));
                            break;
                        case BasicPolyType.NPoly:
                        case BasicPolyType.Strips:
                            faceList.Add(new Strip(sr));
                            break;
                    }
                    indexTotal += faceList[i].Indexes.Length;
                }
            }

            if(polyNrmAddress != 0)
            {
                sr.Seek(polyNrmAddress + offset, SeekOrigin.Begin);
                for(int i = 0; i < indexTotal; i++)
                {
                    polyNrmList.Add(sr.ReadBEV3());
                }
            }
            if (polyClrAddress != 0)
            {
                sr.Seek(polyClrAddress + offset, SeekOrigin.Begin);
                for (int i = 0; i < indexTotal; i++)
                {
                    polyClrList.Add(NinjaModelCommon.ReadColor(bigEndian, GCColorReverse, sr.Read4Bytes()));
                }
            }
            if (polyUvAddress != 0)
            {
                sr.Seek(polyUvAddress + offset, SeekOrigin.Begin);
                for (int i = 0; i < indexTotal; i++)
                {
                    polyUvList.Add(sr.ReadBEV2());
                }
            }
            sr.Seek(bookmark, SeekOrigin.Begin);
        }

        public static void Write(List<byte> outBytes, List<NJSMeshSet> meshList, List<int> POF0Offsets)
        {
            for(int j = 0; j < meshList.Count; j++)
            {
                var mesh = meshList[j];
                outBytes.AddValue(mesh.polyInfo);
                outBytes.AddValue((ushort)mesh.faceList.Count);
                POF0Offsets.Add(outBytes.ReserveInt($"polyAddress{j}"));
                outBytes.AddValue(mesh.polyAttribute);
                POF0Offsets.Add(outBytes.ReserveInt($"polyNrmAddress{j}"));
                POF0Offsets.Add(outBytes.ReserveInt($"polyClrAddress{j}"));
                POF0Offsets.Add(outBytes.ReserveInt($"polyUvAddress{j}"));
                if (mesh.DXValue != null)
                {
                    outBytes.AddValue(mesh.DXValue.Value);
                }
            }
            for (int j = 0; j < meshList.Count; j++)
            {
                var mesh = meshList[j];
                if (mesh.faceList.Count > 0)
                {
                    outBytes.FillInt($"polyAddress{j}", outBytes.Count);
                    for (int i = 0; i < mesh.faceList.Count; i++)
                    {
                        outBytes.AddRange(mesh.faceList[i].GetBytes());
                    }
                    outBytes.AlignWriter(0x4, 0);
                }
                if (mesh.polyNrmList.Count > 0)
                {
                    outBytes.FillInt($"polyNrmAddress{j}", outBytes.Count);
                    for (int i = 0; i < mesh.polyNrmList.Count; i++)
                    {
                        outBytes.AddValue(mesh.polyNrmList[i]);
                    }
                    outBytes.AlignWriter(0x4, 0);
                }
                if (mesh.polyClrList.Count > 0)
                {
                    outBytes.FillInt($"polyClrAddress{j}", outBytes.Count);
                    for (int i = 0; i < mesh.polyClrList.Count; i++)
                    {
                        outBytes.AddValue(mesh.polyClrList[i].ToArgb());
                    }
                    outBytes.AlignWriter(0x4, 0);
                }
                if (mesh.polyUvList.Count > 0)
                {
                    outBytes.FillInt($"polyUvAddress{j}", outBytes.Count);
                    for (int i = 0; i < mesh.polyUvList.Count; i++)
                    {
                        outBytes.AddValue(mesh.polyUvList[i]);
                    }
                    outBytes.AlignWriter(0x4, 0);
                }
            }
        }
    }
}
