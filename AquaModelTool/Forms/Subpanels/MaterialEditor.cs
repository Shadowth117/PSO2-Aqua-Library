using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace AquaModelTool
{
    public partial class MaterialEditor : UserControl
    {
        private ColorDialog colorDialog = new ColorDialog();
        AquaModelLibrary.AquaObject model;
        public MaterialEditor(AquaModelLibrary.AquaObject aquaModel)
        {
            InitializeComponent();
            model = aquaModel;

            //Populate materials dropdown 
            if (model.mateList.Count > 0)
            {
                for (int i = 0; i < model.mateList.Count; i++)
                {
                    matIDCB.Items.Add(i);
                }
                if (model.mateList.Count == 1)
                {
                    matIDCB.Enabled = false;
                }
                matIDCB.SelectedIndex = 0;
                UpdateMaterialDisplay();
            }
        }

        private void UpdateMaterialDisplay()
        {
            var mat = model.mateList[matIDCB.SelectedIndex];
            string blendType = mat.alphaType.GetString();
            switch (blendType)
            {
                case "opaque":
                    opaqueRB.Checked = true;
                    break;
                case "hollow":
                    hollowRB.Checked = true;
                    break;
                case "blendalpha":
                    blendAlphaRB.Checked = true;
                    break;
                case "add":
                    addRB.Checked = true;
                    break;
                default:
                    blendAlphaRB.Checked = true;
                    break;
            }
            matNameTextBox.Text = mat.matName.GetString();
            diffuseRGBButton.BackColor = ARGBFromRGBAVector4(mat.diffuseRGBA);
            tex2RGBAButton.BackColor = ARGBFromRGBAVector4(mat.unkRGBA0);
            tex3RGBAButton.BackColor = ARGBFromRGBAVector4(mat._sRGBA);
            tex4RGBAButton.BackColor = ARGBFromRGBAVector4(mat.unkRGBA1);
            diffuseUD.Value = (decimal)mat.diffuseRGBA.W;
            tex2UD.Value = (decimal)mat.unkRGBA0.W;
            tex3SpecUD.Value = (decimal)mat._sRGBA.W;
            tex4UD.Value = (decimal)mat.unkRGBA1.W;
            specLevelUD.Value = (decimal)mat.unkFloat0;
            unkF32UD.Value = (decimal)mat.unkFloat1;
            unkInt0UD.Value = mat.unkInt0;
            unkInt1UD.Value = mat.unkInt1;
        }

        private void diffuseRGBButton_Click(object sender, EventArgs e)
        {
            var mate = model.mateList[matIDCB.SelectedIndex];
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                diffuseRGBButton.BackColor = colorDialog.Color;
                mate.diffuseRGBA = new Vector4(colorDialog.Color.R / 255, colorDialog.Color.G / 255, colorDialog.Color.B / 255, mate.diffuseRGBA.W);
            }
            model.mateList[matIDCB.SelectedIndex] = mate;
        }

        private void tex2RGBAButton_Click(object sender, EventArgs e)
        {
            var mate = model.mateList[matIDCB.SelectedIndex];
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                tex2RGBAButton.BackColor = colorDialog.Color;
                mate.unkRGBA0 = new Vector4(colorDialog.Color.R / 255, colorDialog.Color.G / 255, colorDialog.Color.B / 255, mate.unkRGBA0.W);
            }
            model.mateList[matIDCB.SelectedIndex] = mate;
        }

        private void tex3RGBAButton_Click(object sender, EventArgs e)
        {
            var mate = model.mateList[matIDCB.SelectedIndex];
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                tex3RGBAButton.BackColor = colorDialog.Color;
                mate._sRGBA = new Vector4(colorDialog.Color.R / 255, colorDialog.Color.G / 255, colorDialog.Color.B / 255, mate._sRGBA.W);
            }
            model.mateList[matIDCB.SelectedIndex] = mate;
        }

        private void tex4RGBAButton_Click(object sender, EventArgs e)
        {
            var mate = model.mateList[matIDCB.SelectedIndex];
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                tex4RGBAButton.BackColor = colorDialog.Color;
                mate.unkRGBA1 = new Vector4(colorDialog.Color.R / 255, colorDialog.Color.G / 255, colorDialog.Color.B / 255, mate.unkRGBA1.W);
            }
            model.mateList[matIDCB.SelectedIndex] = mate;
        }

        private void opaqueRB_CheckedChanged(object sender, EventArgs e)
        {
            var mate = model.mateList[matIDCB.SelectedIndex];
            mate.alphaType.SetString("opaque");
            model.mateList[matIDCB.SelectedIndex] = mate;
        }

        private void blendAlphaRB_CheckedChanged(object sender, EventArgs e)
        {
            RENDhack();
            var mate = model.mateList[matIDCB.SelectedIndex];
            mate.alphaType.SetString("blendalpha");
            model.mateList[matIDCB.SelectedIndex] = mate;
        }

        private void hollowRB_CheckedChanged(object sender, EventArgs e)
        {
            RENDhack();
            var mate = model.mateList[matIDCB.SelectedIndex];
            mate.alphaType.SetString("hollow");
            model.mateList[matIDCB.SelectedIndex] = mate;
        }

        private void addRB_CheckedChanged(object sender, EventArgs e)
        {
            RENDhack();
            var mate = model.mateList[matIDCB.SelectedIndex];
            mate.alphaType.SetString("add");
            model.mateList[matIDCB.SelectedIndex] = mate;
        }

        //Since normally materials are divorced from REND structs in offical models, we'll just fix the necessary REND areas for all that are used with this MATE 
        private void RENDhack()
        {
            List<int> rendIds = new List<int>();
            for(int i = 0; i < model.meshList.Count; i++)
            {
                if(model.meshList[i].mateIndex == matIDCB.SelectedIndex)
                {
                    if(!rendIds.Contains(model.meshList[i].rendIndex))
                    {
                        rendIds.Add(model.meshList[i].rendIndex);
                    }
                }
            }

            for(int i = 0; i < rendIds.Count; i++)
            {
                var rend = model.rendList[rendIds[i]];
                rend.notOpaque = 1;
                rend.unk10 = 0;
                model.rendList[rendIds[i]] = rend;
            }
        }

        private static Color ARGBFromRGBAVector4(Vector4 vec4)
        {
            //Limit input. It can technically be higher than this in theory, but usually it wouldn't be.
            if(vec4.X > 1.0f)
            {
                vec4.X = 1.0f;
            }
            if (vec4.Y > 1.0f)
            {
                vec4.Y = 1.0f;
            }
            if (vec4.Z > 1.0f)
            {
                vec4.Z = 1.0f;
            }
            if (vec4.W > 1.0f)
            {
                vec4.W = 1.0f;
            }
            return Color.FromArgb((int)(vec4.W * 255), (int)(vec4.X * 255), (int)(vec4.Y * 255), (int)(vec4.Z * 255));
        }

        private void matNameTextBox_TextChanged(object sender, EventArgs e)
        {
            var mate = model.mateList[matIDCB.SelectedIndex];
            mate.matName.SetString(matNameTextBox.Text);
            model.mateList[matIDCB.SelectedIndex] = mate;
        }

        private void matIDCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateMaterialDisplay();
        }

        private void diffuseUD_ValueChanged(object sender, EventArgs e)
        {
            var mate = model.mateList[matIDCB.SelectedIndex];
            mate.diffuseRGBA = new Vector4(mate.diffuseRGBA.X, mate.diffuseRGBA.Y, mate.diffuseRGBA.Z, (float)diffuseUD.Value);
            model.mateList[matIDCB.SelectedIndex] = mate;
        }

        private void tex2UD_ValueChanged(object sender, EventArgs e)
        {
            var mate = model.mateList[matIDCB.SelectedIndex];
            mate.unkRGBA0 = new Vector4(mate.unkRGBA0.X, mate.unkRGBA0.Y, mate.unkRGBA0.Z, (float)tex2UD.Value);
            model.mateList[matIDCB.SelectedIndex] = mate;
        }

        private void tex3SpecUD_ValueChanged(object sender, EventArgs e)
        {
            var mate = model.mateList[matIDCB.SelectedIndex];
            mate._sRGBA= new Vector4(mate._sRGBA.X, mate._sRGBA.Y, mate._sRGBA.Z, (float)tex3SpecUD.Value);
            model.mateList[matIDCB.SelectedIndex] = mate;
        }

        private void tex4UD_ValueChanged(object sender, EventArgs e)
        {
            var mate = model.mateList[matIDCB.SelectedIndex];
            mate.unkRGBA1 = new Vector4(mate.unkRGBA1.X, mate.unkRGBA1.Y, mate.unkRGBA1.Z, (float)tex4UD.Value);
            model.mateList[matIDCB.SelectedIndex] = mate;
        }

        private void specLevelUD_ValueChanged(object sender, EventArgs e)
        {
            var mate = model.mateList[matIDCB.SelectedIndex];
            mate.unkFloat0 = (float)specLevelUD.Value;
            model.mateList[matIDCB.SelectedIndex] = mate;
        }

        private void unkInt0UD_ValueChanged(object sender, EventArgs e)
        {
            var mate = model.mateList[matIDCB.SelectedIndex];
            mate.unkInt0 = (int)unkInt0UD.Value;
            model.mateList[matIDCB.SelectedIndex] = mate;
        }

        private void unkF32UD_ValueChanged(object sender, EventArgs e)
        {
            var mate = model.mateList[matIDCB.SelectedIndex];
            mate.unkFloat1 = (float)unkF32UD.Value;
            model.mateList[matIDCB.SelectedIndex] = mate;
        }

        private void unkInt1UD_ValueChanged(object sender, EventArgs e)
        {
            var mate = model.mateList[matIDCB.SelectedIndex];
            mate.unkInt1 = (int)unkInt1UD.Value;
            model.mateList[matIDCB.SelectedIndex] = mate;
        }

    }
}
