using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.Writers;
using System.Numerics;
using System.Text;

namespace AquaModelLibrary.Data.BluePoint.CMSH
{
    public class CMSHBoneData
    {
        public CLength skelPathLength;
        public string skeletonPath = null;
        public int skeletonHash;
        public int size;


        public List<string> boneNames = new List<string>(); //CString name strings

        /// <summary>
        /// Bounding spheres which encompass all vertices affected by the influence of a particular bone in the mesh.
        /// These are calculated by averaging all vertex positions per face which are influenced at all by a bone 
        /// and then making the radius from the furthest vertex from the center.
        /// The center lastly has the bone's transformation removed by transforming it by the bone's inverse world transform. 
        /// </summary>
        public List<Vector4> boneBoundingSpheres = new List<Vector4>(); 

        public CMSHBoneData()
        {

        }

        public CMSHBoneData(BufferedStreamReaderBE<MemoryStream> sr, BPEra era)
        {
            var pos = sr.Position;
            ReadSkeletonPath(sr, era);
            skeletonHash = sr.Read<int>();
            var nameCount = sr.Read<int>();
            size = sr.Read<int>();

            for (int i = 0; i < nameCount; i++)
            {
                boneNames.Add(sr.ReadCStringSeek());
            }
            var boneVec4Count = sr.Read<int>(); //Should be the same as before, but in case it's not
            for (int i = 0; i < boneVec4Count; i++)
            {
                boneBoundingSpheres.Add(sr.Read<Vector4>());
            }
        }

        private void ReadSkeletonPath(BufferedStreamReaderBE<MemoryStream> sr, BPEra era)
        {
            if(era == BPEra.SOTC)
            {
                var sotcLength = sr.Read<byte>();
                var unkByte = sr.Read<byte>();
                skeletonPath = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position, sotcLength - 1));
                sr.Seek(sotcLength - 1, System.IO.SeekOrigin.Current);
            } else
            {
                skelPathLength = new CLength(sr, era);
                skeletonPath = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position, skelPathLength.GetTrueLength()));
                sr.Seek(skelPathLength.GetTrueLength(), System.IO.SeekOrigin.Current);
            }
        }

        public byte[] GetBytes(BPEra era)
        {
            var outBytes = new ByteListWriter();
            outBytes.AddRange((new BPString(skeletonPath)).GetBytes(era));
            outBytes.AddValue(skeletonHash);
            outBytes.AddValue(boneNames.Count);
            outBytes.ReserveInt("BoneNamesSize");

            for(int i = 0; i < boneNames.Count; i++)
            {
                outBytes.AddRange(Encoding.ASCII.GetBytes(boneNames[i]));
                outBytes.Add(0);
            }
            outBytes.AddValue(boneBoundingSpheres.Count);
            foreach(var vec4 in boneBoundingSpheres)
            {
                outBytes.AddValue(vec4);
            }
            outBytes.FillInt("BoneNamesSize", outBytes.Count);

            return outBytes.ToArray();
        }
    }
}
