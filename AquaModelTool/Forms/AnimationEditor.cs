using AquaModelLibrary;
using System;
using System.Numerics;
using System.Windows.Forms;

namespace AquaModelTool
{
    public partial class AnimationEditor : UserControl
    {
        private AquaModelLibrary.AquaUtil.AnimSet animSet;
        private AquaMotion currentMotion;
        public AnimationEditor(AquaModelLibrary.AquaUtil.AnimSet aquaAnimSet)
        {
            InitializeComponent();
            animSet = aquaAnimSet;

            //Populate anims dropdown 
            animIDCB.BeginUpdate();
            for (int i = 0; i < animSet.anims.Count; i++)
            {
                animIDCB.Items.Add(i);
            }
            animIDCB.EndUpdate();
            animIDCB.SelectedIndex = 0;
            currentMotion = animSet.anims[animIDCB.SelectedIndex];

            InitializeAnimTreeView();

            //Add context menu strip for right clicking
        }

        private void InitializeAnimTreeView()
        {
            //Populate the anim tree
            animTreeView.Nodes.Clear();
            for (int i = 0; i < currentMotion.motionKeys.Count; i++)
            {
                TreeNode topNode = new TreeNode(currentMotion.motionKeys[i].mseg.nodeName.GetString());
                topNode.Tag = 0;
                animTreeView.Nodes.Add(topNode);

                for (int j = 0; j < currentMotion.motionKeys[i].keyData.Count; j++)
                {
                    if (!currentMotion.keyTypeNames.ContainsKey(currentMotion.motionKeys[i].keyData[j].keyType))
                    {
                        throw new Exception($"Keyframe type {currentMotion.motionKeys[i].keyData[j].keyType.ToString("X")} not found!");
                    }
                    TreeNode midNode = new TreeNode(currentMotion.keyTypeNames[currentMotion.motionKeys[i].keyData[j].keyType]);
                    midNode.Tag = 1;
                    animTreeView.Nodes[i].Nodes.Add(midNode);

                    //Account for VTBF motions not having to have frametimings if there's only one frame
                    if (currentMotion.motionKeys[i].keyData[j].frameTimings.Count > 0)
                    {
                        for (int k = 0; k < currentMotion.motionKeys[i].keyData[j].frameTimings.Count; k++)
                        {
                            TreeNode lowNode = new TreeNode("Frame " + (currentMotion.motionKeys[i].keyData[j].frameTimings[k] / 0x10));
                            lowNode.Tag = 2;
                            animTreeView.Nodes[i].Nodes[j].Nodes.Add(lowNode);
                        }
                    }
                    else
                    {
                        TreeNode lowNode = new TreeNode("Frame 0");
                        lowNode.Tag = 2;
                        animTreeView.Nodes[i].Nodes[j].Nodes.Add(lowNode);
                    }
                }
            }
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
            //Camera motions should only ever have one node
            if ((int)node.Tag == 0 && currentMotion.moHeader.variant == AquaMotion.cameraAnim)
            {
                MessageBox.Show("You cannot have multiple camera nodes!");
                return;
            }

            //If node.Tag is 1, check that the transform categories aren't maxed already
            if((int)node.Tag == 1)
            {
                switch(currentMotion.moHeader.variant)
                {
                    case AquaMotion.cameraAnim:
                    case AquaMotion.stdAnim:
                    case AquaMotion.stdPlayerAnim:
                        if(node.Parent.Nodes.Count >= 4)
                        {
                            MessageBox.Show("All allowed transform types have already been used!");
                            return;
                        }
                        break;
                    case AquaMotion.materialAnim:
                        if (node.Parent.Nodes.Count >= 8)
                        {
                            MessageBox.Show("All allowed transform types have already been used!");
                            return;
                        }
                        break;
                    default:
                        MessageBox.Show($"Unknown motion variant {currentMotion.moHeader.variant}");
                        return;
                }
            }

            //Since frames must be whole numbers, prevent more frames than the endframe count from being added
            if ((int)node.Tag == 2 && currentMotion.motionKeys[node.Parent.Parent.Index].keyData.Count < currentMotion.moHeader.endFrame)
            {
                MessageBox.Show("You can't add more frames than the frame count!");
                return;
            }

            //Insert the new node
            TreeNode newNode = new TreeNode();
            newNode.Tag = node.Tag;
            node.Parent.Nodes.Insert(node.Index, newNode);
            switch ((int)node.Tag)
            {
                case 0:
                    newNode.Text = node.Text;

                    //Create the lower nodes
                    TreeNode transformNode = new TreeNode();
                    transformNode.Tag = 1;

                    SetupNewTransformNode(node, transformNode);

                    newNode.Nodes.Add(transformNode);
                    break;
                case 1:
                    newNode.Tag = 1;
                    SetupNewTransformNode(node.Parent, newNode);
                    break;
                case 2:
                    newNode.Text = node.Text;
                    var keySet = currentMotion.motionKeys[node.Parent.Parent.Index].keyData[node.Parent.Index];
                    keySet.keyCount++;

                    keySet.frameTimings.Insert(node.Index, currentMotion.motionKeys[node.Parent.Parent.Index].keyData[node.Parent.Index].frameTimings[node.Index]);
                    switch (keySet.dataType)
                    {
                        //0x1 and 0x3 are Vector4 arrays essentially. 0x1 is seemingly a Vector3 with alignment padding, but could potentially have things.
                        case 0x1:
                        case 0x3:
                            keySet.vector4Keys.Insert(node.Index, new Vector4(0, 0, 0, 0));
                            break;

                        //0x4 is texture/uv related, 0x6 is Camera related - Array of floats. 0x4 seems to be used for every .aqv frame set interestingly
                        case 0x4:
                        case 0x6:
                            keySet.floatKeys.Insert(node.Index, 0);
                            break;
                        default:
                            throw new Exception("Unexpected data type!");
                    }
                    break;
            }




            //throw
        }

        private void SetupNewTransformNode(TreeNode node, TreeNode transformNode)
        {
            //Create MSEG and Keyset
            AquaMotion.KeyData keyData = new AquaMotion.KeyData();
            AquaMotion.MSEG newMseg = new AquaMotion.MSEG();
            newMseg.nodeDataCount = 1;
            newMseg.nodeId = currentMotion.motionKeys[node.Index].mseg.nodeId + 1;
            newMseg.nodeName.SetString("Node " + newMseg.nodeId);

            //Set up the dialog
            AnimationTransformSelect tfmDialog = new AnimationTransformSelect(currentMotion.moHeader.variant);
            switch (currentMotion.moHeader.variant)
            {
                case AquaMotion.stdAnim:
                case AquaMotion.stdPlayerAnim:
                    newMseg.nodeType = 0x2;
                    break;
                case AquaMotion.cameraAnim:
                    newMseg.nodeType = 0x20;
                    break;
                case AquaMotion.materialAnim:
                    newMseg.nodeType = 0x10;
                    break;
                default:
                    MessageBox.Show("Unknown animation type. Please report!");
                    break;
            }

            //Find preused transform types and disallow them
            for (int i = 0; i < currentMotion.motionKeys[node.Index].keyData.Count; i++)
            {
                int kType = currentMotion.motionKeys[node.Index].keyData[i].keyType;
                tfmDialog.transformButtons[kType - 1].Enabled = false;
            }
            //Set the first enabled type to checked
            for (int i = 0; i < tfmDialog.transformButtons.Count; i++)
            {
                if (tfmDialog.transformButtons[i].Enabled == false)
                {
                    tfmDialog.transformButtons[i].Checked = false;
                }
                else
                {
                    tfmDialog.transformButtons[i].Checked = true;
                    break;
                }
            }

            //Run Dialog and check results
            tfmDialog.ShowDialog();
            int transformType = tfmDialog.currentChoice;
            transformNode.Text = currentMotion.keyTypeNames[transformType];

            //Add an empty frame
            TreeNode keyNode = new TreeNode("Frame 0");
            keyNode.Tag = 2;
            transformNode.Nodes.Add(keyNode);

            //Add MKEY data
            keyData.mseg = newMseg;
            AquaMotion.MKEY newMkey = new AquaMotion.MKEY();
            newMkey.keyType = transformType;
            newMkey.dataType = AquaMotion.GetKeyDataType(transformType);
            newMkey.unkInt0 = 0;
            newMkey.keyCount = 1;
            switch (newMkey.dataType)
            {
                //0x1 and 0x3 are Vector4 arrays essentially. 0x1 is seemingly a Vector3 with alignment padding, but could potentially have things.
                case 0x1:
                case 0x3:
                    newMkey.vector4Keys.Add(new Vector4(0, 0, 0, 0));
                    break;

                //0x4 is texture/uv related, 0x6 is Camera related - Array of floats. 0x4 seems to be used for every .aqv frame set interestingly
                case 0x4:
                case 0x6:
                    newMkey.floatKeys.Add(0);
                    break;
                default:
                    throw new Exception($"Unexpected data type: {newMkey.dataType}");
            }
            keyData.keyData.Add(newMkey);
            currentMotion.motionKeys.Add(keyData);
        }

        public void animTreeView_Duplicate(TreeNode node)
        {
            //Camera motions should only ever have one node
            if ((int)node.Tag == 0 && currentMotion.moHeader.variant == AquaMotion.cameraAnim)
            {
                MessageBox.Show("You cannot have multiple camera nodes!");
                return;
            }

            //Return if this is a transform category node. There should never be multiple of the same one. Ideally, don't show this option for this category.
            if ((int)node.Tag == 1)
            {
                MessageBox.Show("You can't duplicate transformation categories!");
                return;
            }

            //Since frames must be whole numbers, prevent more frames than the endframe count from being added
            if((int)node.Tag == 2 && currentMotion.motionKeys[node.Parent.Parent.Index].keyData.Count < currentMotion.moHeader.endFrame)
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
                    //Duplicate Transform TreeNodes
                    for(int i = 0; i < node.Nodes.Count; i++)
                    {
                        TreeNode tfmNode = new TreeNode(node.Nodes[i].Text);
                        tfmNode.Tag = node.Nodes[i].Tag;

                        //Duplicate Key Treenodes
                        for (int j = 0; j < node.Nodes.Count; j++)
                        {
                            TreeNode keyNode = new TreeNode(node.Nodes[i].Nodes[j].Text);
                            keyNode.Tag = node.Nodes[i].Nodes[j].Tag;
                            tfmNode.Nodes.Add(keyNode);
                        }
                        newNode.Nodes.Add(tfmNode);
                    }

                    //Set actual data
                    AquaMotion.KeyData oldData = currentMotion.motionKeys[node.Index];
                    AquaMotion.KeyData keyData = new AquaMotion.KeyData();
                    keyData.mseg = new AquaMotion.MSEG();
                    keyData.mseg.nodeName = oldData.mseg.nodeName;
                    keyData.mseg.nodeDataCount = oldData.mseg.nodeDataCount;
                    keyData.mseg.nodeId = oldData.mseg.nodeId;
                    keyData.mseg.nodeType = oldData.mseg.nodeType;
                    
                    //Duplicate keydata
                    for(int i = 0; i < oldData.keyData.Count; i++)
                    {
                        //Duplicate MKEY value data
                        AquaMotion.MKEY oldKey = currentMotion.motionKeys[node.Index].keyData[i];
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
                    currentMotion.motionKeys.Insert(node.Index, keyData);
                    currentMotion.moHeader.nodeCount++;
                    break;
                case 2:
                    var keySet = currentMotion.motionKeys[node.Parent.Parent.Index].keyData[node.Parent.Index];
                    keySet.keyCount++;

                    keySet.frameTimings.Insert(node.Index, currentMotion.motionKeys[node.Parent.Parent.Index].keyData[node.Parent.Index].frameTimings[node.Index]);
                    switch (keySet.dataType)
                    {
                        //0x1 and 0x3 are Vector4 arrays essentially. 0x1 is seemingly a Vector3 with alignment padding, but could potentially have things.
                        case 0x1:
                        case 0x3:
                            keySet.vector4Keys.Insert(node.Index, currentMotion.motionKeys[node.Parent.Parent.Index].keyData[node.Parent.Index].vector4Keys[node.Index]);
                            break;

                        //0x4 is texture/uv related, 0x6 is Camera related - Array of floats. 0x4 seems to be used for every .aqv frame set interestingly
                        case 0x4:
                        case 0x6:
                            keySet.floatKeys.Insert(node.Index, currentMotion.motionKeys[node.Parent.Parent.Index].keyData[node.Parent.Index].floatKeys[node.Index]);
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
                    currentMotion.motionKeys.RemoveAt(node.Index);
                    currentMotion.moHeader.nodeCount--;
                    break;
                case 1:
                    currentMotion.motionKeys[node.Parent.Index].keyData.RemoveAt(node.Index);
                    currentMotion.motionKeys[node.Parent.Index].mseg.nodeDataCount--;
                    break;
                case 2:
                    var keySet = currentMotion.motionKeys[node.Parent.Parent.Index].keyData[node.Parent.Index];
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

        private void animIDCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentMotion = animSet.anims[animIDCB.SelectedIndex];

            InitializeAnimTreeView();
        }
    }
}
