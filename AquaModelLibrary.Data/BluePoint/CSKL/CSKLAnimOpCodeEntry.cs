using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.Writers;

namespace AquaModelLibrary.Data.BluePoint.CSKL
{
    public class CSKLAnimOpCodeEntry
    {
        public class CSKLAnimOpProgram
        {
            public ushort opCode;
            public ushort recordCount;
            public ushort tableOffset;
            public List<AnimOpDescriptor> descriptors = new();
        }

        public class EntryBlock
        {
            public byte type;
            public ushort[] blockData = null;
        }

        /// <summary>
        /// Value usages differs based on op type
        /// </summary>
        public struct AnimOpDescriptor
        {
            public ushort dst;
            public ushort src;
        }

        public byte[] rawData = null;
        public List<EntryBlock> entryBlocks = new();
        public List<AnimOpDescriptor> descriptors = new();

        public const ushort OpLeadTransform = 0x28; 
        public const ushort OpLeadMove = 0x35;     

        public ushort FirstProgramOpcode => entryBlocks.Count > 0 ? entryBlocks[0].type : (ushort)0;
        public ushort FirstProgramRecordCount => entryBlocks.Count > 0 && entryBlocks[0].blockData.Length > 0 ? entryBlocks[0].blockData[0] : (ushort)0;
        public ushort FirstProgramTableOffset => entryBlocks.Count > 0 && entryBlocks[0].blockData.Length > 3 ? entryBlocks[0].blockData[3] : (ushort)0;

        public CSKLAnimOpProgram SecondProgram = null;

        public ushort size;
        public ushort usht_02;
        public int offset;

        public CSKLAnimOpCodeEntry() { }
        public CSKLAnimOpCodeEntry(BufferedStreamReaderBE<MemoryStream> sr)
        {
            size = sr.Read<ushort>();
            usht_02 = sr.Read<ushort>();
            offset = sr.Read<int>();

            var bookmark = sr.Position;
            sr.Seek(offset, SeekOrigin.Begin);
            ParseEntry(sr, size, offset);
            sr.Seek(bookmark, SeekOrigin.Begin);
        }

        public byte[] GetInfoBytes()
        {
            ByteListWriter outBytes = new ByteListWriter();
            outBytes.AddValue(size);
            outBytes.AddValue(usht_02);
            outBytes.AddValue(offset);

            return outBytes.ToArray();
        }

        public void ParseEntry(BufferedStreamReaderBE<MemoryStream> sr, ushort bufferSize, int offset)
        {
            if (bufferSize == 0)
            {
                return;
            }

            int ushortCount = bufferSize / 2;
            var ushortArr = new ushort[ushortCount];
            for (int i = 0; i < ushortCount; i++)
            {
                ushortArr[i] = sr.Read<ushort>();
            }

            int p = 0;
            int bufferGuard = 0;
            //Read entry blocks
            while (p < ushortCount && bufferGuard++ < 0x10000)
            {
                ushort head = ushortArr[p];
                byte type = (byte)(head & 0xFF);
                if (type == 0)
                {
                    break;
                }
                byte blockDataLength = (byte)(head >> 8);
                int takeLength = Math.Min(blockDataLength, ushortCount - p - 1);
                var blockData = new ushort[takeLength];
                if (takeLength > 0)
                {
                    Array.Copy(ushortArr, p + 1, blockData, 0, takeLength);
                }
                entryBlocks.Add(new EntryBlock() { type = type, blockData = blockData });
                p += blockDataLength + 1;
            }

            //Decode entry blocks
            if (entryBlocks.Count > 0 && (entryBlocks[0].type == OpLeadTransform || entryBlocks[0].type == OpLeadMove)
                && entryBlocks[0].blockData.Length >= 5)
            {
                var firstBlock = entryBlocks[0];
                int tableOffset = firstBlock.blockData[3] / 2; 
                if (tableOffset >= 0 && tableOffset < ushortCount - 1)
                {
                    int remainingData = (ushortCount - tableOffset) / 2;
                    //0x28 counts 16 byte records, 0x35 counts pairs
                    int dataCount = firstBlock.type == OpLeadTransform ? firstBlock.blockData[0] * 4 : firstBlock.blockData[0];
                    dataCount = Math.Min(dataCount, remainingData);
                    for (int i = 0; i < dataCount; i++)
                    {
                        descriptors.Add(new AnimOpDescriptor() { dst = ushortArr[tableOffset + i * 2], src = ushortArr[tableOffset + i * 2 + 1] });
                    }
                }
                //0x35 program after a 0x28 (e.g. c0000)
                if (entryBlocks.Count > 1 && entryBlocks[0].type == OpLeadTransform
                    && entryBlocks[1].type == OpLeadMove && entryBlocks[1].blockData.Length >= 5)
                {
                    var secondBlock = entryBlocks[1];
                    SecondProgram = new CSKLAnimOpProgram()
                    {
                        opCode = secondBlock.type,
                        recordCount = secondBlock.blockData[0],
                        tableOffset = secondBlock.blockData[3],
                    };
                    int secondProgramOffset = secondBlock.blockData[3] / 2; 
                    if (secondProgramOffset >= 0 && secondProgramOffset < ushortCount - 1)
                    {
                        int remainingData = (ushortCount - secondProgramOffset) / 2;
                        int dataCount = Math.Min(secondBlock.blockData[0], remainingData);
                        for (int i = 0; i < dataCount; i++)
                        {
                            SecondProgram.descriptors.Add( new AnimOpDescriptor() { dst = ushortArr[secondProgramOffset + i * 2], src = ushortArr[secondProgramOffset + i * 2 + 1] });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Builds forward kinematics op code programs, one per partitionRecord.
        /// </summary>
        public static List<CSKLAnimOpCodeEntry> BuildFkBlocksForPartitions(int[] parentsArr, bool[] isProceduralBoneArr, List<int> boneStartIndices, List<int> boneCountList, out int totalInterPartitionParents)
        {
            totalInterPartitionParents = 0;
            int boneCount = parentsArr.Length;
            Dictionary<int, int> parentPartitionDict = new Dictionary<int, int>();
            var sortedParents = new List<int>();
            if(boneStartIndices.Count > 1)
            {
                for (int i = boneStartIndices[1]; i < boneCount; i++)
                {
                    var parentId = parentsArr[i];
                    for (int j = 0; j < boneStartIndices.Count; j++)
                    {
                        if (i >= (boneStartIndices[j] + boneCountList[j]) && parentId < (boneStartIndices[j] + boneCountList[j]) && !sortedParents.Contains(parentId))
                        {
                            sortedParents.Add(parentId);
                            parentPartitionDict[parentId] = j;
                            totalInterPartitionParents++;
                            break;
                        }
                    }
                }
                sortedParents.Sort();
            }

            var blocks = new List<CSKLAnimOpCodeEntry>(boneStartIndices.Count);
            for (int p = 0; p < boneStartIndices.Count; p++)
            {
                int boneStart = boneStartIndices[p];
                int currentArrayBoneCount = boneCountList[p];

                var x28descriptors = new List<AnimOpDescriptor>(currentArrayBoneCount);
                for(int i = 0; i < boneCountList[p]; i++)
                {
                    int bone = boneStart + i;
                    int parentId = parentsArr[bone];
                    ushort indexAndFlag;
                    if (parentId < 0 || parentId >= boneCount)
                    {
                        indexAndFlag = 0x0009;                                  
                    }
                    else if (parentId >= boneStart && parentId < boneStart + currentArrayBoneCount)
                    {
                        indexAndFlag = (ushort)(((parentId - boneStart + 1) << 6) | 1);       
                    }
                    else if (parentPartitionDict[parentId] < p && sortedParents.Contains(parentId))
                    {
                        indexAndFlag = (ushort)((sortedParents.IndexOf(parentId) << 6) | 3);               
                    }
                    else
                    {
                        indexAndFlag = 0x0009;
                    }
                    if (isProceduralBoneArr != null && bone < isProceduralBoneArr.Length && isProceduralBoneArr[bone])
                    {
                        indexAndFlag |= 0x0008;                                  
                    }
                    x28descriptors.Add(new AnimOpDescriptor() { dst = (ushort)((i + 1) << 4), src = indexAndFlag});      
                }

                //0x35 move pairs appending this part's later-referenced bones to seg3
                var x35Moves = new List<AnimOpDescriptor>();
                int partitionInterparentIndex = 0;
                foreach(var parentId in sortedParents)
                {
                    if (parentPartitionDict[parentId] == p)
                    {
                        int srcSlot = (partitionInterparentIndex++ - boneStart) + 1;
                        for (int row = 0; row < 4; row++)
                        {
                            x35Moves.Add(new AnimOpDescriptor()
                            {
                                dst = (ushort)((sortedParents.IndexOf(parentId) << 6) | (row << 4) | 3),
                                src = (ushort)((srcSlot << 6) | (row << 4) | 1)
                            });
                        }
                    }
                }

                blocks.Add(BuildCombinedFkBlock(x28descriptors, x35Moves));
            }
            return blocks;
        }

        /// <summary>
        /// Assembles the processed data from BuildFkBlocksForPartitionss
        /// </summary>
        public static CSKLAnimOpCodeEntry BuildCombinedFkBlock(List<AnimOpDescriptor> x28Descriptors, List<AnimOpDescriptor> x35Moves)
        {
            int fkRecords = (x28Descriptors.Count + 3) / 4;
            bool hasMoves = x35Moves != null && x35Moves.Count > 0;

            int leadBytes = hasMoves ? 0x1A : 0x0E;
            int headerLen = (leadBytes + 0xF) & ~0xF;
            int fkTableOff = headerLen;
            int fkTableBytes = fkRecords * 16;
            int mvRecords = hasMoves ? (x35Moves.Count + 3) / 4 : 0;
            int mvTableOff = fkTableOff + fkTableBytes;
            int mvTableBytes = mvRecords * 16;

            ByteListWriter outBytes = new();
            outBytes.AddValue((ushort)0x0528);                       
            outBytes.AddValue((ushort)fkRecords);
            outBytes.AddValue((ushort)3);                            
            outBytes.AddValue((ushort)0);
            outBytes.AddValue((ushort)fkTableOff);
            outBytes.AddValue((ushort)0);                            
            if (hasMoves)
            {
                outBytes.AddValue((ushort)0x0535);                   
                outBytes.AddValue((ushort)x35Moves.Count);           
                outBytes.AddValue((ushort)3);
                outBytes.AddValue((ushort)0);
                outBytes.AddValue((ushort)mvTableOff);
                outBytes.AddValue((ushort)0);
            }
            outBytes.AddValue((ushort)0);                            
            while (outBytes.Count < headerLen)
            {
                outBytes.AddValue((ushort)0);                        
            }
            for (int i = 0; i < fkRecords * 4; i++)
            {
                AnimOpDescriptor descriptor = i < x28Descriptors.Count ? x28Descriptors[i] : x28Descriptors[0];
                outBytes.AddValue(descriptor.dst);
                outBytes.AddValue(descriptor.src);
            }
            if (hasMoves)
            {
                for (int i = 0; i < mvRecords * 4; i++)
                {
                    var m = i < x35Moves.Count ? x35Moves[i] : default;
                    outBytes.AddValue(m.dst);
                    outBytes.AddValue(m.src);
                }
            }
            return new CSKLAnimOpCodeEntry()
            {
                rawData = outBytes.ToArray(),
            };
        }
    }
}
