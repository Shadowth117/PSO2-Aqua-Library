using System;
using System.Windows.Forms;

namespace AquaModelTool
{
    public partial class BTIEditor : UserControl
    {
        public AquaModelLibrary.AquaBTI_MotionConfig bti;
        public BTIEditor(AquaModelLibrary.AquaBTI_MotionConfig newBti)
        {
            bti = newBti;

            InitializeComponent();
            PopulateModelDropdown();
        }

        public void PopulateModelDropdown()
        {
            btiIEntryCB.BeginUpdate();
            btiIEntryCB.Items.Clear();
            for (int i = 0; i < bti.btiEntries.Count; i++)
            {
                btiIEntryCB.Items.Add(i);
            }
            btiIEntryCB.EndUpdate();
            btiIEntryCB.SelectedIndex = 0;
            if (btiIEntryCB.Items.Count < 2)
            {
                btiIEntryCB.Enabled = false;
            }
        }


        private void modelIDCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            UdpateEditor();
        }

        private void UdpateEditor()
        {
            //Default to material editor
            modelPanel.Controls.Clear();
            UserControl control;
            control = new BtiEntryEditor();

            modelPanel.Controls.Add(control);
            control.Dock = DockStyle.Fill;
        }

        private void startFrameUD_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
