using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.BillyHatcher.SetData
{
    public class SetCameraList
    {
        public List<SetCamera> setCameras = new List<SetCamera>();
        public SetCameraList() { }
        public SetCameraList(byte[] file)
        {
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        public SetCameraList(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr._BEReadActive = true;
            while (sr.Position + 0x80 < sr.BaseStream.Length)
            {
                SetCamera setCamera = new SetCamera();
                setCamera.type0 = sr.ReadBE<ushort>();
                setCamera.type1 = sr.ReadBE<ushort>();
                setCamera.flt_04 = sr.ReadBE<float>();
                setCamera.flt_08 = sr.ReadBE<float>();
                setCamera.flt_0C = sr.ReadBE<float>();
                setCamera.Position = sr.ReadBEV3();
                setCamera.flt_1C = sr.ReadBE<float>();

                setCamera.flt_1C = sr.ReadBE<float>();
                setCamera.flt_1C = sr.ReadBE<float>();
                setCamera.flt_1C = sr.ReadBE<float>();
                setCamera.flt_1C = sr.ReadBE<float>();

                setCamera.flt_20 = sr.ReadBE<float>();
                setCamera.flt_24 = sr.ReadBE<float>();
                setCamera.flt_28 = sr.ReadBE<float>();
                setCamera.int_2C = sr.ReadBE<int>();

                setCamera.vec3_30 = sr.ReadBEV3();
                setCamera.flt_3C = sr.ReadBE<float>();

                setCamera.int_40 = sr.ReadBE<int>();
                setCamera.vec3_44 = sr.ReadBEV3();

                setCamera.int_50 = sr.ReadBE<int>();
                setCamera.vec2_54 = sr.ReadBEV2();
                setCamera.vec2_5C = sr.ReadBEV2();

                setCamera.int_60 = sr.ReadBE<int>();
                setCamera.int_64 = sr.ReadBE<int>();
                setCamera.int_68 = sr.ReadBE<int>();
                setCamera.int_6C = sr.ReadBE<int>();

                setCamera.int_70 = sr.ReadBE<int>();
                setCamera.int_74 = sr.ReadBE<int>();
                setCamera.int_78 = sr.ReadBE<int>();
                setCamera.int_7C = sr.ReadBE<int>();

                setCameras.Add(setCamera);
            }
        }

        public byte[] GetBytes()
        {
            List<byte> outBytes = new List<byte>();
            ByteListExtension.AddAsBigEndian = true;
            foreach (var setCam in setCameras)
            {
                outBytes.AddValue(setCam.type0);
                outBytes.AddValue(setCam.type1);
                outBytes.AddValue(setCam.flt_04);
                outBytes.AddValue(setCam.flt_08);
                outBytes.AddValue(setCam.flt_0C);
                outBytes.AddValue(setCam.Position);
                outBytes.AddValue(setCam.flt_1C);
                outBytes.AddValue(setCam.flt_20);
                outBytes.AddValue(setCam.flt_24);
                outBytes.AddValue(setCam.flt_28);
                outBytes.AddValue(setCam.int_2C);
                outBytes.AddValue(setCam.vec3_30);
                outBytes.AddValue(setCam.flt_3C);
                outBytes.AddValue(setCam.int_40);
                outBytes.AddValue(setCam.vec3_44);
                outBytes.AddValue(setCam.int_50);
                outBytes.AddValue(setCam.vec2_54);
                outBytes.AddValue(setCam.vec2_5C);
                outBytes.AddValue(setCam.int_60);
                outBytes.AddValue(setCam.int_64);
                outBytes.AddValue(setCam.int_68);
                outBytes.AddValue(setCam.int_6C);
                outBytes.AddValue(setCam.int_70);
                outBytes.AddValue(setCam.int_74);
                outBytes.AddValue(setCam.int_78);
                outBytes.AddValue(setCam.int_7C);
            }

            ByteListExtension.Reset();
            return outBytes.ToArray();
        }
    }

    public struct SetCamera
    {
        public ushort type0;
        public ushort type1;
        /// <summary>
        /// Usually 1. Defaults to 1.
        /// </summary>
        public float flt_04;
        /// <summary>
        /// Usually less than 1 or 1. Defaults to 1.
        /// </summary>
        public float flt_08; 
        public float flt_0C;

        public Vector3 Position;
        public float flt_1C;

        public float flt_20;
        public float flt_24;
        public float flt_28;
        /// <summary>
        /// Defaults to 0xC
        /// </summary>
        public int int_2C; 

        public Vector3 vec3_30;
        public float flt_3C;

        public int int_40;
        public Vector3 vec3_44;

        public int int_50;
        public Vector2 vec2_54;
        public Vector2 vec2_5C;

        public int int_60;
        public int int_64;
        public int int_68;
        public int int_6C;

        public int int_70;
        public int int_74;
        public int int_78;
        public int int_7C;
    }
}
