using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaMotionData;
using AquaModelLibrary.Data.DataTypes.SetLengthStrings;

namespace AquaModelLibrary.Data.Utility
{
    public class NGSAnimUpdater
    {
        /*
         * Left Side
            (4)l_thigh - (52)l_thigh_alt
            (5)l_thigh_tw - (54)l_thigh_tw2_alt
            (6)l_calf - (55)l_calf0_alt
            (7)l_foot_effl - (57)l_foot_alt
            (9)l_legadd - (60)l_calf0_kmn

         * Right Side
            (11)r_thigh - (63)r_thigh_alt
            (12)r_thigh_tw - (65)r_thigh_tw2_alt
            (13)r_calf - (66)r_calf0_alt
            (14)r_foot_effr - (68)r_foot_alt
            (16)r_legadd - (71)r_calf0_kmn
         */

        public Dictionary<int, int> remapDict = new Dictionary<int, int>()
        {
            {52, 4 },
            {54, 5 },
            {55, 6 },
            {57, 7 },
            {60, 9 },

            {63, 11 },
            {65, 12 },
            {66, 13 },
            {68, 14 },
            {71, 16 },
        }; //Goes one way since NGS anims should play nice with pso2 classic models already.
           //Maps pso2 classic bones to NGS bones for converting custom animations.

        //Note, as of this writing, NGS only writes from ids 0 - 171 to animations, with the 172nd animation "node" being the NodeTreeFlag thing.
        //Nodes after this are presumably handled in the fltd and then by the physics engine. 
        public int oldRange = 50; //50 because animations don't keep the bodymodel bit
        public int endRange = 171; //camera_target
        public List<string> nodeNames = new List<string>(); //Default node names for NGS player models
        public Dictionary<int, Quaternion> defaultRots = new Dictionary<int, Quaternion>(); //Bind pose rotations for animations
        public Dictionary<int, Vector4> defaultPos = new Dictionary<int, Vector4>(); //Bind pose translations for animations


        //Basically, bone nodes have a matrix with world coordinates, but we need coords local to the parent. Therefore, we grab these here.
        public void GetDefaultTransformsFromBones(AquaNode bones)
        {
            nodeNames.Clear();
            defaultRots.Clear();
            for (int i = 0; i <= endRange; i++) //<= since we do want to hit that last one
            {
                var bone = bones.nodeList[i];
                Matrix4x4 inverseWorldMatrix = new Matrix4x4(bone.m1.X, bone.m1.Y, bone.m1.Z, bone.m1.W, bone.m2.X, bone.m2.Y, bone.m2.Z, bone.m2.W,
                    bone.m3.X, bone.m3.Y, bone.m3.Z, bone.m3.W, bone.m4.X, bone.m4.Y, bone.m4.Z, bone.m4.W);
                Matrix4x4.Invert(inverseWorldMatrix, out Matrix4x4 worldMatrix);

                Quaternion localRot;
                Vector4 localPos;
                if (bone.parentId == -1)
                {
                    localRot = Quaternion.Inverse(Quaternion.CreateFromRotationMatrix(inverseWorldMatrix));
                    localPos = new Vector4(inverseWorldMatrix.M41, inverseWorldMatrix.M42, inverseWorldMatrix.M43, inverseWorldMatrix.M44);
                }
                else
                {
                    var boneParent = bones.nodeList[bone.parentId];
                    Matrix4x4 parentInverseWorldMatrix = new Matrix4x4(boneParent.m1.X, boneParent.m1.Y, boneParent.m1.Z, boneParent.m1.W, boneParent.m2.X,
                        boneParent.m2.Y, boneParent.m2.Z, boneParent.m2.W, boneParent.m3.X, boneParent.m3.Y, boneParent.m3.Z, boneParent.m3.W, boneParent.m4.X,
                        boneParent.m4.Y, boneParent.m4.Z, boneParent.m4.W);

                    var localMatrix = Matrix4x4.Multiply(worldMatrix, parentInverseWorldMatrix);

                    localRot = Quaternion.CreateFromRotationMatrix(localMatrix);
                    localPos = new Vector4(localMatrix.M41, localMatrix.M42, localMatrix.M43, localMatrix.M44);
                }

                nodeNames.Add(bone.boneName.GetString());
                defaultRots.Add(i, localRot);
                defaultPos.Add(i, localPos);
            }

        }

        public void UpdateToNGSPlayerMotion(AquaMotion motion)
        {
            //Add the nodeTreeFlag if it's there
            KeyData nodeTree = null;
            if (motion.motionKeys[motion.motionKeys.Count - 1].mseg.nodeName.GetString().Contains("NodeTreeFlag"))
            {
                nodeTree = motion.motionKeys[motion.motionKeys.Count - 1];
            }

            //Remove to the old count
            while (motion.motionKeys.Count > oldRange)
            {
                motion.motionKeys.RemoveAt(motion.motionKeys.Count - 1);
            }

            //Go through and add NGS nodes 
            for (int i = oldRange; i < endRange + 1; i++)
            {
                var keySet = new KeyData();
                keySet.mseg.nodeDataCount = 3;
                keySet.mseg.nodeId = i;
                keySet.mseg.nodeType = 2;
                keySet.mseg.nodeName = new PSO2String(nodeNames[i]);

                var pos = new MKEY();
                var rot = new MKEY();
                var scale = new MKEY();

                //If the key isn't in there, set to the default. Else set based on the old id
                if (!remapDict.ContainsKey(i))
                {
                    // position
                    pos.dataType = 1;
                    pos.keyCount = 1;
                    pos.keyType = 1;
                    pos.vector4Keys.Add(defaultPos[i]);

                    // rotation
                    rot.dataType = 3;
                    rot.keyCount = 1;
                    rot.keyType = 2;
                    rot.vector4Keys.Add(new Vector4(defaultRots[i].X, defaultRots[i].Y, defaultRots[i].Z, defaultRots[i].W));

                    // scale
                    scale.dataType = 1;
                    scale.keyCount = 1;
                    scale.keyType = 3;
                    scale.vector4Keys.Add(new Vector4(1.0f, 1.0f, 1.0f, 0));
                }
                else
                {
                    for (int keyData = 0; keyData < motion.motionKeys[remapDict[i]].keyData.Count; keyData++)
                    {
                        switch (motion.motionKeys[remapDict[i]].keyData[keyData].keyType)
                        {
                            case 1:
                                pos = motion.motionKeys[remapDict[i]].keyData[keyData];
                                if (motion.motionKeys[remapDict[i]].keyData[keyData].vector4Keys.Count > 1)
                                {
                                    for (int frameId = 0; frameId < motion.motionKeys[remapDict[i]].keyData[keyData].vector4Keys.Count; frameId++)
                                    {
                                        var frame = motion.motionKeys[remapDict[i]].keyData[keyData].vector4Keys[frameId];
                                        frame = frame - defaultPos[remapDict[i]];
                                        motion.motionKeys[remapDict[i]].keyData[keyData].vector4Keys[frameId] = frame + defaultPos[i];
                                    }
                                }
                                break;
                            case 2:
                                rot = motion.motionKeys[remapDict[i]].keyData[keyData];

                                //Extract and apply rotation for every frame
                                for (int frameId = 0; frameId < motion.motionKeys[remapDict[i]].keyData[keyData].vector4Keys.Count; frameId++)
                                {
                                    var frame = motion.motionKeys[remapDict[i]].keyData[keyData].vector4Keys[frameId];
                                    var quat = new Quaternion(frame.X, frame.Y, frame.Z, frame.W);
                                    quat = quat * Quaternion.Inverse(defaultRots[remapDict[i]]);
                                    quat = quat * defaultRots[i];

                                    motion.motionKeys[remapDict[i]].keyData[keyData].vector4Keys[frameId] = new Vector4(quat.X, quat.Y, quat.Z, quat.W);
                                }
                                break;
                            case 3:
                                scale = motion.motionKeys[remapDict[i]].keyData[keyData];
                                break;
                        }
                    }

                    //Check if anything was left out and set to defaults if so
                    if (pos.keyType == 0)
                    {
                        pos.dataType = 1;
                        pos.keyCount = 1;
                        pos.keyType = 1;
                        pos.vector4Keys.Add(defaultPos[i]);
                    }
                    if (rot.keyType == 0)
                    {
                        rot.dataType = 3;
                        rot.keyCount = 1;
                        rot.keyType = 2;
                        rot.vector4Keys.Add(new Vector4(defaultRots[i].X, defaultRots[i].Y, defaultRots[i].Z, defaultRots[i].W));
                    }
                    if (scale.keyType == 0)
                    {
                        scale.dataType = 1;
                        scale.keyCount = 1;
                        scale.keyType = 3;
                        scale.vector4Keys.Add(new Vector4(1.0f, 1.0f, 1.0f, 0));
                    }
                }

                keySet.keyData.Add(pos);
                keySet.keyData.Add(rot);
                keySet.keyData.Add(scale);

                motion.motionKeys.Add(keySet);
            }

            if (nodeTree != null)
            {
                nodeTree.mseg.nodeId = 0;
                motion.motionKeys.Add(nodeTree);
            }
            else
            {
                //Generate NodeTreeFlag
                nodeTree = new KeyData();
                nodeTree.mseg.nodeDataCount = 3;
                nodeTree.mseg.nodeId = 0;
                nodeTree.mseg.nodeType = 16;
                nodeTree.mseg.nodeName = new PSO2String("__NodeTreeFlag__");

                for (int set = 0; set < 3; set++)
                {
                    var mkey = new MKEY();
                    mkey.dataType = 5;
                    mkey.keyCount = motion.moHeader.endFrame + 1;
                    mkey.keyType = 16 + set;
                    for (int key = 0; key < mkey.keyCount; key++)
                    {
                        if (key == 0)
                        {
                            mkey.frameTimings.Add(0x9);
                        }
                        else if (key == mkey.keyCount - 1)
                        {
                            mkey.frameTimings.Add((uint)(key * mkey.GetTimeMultiplier() + 0xA));
                        }
                        else
                        {
                            mkey.frameTimings.Add((uint)(key * mkey.GetTimeMultiplier() + 0x8));
                        }
                        mkey.intKeys.Add(0x31);
                    }
                    nodeTree.keyData.Add(mkey);
                }
            }
            motion.moHeader.nodeCount = motion.motionKeys.Count;
        }
    }
}
