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

            InitializeComponent();

            //Populate models dropdown 
            modelIDCB.BeginUpdate();
            for(int i = 0; i < modelset.models.Count; i++)
            {
                modelIDCB.Items.Add(i);
            }
            modelIDCB.EndUpdate();
            modelIDCB.SelectedIndex = 0;
        }

        public bool GetAllTransparentChecked()
        {
            return allAlphaCheckBox.Checked;
        }

        private void modelIDCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            modelPanel.Controls.Clear();

            //Default to material editor until other things are able to go here.
            var control = new MaterialEditor(modelset.models[modelIDCB.SelectedIndex]);
            modelPanel.Controls.Add(control);
            control.Dock = DockStyle.Fill;
        }
    }
}