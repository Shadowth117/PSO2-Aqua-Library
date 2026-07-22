using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.Extensions;
using System.Numerics;
using AquaModelLibrary.Data.Ninja.Motion;

namespace AquaModelLibrary.Data.BillyHatcher.LNDH
{
    /// <summary>
    /// Motion data containers. 
    /// </summary>
    public class MPL
    {
        public MPLHeader header;
        public List<MPLMotionMapping> motionMappingList = new List<MPLMotionMapping>();
        public Dictionary<int, MPLMotionRootParent> motionDict = new Dictionary<int, MPLMotionRootParent>();
        public List<MPLMotionRootParent> motionList = new List<MPLMotionRootParent>();

        public MPL() { }
        public MPL(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            header = new MPLHeader();
            header.int_00 = sr.ReadBE<int>();
            header.lowestKey = sr.ReadBE<int>();
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
                motionMap.entryIsUsed = sr.ReadBE<float>();
                motionMappingList.Add(motionMap);
            }

            sr.Seek(header.motionOffset + 0x20, SeekOrigin.Begin);
            for (int i = 0; i < header.motionCount; i++)
            {
                MPLMotionRootParent motionStart = new MPLMotionRootParent();
                motionStart.int_00 = sr.ReadBE<int>();
                motionStart.offset = sr.ReadBE<int>();
                motionList.Add(motionStart);
            }

            foreach (var motionStart in motionList)
            {
                sr.Seek(motionStart.offset + 0x20, SeekOrigin.Begin);
                motionStart.motionRef = new MPLMotionParent();
                motionStart.motionRef.int_00 = sr.ReadBE<int>();
                motionStart.motionRef.offset = sr.ReadBE<int>();
                sr.Seek(motionStart.motionRef.offset + 0x20, SeekOrigin.Begin);
                motionStart.motionRef.motion = new NJSMotion(sr, true, 0x20);
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

        public void Write(List<byte> outBytes, List<int> offsets)
        {
            ByteListExtension.AddAsBigEndian = true;
            outBytes.AddValue(header.int_00);
            outBytes.AddValue(header.lowestKey);
            outBytes.AddValue(motionMappingList.Count);

            offsets.Add(outBytes.Count);
            outBytes.ReserveInt("MPLMotionMapping");
            outBytes.AddValue(motionList.Count);
            offsets.Add(outBytes.Count);
            outBytes.ReserveInt("MotionStartOffset");

            //Write MotionMapping
            outBytes.FillInt("MPLMotionMapping", outBytes.Count);
            for (int i = 0; i < motionMappingList.Count; i++)
            {
                var unkData0 = motionMappingList[i];
                outBytes.AddValue(unkData0.mplMotionKey);
                outBytes.AddValue(unkData0.mplMotionId);
                outBytes.AddValue(unkData0.entryIsUsed);
            }

            //Write Motion
            outBytes.FillInt("MotionStartOffset", outBytes.Count);
            for (int i = 0; i < motionList.Count; i++)
            {
                var motionStart = motionList[i];
                outBytes.AddValue(motionStart.int_00);
                offsets.Add(outBytes.Count);
                outBytes.ReserveInt($"MotionRefOffset{i}");
            }

            for (int i = 0; i < motionList.Count; i++)
            {
                var refRef = motionList[i].motionRef;
                outBytes.FillInt($"MotionRefOffset{i}", outBytes.Count);
                outBytes.AddValue(refRef.int_00);

                offsets.Add(outBytes.Count);
                outBytes.AddValue(outBytes.Count + 4); //Pointer to the motion directly after

                refRef.motion.Write(outBytes, offsets, NJSMotion.MotionWriteMode.BillyMode);
            }

            return;
        }
    }

    public struct MPLHeader
    {
        public int int_00;
        public int lowestKey; //The starting value of offset0's data
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
        /// Seemingly determines if the mapping is used. This seems to only be 0 when the id and key are both -1.
        /// Likely artifacts from conversion optimized partly out.
        /// </summary>
        public float entryIsUsed;
    }

    public class MPLMotionRootParent
    {
        public int int_00;
        public int offset;

        public MPLMotionParent motionRef = null;
    }

    public class MPLMotionParent
    {
        public int int_00;
        public int offset;

        public NJSMotion motion = null;
    }

}
