using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.Extensions;
using System.Numerics;
using static AquaModelLibrary.Data.BillyHatcher.LNDH.MPL;

namespace AquaModelLibrary.Data.BillyHatcher.LNDH
{
    /// <summary>
    /// These are motion data containers. 
    /// </summary>
    public class MPL
    {
        public MPLHeader header;
        public List<MPLMotionMapping> motionMappingList = new List<MPLMotionMapping>();
        public Dictionary<int, MPLMotionStart> motionDict = new Dictionary<int, MPLMotionStart>();
        public List<MPLMotionStart> motionList = new List<MPLMotionStart>();

        public enum MPLMotionLayout : ushort
        {
            ShortBAMSEuler = 0x21,
            ShortBAMSEulerAndExtra = 0x25,
            IntBAMSEuler = 0x3,
            IntBAMSEuler2 = 0x7,
            Quaternion = 0x2001,
        }

        public enum MPLMotionType : ushort
        {
            Rotation = 0x2,
            RotationAndUnknown = 0x3,
            RotationAndUnknown2 = 0x42,
        }

        public MPL() { }
        public MPL(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            header = new MPLHeader();
            header.int_00 = sr.ReadBE<int>();
            header.int_04 = sr.ReadBE<int>();
            header.motionMappingCount = sr.ReadBE<int>();
            header.motionMappingOffset = sr.ReadBE<int>();
            header.motionCount = sr.ReadBE<int>();
            header.motionOffset = sr.ReadBE<int>();

            sr.Seek(header.motionMappingOffset + 0x20, SeekOrigin.Begin);
            for (int i = 0; i < header.motionMappingCount; i++)
            {
                MPLMotionMapping motionMap = new MPLMotionMapping();
                motionMap.mplMotionKey = sr.ReadBE<short>();
                motionMap.mplMotionId = sr.ReadBE<short>();
                motionMap.flt_04 = sr.ReadBE<float>();
                motionMappingList.Add(motionMap);
            }

            sr.Seek(header.motionOffset + 0x20, SeekOrigin.Begin);
            for (int i = 0; i < header.motionCount; i++)
            {
                MPLMotionStart motionStart = new MPLMotionStart();
                motionStart.int_00 = sr.ReadBE<int>();
                motionStart.offset = sr.ReadBE<int>();
                motionList.Add(motionStart);
            }

            foreach (var motionStart in motionList)
            {
                sr.Seek(motionStart.offset + 0x20, SeekOrigin.Begin);
                motionStart.motionRef = new MPLMotionRef();
                motionStart.motionRef.int_00 = sr.ReadBE<int>();
                motionStart.motionRef.offset = sr.ReadBE<int>();
                sr.Seek(motionStart.motionRef.offset + 0x20, SeekOrigin.Begin);
                var info0 = motionStart.motionRef.motionInfo0 = new MPLMotionInfo0();
                info0.offset = sr.ReadBE<int>();
                info0.int_04 = sr.ReadBE<int>();
                info0.motionLayout = (MPLMotionLayout)sr.ReadBE<ushort>();
                info0.motionType = (MPLMotionType)sr.ReadBE<ushort>();
                sr.Seek(motionStart.motionRef.motionInfo0.offset + 0x20, SeekOrigin.Begin);

                info0.motionInfo1 = new MPLMotionInfo1();
                info0.motionInfo1.int_00 = sr.ReadBE<int>();
                info0.motionInfo1.offset = sr.ReadBE<int>();
                info0.motionInfo1.int_08 = sr.ReadBE<int>();
                info0.motionInfo1.bt_0C = sr.ReadBE<byte>();
                info0.motionInfo1.bt_0D = sr.ReadBE<byte>();
                info0.motionInfo1.bt_0E = sr.ReadBE<byte>();
                info0.motionInfo1.motionDataCount0 = sr.ReadBE<byte>();
                if (info0.motionInfo1.motionDataCount0 == 0)
                {
                    info0.motionInfo1.motionDataCount1 = sr.ReadBE<int>();
                }
                sr.Seek(motionStart.motionRef.motionInfo0.motionInfo1.offset + 0x20, SeekOrigin.Begin);

                info0.motionInfo1.motCount = info0.motionInfo1.motionDataCount0 > 0 ? info0.motionInfo1.motionDataCount0 : info0.motionInfo1.motionDataCount1;

                for (int i = 0; i < info0.motionInfo1.motCount; i++)
                {
                    MPLMotionData motionData = new MPLMotionData();
                    switch (info0.motionLayout)
                    {
                        case MPLMotionLayout.ShortBAMSEuler:
                            motionData.frame = sr.ReadBE<ushort>();
                            motionData.shortsFrame = new short[] { sr.ReadBE<short>(), sr.ReadBE<short>(), sr.ReadBE<short>() };
                            break;
                        case MPLMotionLayout.Quaternion:
                            motionData.frame = sr.ReadBE<int>();
                            var w = sr.ReadBE<float>();
                            motionData.quatFrame = new Quaternion(sr.ReadBE<float>(), sr.ReadBE<float>(), sr.ReadBE<float>(), w);
                            break;
                        case MPLMotionLayout.ShortBAMSEulerAndExtra:
                            motionData.frame = sr.ReadBE<ushort>();
                            motionData.shortsFrame = new short[] { sr.ReadBE<short>(), sr.ReadBE<short>(), sr.ReadBE<short>(), sr.ReadBE<short>(), sr.ReadBE<short>(), sr.ReadBE<short>() };
                            break;
                        case MPLMotionLayout.IntBAMSEuler:
                        case MPLMotionLayout.IntBAMSEuler2:
                            motionData.frame = sr.ReadBE<int>();
                            motionData.intsFrame = new int[] { sr.ReadBE<int>(), sr.ReadBE<int>(), sr.ReadBE<int>() };
                            break;
                        default:
                            throw new Exception();
                    }
                    info0.motionInfo1.motionData.Add(motionData);
                }
            }

            //Create a dictionary for quick reference
            foreach (var map in motionMappingList)
            {
                if (map.mplMotionKey != -1)
                {
                    motionDict.Add(map.mplMotionKey, motionList[map.mplMotionId]);
                }
            }
        }

        public byte[] GetBytes(int offset, out List<int> offsets)
        {
            offsets = new List<int>();

            ByteListExtension.AddAsBigEndian = true;
            List<byte> outBytes = new List<byte>();
            outBytes.AddValue(header.int_00);
            outBytes.AddValue(header.int_04);
            outBytes.AddValue(motionMappingList.Count);

            offsets.Add(offset + outBytes.Count);
            outBytes.ReserveInt("MPLMotionMapping");
            outBytes.AddValue(motionList.Count);
            offsets.Add(offset + outBytes.Count);
            outBytes.ReserveInt("MotionStartOffset");

            //Write MotionMapping
            outBytes.FillInt("MPLMotionMapping", outBytes.Count + offset);
            for (int i = 0; i < motionMappingList.Count; i++)
            {
                var unkData0 = motionMappingList[i];
                outBytes.AddValue(unkData0.mplMotionKey);
                outBytes.AddValue(unkData0.mplMotionId);
                outBytes.AddValue(unkData0.flt_04);
            }

            //Write Motion
            outBytes.FillInt("MotionStartOffset", outBytes.Count + offset);
            for (int i = 0; i < motionList.Count; i++)
            {
                var motionStart = motionList[i];
                outBytes.AddValue(motionStart.int_00);
                offsets.Add(offset + outBytes.Count);
                outBytes.ReserveInt($"MotionRefOffset{i}");
            }

            for (int i = 0; i < motionList.Count; i++)
            {
                var refRef = motionList[i].motionRef;
                outBytes.FillInt($"MotionRefOffset{i}", outBytes.Count + offset);
                outBytes.AddValue(refRef.int_00);
                offsets.Add(offset + outBytes.Count);
                outBytes.ReserveInt($"MotionInfo0{i}");

                var info0 = refRef.motionInfo0;
                outBytes.FillInt($"MotionInfo0{i}", outBytes.Count + offset);
                offsets.Add(offset + outBytes.Count);
                outBytes.ReserveInt($"MotionInfo1{i}");
                outBytes.AddValue(info0.int_04);
                outBytes.AddValue((ushort)info0.motionLayout);
                outBytes.AddValue((ushort)info0.motionType);

                var info1 = info0.motionInfo1;
                outBytes.FillInt($"MotionInfo1{i}", outBytes.Count + offset);
                outBytes.AddValue(info1.int_00);
                offsets.Add(offset + outBytes.Count);
                outBytes.ReserveInt($"MPLMotionData{i}");
                outBytes.AddValue(info1.int_08);
                outBytes.Add(info1.bt_0C);
                outBytes.Add(info1.bt_0D);
                outBytes.Add(info1.bt_0E);
                outBytes.Add(info1.motionDataCount0);
                if (info1.motionDataCount1 > 0)
                {
                    outBytes.AddValue(info1.motionDataCount1);
                }

                outBytes.FillInt($"MPLMotionData{i}", outBytes.Count + offset);
                foreach (var motionData in info1.motionData)
                {
                    outBytes.AddValue(motionData.frame);
                    switch (info0.motionLayout)
                    {
                        case MPLMotionLayout.ShortBAMSEuler:
                            outBytes.AddValue(motionData.shortsFrame[0]);
                            outBytes.AddValue(motionData.shortsFrame[1]);
                            outBytes.AddValue(motionData.shortsFrame[2]);
                            break;
                        case MPLMotionLayout.Quaternion:
                            outBytes.AddValue(motionData.quatFrame.W);
                            outBytes.AddValue(motionData.quatFrame.X);
                            outBytes.AddValue(motionData.quatFrame.Y);
                            outBytes.AddValue(motionData.quatFrame.Z);
                            break;
                        case MPLMotionLayout.ShortBAMSEulerAndExtra:
                            outBytes.AddValue(motionData.shortsFrame[0]);
                            outBytes.AddValue(motionData.shortsFrame[1]);
                            outBytes.AddValue(motionData.shortsFrame[2]);
                            outBytes.AddValue(motionData.shortsFrame[3]);
                            outBytes.AddValue(motionData.shortsFrame[4]);
                            outBytes.AddValue(motionData.shortsFrame[5]);
                            break;
                        case MPLMotionLayout.IntBAMSEuler:
                        case MPLMotionLayout.IntBAMSEuler2:
                            outBytes.AddValue(motionData.intsFrame[0]);
                            outBytes.AddValue(motionData.intsFrame[1]);
                            outBytes.AddValue(motionData.intsFrame[2]);
                            break;
                    }
                }
            }

            return outBytes.ToArray();
        }
    }

    public struct MPLHeader
    {
        public int int_00;
        public int int_04; //The starting value of offset0's data
        public int motionMappingCount;
        public int motionMappingOffset;
        public int motionCount;
        public int motionOffset;
    }

    /// <summary>
    /// For unknown reasons, some of these are seemingly null with values of -1, -1, 0
    /// </summary>
    public struct MPLMotionMapping
    {
        /// <summary>
        /// This key is how animated models will reference an MPL animation. This key's determination is unknown. 
        /// Perhaps how high it is implies it's part of a list of motions that are referenced outside the LND?
        /// </summary>
        public short mplMotionKey;
        /// <summary>
        /// This id is the id for motion data within this MPL.
        /// </summary>
        public short mplMotionId;
        /// <summary>
        /// Usually 1.
        /// </summary>
        public float flt_04;
    }

    public class MPLMotionStart
    {
        public int int_00;
        public int offset;

        public MPLMotionRef motionRef = null;
    }

    public class MPLMotionRef
    {
        public int int_00;
        public int offset;

        public MPLMotionInfo0 motionInfo0 = null;
    }

    public class MPLMotionInfo0
    {
        public int offset;
        public int int_04;
        public MPLMotionLayout motionLayout;
        public MPLMotionType motionType;

        public MPLMotionInfo1 motionInfo1 = null;
    }

    public class MPLMotionInfo1
    {
        public int int_00;
        public int offset;
        public int int_08;
        public byte bt_0C;
        public byte bt_0D;
        public byte bt_0E;
        public byte motionDataCount0;
        public int motionDataCount1;

        public int motCount;
        public List<MPLMotionData> motionData = new List<MPLMotionData>();
    }

    public class MPLMotionData
    {
        public int frame;
        public Quaternion quatFrame;
        public int[] intsFrame;
        public short[] shortsFrame;

        public Vector3 BAMSToDegShorts()
        {
            return new Vector3((float)(shortsFrame[0] / (65536 / 360.0)), (float)(shortsFrame[1] / (65536 / 360.0)), (float)(shortsFrame[2] / (65536 / 360.0)));
        }
        public Vector3 BAMSToDegInts()
        {
            return new Vector3((float)(intsFrame[0] / (65536 / 360.0)), (float)(intsFrame[1] / (65536 / 360.0)), (float)(intsFrame[2] / (65536 / 360.0)));
        }
    }


}
