using AquaModelLibrary.Helpers.Writers;

namespace AquaModelLibrary.Data.BluePoint.CSKL
{
    public struct CSKLPartition
    {
        public ushort boneCount;
        public ushort morphTargetCount;
        /// <summary>
        /// Amount of animation key type channels used, as in bones vs morph targets.
        /// If both are used, this will be 2. If one is used, this will be 1.
        /// In some unused partition data, this may be another value.
        /// </summary>
        public byte keyTypeCount;
        /// <summary>
        /// Index of this partition relative to its set in a given parition record.
        /// </summary>
        public byte partitionRecordIndex;
        /// <summary>
        /// Where this partition's bones start. 
        /// If the last partition's boneCount was 0x80 long, this field should be 0x80.
        /// </summary>
        public ushort boneStartingIndex;
        /// <summary>
        /// Where this partition's bones start. 
        /// If the last partition's boneCount was 0x80 long, this field should be 0x80.
        /// May have other values in unused partition entries.
        /// </summary>
        public ushort morpthTargetStartingIndex;

        public byte[] GetBytes()
        {
            ByteListWriter outBytes = new();
            outBytes.AddValue(boneCount);
            outBytes.AddValue(morphTargetCount);
            outBytes.Add(keyTypeCount);
            outBytes.Add(partitionRecordIndex);
            outBytes.AddValue(boneStartingIndex);
            outBytes.AddValue(morpthTargetStartingIndex);

            return outBytes.ToArray();
        }
    }
}
