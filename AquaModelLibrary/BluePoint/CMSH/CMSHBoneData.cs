using AquaModelLibrary.AquaMethods;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.BluePoint.CMSH
{
    public class CMSHBoneData
    {
        public byte skelPathLength;
        public string skeletonPath;
        public int nameCount;
        public int unk0;
        public int size;

        public List<string> boneNames = new List<string>(); //CString name strings

        public int boneVec4Count;
        public List<Vector4> boneVec4Array = new List<Vector4>(); //Some entries are 0ed out. 0ed entries may be bones unused in current mesh.

        public CMSHBoneData()
        {

        }

        public CMSHBoneData(BufferedStreamReader sr)
        {
            var pos = sr.Position();
            skelPathLength = sr.Read<byte>();
            skeletonPath = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position(), skelPathLength));
            sr.Seek(skelPathLength, System.IO.SeekOrigin.Current);
            unk0 = sr.Read<int>();
            nameCount = sr.Read<int>();
            size = sr.Read<int>();

            for(int i = 0; i < nameCount; i++)
            {
                boneNames.Add(AquaGeneralMethods.ReadCStringSeek(sr));
            }
            boneVec4Count = sr.Read<int>(); //Should be the same as before, but in case it's not
            for(int i = 0; i < boneVec4Count; i++)
            {
                boneVec4Array.Add(sr.Read<Vector4>());
            }
        }
    }
}
