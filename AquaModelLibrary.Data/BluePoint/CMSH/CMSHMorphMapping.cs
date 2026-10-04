using AquaModelLibrary.Data.BluePoint.CGPR;
using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.Writers;
using System.ComponentModel;
using System.Numerics;

namespace AquaModelLibrary.Data.BluePoint.CMSH
{
    /// <summary>
    /// Mapping for vertex morph/blendshape deltas. This must always be alongside that data. 
    /// </summary>
    public class CMSHMorphMapping
    {
        public long position;
        /// <summary>
        /// Different skeleton hash than what cskl and cani use.
        /// Same per skeleton in all meshes that use the same skeleton, but only used when checking LODs in the same mesh, so it can be anything as long as that's consistent.
        /// CMSHBoneData's skeletonhash is this same value. 
        /// </summary>
        public int skeletonHash;
        public int flags;
        public int unk1;
        public int count;
        public List<List<CMSHMorphMapEntry>> morphMapListList = new();

        public struct CMSHMorphMapEntry
        {
            public int faceIndex;
            public int vertMask;
        }

        public CMSHMorphMapping()
        {

        }

        public CMSHMorphMapping(BufferedStreamReaderBE<MemoryStream> sr, int vertCount)
        {
            position = sr.Position;

            skeletonHash = sr.Read<int>();
            flags = sr.Read<int>();
            unk1 = sr.Read<int>();
            count = sr.Read<int>();
            var pos = sr.Position;

            int groupCount = (vertCount + 31) / 32;
            int channelCount = count / Math.Max(1, groupCount);
            for (int i = 0; i < channelCount; i++)
            {
                var morphMapEntryList = new List<CMSHMorphMapEntry>();
                for (int j = 0; j < groupCount; j++)
                {
                    morphMapEntryList.Add(sr.Read<CMSHMorphMapEntry>());
                }
                morphMapListList.Add(morphMapEntryList);
            }
        }

        public class MorphMapping
        {
            public int channelId = -1;
            public Dictionary<int, int> vertDeltaOffsets = new Dictionary<int, int>();
            public MorphMapping() { }
            public MorphMapping(int _channelIndex)
            {
                channelId = _channelIndex;
            }
        }

        public List<MorphMapping> GetProcessedMorphMapping()
        {
            List<MorphMapping> mapList = new();
            for(int i = 0; i < morphMapListList.Count; i++)
            {
                for(int j = 0; j < morphMapListList[i].Count; j++)
                {
                    var entry = morphMapListList[i][j];
                    //Skip totally empty entries
                    if(entry.vertMask == 0)
                    {
                        continue;
                    }
                    int faceoffset = (entry.faceIndex & 0x7FFFFFFF) * 4;
                    int stride = (entry.faceIndex & 0x80000000) != 0 ? 12 : 8;
                    var map = new MorphMapping(i);
                    for(int bit = 0; bit < 32; bit++)
                    {
                        //If this vertex isn't used, we skip it
                        if ((entry.vertMask & (1u << bit)) == 0)
                        {
                            continue;
                        }
                        int column = BitOperations.PopCount((uint)entry.vertMask & ((1u << bit) - 1));
                        map.vertDeltaOffsets.Add(j * 32 * bit, faceoffset + column * stride);
                    }
                    mapList.Add(map);
                }
            }

            return mapList;
        }

        public byte[] GetBytes()
        {
            var outBytes = new ByteListWriter();
            outBytes.AddValue(skeletonHash);
            outBytes.AddValue(flags);
            outBytes.AddValue(unk1);
            outBytes.AddValue(morphMapListList.Count);
            foreach (var channelList in morphMapListList)
            {
                foreach (var group in channelList)
                {
                    outBytes.AddValue(group.faceIndex);
                    outBytes.AddValue(group.vertMask);
                }
            }

            return outBytes.ToArray();
        }
    }
}
