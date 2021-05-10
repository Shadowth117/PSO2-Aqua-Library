using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace AquaModelTool
{
    public unsafe partial class AqeAnimEditor : UserControl
    {
        AquaModelLibrary.AquaEffect.EFCTObject efct;
        private ColorDialog colorDialog = new ColorDialog();
        TreeNode node;
        public AqeAnimEditor(AquaModelLibrary.AquaEffect.EFCTObject efctObj, TreeNode thisNode)
        {
            InitializeComponent();
            node = thisNode;
            efct = efctObj;
            startFrameUD.Value = (decimal)efct.efct.startFrame;
            endFrameUD.Value = (decimal)efct.efct.endFrame;
        }


        private void startFrameUDValue_Changed(object sender, EventArgs e)
        {
            efct.efct.startFrame = (float)startFrameUD.Value;

            startFrameUD.Value = (decimal)efct.efct.startFrame;
        }

        private void endFrameUD_ValueChanged(object sender, EventArgs e)
        {
            efct.efct.endFrame = (float)endFrameUD.Value;

            endFrameUD.Value = (decimal)efct.efct.endFrame;
        }

    }
}
