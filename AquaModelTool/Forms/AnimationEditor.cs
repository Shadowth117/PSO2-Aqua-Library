using AquaModelLibrary;
using System;
using System.Windows.Forms;

namespace AquaModelTool
{
    public partial class AnimationEditor : UserControl
    {
        AquaMotion motion; 
        public AnimationEditor(AquaMotion aquaMotion)
        {
            InitializeComponent();
            motion = aquaMotion;

            //Populate the anim tree
            for(int i = 0; i < motion.motionKeys.Count; i++)
            {
                TreeNode topNode = new TreeNode(motion.motionKeys[i].mseg.boneName.GetString());
                topNode.Tag = 0;
                animTreeView.Nodes.Add(topNode);

                for(int j = 0; j < motion.motionKeys[i].keyData.Count; j++)
                {
                    if(!motion.keyTypeNames.ContainsKey(motion.motionKeys[i].keyData[j].keyType))
                    {
                        throw new Exception($"Keyframe type {motion.motionKeys[i].keyData[j].keyType} not found!");
                    }
                    TreeNode midNode = new TreeNode(motion.keyTypeNames[motion.motionKeys[i].keyData[j].keyType]);
                    midNode.Tag = 1;
                    animTreeView.Nodes[i].Nodes.Add(midNode);

                    for(int k = 0; k < motion.motionKeys[i].keyData[j].frameTimings.Count; k++)
                    {
                        TreeNode lowNode = new TreeNode("Frame " + (motion.motionKeys[i].keyData[j].frameTimings[k] / 0x10));
                        lowNode.Tag = 2;
                        animTreeView.Nodes[i].Nodes[j].Nodes.Add(lowNode);
                    }
                }
            }

            //Add context menu strip for right clicking
        }

        public void animTreeView_MouseDown(object sender, MouseEventArgs e)
        {
            TreeNode node_here = animTreeView.GetNodeAt(e.X, e.Y);

            //Return if there's nothing selected
            if (node_here == null) return;

            if (e.Button == MouseButtons.Right)
            {
            } else if (e.Button == MouseButtons.Left)
            {

            }

        }

        public void animTreeView_Insert(TreeNode node)
        {
            //If node.Tag is 1, check that the transform categories aren't maxed already and create a popup to ask the user which they'd like to add

            //Since frames must be whole numbers, prevent more frames than the endframe count from being added
            if ((int)node.Tag == 2 && motion.motionKeys[node.Parent.Parent.Index].keyData.Count < motion.moHeader.endFrame)
            {
                MessageBox.Show("You can't add more frames than the frame count!");
                return;
            }

            //Insert the new node
            TreeNode newNode = new TreeNode(node.Text);
            newNode.Tag = node.Tag;
            node.Parent.Nodes.Insert(node.Index, newNode);

            //throw
        }

        public void animTreeView_Duplicate(TreeNode node)
        {
            //Return if this is a transform category node. There should never be multiple of the same one. Ideally, don't show this option for this category.
            if((int)node.Tag == 1)
            {
                MessageBox.Show("You can't duplicate transformation categories!");
                return;
            }

            //Since frames must be whole numbers, prevent more frames than the endframe count from being added
            if((int)node.Tag == 2 && motion.motionKeys[node.Parent.Parent.Index].keyData.Count < motion.moHeader.endFrame)
            {
                MessageBox.Show("You can't add more frames than the frame count!");
                return;
            }
            
            //Recreate and insert the new node
            TreeNode newNode = new TreeNode(node.Text);
            newNode.Tag = node.Tag;
            node.Parent.Nodes.Insert(node.Index, newNode);

            //Set the duplicate data
            switch ((int)node.Tag)
            {
                case 0:
                    AquaMotion.KeyData oldData = motion.motionKeys[node.Index];
                    AquaMotion.KeyData keyData = new AquaMotion.KeyData();
                    keyData.mseg = new AquaMotion.MSEG();
                    keyData.mseg.boneName = oldData.mseg.boneName;
                    keyData.mseg.nodeDataCount = oldData.mseg.nodeDataCount;
                    keyData.mseg.nodeId = oldData.mseg.nodeId;
                    keyData.mseg.nodeType = oldData.mseg.nodeType;
                    
                    //Duplicate keydata
                    for(int i = 0; i < oldData.keyData.Count; i++)
                    {
                        //Duplicate MKEY value data
                        AquaMotion.MKEY oldKey = motion.motionKeys[node.Index].keyData[i];
                        AquaMotion.MKEY newKey = new AquaMotion.MKEY();
                        newKey.dataType = oldKey.dataType;
                        newKey.keyCount = oldKey.keyCount;
                        newKey.keyType = oldKey.keyType;
                        newKey.unkInt0 = oldKey.unkInt0;
                        newKey.floatKeys.AddRange(oldKey.floatKeys);
                        newKey.frameTimings.AddRange(oldKey.frameTimings);
                        newKey.intKeys.AddRange(oldKey.intKeys);
                        newKey.vector4Keys.AddRange(oldKey.vector4Keys);

                        keyData.keyData.Add(newKey);
                    }
                    motion.motionKeys.Insert(node.Index, keyData);
                    motion.moHeader.nodeCount++;
                    break;
                case 2:
                    var keySet = motion.motionKeys[node.Parent.Parent.Index].keyData[node.Parent.Index];
                    keySet.keyCount++;

                    keySet.frameTimings.Insert(node.Index, motion.motionKeys[node.Parent.Parent.Index].keyData[node.Parent.Index].frameTimings[node.Index]);
                    switch (keySet.dataType)
                    {
                        //0x1 and 0x3 are Vector4 arrays essentially. 0x1 is seemingly a Vector3 with alignment padding, but could potentially have things.
                        case 0x1:
                        case 0x3:
                            keySet.vector4Keys.Insert(node.Index, motion.motionKeys[node.Parent.Parent.Index].keyData[node.Parent.Index].vector4Keys[node.Index]);
                            break;

                        //0x4 is texture/uv related, 0x6 is Camera related - Array of floats. 0x4 seems to be used for every .aqv frame set interestingly
                        case 0x4:
                        case 0x6:
                            keySet.floatKeys.Insert(node.Index, motion.motionKeys[node.Parent.Parent.Index].keyData[node.Parent.Index].floatKeys[node.Index]);
                            break;
                        default:
                            throw new Exception("Unexpected data type!");
                    }
                    break;
                default:
                    throw new Exception("Unexpected node tag!");
            }
        }

        public void animTreeView_Delete(TreeNode node)
        {
            //Make sure the user can't delete the first node; the game always needs one.
            if (node.Index == 0)
            {
                MessageBox.Show("You can't delete the first node in a category!");
                return;
            }

            //Make sure the user can't create empty node categories.
            if (node.Parent.Nodes.Count < 2)
            {
                MessageBox.Show("You can't delete the only node in a category!");
                return;
            }

            node.Parent.Nodes.RemoveAt(node.Index);

            //Remove the actual data as well
            switch((int)node.Tag)
            {
                case 0:
                    motion.motionKeys.RemoveAt(node.Index);
                    motion.moHeader.nodeCount--;
                    break;
                case 1:
                    motion.motionKeys[node.Parent.Index].keyData.RemoveAt(node.Index);
                    motion.motionKeys[node.Parent.Index].mseg.nodeDataCount--;
                    break;
                case 2:
                    var keySet = motion.motionKeys[node.Parent.Parent.Index].keyData[node.Parent.Index];
                    keySet.keyCount--;
                    keySet.frameTimings.RemoveAt(node.Index);

                    switch (keySet.dataType)
                    {
                        //0x1 and 0x3 are Vector4 arrays essentially. 0x1 is seemingly a Vector3 with alignment padding, but could potentially have things.
                        case 0x1:
                        case 0x3:
                            keySet.vector4Keys.RemoveAt(node.Index);
                            break;

                        //0x4 is texture/uv related, 0x6 is Camera related - Array of floats. 0x4 seems to be used for every .aqv frame set interestingly
                        case 0x4:
                        case 0x6:
                            keySet.floatKeys.RemoveAt(node.Index);
                            break;
                        default:
                            throw new Exception("Unexpected data type!");
                    }
                    break;
                default:
                    throw new Exception("Unexpected node tag!");
            }
        }

    }
}
