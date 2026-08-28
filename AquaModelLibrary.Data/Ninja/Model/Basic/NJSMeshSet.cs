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
                    polyClrList.Add(ReadColor(bigEndian, GCColorReverse, sr.Read4Bytes()));
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
        }

        private Color ReadColor(bool bigEndian, bool GCColorReverse, byte[] colorBytes)
        {
            switch (bigEndian)
            {
                case true:
                    switch(GCColorReverse)
                    {
                        case true:
                            return Color.FromArgb(colorBytes[3], colorBytes[0], colorBytes[1], colorBytes[2]);
                        case false:
                            return Color.FromArgb(colorBytes[0], colorBytes[1], colorBytes[2], colorBytes[3]);
                    }
                case false:
                    switch (GCColorReverse)
                    {
                        case true:
                            return Color.FromArgb(colorBytes[0], colorBytes[3], colorBytes[2], colorBytes[1]);
                        case false:
                            return Color.FromArgb(colorBytes[3], colorBytes[2], colorBytes[1], colorBytes[0]);
                    }
            }
        }

        public void Write(List<byte> outBytes, List<int> POF0Offsets)
        {
            outBytes.AddValue(polyInfo);
            outBytes.AddValue((ushort)faceList.Count);
            outBytes.ReserveInt("polyAddress");
            outBytes.AddValue(polyAttribute);
            outBytes.ReserveInt("polyNrmAddress");
            outBytes.ReserveInt("polyClrAddress");
            outBytes.ReserveInt("polyUvAddress");
            if (DXValue != null)
            {
                outBytes.AddValue(DXValue.Value);
            }

            if(faceList.Count > 0)
            {
                outBytes.FillInt("polyAddress", outBytes.Count);
                for(int i = 0; i < faceList.Count; i++)
                {
                    outBytes.AddRange(faceList[i].GetBytes());
                }
                outBytes.AlignWriter(0x4, 0);
            }
            if(polyNrmList.Count > 0)
            {
                outBytes.FillInt("polyNrmAddress", outBytes.Count);
                for (int i = 0; i < polyNrmList.Count; i++)
                {
                    outBytes.AddValue(polyNrmList[i]);
                }
                outBytes.AlignWriter(0x4, 0);
            }
            if(polyClrList.Count > 0)
            {
                outBytes.FillInt("polyClrAddress", outBytes.Count);
                for (int i = 0; i < polyClrList.Count; i++)
                {
                    outBytes.AddValue(polyClrList[i].ToArgb());
                }
                outBytes.AlignWriter(0x4, 0);
            }
            if(polyUvList.Count > 0)
            {
                outBytes.FillInt("polyUvAddress", outBytes.Count);
                for (int i = 0; i < polyUvList.Count; i++)
                {
                    outBytes.AddValue(polyUvList[i]);
                }
                outBytes.AlignWriter(0x4, 0);
            }
        }
    }
}
