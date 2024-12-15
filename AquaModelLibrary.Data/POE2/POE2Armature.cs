using AquaModelLibrary.Helpers.Readers;
using System.Numerics;
using System.Text;

namespace AquaModelLibrary.Data.POE2
{
    public class POE2Armature
    {
        public List<Bone> bones = new List<Bone>();
        public List<Anim> animList = new List<Anim>();
        public POE2Armature() { }

        public POE2Armature(byte[] file)
        {
            Read(file);
        }

        public POE2Armature(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }
        public void Read(byte[] file)
        {
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }
        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            var bt_0 = sr.ReadBE<byte>();
            var boneCount = sr.ReadBE<byte>();
            var bt_1 = sr.ReadBE<byte>();
            var animCount = sr.ReadBE<byte>();
            var int_04 = sr.ReadBE<int>();

            for(int i = 0; i < boneCount; i++)
            {
                bones.Add(new Bone(sr));
            }
            for(int i = 0; i < animCount; i++)
            {
                animList.Add(new Anim(sr));
            }
            var compressedAnimDataStart = sr.Position;
            ReadAnimData(POE2ArchiveUtility.DecompressArchive(sr.ReadBytesSeek((int)(sr.BaseStream.Length - compressedAnimDataStart)))[0].file);
        }

        public void ReadAnimData(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                ReadAnimData(sr);
            }
        }

        public void ReadAnimData(BufferedStreamReaderBE<MemoryStream> sr)
        {
            for(int a = 0; a < animList.Count; a++)
            {
                var anim = animList[a];
                sr.Seek(anim.animOffset, SeekOrigin.Begin);

                for (int i = 0; i < bones.Count; i++)
                {
                    anim.keySets.Add(new KeySet(sr));
                }
            }
        }

        public class KeySet
        {
            public byte unkByte;
            public int nodeId;

            public List<float> scaleFrameTimes = new List<float>();
            public List<Vector3> scaleFrameData = new List<Vector3>();
            public List<float> rotFrameTimes = new List<float>();
            public List<Quaternion> rotFrameData = new List<Quaternion>();
            public List<float> posFrameTimes = new List<float>();
            public List<Vector3> posFrameData = new List<Vector3>();

            public KeySet() { }

            public KeySet(byte[] file)
            {
                Read(file);
            }

            public KeySet(BufferedStreamReaderBE<MemoryStream> sr)
            {
                Read(sr);
            }
            public void Read(byte[] file)
            {
                using (MemoryStream ms = new MemoryStream(file))
                using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
                {
                    Read(sr);
                }
            }
            public void Read(BufferedStreamReaderBE<MemoryStream> sr)
            {
                unkByte = sr.ReadBE<byte>();
                nodeId = sr.ReadBE<int>();
                var scaleFrameCount = sr.ReadBE<int>();
                var rotFrameCount = sr.ReadBE<int>();
                var posFrameCount = sr.ReadBE<int>();
                var unkInt0 = sr.ReadBE<int>();
                var unkInt1 = sr.ReadBE<int>();
                var unkInt2 = sr.ReadBE<int>();
                var unkInt3 = sr.ReadBE<int>();

                for(int i = 0; i < scaleFrameCount; i++)
                {
                    scaleFrameTimes.Add(sr.ReadBE<float>());
                    scaleFrameData.Add(sr.ReadBEV3());
                }
                for (int i = 0; i < rotFrameCount; i++)
                {
                    rotFrameTimes.Add(sr.ReadBE<float>());
                    rotFrameData.Add(sr.ReadBEQuat());
                }
                for (int i = 0; i < posFrameCount; i++)
                {
                    posFrameTimes.Add(sr.ReadBE<float>());
                    posFrameData.Add(sr.ReadBEV3());
                }
            }
        }

        public class Bone
        {
            public byte nextSiblingId;
            public byte childId;
            public Matrix4x4 boneMatrix;
            public string boneName;

            public Bone() { }

            public Bone(byte[] file)
            {
                Read(file);
            }

            public Bone(BufferedStreamReaderBE<MemoryStream> sr)
            {
                Read(sr);
            }
            public void Read(byte[] file)
            {
                using (MemoryStream ms = new MemoryStream(file))
                using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
                {
                    Read(sr);
                }
            }
            public void Read(BufferedStreamReaderBE<MemoryStream> sr)
            {
                nextSiblingId = sr.ReadBE<byte>();
                childId = sr.ReadBE<byte>();
                boneMatrix = sr.Read<Matrix4x4>();
                var nameLength = sr.ReadBE<ushort>();
                boneName = Encoding.UTF8.GetString(sr.ReadBytesSeek(nameLength));
            }
        }

        public class Anim
        {
            public ushort boneCount;
            public ushort usht_02;
            public byte bt_04;
            public int nameLength;
            public int animOffset;
            public int animSize;
            public string animName;

            public List<KeySet> keySets = new List<KeySet>();
            public Anim() { }

            public Anim(byte[] file)
            {
                Read(file);
            }

            public Anim(BufferedStreamReaderBE<MemoryStream> sr)
            {
                Read(sr);
            }
            public void Read(byte[] file)
            {
                using (MemoryStream ms = new MemoryStream(file))
                using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
                {
                    Read(sr);
                }
            }
            public void Read(BufferedStreamReaderBE<MemoryStream> sr)
            {
                boneCount = sr.ReadBE<ushort>();
                usht_02 = sr.ReadBE<ushort>();
                bt_04 = sr.ReadBE<byte>();
                nameLength = sr.ReadBE<ushort>();
                animOffset = sr.ReadBE<int>();
                animSize = sr.ReadBE<int>();
                animName = Encoding.UTF8.GetString(sr.ReadBytesSeek(nameLength));
            }
        }
    }
}
