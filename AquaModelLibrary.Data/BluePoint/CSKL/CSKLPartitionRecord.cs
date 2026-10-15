using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.Writers;
using UnluacNET;
using static AquaModelLibrary.Data.BillyHatcher.PolyAnim.DataChunks;

namespace AquaModelLibrary.Data.BluePoint.CSKL
{
    public class CSKLPartitionRecord
    {
        /// <summary>
        /// The starting index for the first bone in the record.
        /// These are at 512 (0x200) bone intervals
        /// </summary>
        public ushort boneStartIndex;
        /// <summary>
        /// Index of the first partition entry for this record.
        /// </summary>
        public ushort firstPartitionEntryIndex;
        /// <summary>
        /// Same as above, but + 1 if it's the last entry, seemingly
        /// </summary>
        public ushort firstPartitionEntryIndex2;
        public ushort animOpCodeEntryId;
        public ushort usht_08;
        public ushort usht_0A;
        public ushort unkDataOffset;
        /// <summary>
        /// The amount of partitions tied to this record that use aniamted bones.
        /// </summary>
        public byte partitionCount;
        /// <summary>
        /// Always 1?
        /// </summary>
        public byte bt_0F;
        public ushort boneCount;
        /// <summary>
        /// Always the same as the previous count
        /// </summary>
        public ushort boneCount2;
        public ushort morphTargetCount;
        /// <summary>
        /// Always the same as the previous count
        /// </summary>
        public ushort morphTargetCount2;
        /// <summary>
        /// Calced by ((1 << (currentListId + 1)) - 1)
        ///So for id 0, 1. For id 1, you get 3. For id 2, you get 7 etc. 
        /// </summary>
        public uint partitionRecordMask;
        public int padding_1C;

        public List<PartitionRecordUnkData> unkData = new();

        public CSKLPartitionRecord() { }

        public CSKLPartitionRecord(BufferedStreamReaderBE<MemoryStream> sr)
        {
            var start = sr.Position;

            boneStartIndex = sr.Read<ushort>();
            firstPartitionEntryIndex = sr.Read<ushort>();
            firstPartitionEntryIndex2 = sr.Read<ushort>();
            animOpCodeEntryId = sr.Read<ushort>();
            usht_08 = sr.Read<ushort>();
            usht_0A = sr.Read<ushort>();
            unkDataOffset = sr.Read<ushort>();
            partitionCount = sr.Read<byte>();
            bt_0F = sr.Read<byte>();
            boneCount = sr.Read<ushort>();
            boneCount2 = sr.Read<ushort>();
            morphTargetCount = sr.Read<ushort>();
            morphTargetCount2 = sr.Read<ushort>();
            partitionRecordMask = sr.Read<uint>();
            padding_1C = sr.Read<int>();

            var bookmark = sr.Position;
            sr.Seek(start + unkDataOffset, SeekOrigin.Begin);
            for (int i = 0; i < morphTargetCount; i++)
            {
                unkData.Add(sr.Read<PartitionRecordUnkData>());
            }
            sr.Seek(bookmark, SeekOrigin.Begin);
        }

        public byte[] GetBytesRecord()
        {
            ByteListWriter outBytes = new();
            outBytes.AddValue(boneStartIndex);
            outBytes.AddValue(firstPartitionEntryIndex);
            outBytes.AddValue(firstPartitionEntryIndex2);
            outBytes.AddValue(animOpCodeEntryId);
            outBytes.AddValue(usht_08);
            outBytes.AddValue(usht_0A);
            outBytes.AddValue(unkDataOffset);
            outBytes.Add(partitionCount);
            outBytes.Add(bt_0F);

            outBytes.AddValue(boneCount);
            outBytes.AddValue(boneCount2);
            outBytes.AddValue(morphTargetCount);
            outBytes.AddValue(morphTargetCount2);
            outBytes.AddValue(partitionRecordMask);
            outBytes.AddValue(padding_1C);

            return outBytes.ToArray();
        }

        public struct PartitionRecordUnkData
        {
            public ushort data0;
            public ushort data1;
        }

        public static byte[] GetUnkDataBytes(CSKLPartitionRecord record)
        {
            ByteListWriter outBytes = new();
            record.RebuildUnkData();
            foreach(var data in record.unkData)
            {
                outBytes.AddValue(data.data0);
                outBytes.AddValue(data.data1);
            }

            return outBytes.ToArray();
        }

        public void RebuildUnkData()
        {
            unkData.Clear();
            for (int i = 0; i < morphTargetCount; i++)
            {
                unkData.Add(new PartitionRecordUnkData()
                {
                    data0 = (ushort)(2 + 4 * i),
                    data1 = (ushort)(4 * i),
                });
            }
        }
    }
}
