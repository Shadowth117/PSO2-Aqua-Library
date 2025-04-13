using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.BillyHatcher.SetData
{
    public class SetLightParam
    {
        /// <summary>
        /// 0x80 of these, always
        /// </summary>
        public LightParam[] lightParams;

        public SetLightParam() { }

        public SetLightParam(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        public SetLightParam(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        private void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            lightParams = new LightParam[0x80];

            sr._BEReadActive = true;
            for (int i = 0; i < 0x80; i++)
            {
                var light = new LightParam();
                light.usht0 = sr.ReadBE<ushort>();
                light.usht1 = sr.ReadBE<ushort>();
                light.lightDirection = sr.ReadBEV3();

                light.directionalLightingColor = sr.Read4Bytes();
                light.ambientLightingColor = sr.Read4Bytes();
                light.unkRange0 = sr.ReadBE<float>();
                light.flt1C = sr.ReadBE<float>();

                light.int20 = sr.ReadBE<int>();
                light.unkRange1 = sr.ReadBE<int>();
                light.int28 = sr.ReadBE<int>();
                light.int2C = sr.ReadBE<int>();

                light.int30 = sr.ReadBE<int>();
                light.int34 = sr.ReadBE<int>();
                light.int38 = sr.ReadBE<int>();
                light.int3C = sr.ReadBE<int>();

                lightParams[i] = light;
            }
        }

        public byte[] GetBytes()
        {
            List<byte> outBytes = new List<byte>();
            for(int i = 0; i < 0x80; i++)
            {
                var light = lightParams[i];
                outBytes.AddValue(light.usht0);
                outBytes.AddValue(light.usht1);
                outBytes.AddValue(light.lightDirection);

                outBytes.AddValue(light.directionalLightingColor);
                outBytes.AddValue(light.ambientLightingColor);
                outBytes.AddValue(light.unkRange0);
                outBytes.AddValue(light.flt1C);

                outBytes.AddValue(light.int20);
                outBytes.AddValue(light.unkRange1);
                outBytes.AddValue(light.int28);
                outBytes.AddValue(light.int2C);

                outBytes.AddValue(light.int30);
                outBytes.AddValue(light.int34);
                outBytes.AddValue(light.int38);
                outBytes.AddValue(light.int3C);
            }

            return outBytes.ToArray();
        }
    }
    public struct LightParam
    {
        public ushort usht0;
        public ushort usht1;
        public Vector3 lightDirection;

        public byte[] directionalLightingColor; //4 byte RGBA color
        public byte[] ambientLightingColor; //4 byte RGBA color
        public float unkRange0; //Range of some sort
        public float flt1C;

        public int int20;
        public float unkRange1; //Smaller than the first range
        public int int28;
        public int int2C;

        public int int30;
        public int int34;
        public int int38;
        public int int3C;
    }

}
