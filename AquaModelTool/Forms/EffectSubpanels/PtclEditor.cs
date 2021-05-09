using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace AquaModelTool
{
    public unsafe partial class PtclEditor : UserControl
    {
        AquaModelLibrary.AquaEffect.PTCLObject ptcl;
        private ColorDialog colorDialog = new ColorDialog();
        TreeNode node;
        public PtclEditor(AquaModelLibrary.AquaEffect.PTCLObject ptclObj, TreeNode thisNode)
        {
            InitializeComponent();
            node = thisNode;
            ptcl = ptclObj;
            startFrameUD.Value = (decimal)ptcl.ptcl.startFrame;
            endFrameUD.Value = (decimal)ptcl.ptcl.endFrame;
            modelNameBox.Text = ptclObj.ptcl.soundName.GetString();
            diffuseRGBButton.BackColor = Color.FromArgb(ptcl.ptcl.color[3], ptcl.ptcl.color[2], ptcl.ptcl.color[1], ptcl.ptcl.color[0]);
            diffuseUD.Value = ptcl.ptcl.color[3];

            sizeXUD.Value = (decimal)ptcl.ptcl.unkVec3_0.X;
            sizeYUD.Value = (decimal)ptcl.ptcl.unkVec3_0.Y;
            sizeZUD.Value = (decimal)ptcl.ptcl.unkVec3_0.Z;

            sizeRandomXUD.Value = (decimal)ptcl.ptcl.unkVec3_1.X;
            sizeRandomYUD.Value = (decimal)ptcl.ptcl.unkVec3_1.Y;
            sizeRandomZUD.Value = (decimal)ptcl.ptcl.unkVec3_1.Z;

            rotXUD.Value = (decimal)ptcl.ptcl.unkVec3_2.X;
            rotYUD.Value = (decimal)ptcl.ptcl.unkVec3_2.Y;
            rotZUD.Value = (decimal)ptcl.ptcl.unkVec3_2.Z;

            float30UD.Value = (decimal)ptcl.ptcl.float_30;
            int48UD.Value = ptcl.ptcl.int_48;
            int50UD.Value = ptcl.ptcl.int_50;
            boolInt54UD.Value = ptcl.ptcl.boolInt_54;
            boolInt58UD.Value = ptcl.ptcl.boolInt_58;
            boolInt5CUD.Value = ptcl.ptcl.boolInt_5C;
            float60UD.Value = (decimal)ptcl.ptcl.float_60;
            float64UD.Value = (decimal)ptcl.ptcl.float_64;
        }


        private void startFrameUDValue_Changed(object sender, EventArgs e)
        {
            ptcl.ptcl.startFrame = (float)startFrameUD.Value;

            startFrameUD.Value = (decimal)ptcl.ptcl.startFrame;
        }

        private void endFrameUD_ValueChanged(object sender, EventArgs e)
        {
            ptcl.ptcl.endFrame = (float)endFrameUD.Value;

            endFrameUD.Value = (decimal)ptcl.ptcl.endFrame;
        }

        private void diffuseRGBButton_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                diffuseRGBButton.BackColor = colorDialog.Color;
                ptcl.ptcl.color[0] = colorDialog.Color.B;
                ptcl.ptcl.color[1] = colorDialog.Color.G;
                ptcl.ptcl.color[2] = colorDialog.Color.R;
            }
        }

        private void diffuseUD_ValueChanged(object sender, EventArgs e)
        {
            ptcl.ptcl.color[3] = (byte)diffuseUD.Value;

            diffuseUD.Value = ptcl.ptcl.color[3];
        }

        private void soundNameBox_TextChanged(object sender, EventArgs e)
        {
            ptcl.ptcl.soundName.SetString(modelNameBox.Text);
        }

        private void posXUDValue_Changed(object sender, EventArgs e)
        {
            var vec3 = ptcl.ptcl.unkVec3_0;
            vec3.X = (float)sizeXUD.Value;
            ptcl.ptcl.unkVec3_0 = vec3;
            
            //Set from the casted number so it's clear to the user what the new value will be. Decimals don't match to floats well
            sizeXUD.Value = (decimal)ptcl.ptcl.unkVec3_0.X;
        }

        private void posYUDValue_Changed(object sender, EventArgs e)
        {
            var vec3 = ptcl.ptcl.unkVec3_0;
            vec3.Y = (float)sizeYUD.Value;
            ptcl.ptcl.unkVec3_0 = vec3;

            //Set from the casted number so it's clear to the user what the new value will be. Decimals don't match to floats well
            sizeYUD.Value = (decimal)ptcl.ptcl.unkVec3_0.Y;
        }

        private void posZUDValue_Changed(object sender, EventArgs e)
        {
            var vec3 = ptcl.ptcl.unkVec3_0;
            vec3.Z = (float)sizeZUD.Value;
            ptcl.ptcl.unkVec3_0 = vec3;

            //Set from the casted number so it's clear to the user what the new value will be. Decimals don't match to floats well
            sizeZUD.Value = (decimal)ptcl.ptcl.unkVec3_0.Z;
        }

        private void rotXUD_ValueChanged(object sender, EventArgs e)
        {
            var vec3 = ptcl.ptcl.unkVec3_1;
            vec3.X = (float)sizeRandomXUD.Value;
            ptcl.ptcl.unkVec3_1 = vec3;

            //Set from the casted number so it's clear to the user what the new value will be. Decimals don't match to floats well
            sizeRandomXUD.Value = (decimal)ptcl.ptcl.unkVec3_1.X;
        }

        private void rotYUD_ValueChanged(object sender, EventArgs e)
        {
            var vec3 = ptcl.ptcl.unkVec3_1;
            vec3.Y = (float)sizeRandomYUD.Value;
            ptcl.ptcl.unkVec3_1 = vec3;

            //Set from the casted number so it's clear to the user what the new value will be. Decimals don't match to floats well
            sizeRandomYUD.Value = (decimal)ptcl.ptcl.unkVec3_1.Y;
        }

        private void rotZUD_ValueChanged(object sender, EventArgs e)
        {
            var vec3 = ptcl.ptcl.unkVec3_1;
            vec3.Z = (float)sizeRandomZUD.Value;
            ptcl.ptcl.unkVec3_1 = vec3;

            //Set from the casted number so it's clear to the user what the new value will be. Decimals don't match to floats well
            sizeRandomZUD.Value = (decimal)ptcl.ptcl.unkVec3_1.Z;
        }

        private void scaleXUD_ValueChanged(object sender, EventArgs e)
        {
            var vec3 = ptcl.ptcl.unkVec3_2;
            vec3.X = (float)rotXUD.Value;
            ptcl.ptcl.unkVec3_2 = vec3;

            //Set from the casted number so it's clear to the user what the new value will be. Decimals don't match to floats well
            rotXUD.Value = (decimal)ptcl.ptcl.unkVec3_2.X;
        }

        private void scaleYUD_ValueChanged(object sender, EventArgs e)
        {
            var vec3 = ptcl.ptcl.unkVec3_2;
            vec3.Y = (float)rotYUD.Value;
            ptcl.ptcl.unkVec3_2 = vec3;

            //Set from the casted number so it's clear to the user what the new value will be. Decimals don't match to floats well
            rotYUD.Value = (decimal)ptcl.ptcl.unkVec3_2.Y;
        }

        private void scaleZUD_ValueChanged(object sender, EventArgs e)
        {
            var vec3 = ptcl.ptcl.unkVec3_2;
            vec3.Z = (float)rotZUD.Value;
            ptcl.ptcl.unkVec3_2 = vec3;

            //Set from the casted number so it's clear to the user what the new value will be. Decimals don't match to floats well
            rotZUD.Value = (decimal)ptcl.ptcl.unkVec3_2.Z;
        }

        private void float30UD_ValueChanged(object sender, EventArgs e)
        {
            ptcl.ptcl.float_30 = (float)float30UD.Value;

            float30UD.Value = (decimal)ptcl.ptcl.float_30;
        }

        private void int48UD_ValueChanged(object sender, EventArgs e)
        {
            ptcl.ptcl.int_48 = (int)int48UD.Value;

            int48UD.Value = ptcl.ptcl.int_48;
        }

        private void int50UD_ValueChanged(object sender, EventArgs e)
        {
            ptcl.ptcl.int_50 = (int)int50UD.Value;

            int50UD.Value = ptcl.ptcl.int_50;
        }

        private void boolInt54UD_ValueChanged(object sender, EventArgs e)
        {
            ptcl.ptcl.boolInt_54 = (int)boolInt54UD.Value;

            boolInt54UD.Value = ptcl.ptcl.boolInt_54;
        }

        private void boolInt58UD_ValueChanged(object sender, EventArgs e)
        {
            ptcl.ptcl.boolInt_58 = (int)boolInt58UD.Value;

            boolInt58UD.Value = ptcl.ptcl.boolInt_58;
        }

        private void boolInt5CUD_ValueChanged(object sender, EventArgs e)
        {
            ptcl.ptcl.boolInt_5C = (int)boolInt5CUD.Value;

            boolInt5CUD.Value = ptcl.ptcl.boolInt_5C;
        }

        private void float60UD_ValueChanged(object sender, EventArgs e)
        {
            ptcl.ptcl.float_60 = (float)float60UD.Value;

            float60UD.Value = (decimal)ptcl.ptcl.float_60;
        }

        private void float64UD_ValueChanged(object sender, EventArgs e)
        {
            ptcl.ptcl.float_64 = (float)float64UD.Value;

            float64UD.Value = (decimal)ptcl.ptcl.float_64;
        }
    }
}
