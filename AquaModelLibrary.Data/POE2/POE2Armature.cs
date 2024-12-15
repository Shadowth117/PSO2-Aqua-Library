using AquaModelLibrary.Data.BluePoint.CSKL;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaMotionData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaNodeData;
using AquaModelLibrary.Helpers.MathHelpers;
using AquaModelLibrary.Helpers.Readers;
using System.ComponentModel.DataAnnotations;
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

        public AquaNode ToAqn()
        {
            AquaNode aqn = new AquaNode();

            Dictionary<int, int> parentDict = new Dictionary<int, int>();
            for (int i = 0; i < bones.Count; i++)
            {
                var parentId = 0xFF;
                var bone = bones[i];
                if (parentDict.ContainsKey(i))
                {
                    parentId = parentDict[i];
                }
                if(bone.childId != 0xFF)
                {
                    parentDict.Add(bone.childId, i);
                }
                if(bone.nextSiblingId != 0xFF && parentId != 0xFF)
                {
                    parentDict.Add(bone.nextSiblingId, parentId);
                }
                var tfmMat = Matrix4x4.Identity;

                Matrix4x4 mat = bone.boneMatrix;

                Matrix4x4.Decompose(mat, out var scale, out var quatRot, out var translation);

                //If there's a parent, multiply by it
                if (parentId != 0xFF)
                {
                    var pn = aqn.nodeList[parentId];
                    var parentInvTfm = new Matrix4x4(pn.m1.X, pn.m1.Y, pn.m1.Z, pn.m1.W,
                                                  pn.m2.X, pn.m2.Y, pn.m2.Z, pn.m2.W,
                                                  pn.m3.X, pn.m3.Y, pn.m3.Z, pn.m3.W,
                                                  pn.m4.X, pn.m4.Y, pn.m4.Z, pn.m4.W);

                    Matrix4x4.Invert(parentInvTfm, out var invParentInvTfm);
                    mat = mat * invParentInvTfm;
                }
                if (parentId == 0xFF && i != 0)
                {
                    parentId = 0;
                } else if (parentId == 0xFF && i == 0)
                {
                    parentId = -1;
                }

                //Create AQN node
                NODE aqNode = new NODE();
                aqNode.boneShort1 = 0x1C0;
                aqNode.animatedFlag = 1;
                aqNode.parentId = parentId;
                aqNode.firstChild = bone.childId;
                aqNode.nextSibling = bone.nextSiblingId;
                aqNode.unkNode = -1;

                aqNode.pos = translation;
                aqNode.eulRot = MathExtras.QuaternionToEuler(quatRot);
                aqNode.scale = new Vector3(1, 1, 1);

                Matrix4x4.Invert(mat, out var invMat);
                aqNode.m1 = new Vector4(invMat.M11, invMat.M12, invMat.M13, invMat.M14);
                aqNode.m2 = new Vector4(invMat.M21, invMat.M22, invMat.M23, invMat.M24);
                aqNode.m3 = new Vector4(invMat.M31, invMat.M32, invMat.M33, invMat.M34);
                aqNode.m4 = new Vector4(invMat.M41, invMat.M42, invMat.M43, invMat.M44);
                aqNode.boneName.SetString(bone.boneName);
                aqn.nodeUnicodeNames.Add(bone.boneName);
                aqn.nodeList.Add(aqNode);
            }

            return aqn;
        }

        public void ToAqm(out List<AquaMotion> motionList, out List<string> motionNames)
        {
            motionList = new List<AquaMotion>();
            motionNames = new List<string>();
            for(int i = 0; i < animList.Count; i++)
            {
                var anim = animList[i];
                motionNames.Add(anim.animName);

                AquaMotion motion = new AquaMotion();
                motion.moHeader = new MOHeader();
                motion.moHeader.endFrame = (int)anim.keySets[0].scaleFrameTimes[^1];
                motion.moHeader.nodeCount = anim.keySets.Count;
                motion.motionKeys = new List<KeyData>();
                for(int ks = 0; ks < anim.keySets.Count; ks++)
                {
                    var keySet = anim.keySets[ks];
                    KeyData keyData = new KeyData();
                    keyData.mseg.nodeName.SetString(bones[keySet.nodeId].boneName);
                    keyData.mseg.nodeId = keySet.nodeId;
                    keyData.keyData = new List<MKEY>();
                    
                    if(keySet.posFrameData.Count > 0)
                    {
                        MKEY mkey = new MKEY();
                        mkey.dataType = 1;
                        mkey.keyType = 1;
                        mkey.keyCount = keySet.posFrameData.Count;

                        for (int k = 0; k < keySet.posFrameData.Count; k++)
                        {
                            //POE2 frame times always seem to be integers stored as floats.
                            //There also always seems to be 2 so we don't have to account for AQM's one frame case
                            mkey.frameTimings.Add((uint)keySet.posFrameTimes[k] * 0x10 + GetPSO2FrameFlag(keySet.posFrameTimes, k));
                            mkey.vector4Keys.Add(new Vector4(keySet.posFrameData[k], 0));
                        }
                        keyData.keyData.Add(mkey);
                    }
                    if (keySet.rotFrameData.Count > 0)
                    {
                        MKEY mkey = new MKEY();
                        mkey.dataType = 3;
                        mkey.keyType = 2;
                        mkey.keyCount = keySet.rotFrameData.Count;

                        for (int k = 0; k < keySet.rotFrameData.Count; k++)
                        {
                            mkey.frameTimings.Add((uint)keySet.rotFrameTimes[k] * 0x10 + GetPSO2FrameFlag(keySet.posFrameTimes, k));
                            mkey.vector4Keys.Add(keySet.rotFrameData[k].ToVec4());
                        }
                        keyData.keyData.Add(mkey);
                    }
                    if (keySet.scaleFrameData.Count > 0)
                    {
                        MKEY mkey = new MKEY();
                        mkey.dataType = 1;
                        mkey.keyType = 3;
                        mkey.keyCount = keySet.scaleFrameData.Count;

                        for (int k = 0; k < keySet.scaleFrameData.Count; k++)
                        {
                            mkey.frameTimings.Add((uint)keySet.scaleFrameTimes[k] * 0x10 + GetPSO2FrameFlag(keySet.posFrameTimes, k));
                            mkey.vector4Keys.Add(new Vector4(keySet.scaleFrameData[k], 0));
                        }
                        keyData.keyData.Add(mkey);
                    }
                    motion.motionKeys.Add(keyData);
                }

                motionList.Add(motion);
            }
        }

        private uint GetPSO2FrameFlag<T>(List<T> frameList, int frameIndex)
        {
            uint flag = 0;
            if (frameIndex == 0)
            {
                flag = 1;
            }
            else if (frameIndex == frameList.Count - 1)
            {
                flag = 2;
            }

            return flag;
        }
    }
}
