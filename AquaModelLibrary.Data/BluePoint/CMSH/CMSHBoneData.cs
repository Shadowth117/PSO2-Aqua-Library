using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;
using System.Text;

namespace AquaModelLibrary.Data.BluePoint.CMSH
{
    public class CMSHBoneData
    {
        public CLength skelPathLength;
        public string skeletonPath = null;
        public int nameCount;
        public int unk0;
        public int size;


        public List<string> boneNames = new List<string>(); //CString name strings

        public int boneVec4Count;
        public List<Vector4> boneVec4Array = new List<Vector4>(); //Some entries are 0ed out. 0ed entries may be bones unused in current mesh.

        public CMSHBoneData()
        {

        }

        public CMSHBoneData(BufferedStreamReaderBE<MemoryStream> sr, BPEra era)
        {
            var pos = sr.Position;
            ReadSkeletonPath(sr, era);
            unk0 = sr.Read<int>();
            nameCount = sr.Read<int>();
            size = sr.Read<int>();

            for (int i = 0; i < nameCount; i++)
            {
                boneNames.Add(sr.ReadCStringSeek());
            }
            boneVec4Count = sr.Read<int>(); //Should be the same as before, but in case it's not
            for (int i = 0; i < boneVec4Count; i++)
            {
                boneVec4Array.Add(sr.Read<Vector4>());
            }
        }

        private void ReadSkeletonPath(BufferedStreamReaderBE<MemoryStream> sr, BPEra era)
        {
            skelPathLength = new CLength(sr, era);
            skeletonPath = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position, skelPathLength.GetTrueLength()));
            sr.Seek(skelPathLength.GetTrueLength(), System.IO.SeekOrigin.Current);
        }

        public byte[] GetBytes(BPEra era)
        {
            List<byte> outBytes = new List<byte>();
            outBytes.AddRange((new BPString(skeletonPath)).GetBytes(era));
            outBytes.AddValue(unk0);
            outBytes.AddValue(boneNames.Count);
            outBytes.ReserveInt("BoneNamesSize");

            for(int i = 0; i < boneNames.Count; i++)
            {
                outBytes.AddRange(Encoding.ASCII.GetBytes(boneNames[i]));
                outBytes.Add(0);
            }
            outBytes.AddValue(boneVec4Array.Count);
            foreach(var vec4 in boneVec4Array)
            {
                outBytes.AddValue(vec4);
            }
            outBytes.FillInt("BoneNamesSize", outBytes.Count);

            return outBytes.ToArray();
        }
    }
}
