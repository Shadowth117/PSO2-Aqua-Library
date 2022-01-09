using AquaModelLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AquaModelTool
{
    public partial class RenderEditor : UserControl
    {
        private AquaObject model;
        private int currentRenderId = 0;
        public RenderEditor(AquaObject newModel)
        {
            model = newModel;
            InitializeComponent();
            //Populate REND dropdown 
            if (model.rendList.Count > 0)
            {
                currentRenderId = 0;
                for (int i = 0; i < model.rendList.Count; i++)
                {
                    rendIDCB.Items.Add(i);
                }
                if (model.rendList.Count == 1)
                {
                    rendIDCB.Enabled = false;
                }
                rendIDCB.SelectedIndex = 0;
                UpdateRENDDisplay();
            }
        }

        public void UpdateRENDDisplay()
        {
            unk0UD.Value = model.rendList[currentRenderId].unk0;
            twoSidedUD.Value = model.rendList[currentRenderId].twosided;
            int0CUD.Value = model.rendList[currentRenderId].int_0C;
            unk1UD.Value = model.rendList[currentRenderId].unk1;

            unk2UD.Value = model.rendList[currentRenderId].unk2;
            unk3UD.Value = model.rendList[currentRenderId].unk3;
            unk4UD.Value = model.rendList[currentRenderId].unk4;
            unk5UD.Value = model.rendList[currentRenderId].unk5;

            unk6UD.Value = model.rendList[currentRenderId].unk6;
            unk7UD.Value = model.rendList[currentRenderId].unk7;
            unk8UD.Value = model.rendList[currentRenderId].unk8;
            unk9UD.Value = model.rendList[currentRenderId].unk9;

            alphaCutoffUD.Value = model.rendList[currentRenderId].alphaCutoff;
            unk11UD.Value = model.rendList[currentRenderId].unk11;
            unk12UD.Value = model.rendList[currentRenderId].unk12;
            unk13UD.Value = model.rendList[currentRenderId].unk13;

        }

        private void rendIDCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentRenderId = rendIDCB.SelectedIndex;
            UpdateRENDDisplay();
        }

        private void unk0UD_ValueChanged(object sender, EventArgs e)
        {
            AquaObject.REND ngsRend = model.rendList[currentRenderId];
            ngsRend.unk0 = (int)unk0UD.Value;
            model.rendList[currentRenderId] = ngsRend;
        }

        private void twoSidedUD_ValueChanged(object sender, EventArgs e)
        {
            AquaObject.REND ngsRend = model.rendList[currentRenderId];
            ngsRend.twosided = (int)twoSidedUD.Value;
            model.rendList[currentRenderId] = ngsRend;
        }

        private void int0CUD_ValueChanged(object sender, EventArgs e)
        {
            AquaObject.REND ngsRend = model.rendList[currentRenderId];
            ngsRend.int_0C = (int)int0CUD.Value;
            model.rendList[currentRenderId] = ngsRend;
        }

        private void unk1UD_ValueChanged(object sender, EventArgs e)
        {
            AquaObject.REND ngsRend = model.rendList[currentRenderId];
            ngsRend.unk1 = (int)unk1UD.Value;
            model.rendList[currentRenderId] = ngsRend;
        }

        private void unk2UD_ValueChanged(object sender, EventArgs e)
        {
            AquaObject.REND ngsRend = model.rendList[currentRenderId];
            ngsRend.unk2 = (int)unk2UD.Value;
            model.rendList[currentRenderId] = ngsRend;
        }

        private void unk3UD_ValueChanged(object sender, EventArgs e)
        {
            AquaObject.REND ngsRend = model.rendList[currentRenderId];
            ngsRend.unk3 = (int)unk3UD.Value;
            model.rendList[currentRenderId] = ngsRend;
        }

        private void unk4UD_ValueChanged(object sender, EventArgs e)
        {
            AquaObject.REND ngsRend = model.rendList[currentRenderId];
            ngsRend.unk4 = (int)unk4UD.Value;
            model.rendList[currentRenderId] = ngsRend;
        }

        private void unk5UD_ValueChanged(object sender, EventArgs e)
        {
            AquaObject.REND ngsRend = model.rendList[currentRenderId];
            ngsRend.unk5 = (int)unk5UD.Value;
            model.rendList[currentRenderId] = ngsRend;
        }

        private void unk6UD_ValueChanged(object sender, EventArgs e)
        {
            AquaObject.REND ngsRend = model.rendList[currentRenderId];
            ngsRend.unk6 = (int)unk6UD.Value;
            model.rendList[currentRenderId] = ngsRend;
        }

        private void unk7UD_ValueChanged(object sender, EventArgs e)
        {
            AquaObject.REND ngsRend = model.rendList[currentRenderId];
            ngsRend.unk7 = (int)unk7UD.Value;
            model.rendList[currentRenderId] = ngsRend;
        }

        private void unk8UD_ValueChanged(object sender, EventArgs e)
        {
            AquaObject.REND ngsRend = model.rendList[currentRenderId];
            ngsRend.unk8 = (int)unk8UD.Value;
            model.rendList[currentRenderId] = ngsRend;
        }

        private void unk9UD_ValueChanged(object sender, EventArgs e)
        {
            AquaObject.REND ngsRend = model.rendList[currentRenderId];
            ngsRend.unk9 = (int)unk9UD.Value;
            model.rendList[currentRenderId] = ngsRend;
        }

        private void alphaCutoffUD_ValueChanged(object sender, EventArgs e)
        {
            AquaObject.REND ngsRend = model.rendList[currentRenderId];
            ngsRend.alphaCutoff = (int)alphaCutoffUD.Value;
            model.rendList[currentRenderId] = ngsRend;
        }

        private void unk11UD_ValueChanged(object sender, EventArgs e)
        {

            AquaObject.REND ngsRend = model.rendList[currentRenderId];
            ngsRend.unk11 = (int)unk11UD.Value;
            model.rendList[currentRenderId] = ngsRend;
        }

        private void unk12UD_ValueChanged(object sender, EventArgs e)
        {
            AquaObject.REND ngsRend = model.rendList[currentRenderId];
            ngsRend.unk12 = (int)unk12UD.Value;
            model.rendList[currentRenderId] = ngsRend;
        }

        private void unk13UD_ValueChanged(object sender, EventArgs e)
        {
            AquaObject.REND ngsRend = model.rendList[currentRenderId];
            ngsRend.unk13 = (int)unk13UD.Value;
            model.rendList[currentRenderId] = ngsRend;
        }
    }
}
