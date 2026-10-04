using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.Writers;
using System.Buffers.Binary;
using System.Numerics;
using static AquaModelLibrary.Data.BluePoint.CMSH.CMSHMorphMapping;

namespace AquaModelLibrary.Data.BluePoint.CMSH
{
    /// <summary>
    /// Vertex morph/blendshape delta data. Must alway be alongside a morph mapping.
    /// Delta data cannot be read without the morph mapping
    /// </summary>
    public class CMSHDeltas
    {
        public long position;

        public int size;
        public byte[] buffer = null;

        public CMSHDeltas() { }
        public CMSHDeltas(BufferedStreamReaderBE<MemoryStream> sr)
        {
            position = sr.Position;

            size = sr.Read<int>();
            buffer = sr.ReadBytes(sr.Position, size);
            sr.Seek(size, System.IO.SeekOrigin.Current);
        }

        public class MorphDelta
        {
            public int channelId = -1;
            public Dictionary<int, Vector3> vertexDeltaDict = new();
        }

        public Dictionary<int, MorphDelta> DecodeChannels(List<MorphMapping> morphMapList)
        {
            var deltaDict = new Dictionary<int, MorphDelta>();
            foreach (var mapping in morphMapList)
            {
                if (!deltaDict.TryGetValue(mapping.channelId, out var morphDelta))
                {
                    deltaDict[mapping.channelId] = morphDelta = new MorphDelta { channelId = mapping.channelId };
                }
                foreach (var (vertId, offset) in mapping.vertDeltaOffsets)
                {
                    var delta = DecodeDelta(BitConverter.ToUInt32(buffer, offset));
                    // In the case that there's somehow a dupe entry, the first for that mapping is used 
                    if (!morphDelta.vertexDeltaDict.ContainsKey(vertId))
                    {
                        morphDelta.vertexDeltaDict[vertId] = delta;
                    }
                }
            }

            return deltaDict;
        }

        /// <summary>
        /// The shader pretty much just does this with it
        /// </summary>
        private static Vector3 DecodeDelta(uint rawDelta)
        {
            int mx = (int)(rawDelta & 0x1FF);
            int my = (int)((rawDelta >> 9) & 0x1FF);
            int mz = (int)((rawDelta >> 18) & 0x1FF);
            int e = (int)((rawDelta >> 27) & 0x1F);
            float scale = MathF.Pow(2, e - 15);

            return new Vector3(
                (mx - 255) / 255f * scale,
                (my - 255) / 255f * scale,
                (mz - 255) / 255f * scale);
        }

        public byte[] GetBytes()
        {
            var outBytes = new ByteListWriter();

            outBytes.AddValue(buffer.Length);
            outBytes.AddRange(buffer);

            return outBytes.ToArray();
        }
    }
}
