using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.AM2.BorderBreakPS4
{
    //All common skeletons
    //Nearly half the file after the header is empty, followed by a series of int16s pertaining to bone child ids.
    public class MOT_BONE
    {
        public boneHeader header;
        public skeletonHeader skeletonHeaderData;
        public List<int> skeletonOffsets = new List<int>();
        public List<int> skeletonNameOffsets = new List<int>();
        public List<skeletonHeader> skeletonHeaders = new List<skeletonHeader>();
        public List<string> skeletonNames = new List<string>();
        public List<List<BoneObject>> skeletonList = new List<List<BoneObject>>();

        public MOT_BONE()
        {

        }

        public MOT_BONE(BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            header = streamReader.Read<boneHeader>();

            //Read skeleton names
            streamReader.Seek(header.skeletonNameOffsetsOffset, SeekOrigin.Begin);
            for (int i = 0; i < header.skeletonCount; i++)
            {
                skeletonNameOffsets.Add(streamReader.Read<int>());
                streamReader.Seek(4, SeekOrigin.Current);
            }
            foreach (var nameOffset in skeletonNameOffsets)
            {
                streamReader.Seek(nameOffset, SeekOrigin.Begin);
                skeletonNames.Add(streamReader.ReadCString());
            }

            //Read skeletons
            streamReader.Seek(header.skeletonOffsetsOffset, SeekOrigin.Begin);
            List<int> skeletonHeaderOffsets = new List<int>();
            for (int i = 0; i < header.skeletonCount; i++)
            {
                skeletonHeaderOffsets.Add(streamReader.Read<int>());
                streamReader.Seek(4, SeekOrigin.Current);
            }
            foreach (var offset in skeletonHeaderOffsets)
            {
                streamReader.Seek(offset, SeekOrigin.Begin);
                skeletonHeaders.Add(streamReader.Read<skeletonHeader>());
            }

            foreach (var skelHead in skeletonHeaders)
            {
                List<BoneObject> skeleton = new List<BoneObject>();
                streamReader.Seek(skelHead.skeletonOffset, SeekOrigin.Begin);
                for (int i = 0; i < skelHead.boneCount; i++)
                {
                    BoneObject boneObj = new BoneObject();
                    boneObj.boneStruct = streamReader.Read<bone>();
                    skeleton.Add(boneObj);
                }
                foreach (var boneObj in skeleton)
                {
                    streamReader.Seek(boneObj.boneStruct.boneNameOffset, SeekOrigin.Begin);
                    boneObj.name = streamReader.ReadCString();
                    streamReader.Seek(boneObj.boneStruct.childBoneIdOffset, SeekOrigin.Begin);
                    for (int i = 0; i < boneObj.boneStruct.childBoneCount; i++)
                    {
                        boneObj.childrenIds.Add(streamReader.Read<ushort>());
                    }
                }
                skeletonList.Add(skeleton);
            }
        }

        public struct boneHeader
        {
            public int int_00;
            public int skeletonCount;
            public int skeletonOffsetsOffset;
            public int int_0C;

            public int skeletonNameOffsetsOffset;
        }

        public struct skeletonHeader
        {
            public ushort usht_00;
            public ushort boneCount;
            public int int_04;
            public int skeletonOffset;
            public int int_0C;
        }

        public class BoneObject
        {
            public bone boneStruct;
            public string name;
            public List<ushort> childrenIds = new List<ushort>();
        }

        public struct bone
        {
            public int boneNameOffset;
            public int int_04;
            public ushort usht_08;
            public ushort usht_0A;
            public ushort usht_0C;
            public ushort usht_0E;

            public Vector3 position;
            public Vector3 eulerRotation; //Appears to be local rotations in radians, rotation order may be conditionally swapped
            public Vector3 scale;

            public int childBoneCount;
            public int childBoneIdOffset; //0 if above is 0
            public int reserve0;
        }
    }
}
