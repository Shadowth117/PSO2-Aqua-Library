using Reloaded.Memory.Streams;
using System.Collections.Generic;

namespace AquaModelLibrary.Extra.Ninja.BillyHatcher.LNDH
{
    /// <summary>
    /// These are motion data containers. 
    /// </summary>
    public class MPL
    {
        public MPLHeader header;
        public List<MPLMotionMapping> motionMappingList = new List<MPLMotionMapping>();
        public List<MPLUnkRef> mplLinkRefList = new List<MPLUnkRef>();

        public MPL() { }
        public MPL(BufferedStreamReader sr)
        {
            Read(sr);
        }

        public void Read(BufferedStreamReader sr)
        {
            header = new MPLHeader();
            header.int_00 = sr.ReadBE<int>();
            header.int_04 = sr.ReadBE<int>();
            header.motionMappingCount = sr.ReadBE<int>();
            header.motionMappingOffset = sr.ReadBE<int>();
            header.mPLUnkRefCount = sr.ReadBE<int>();
            header.mPLUnkRefOffset = sr.ReadBE<int>();

            sr.Seek(header.motionMappingOffset + 0x20, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < header.motionMappingCount; i++)
            {
                MPLMotionMapping motionMap = new MPLMotionMapping();
                motionMap.mplMotionKey = sr.ReadBE<short>();
                motionMap.mplMotionId = sr.ReadBE<short>();
                motionMap.isValidMapping = sr.ReadBE<float>();
                motionMappingList.Add(motionMap);
            }

            sr.Seek(header.mPLUnkRefOffset + 0x20, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < header.mPLUnkRefCount; i++)
            {
                MPLUnkRef unkRef = new MPLUnkRef();
                unkRef.int_00 = sr.ReadBE<int>();
                unkRef.offset = sr.ReadBE<int>();
                mplLinkRefList.Add(unkRef);
            }

            foreach (var unkRef in mplLinkRefList)
            {
                sr.Seek(unkRef.offset + 0x20, System.IO.SeekOrigin.Begin);
                unkRef.unkRefRef = new MPLUnkRefRef();
                unkRef.unkRefRef.int_00 = sr.ReadBE<int>();
                unkRef.unkRefRef.offset = sr.ReadBE<int>();
                var info0 = unkRef.unkRefRef.unkData1Info0 = new MPLUnkData1Info0();
                info0.offset = sr.ReadBE<int>();
                info0.int_04 = sr.ReadBE<int>();
                info0.bt_08 = sr.ReadBE<byte>();
                info0.bt_09 = sr.ReadBE<byte>();
                info0.bt_0A = sr.ReadBE<byte>();
                info0.bt_0B = sr.ReadBE<byte>();

                info0.unkData1Info1 = new MPLUnkData1Info1();
                info0.unkData1Info1.int_00 = sr.ReadBE<int>();
                info0.unkData1Info1.offset = sr.ReadBE<int>();
                info0.unkData1Info1.int_08 = sr.ReadBE<int>();
                info0.unkData1Info1.bt_0C = sr.ReadBE<byte>();
                info0.unkData1Info1.bt_0D = sr.ReadBE<byte>();
                info0.unkData1Info1.bt_0E = sr.ReadBE<byte>();
                info0.unkData1Info1.unkData1Count = sr.ReadBE<byte>();

                for (int i = 0; i < info0.unkData1Info1.unkData1Count; i++)
                {
                    MPLUnkData1 unkData1 = new MPLUnkData1();
                    unkData1.int_00 = sr.ReadBE<int>();
                    unkData1.flt_04 = sr.ReadBE<float>();
                    unkData1.flt_08 = sr.ReadBE<float>();
                    unkData1.int_0C = sr.ReadBE<int>();
                    unkData1.int_10 = sr.ReadBE<int>();
                    info0.unkData1Info1.unkData1List.Add(unkData1);
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
            outBytes.ReserveInt("UnkData0Offset");
            outBytes.AddValue(mplLinkRefList.Count);
            offsets.Add(offset + outBytes.Count);
            outBytes.ReserveInt("UnkRefOffset");

            //Write unkData0
            outBytes.FillInt("UnkData0Offset", outBytes.Count - 0x20);
            for (int i = 0; i < motionMappingList.Count; i++)
            {
                var unkData0 = motionMappingList[i];
                outBytes.AddValue(unkData0.mplMotionKey);
                outBytes.AddValue(unkData0.mplMotionId);
                outBytes.AddValue(unkData0.isValidMapping);
            }

            //Write unkData1
            outBytes.FillInt("UnkRefOffset", outBytes.Count - 0x20);
            for (int i = 0; i < mplLinkRefList.Count; i++)
            {
                var unkData1 = mplLinkRefList[i];
                outBytes.AddValue(unkData1.int_00);
                offsets.Add(offset + outBytes.Count);
                outBytes.ReserveInt($"UnkRefRefOffset{i}");
            }

            for (int i = 0; i < mplLinkRefList.Count; i++)
            {
                var refRef = mplLinkRefList[i].unkRefRef;
                outBytes.FillInt($"UnkRefRefOffset{i}", outBytes.Count - 0x20);
                outBytes.AddValue(refRef.int_00);
                offsets.Add(offset + outBytes.Count);
                outBytes.ReserveInt($"MPLUnkData1Info0{i}");

                var info0 = refRef.unkData1Info0;
                outBytes.FillInt($"MPLUnkData1Info0{i}", outBytes.Count - 0x20);
                offsets.Add(offset + outBytes.Count);
                outBytes.ReserveInt($"MPLUnkData1Info1{i}");
                outBytes.AddValue(info0.int_04);
                outBytes.Add(info0.bt_08);
                outBytes.Add(info0.bt_09);
                outBytes.Add(info0.bt_0A);
                outBytes.Add(info0.bt_0B);

                var info1 = info0.unkData1Info1;
                outBytes.FillInt($"MPLUnkData1Info1{i}", outBytes.Count - 0x20);
                outBytes.AddValue(info1.int_00);
                offsets.Add(offset + outBytes.Count);
                outBytes.ReserveInt($"MPLUnkData1{i}");
                outBytes.AddValue(info1.int_08);
                outBytes.AddValue(info1.bt_0C);
                outBytes.AddValue(info1.bt_0D);
                outBytes.AddValue(info1.bt_0E);
                outBytes.AddValue(info1.unkData1Count);

                outBytes.FillInt($"MPLUnkData1{i}", outBytes.Count - 0x20);
                foreach (var unkData1 in info1.unkData1List)
                {
                    outBytes.AddValue(unkData1.int_00);
                    outBytes.AddValue(unkData1.flt_04);
                    outBytes.AddValue(unkData1.flt_08);
                    outBytes.AddValue(unkData1.int_0C);
                    outBytes.AddValue(unkData1.int_10);
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
        public int mPLUnkRefCount;
        public int mPLUnkRefOffset;
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
        /// Usually 1. Signifies if it's a valid map?? Mistakenly not a boolean?
        /// </summary>
        public float isValidMapping;
    }

    public class MPLUnkRef
    {
        public int int_00;
        public int offset;

        public MPLUnkRefRef unkRefRef = null;
    }

    public class MPLUnkRefRef
    {
        public int int_00;
        public int offset;

        public MPLUnkData1Info0 unkData1Info0 = null;
    }

    public class MPLUnkData1Info0
    {
        public int offset;
        public int int_04;
        public byte bt_08;
        public byte bt_09;
        public byte bt_0A;
        public byte bt_0B;

        public MPLUnkData1Info1 unkData1Info1 = null;
    }

    public class MPLUnkData1Info1
    {
        public int int_00;
        public int offset;
        public int int_08;
        public byte bt_0C;
        public byte bt_0D;
        public byte bt_0E;
        public byte unkData1Count;

        public List<MPLUnkData1> unkData1List = new List<MPLUnkData1>();
    }

    public class MPLUnkData1
    {
        public int int_00;
        public float flt_04;
        public float flt_08;
        public int int_0C;
        public int int_10;
    }

}
