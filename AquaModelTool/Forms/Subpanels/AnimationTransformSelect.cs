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
    public partial class AnimationTransformSelect : Form
    {
        public int currentChoice = 0x1;
        public List<RadioButton> transformButtons;
        public AnimationTransformSelect()
        {
            InitializeComponent();
            transformButtons = new List<RadioButton>()
            {
                posButton,
                rotButton,
                scaleButton,
                unk4Button,
                null,         //0x5
                null,         //0x6
                null,         //0x7
                null,         //0x8
                null,         //0x9
                null,         //0xA
                null,         //0xB
                null,         //0xC
                null,         //0xD
                null,         //0xE
                null,         //0xF
                null,         //0x10
                null,         //0x11
                null,         //0x12
                null,         //0x13
                unk14Button,
                unk15Button,
                unk16Button,
                unk17Button,
                unk18Button,
                unk19Button,
                unk1AButton,
                unk1BButton,
                unk1CButton,
                unk1DButton
            };
        }

        public void standardAnim()
        {
            currentChoice = 0x1;
            posButton.Checked = true;
            posButton.Enabled = true;
            rotButton.Enabled = true;
            scaleButton.Enabled = true;
            unk4Button.Enabled = false;
            unk14Button.Enabled = false;
            unk15Button.Enabled = false;
            unk16Button.Enabled = false;
            unk17Button.Enabled = false;
            unk18Button.Enabled = false;
            unk19Button.Enabled = false;
            unk1AButton.Enabled = false;
            unk1BButton.Enabled = false;
            unk1CButton.Enabled = false;
            unk1DButton.Enabled = false;
        }

        public void cameraAnim()
        {
            currentChoice = 0x1;
            posButton.Checked = true;
            posButton.Enabled = true;
            rotButton.Enabled = false;
            scaleButton.Enabled = false;
            unk4Button.Enabled = true;
            unk14Button.Enabled = true;
            unk15Button.Enabled = true;
            unk16Button.Enabled = false;
            unk17Button.Enabled = false;
            unk18Button.Enabled = false;
            unk19Button.Enabled = false;
            unk1AButton.Enabled = false;
            unk1BButton.Enabled = false;
            unk1CButton.Enabled = false;
            unk1DButton.Enabled = false;
        }

        public void uvAnim()
        {
            currentChoice = 0x16;
            posButton.Checked = false;
            posButton.Enabled = false;
            rotButton.Enabled = false;
            scaleButton.Enabled = false;
            unk4Button.Enabled = false;
            unk14Button.Enabled = false;
            unk15Button.Enabled = false;
            unk16Button.Enabled = true;
            unk17Button.Enabled = true;
            unk18Button.Enabled = true;
            unk19Button.Enabled = true;
            unk1AButton.Enabled = true;
            unk1BButton.Enabled = true;
            unk1CButton.Enabled = true;
            unk1DButton.Enabled = true;
        }

        private void posButton_CheckedChanged(object sender, EventArgs e)
        {
            currentChoice = 0x1;
        }

        private void rotButton_CheckedChanged(object sender, EventArgs e)
        {
            currentChoice = 0x2;
        }

        private void scaleButton_CheckedChanged(object sender, EventArgs e)
        {
            currentChoice = 0x3;
        }

        private void unk4Button_CheckedChanged(object sender, EventArgs e)
        {
            currentChoice = 0x4;
        }

        private void unk14Button_CheckedChanged(object sender, EventArgs e)
        {
            currentChoice = 0x14;
        }

        private void unk15Button_CheckedChanged(object sender, EventArgs e)
        {
            currentChoice = 0x15;
        }

        private void unk16Button_CheckedChanged(object sender, EventArgs e)
        {
            currentChoice = 0x16;
        }

        private void unk17Button_CheckedChanged(object sender, EventArgs e)
        {
            currentChoice = 0x17;
        }

        private void unk18Button_CheckedChanged(object sender, EventArgs e)
        {
            currentChoice = 0x18;
        }

        private void unk19Button_CheckedChanged(object sender, EventArgs e)
        {
            currentChoice = 0x19;
        }

        private void unk1AButton_CheckedChanged(object sender, EventArgs e)
        {
            currentChoice = 0x1A;
        }

        private void unk1BButton_CheckedChanged(object sender, EventArgs e)
        {
            currentChoice = 0x1B;
        }

        private void unk1CButton_CheckedChanged(object sender, EventArgs e)
        {
            currentChoice = 0x1C;
        }

        private void unk1DButton_CheckedChanged(object sender, EventArgs e)
        {
            currentChoice = 0x1D;
        }
    }
}
