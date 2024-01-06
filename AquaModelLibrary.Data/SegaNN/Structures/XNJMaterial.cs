using System.ComponentModel;
using System.Numerics;

namespace  AquaModelLibrary.Data.NNStructs.Structures
{
    public class XNJMaterial
    {
        readonly byte[] opaqueBytes = { 0x01, 0x00, 0x00, 0x00, 0x02, 0x03, 0x00, 0x00, 0x03, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x04, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x03, 0x02, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        readonly byte[] translucentBytes = { 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x03, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        readonly byte[] translucentAltBytes = { 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x04, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x03, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        //This is in the top-level list
        public int unknownTopLevelInt;
        //This is the actual material group
        public int unknownID;
        public int unknownInt1;
        //Stuff in the first subgroup
        public Matrix4x4 colorCorrection;
        public float specularIntensity = 8.0f;
        public int unknownSubInt1;
        public int unknownSubInt2;
        public int unknownSubInt3;
        public byte[] materialD3DRenderFlagsUnparsed; //0x40 bytes
        public RenderValues renderData { get; set; }
        //Stuff in the third subgroup
        //Can be chained.
        public BindingList<TextureEntry> textureList { get; set; }
        //Back into the main group
        public int unknownInt2;
        public int unknownInt3;
        public int unknownInt4;

        public XNJMaterial()
        {

        }
    }

    public class TextureEntry
    {
        public byte texturingMode { get; set; } = 1;
        /// <summary>
        /// 0/1 = UV0, 2 = UV1, 0x20 = environment map
        /// </summary>
        public byte uvMappingMode { get; set; } = 1;
        public byte textureMappingMode { get; set; } = 12;
        public byte flag4 { get; set; } = 64;
        public int textureId { get; set; } = 0;
        public float translateU { get; set; } = 0;
        public float translateV { get; set; } = 0;
        public float unknownFloat1 { get; set; } = 0;
        public float unknownFloat2 { get; set; } = 0;
        /// <summary>
        /// Used to look up minfilter/mipfilter/maxanisotropy
        /// </summary>
        public short minifyIndex { get; set; } = 4;
        public short magnifyIndex { get; set; } = 1;
        public uint unknownInt2 { get; set; } = 0;
        public uint unknownInt3 { get; set; } = 0;
        public uint unknownInt4 { get; set; } = 0;
        public uint unknownInt5 { get; set; } = 0;
        public uint unknownInt6 { get; set; } = 0;
        public override string ToString()
        {
            return "(" + texturingMode.ToString("X2") + " " + uvMappingMode.ToString("X2") + " " + textureMappingMode.ToString("X2") + " " + flag4.ToString("X2") + ") Tex: " + textureId
                + " (" + translateU + ", " + translateV + ") " + "(" + unknownFloat1 + ", " + unknownFloat2 + "), "
                + "(" + minifyIndex + ", " + magnifyIndex + "), " + unknownInt2 + " " + unknownInt3 + " " + unknownInt4 + " " + unknownInt5 + " " + unknownInt6;
        }
    }

    public class RenderValues
    {
        public bool AlphaBlendEnable { get; set; }
        public Direct3DRenderStateBlendMode SourceBlend { get; set; }
        public Direct3DRenderStateBlendMode DestinationBlend { get; set; }
        public int unusedInt1 { get; set; }
        public Direct3DRenderStateBlendOperation BlendOperation { get; set; }
        public int unusedInt2 { get; set; }
        public bool AlphaTestEnable { get; set; }
        public Direct3DComparisonFunction AlphaFunction { get; set; }
        public byte AlphaRef { get; set; }
        public bool ZEnable { get; set; }
        public Direct3DComparisonFunction ZFunction { get; set; }
        public bool ZWriteEnable { get; set; }
        public int unusedInt3 { get; set; }
        public int unusedInt4 { get; set; }
        public int unusedInt5 { get; set; }
        public int unusedInt6 { get; set; }

        public RenderValues(byte[] parameters)
        {
            if (parameters.Length < 48) //I'm not requiring the 4 unused values at the end, but if any of the meaningful stuff's gone...
                throw new Exception("Failed to parse render values for material; required at least 48 bytes, received " + parameters.Length);
            AlphaBlendEnable = BitConverter.ToBoolean(parameters, 0);
            SourceBlend = Direct3DEnums.PsuValueToBlendMode(BitConverter.ToInt32(parameters, 4));
            DestinationBlend = Direct3DEnums.PsuValueToBlendMode(BitConverter.ToInt32(parameters, 8));
            unusedInt1 = BitConverter.ToInt32(parameters, 12);
            BlendOperation = Direct3DEnums.PsuValueToBlendOperation(BitConverter.ToInt32(parameters, 16));
            unusedInt2 = BitConverter.ToInt32(parameters, 20);
            AlphaTestEnable = BitConverter.ToBoolean(parameters, 24);
            AlphaFunction = Direct3DEnums.PsuValueToCompareFunc(BitConverter.ToInt32(parameters, 28));
            AlphaRef = parameters[32];
            ZEnable = BitConverter.ToBoolean(parameters, 36);
            ZFunction = Direct3DEnums.PsuValueToCompareFunc(BitConverter.ToInt32(parameters, 40));
            ZWriteEnable = BitConverter.ToBoolean(parameters, 44);
            if (parameters.Length > 51)
                unusedInt3 = BitConverter.ToInt32(parameters, 48);
            if (parameters.Length > 55)
                unusedInt4 = BitConverter.ToInt32(parameters, 52);
            if (parameters.Length > 59)
                unusedInt5 = BitConverter.ToInt32(parameters, 56);
            if (parameters.Length > 62)
                unusedInt6 = BitConverter.ToInt32(parameters, 60);
        }

        public byte[] ToBytes()
        {
            MemoryStream outStream = new MemoryStream();
            BinaryWriter outWriter = new BinaryWriter(outStream);
            outWriter.Write(Convert.ToInt32(AlphaBlendEnable));
            outWriter.Write(SourceBlend.ToPsuValue());
            outWriter.Write(DestinationBlend.ToPsuValue());
            outWriter.Write(unusedInt1);
            outWriter.Write(BlendOperation.ToPsuValue());
            outWriter.Write(unusedInt2);
            outWriter.Write(Convert.ToInt32(AlphaTestEnable));
            outWriter.Write(AlphaFunction.ToPsuValue());
            outWriter.Write((int)AlphaRef);
            outWriter.Write(Convert.ToInt32(ZEnable));
            outWriter.Write(ZFunction.ToPsuValue());
            outWriter.Write(Convert.ToInt32(ZWriteEnable));
            outWriter.Write(unusedInt3);
            outWriter.Write(unusedInt4);
            outWriter.Write(unusedInt5);
            outWriter.Write(unusedInt6);
            return outStream.ToArray();
        }
    }
}
