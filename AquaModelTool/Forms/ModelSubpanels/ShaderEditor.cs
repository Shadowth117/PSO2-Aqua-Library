using AquaModelLibrary;
using System;
using System.Windows.Forms;

namespace AquaModelTool
{
    public partial class ShaderEditor : UserControl
    {
        private AquaObject model;
        private int currentShaderId = 0;
        private int currentExtraId = 0;
        private NGSAquaObject.NGSSHAD ngsShad = null;
        public ShaderEditor(AquaObject aquaModel)
        {
            model = aquaModel;
            InitializeComponent();
            SetExtraState(false);
            //Populate shaders dropdown 
            if (model.shadList.Count > 0)
            {
                currentShaderId = 0;
                currentExtraId = 0;
                for (int i = 0; i < model.shadList.Count; i++)
                {
                    shadIDCB.Items.Add(i);
                }
                if (model.shadList.Count == 1)
                {
                    shadIDCB.Enabled = false;
                }
                shadIDCB.SelectedIndex = 0;
                UpdateShaderDisplay();
            }
        }

        private void SetExtraState(bool state)
        {
            flags0UD.Enabled = state;
            flags1UD.Enabled = state;
            flags2UD.Enabled = state;
            valueXUD.Enabled = state;
            valueYUD.Enabled = state;
            valueZUD.Enabled = state;
            valueWUD.Enabled = state;
        }

        public void ResetExtras()
        {
            SetExtraState(false);
            flags0UD.Value = 0;
            flags1UD.Value = 0;
            flags2UD.Value = 0;
            valueXUD.Value = 0;
            valueYUD.Value = 0;
            valueZUD.Value = 0;
            valueWUD.Value = 0;
        }

        public void UpdateShaderDisplay()
        {
            shaderExtraLB.Items.Clear();
            currentExtraId = 0;

            pixelShaderTB.Text = model.shadList[currentShaderId].pixelShader.GetString();
            vShaderTB.Text = model.shadList[currentShaderId].vertexShader.GetString();
            unk0UD.Value = model.shadList[currentShaderId].unk0;

            if (model.shadList[currentShaderId].GetType().Equals(typeof(NGSAquaObject.NGSSHAD)))
            {
                ngsShad = (NGSAquaObject.NGSSHAD)model.shadList[currentShaderId];

                //Populate listbox
                for (int i = 0; i < ngsShad.shadExtra.Count; i++)
                {
                    shaderExtraLB.Items.Add(ngsShad.shadExtra[i].entryString.GetString());
                }

                UpdateShaderExtraDisplay();
            }
            else
            {
                ngsShad = null;
                ResetExtras();
            }
        }

        public void UpdateShaderExtraDisplay()
        {
            if (ngsShad.shadExtra.Count > 0)
            {
                SetExtraState(true);

                flags0UD.Value = ngsShad.shadExtra[currentExtraId].entryFlag0;
                flags1UD.Value = ngsShad.shadExtra[currentExtraId].entryFlag1;
                flags2UD.Value = ngsShad.shadExtra[currentExtraId].entryFlag2;
                valueXUD.Value = (decimal)ngsShad.shadExtra[currentExtraId].entryFloats.X;
                valueYUD.Value = (decimal)ngsShad.shadExtra[currentExtraId].entryFloats.Y;
                valueZUD.Value = (decimal)ngsShad.shadExtra[currentExtraId].entryFloats.Z;
                valueWUD.Value = (decimal)ngsShad.shadExtra[currentExtraId].entryFloats.W;
            }
        }

        private void shadIDCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentShaderId = shadIDCB.SelectedIndex;
            UpdateShaderDisplay();
        }

        private void pixelShaderTB_TextChanged(object sender, EventArgs e)
        {
            model.shadList[currentShaderId].pixelShader.SetString(pixelShaderTB.Text);
        }

        private void vShaderTB_TextChanged(object sender, EventArgs e)
        {
            model.shadList[currentShaderId].vertexShader.SetString(vShaderTB.Text);
        }

        private void unk0UD_ValueChanged(object sender, EventArgs e)
        {
            model.shadList[currentShaderId].unk0 = (int)unk0UD.Value;
        }

        private void shaderExtraLB_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentExtraId = shaderExtraLB.SelectedIndex;
            if (currentExtraId >= 0 && ngsShad != null)
            {
                UpdateShaderExtraDisplay();
            }
            else
            {
                ResetExtras();
            }
        }

        private void flags0UD_ValueChanged(object sender, EventArgs e)
        {
            if (ngsShad != null && shaderExtraLB.SelectedIndex != -1)
            {
                var extra = ngsShad.shadExtra[currentExtraId];
                extra.entryFlag0 = (short)flags0UD.Value;
                ngsShad.shadExtra[currentExtraId] = extra;
            }
        }

        private void flags1UD_ValueChanged(object sender, EventArgs e)
        {
            if (ngsShad != null && shaderExtraLB.SelectedIndex != -1)
            {
                var extra = ngsShad.shadExtra[currentExtraId];
                extra.entryFlag1 = (short)flags1UD.Value;
                ngsShad.shadExtra[currentExtraId] = extra;
            }
        }

        private void flags2UD_ValueChanged(object sender, EventArgs e)
        {
            if (ngsShad != null && shaderExtraLB.SelectedIndex != -1)
            {
                var extra = ngsShad.shadExtra[currentExtraId];
                extra.entryFlag2 = (short)flags2UD.Value;
                ngsShad.shadExtra[currentExtraId] = extra;
            }
        }

        private void valueXUD_ValueChanged(object sender, EventArgs e)
        {
            if (ngsShad != null && shaderExtraLB.SelectedIndex != -1)
            {
                var extra = ngsShad.shadExtra[currentExtraId];
                var vec4 = extra.entryFloats;
                vec4.X = (float)valueXUD.Value;
                extra.entryFloats = vec4;
                ngsShad.shadExtra[currentExtraId] = extra;
            }
        }

        private void valueYUD_ValueChanged(object sender, EventArgs e)
        {
            if (ngsShad != null && shaderExtraLB.SelectedIndex != -1)
            {
                var extra = ngsShad.shadExtra[currentExtraId];
                var vec4 = extra.entryFloats;
                vec4.Y = (float)valueYUD.Value;
                extra.entryFloats = vec4;
                ngsShad.shadExtra[currentExtraId] = extra;
            }
        }

        private void valueZUD_ValueChanged(object sender, EventArgs e)
        {
            if (ngsShad != null && shaderExtraLB.SelectedIndex != -1)
            {
                var extra = ngsShad.shadExtra[currentExtraId];
                var vec4 = extra.entryFloats;
                vec4.Z = (float)valueZUD.Value;
                extra.entryFloats = vec4;
                ngsShad.shadExtra[currentExtraId] = extra;
            }
        }

        private void valueWUD_ValueChanged(object sender, EventArgs e)
        {
            if (ngsShad != null && shaderExtraLB.SelectedIndex != -1)
            {
                var extra = ngsShad.shadExtra[currentExtraId];
                var vec4 = extra.entryFloats;
                vec4.W = (float)valueWUD.Value;
                extra.entryFloats = vec4;
                ngsShad.shadExtra[currentExtraId] = extra;
            }
        }
    }
}
