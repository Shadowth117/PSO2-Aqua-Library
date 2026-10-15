using AquaModelLibrary.Helpers;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.Writers;
using System.Numerics;

namespace AquaModelLibrary.Data.BluePoint.CSKL
{
    public class CSKL
    {
        public bool wasCompressed = false;
        public CSKLHeader header = null;
        public List<CSKLTransform> transforms = new();
        public List<Matrix4x4> invWorldTransforms = new();
        public CSKLMetaData metadata = null;
        public CSKLNames names = null;
        public CFooter footerData;
        public List<float> morphTargetChannelWeightDefaults = new();

        //The partition data and anim op code entries can be regenerated from the transform data
        public List<CSKLPartitionRecord> partitionRecords = new();
        public List<CSKLPartition> partitions = new();
        public List<CSKLAnimOpCodeEntry> animOpCodeEntries = new();

        public const int DeSRMaxBonesPerRecord = 512;

        public CSKL() { }

        public CSKL(byte[] file)
        {
            file = CompressionHandler.CheckCompression(file, out wasCompressed);
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        private void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr.Seek(sr.BaseStream.Length - 0xC, SeekOrigin.Begin);
            footerData = sr.Read<CFooter>();

            sr.Seek(0, SeekOrigin.Begin);
            header = new CSKLHeader(sr, footerData.version);

            switch (footerData.version)
            {
                case 0x9:
                    metadata = new CSKLMetaData();
                    Dictionary<int, CSKLFamilyIds> familyDict = new Dictionary<int, CSKLFamilyIds>();
                    while (sr.Position < header.transformListOffset)
                    {
                        var newFamilyIds = new CSKLFamilyIds();
                        var nodeId = sr.ReadBE<short>();
                        newFamilyIds.parentId = sr.ReadBE<short>();
                        if(newFamilyIds.parentId == 32767)
                        {
                            newFamilyIds.parentId = -1;
                        }
                        if(!familyDict.ContainsKey(nodeId))
                        {
                            familyDict.Add(nodeId, newFamilyIds);
                        } 

                        if (nodeId == header.boneCount - 1)
                        {
                            break;
                        }
                    }
                    for(int i = 0; i < header.boneCount; i++)
                    {
                        metadata.familyIds.Add(familyDict[i]);
                    }

                    //Process bone metadata
                    for(int i = 0; i < metadata.familyIds.Count; i++)
                    {
                        var parId = metadata.familyIds[i].parentId;
                        if ( parId != -1)
                        {
                            var parentBone = metadata.familyIds[parId];
                            if (parentBone.firstChildId == -1)
                            {
                                parentBone.firstChildId = i;
                            }
                            else
                            {
                                var prevSibling = metadata.familyIds[parentBone.firstChildId];
                                while(prevSibling.nextSiblingId != -1)
                                {
                                    prevSibling = metadata.familyIds[prevSibling.nextSiblingId];
                                }
                                prevSibling.nextSiblingId = i;
                            }
                        }
                    }

                    sr.Seek(header.transformListOffset, System.IO.SeekOrigin.Begin);
                    for (int i = 0; i < header.boneCount; i++)
                    {
                        transforms.Add(new CSKLTransform(sr, footerData.version));
                    }

                    sr.Seek(header.boneNamesOffset, System.IO.SeekOrigin.Begin);
                    names = new CSKLNames(sr, footerData.version, header.boneCount);
                    break;
                case 0x19:
                    sr.Seek(header.transformListOffset, System.IO.SeekOrigin.Begin);
                    for (int i = 0; i < header.boneCount; i++)
                    {
                        transforms.Add(new CSKLTransform(sr, footerData.version));
                    }
                    sr.Seek(header.inverseBoneMatricesOffset, System.IO.SeekOrigin.Begin);
                    for (int i = 0; i < header.boneCount; i++)
                    {
                        invWorldTransforms.Add(Matrix4x4.Transpose(new Matrix4x4(sr.Read<float>(), sr.Read<float>(), sr.Read<float>(), sr.Read<float>(),
                                                                            sr.Read<float>(), sr.Read<float>(), sr.Read<float>(), sr.Read<float>(),
                                                                            sr.Read<float>(), sr.Read<float>(), sr.Read<float>(), sr.Read<float>(),
                                                                            0, 0, 0, 1)));
                    }
                    sr.Seek(header.boneMetadataOffset, System.IO.SeekOrigin.Begin);
                    metadata = new CSKLMetaData(sr, header.boneCount, header.morphTargetCount);
                    names = new CSKLNames(sr, footerData.version);

                    sr.Seek(header.partitionRecordOffset, SeekOrigin.Begin);
                    for(int i = 0; i < header.partitionRecordCount; i++)
                    {
                        partitionRecords.Add(new CSKLPartitionRecord(sr));
                    }

                    sr.Seek(header.partitionEntryOffset, SeekOrigin.Begin);
                    for (int i = 0; i < header.partitionEntryCount; i++)
                    {
                        partitions.Add(sr.Read<CSKLPartition>());
                    }

                    if (header.morphTargetCount > 0 && header.transformEndOffset + header.morphTargetCount * 4 <= header.inverseBoneMatricesOffset)
                    {
                        sr.Seek(header.transformEndOffset, SeekOrigin.Begin);
                        for (int i = 0; i < header.morphTargetCount; i++)
                        {
                            morphTargetChannelWeightDefaults.Add(sr.Read<float>());
                        }
                    }

                    sr.Seek(header.animOpCodeEntryOffset, SeekOrigin.Begin);
                    int animOpCodeEntryCount = (header.transformListOffset - header.animOpCodeEntryOffset) / 8;
                    for (int i = 0; i < animOpCodeEntryCount; i++)
                    {
                        animOpCodeEntries.Add(new CSKLAnimOpCodeEntry(sr));
                    }
                    break;
            }
        }

        public byte[] GetBytes()
        {
            switch(footerData.version)
            {
                case 0x19:
                    return GetBytesDeSR();
                default:
                    throw new NotImplementedException();
            }
        }

        public byte[] GetBytesDeSR()
        {
            ByteListWriter outBytes = new();
            var boneCount = transforms.Count;
            partitionRecords.Clear();
            partitions.Clear();
            animOpCodeEntries.Clear();

            //Setup initial partition records
            int cycleStartingBone = 0;
            int partitionIndex = 0;
            while (cycleStartingBone < boneCount)
            {
                int partitionRecordBoneCount = Math.Min(DeSRMaxBonesPerRecord, boneCount - cycleStartingBone);
                bool first = partitionIndex == 0;
                partitionRecords.Add(new CSKLPartitionRecord()
                {
                    boneStartIndex = (ushort)cycleStartingBone,
                    animOpCodeEntryId = (ushort)partitionIndex,      
                    bt_0F = 1,
                    boneCount = (ushort)partitionRecordBoneCount,
                    boneCount2 = (ushort)partitionRecordBoneCount,
                    morphTargetCount = (ushort)(first ? header.morphTargetCount : 0),
                    morphTargetCount2 = (ushort)(first ? header.morphTargetCount : 0),
                });
                cycleStartingBone += partitionRecordBoneCount;
                partitionIndex++;
            }

            //Setup metadata boneFlag0 based on patition
            if (metadata?.familyIds != null && metadata.familyIds.Count >= boneCount)
            {
                cycleStartingBone = 0;
                for (int p = 0; p < partitionRecords.Count; p++)
                {
                    var pr = partitionRecords[p];
                    if (p > 0)
                    {
                        for (int b = cycleStartingBone; b < cycleStartingBone + pr.boneCount && b < boneCount; b++)
                        {
                            metadata.familyIds[b].partitionIndex = (ushort)(p << 8);
                        }
                    }
                    cycleStartingBone += pr.boneCount;
                }
            }

            //Setup Partition Entries
            int partiontBaseBoneIndex = 0;
            int partitionBaseMorphIndex = 0;
            int entryIdx = 0;

            //We still need a defaulted CSKLPartitionRecord and CSKLPartition if there's no bones somehow
            int partitionRecordCount = Math.Max(1, partitionRecords.Count);
            int bonesRemaining = boneCount;

            var partitionTempInfoList = new List<(bool isSkeletalPartition, int bonesUsed, int partitionIndex)>();
            int totalBonesHandled = 0;
            for (int p = 0; p < partitionRecordCount; p++)
            {
                var partitionRecord = p < partitionRecords.Count ? partitionRecords[p] : null;
                int partBones = partitionRecord != null ? partitionRecord.boneCount : bonesRemaining;
                partBones = Math.Min(partBones, bonesRemaining);
                int bonesHandled = 0;
                while (bonesHandled < partBones)
                {
                    int chunk = Math.Min(0x80, partBones - bonesHandled);
                    partitionTempInfoList.Add((isSkeletalPartition: true, bonesUsed: chunk, partitionIndex: p));
                    bonesHandled += chunk;
                    totalBonesHandled += chunk;
                    bonesRemaining -= chunk;
                }

                if (p == 0 && header.morphTargetCount > 0)
                {
                    int sectionsLeft = header.morphTargetCount;
                    while (sectionsLeft > 0)
                    {
                        int chunk = Math.Min(0x80, sectionsLeft);
                        partitionTempInfoList.Add((isSkeletalPartition: false, bonesUsed: chunk, partitionIndex: 0)); 
                        sectionsLeft -= chunk;
                    }
                }
            }
            //Pack partition entries
            var partitionHandled = new bool[partitionRecords.Count];
            for (int partIndex = 0; partIndex < partitionTempInfoList.Count; partIndex++)
            {
                var (isSkeletalPartition, bonesUsed, currentPartitionIndex) = partitionTempInfoList[partIndex];
                bool merge = isSkeletalPartition
                    && partIndex + 1 < partitionTempInfoList.Count
                    && !partitionTempInfoList[partIndex + 1].isSkeletalPartition
                    && partitionTempInfoList[partIndex + 1].partitionIndex == currentPartitionIndex;    
                int runCount = merge ? 2 : 1;

                int runIdx = 0;
                for (int q = 0; q < partIndex; q++)
                {
                    if (partitionTempInfoList[q].partitionIndex == currentPartitionIndex) runIdx++;
                }
                var entry = new CSKLPartition()
                {
                    keyTypeCount = (byte)runCount,
                    partitionRecordIndex = (byte)runIdx,
                };
                if (isSkeletalPartition)
                {
                    entry.boneCount = (ushort)bonesUsed;
                    entry.boneStartingIndex = (ushort)partiontBaseBoneIndex;
                    partiontBaseBoneIndex += bonesUsed;
                }
                if (merge)
                {
                    var curentBonesUsed = partitionTempInfoList[partIndex + 1].bonesUsed;
                    entry.morphTargetCount = (ushort)curentBonesUsed;
                    entry.morpthTargetStartingIndex = (ushort)partitionBaseMorphIndex;
                    partitionBaseMorphIndex += curentBonesUsed;
                }
                else if (!isSkeletalPartition)
                {
                    entry.morphTargetCount = (ushort)bonesUsed;
                    entry.morpthTargetStartingIndex = (ushort)partitionBaseMorphIndex;
                    partitionBaseMorphIndex += bonesUsed;
                }
                partitions.Add(entry);
                var owner = partitionRecords[Math.Min(currentPartitionIndex, partitionRecords.Count - 1)];
                if (currentPartitionIndex < partitionHandled.Length && !partitionHandled[currentPartitionIndex])
                {
                    partitionHandled[currentPartitionIndex] = true;
                    owner.firstPartitionEntryIndex = (ushort)entryIdx;
                    owner.firstPartitionEntryIndex2 = (ushort)entryIdx;
                }
                owner.partitionCount++;
                entryIdx++;
                if (merge) partIndex++; 
            }
            partitionRecords[^1].firstPartitionEntryIndex2 += 1; 
            foreach (var pr in partitionRecords)
            {
                pr.morphTargetCount = (ushort)(pr == partitionRecords[0] ? header.morphTargetCount : 0);
                pr.morphTargetCount2 = pr.morphTargetCount;
                pr.partitionRecordMask = partitionRecords.Count == 1 ? 1u : (uint)((1 << partitionRecords.Count) - 1);
            }
            if (header.morphTargetCount > 0)
            {
                partitions.Add(new CSKLPartition());
            }

            int partitionEntriesOffset = 0x40 + partitionRecords.Count * 0x20;
            int partitionEntryCount = partitions.Count;

            ByteListWriter unkDataBytesAllEntries = new ByteListWriter();
            int unkDataRoot = partitionEntriesOffset + partitionEntryCount * 10 - 0x40;
            foreach (var part in partitionRecords)
            {
                if (part.morphTargetCount > 0)
                {
                    part.unkDataOffset = (ushort)(unkDataRoot + (int)unkDataBytesAllEntries.Count);
                    unkDataBytesAllEntries.AddRange(CSKLPartitionRecord.GetUnkDataBytes(part));
                }
                else
                {
                    part.unkDataOffset = 0;
                }
            }

            List<CSKLAnimOpCodeEntry> opCodeBlocks = new();
            if (opCodeBlocks.Count == 0 && metadata?.familyIds?.Count > 0 && boneCount > 0)
            {
                var parentsArr = new int[boneCount];
                var isProceduralBoneArr = new bool[boneCount];
                for (int i = 0; i < boneCount; i++)
                {
                    int p = metadata.familyIds[i].parentId;
                    parentsArr[i] = p;
                    isProceduralBoneArr[i] = (metadata.familyIds[i].boneFlags & 0x8000) != 0;
                }
                List<int> boneStartIndices = new();
                List<int> boneCountList = new();
                int currentCount = 0;
                foreach (var pr in partitionRecords)
                {
                    boneStartIndices.Add(pr.boneStartIndex);
                    boneCountList.Add(pr.boneCount);
                    currentCount += pr.boneCount;
                }
                opCodeBlocks.AddRange(CSKLAnimOpCodeEntry.BuildFkBlocksForPartitions(parentsArr, isProceduralBoneArr, boneStartIndices, boneCountList, out int totalInterPartitionParents));
                header.interPartitionParentBlocksSize = (ushort)(totalInterPartitionParents * 0x40);
            }

            //Names block
            byte[] namesBytes = names.GetBytes();

            //Header
            outBytes.AddValue(header.magic);
            outBytes.AddValue(header.hash);
            outBytes.AddValue(header.hash2);
            outBytes.ReserveInt("FileSize");

            outBytes.AddValue((ushort)boneCount);
            outBytes.AddValue((ushort)boneCount);
            outBytes.AddValue((ushort)morphTargetChannelWeightDefaults.Count);
            outBytes.Add((byte)partitionRecords.Count);
            outBytes.Add(header.bt_17);
            outBytes.AddValue((ushort)partitionEntryCount);
            outBytes.AddValue(header.interPartitionParentBlocksSize);
            outBytes.AddValue(0);

            outBytes.ReserveInt("TransformOffset");
            outBytes.ReserveInt("MorphDefaultsOffset");
            outBytes.ReserveInt("InverseWorldOffset");
            outBytes.AddValue(0x40); //Partition Records

            outBytes.ReserveInt("PartitionEntriesOffset");
            outBytes.ReserveInt("AnimOpCodeOffset");
            outBytes.AddValue(0);
            outBytes.ReserveInt("MetaDataOffset");

            //Part records
            foreach (var part in partitionRecords)
            {
                outBytes.AddRange(part.GetBytesRecord());
            }

            //Partition entries
            outBytes.FillInt("PartitionEntriesOffset", outBytes.Count);
            foreach (var entry in partitions)
            {
                outBytes.AddRange(entry.GetBytes());
            }

            //Unknown data
            outBytes.AddValue(unkDataBytesAllEntries.ToArray());
            outBytes.AlignWriter(0x8);

            outBytes.FillInt("AnimOpCodeOffset", outBytes.Count);
            //AnimOp data
            int opCodeDataCount = Math.Max(1, opCodeBlocks.Count == 0 ? 1 : opCodeBlocks.Count) + 1;
            for (int i = 0; i < opCodeDataCount; i++)
            {
                if (i < opCodeBlocks.Count)
                {
                    outBytes.ReserveUint16($"AnimOpBlockSize{i}");
                    outBytes.AddValue((ushort)0);
                    outBytes.ReserveInt($"AnimOpBlockOffset{i}");
                }
                else
                {
                    outBytes.AddValue((ushort)0);
                    outBytes.AddValue((ushort)0);
                    outBytes.AddValue(0u);
                }
            }

            //Transforms
            outBytes.FillInt("TransformOffset", outBytes.Count);
            foreach (var tfm in transforms)
            {
                outBytes.AddRange(tfm.GetBytes(0x19));
            }

            //Morph Target Defaults + Anim Op Data Blocks
            int morphDefaultsOffset = 0;
            morphDefaultsOffset = outBytes.Count;
            if (morphTargetChannelWeightDefaults.Count > 0)
            {
                outBytes.FillInt("MorphDefaultsOffset", outBytes.Count);
            }
            ByteListWriter opData = new ByteListWriter();
            //These defaults can exist without their names, but if the names exist the defaults *must* exist
            if (names.morphTargetNames.names.Count > 0 && morphTargetChannelWeightDefaults.Count == 0)
            {
                morphTargetChannelWeightDefaults.Clear();
                for (int i = 0; i < names.morphTargetNames.names.Count; i++)
                {
                    morphTargetChannelWeightDefaults.Add(1);
                }
            }
            if (morphTargetChannelWeightDefaults.Count > 0)
            {
                int sectionLength = morphTargetChannelWeightDefaults.Count * 4;
                int gapBytes = (sectionLength + 0xF) & ~0xF;
                for (int i = 0; i < morphTargetChannelWeightDefaults.Count; i++)
                {
                    opData.AddValue(i < morphTargetChannelWeightDefaults.Count ? morphTargetChannelWeightDefaults[i] : 1);
                }
                for (int i = sectionLength; i < gapBytes; i++)
                {
                    opData.Add(0);
                }
            }
            if (opCodeBlocks.Count > 0)
            {
                for(int i = 0; i < opCodeBlocks.Count; i++)
                {
                    var block = opCodeBlocks[i];
                    int pos = opData.Count;
                    int aligned = (pos + 0xF) & ~0xF;
                    outBytes.FillInt($"AnimOpBlockOffset{i}", morphDefaultsOffset + aligned);

                    opData.AddRange(new byte[aligned - pos]);
                    opData.AddRange(block.rawData);
                }
                int endPos = opData.Count;
                int endAligned = (endPos + 0xF) & ~0xF;
                opData.AddRange(new byte[endAligned - endPos]);
            }
            byte[] postTransform = opData.ToArray();
            for (int i = 0; i < opCodeBlocks.Count; i++)
            {
                var block = opCodeBlocks[i];
                outBytes.FillUint16($"AnimOpBlockSize{i}", (ushort)(block.rawData?.Length ?? 0));
            }

            outBytes.AddValue(postTransform);

            //Inverse binds
            outBytes.FillInt("InverseWorldOffset", outBytes.Count);
            foreach (var mat in invWorldTransforms)
            {
                //Column-major 3x4
                outBytes.AddValue(mat.M11); outBytes.AddValue(mat.M21); outBytes.AddValue(mat.M31); outBytes.AddValue(mat.M41);
                outBytes.AddValue(mat.M12); outBytes.AddValue(mat.M22); outBytes.AddValue(mat.M32); outBytes.AddValue(mat.M42);
                outBytes.AddValue(mat.M13); outBytes.AddValue(mat.M23); outBytes.AddValue(mat.M33); outBytes.AddValue(mat.M43);
            }

            //Metadata
            var metaDataOff = outBytes.Count;
            outBytes.FillInt("MetaDataOffset", metaDataOff);
            outBytes.AddValue(metadata.usht_00);
            outBytes.AddValue(metadata.structSize);
            outBytes.ReserveInt("SkeletonOffset");
            outBytes.ReserveInt("MorphHashOffset");
            outBytes.AddValue(metadata.int_0C);
            outBytes.AddValue(metadata.int_10);
            outBytes.ReserveInt("FamilyIdsOffset");
            outBytes.ReserveInt("NamesRelativeOffset");

            if (boneCount > 0)
            {
                metadata.skeletonHashes.Clear();
                var nameList = names?.skeletonNames?.names;
                for (int i = 0; i < boneCount; i++)
                {
                    string full = (nameList != null && i < nameList.Count) ? nameList[i] : null;
                    if (full != null)
                    {
                        string boneName = full.Substring(full.LastIndexOf('|') + 1);
                        metadata.skeletonHashes.Add(HashHelpers.FNV1a32Hash(boneName.ToUpper()));
                    }
                    else
                    {
                        metadata.skeletonHashes.Add(0);
                    }
                }
            }
            if (names.morphTargetNames.names.Count > 0)
            {
                var morphNameList = names?.morphTargetNames?.names;
                if (morphNameList != null && morphNameList.Count >= header.morphTargetCount)
                {
                    for (int i = 0; i < header.morphTargetCount; i++)
                    {
                        metadata.morphTargetHashes.Add(HashHelpers.FNV1a32Hash(morphNameList[i].ToUpper(), CSKLMetaData.MorphTargetFNVPrime));
                    }
                }
            }

            outBytes.FillInt("SkeletonOffset", outBytes.Count - metaDataOff);
            foreach (var key in metadata.skeletonHashes)
            {
                outBytes.AddValue(key);
            }

            if (metadata.morphTargetHashes.Count > 0)
            {
                outBytes.FillInt("MorphHashOffset", outBytes.Count - metaDataOff);
                foreach (var key in metadata.morphTargetHashes)
                {
                    outBytes.AddValue(key);
                }
            }
            outBytes.FillInt("FamilyIdsOffset", outBytes.Count - metaDataOff);
            foreach (var family in metadata.familyIds)
            {
                outBytes.AddRange(family.GetBytes());
            }
            outBytes.AlignWriter(0x10);

            //Names block + remaining data
            outBytes.FillInt("NamesRelativeOffset", outBytes.Count - metaDataOff);
            outBytes.AddValue(namesBytes);

            outBytes.FillInt("FileSize", outBytes.Count);
            //Footer
            outBytes.AddValue(footerData.magic);
            outBytes.Add(footerData.version);
            outBytes.Add(footerData.unkByte0);
            outBytes.Add(footerData.unkByte1);
            outBytes.Add(footerData.unkByte2);
            outBytes.AddValue(outBytes.Count - 0x8);

            return outBytes.ToArray();
        }
    }
}
