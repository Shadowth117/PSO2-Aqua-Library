using AquaModelLibrary.Data.PSO2.Aqua.AquaMotionData;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    //Though the NIFL format is used for storage, VTBF format tag references for data will be commented where appropriate. Some offset/reserve related things are NIFL only, however.

    //Cameras, UV, and standard motions are essentially the same format.
    public unsafe class AquaMotion : AquaCommon
    {
        public MOHeader moHeader;
        public List<KeyData> motionKeys = new List<KeyData>();

        //Converts scale keys to the more typical absolute scaling so they can be edited in a nice way externally.
        public void PrepareScalingForExport(AquaNode aqn)
        {
            int boneCount = System.Math.Min(motionKeys.Count, aqn.nodeList.Count);
            for (int i = 0; i < boneCount; i++)
            {
                var node = aqn.nodeList[i];
                if (node.parentId >= 0)
                {
                    var nodeScale = motionKeys[i].GetMKEYofType(3);
                    var parentNodeScale = motionKeys[node.parentId].GetMKEYofType(3);
                    if (nodeScale == null)
                    {
                        continue;
                    }

                    //Fix up the keyset in case there's only one key
                    //Normally, there's no frametimings at all with single keys, so this simplifies the next part
                    if (parentNodeScale.frameTimings.Count > 1 && nodeScale.frameTimings.Count < 2)
                    {
                        nodeScale.frameTimings.Clear();
                        nodeScale.frameTimings.Add(0x1);
                        nodeScale.frameTimings.Add((ushort)(moHeader.endFrame * 0x10 + 0x2));
                        nodeScale.vector4Keys.Add(nodeScale.vector4Keys[0]); //If there's scale, there should always be at least one key for it
                    }

                    //Create keyframes for each parent key so we can cancel them all
                    List<uint> timingsToAdd = new List<uint>();
                    for (int t = 0; t < parentNodeScale.frameTimings.Count; t++)
                    {
                        if (!nodeScale.frameTimings.Contains(parentNodeScale.frameTimings[t]))
                        {
                            timingsToAdd.Add((uint)parentNodeScale.frameTimings[t]);
                        }
                    }
                    nodeScale.CreateVec4KeysAtTimes(timingsToAdd);
                }
            }

            //Loop backwards through and use the relative scaling of the parent to cancel the child scaling
            for (int i = (boneCount - 1); i > 0; i--)
            {
                var node = aqn.nodeList[i];
                if (node.parentId >= 0)
                {
                    var nodeScale = motionKeys[i].GetMKEYofType(3);
                    var parentNodeScale = motionKeys[node.parentId].GetMKEYofType(3);
                    int mode = 0;

                    //Check order flags
                    if ((node.boneShort1 & 1) > 0)
                    {
                        mode = 1;
                    }

                    if (nodeScale != null)
                    {
                        //Get rid of parental influence
                        for (int t = 0; t < nodeScale.frameTimings.Count; t++)
                        {
                            var currentTime = nodeScale.frameTimings[t];
                            var value = parentNodeScale.GetLinearInterpolatedVec4Key(currentTime);

                            nodeScale.RemoveParentScaleInfluenceAtTime(nodeScale.frameTimings[t], mode, value);
                        }
                    }
                }
            }
        }

    }
}
