﻿using System;
using System.Windows.Forms;

namespace AquaModelTool
{
    public partial class ModelEditor : UserControl
    {
        public AquaModelLibrary.AquaUtil.ModelSet modelset;
        public ModelEditor(AquaModelLibrary.AquaUtil.ModelSet aquaModelset)
        {
            modelset = aquaModelset;

            /*
            if(modelset.models[0].objc.type == 0xC33 || modelset.models[0].objc.type == 0xC32)
            {
                MessageBox.Show("NGS era models are not properly supported. Saving will probably give unexpected results.");
            }*/

            InitializeComponent();
            PopulateModelDropdown();
            SetDropdown();
        }

        public void PopulateModelDropdown()
        {
            modelIDCB.BeginUpdate();
            modelIDCB.Items.Clear();
            for (int i = 0; i < modelset.models.Count; i++)
            {
                modelIDCB.Items.Add(i);
            }
            modelIDCB.EndUpdate();
            modelIDCB.SelectedIndex = 0;
            if (modelIDCB.Items.Count < 2)
            {
                modelIDCB.Enabled = false;
            }
        }

        public bool GetAllTransparentChecked()
        {
            return allAlphaCheckBox.Checked;
        }

        private void modelIDCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetDropdown();
            UdpateEditor();
        }

        public void SetDropdown()
        {
            editorCB.Items.Clear();
            if(modelset.models[modelIDCB.SelectedIndex].mateList.Count > 0)
            {
                editorCB.Items.Add("Materials");
            }
            if(modelset.models[modelIDCB.SelectedIndex].shadList.Count > 0)
            {
                editorCB.Items.Add("Shaders");
            }
            editorCB.SelectedIndex = 0;
        }

        private void editorCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            UdpateEditor();
        }

        private void UdpateEditor()
        {
            //Default to material editor
            modelPanel.Controls.Clear();
            UserControl control;

            switch (editorCB.Items[editorCB.SelectedIndex].ToString())
            {
                case "Materials":
                    control = new MaterialEditor(modelset.models[modelIDCB.SelectedIndex]);
                    break;
                case "Shaders":
                    control = new ShaderEditor(modelset.models[modelIDCB.SelectedIndex]);
                    break;
                default:
                    throw new Exception("Unexpected selection!");
                    break;
            }

            modelPanel.Controls.Add(control);
            control.Dock = DockStyle.Fill;
        }
    }
}
