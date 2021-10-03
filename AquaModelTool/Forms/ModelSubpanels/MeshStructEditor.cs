using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AquaModelLibrary;

namespace AquaModelTool
{
    public partial class MeshStructEditor : UserControl
    {
        AquaObject model;
        List<AquaObject.MESH> meshList;
        public MeshStructEditor(AquaObject newModel, List<AquaObject.MESH> newMeshList, bool isList2 = false)
        {
            InitializeComponent();

            if(isList2)
            {
                meshLabel.Text = meshLabel.Text.Replace("Mesh", "Mesh2");
            }

            model = newModel;
            meshList = newMeshList;

            //Populate comboboxes. Check if id is within combobox's range and if not, add to the combobox
            //In the case of mesh2s with negative ids, the combobox will NOT get populated.
            var mesh = meshList[0];
            int matCount = (mesh.mateIndex > newModel.mateList.Count) || (mesh.mateIndex == -1) ? mesh.mateIndex : newModel.mateList.Count;
            int rendCount = (mesh.rendIndex > newModel.rendList.Count) || (mesh.rendIndex == -1) ? mesh.rendIndex : newModel.rendList.Count;
            int shadCount = (mesh.shadIndex > newModel.shadList.Count) || (mesh.shadIndex == -1) ? mesh.shadIndex : newModel.shadList.Count;
            int tsetCount = (mesh.tsetIndex > newModel.tsetList.Count) || (mesh.tsetIndex == -1) ? mesh.tsetIndex : newModel.tsetList.Count;

            int vsetCount = (mesh.vsetIndex > newModel.vsetList.Count) || (mesh.vsetIndex == -1) ? mesh.vsetIndex : newModel.vsetList.Count;
            int psetCount = (mesh.psetIndex > newModel.psetList.Count) || (mesh.psetIndex == -1) ? mesh.psetIndex : newModel.psetList.Count;

            SetComboBox(meshIDCB, meshList.Count);

            SetComboBox(matIDCB, matCount);
            SetComboBox(renderIDCB, rendCount);
            SetComboBox(shadIDCB, shadCount);
            SetComboBox(tsetIDCB, tsetCount);
            SetComboBox(vsetIDCB, vsetCount);
            SetComboBox(faceSetIDCB, psetCount);

            meshIDCB.SelectedIndex = 0;
        }

        public void SetComboBox(ComboBox cb, int count)
        {
            for (int i = 0; i < count; i++)
            {
                cb.Items.Add(i);
            }

            //Disable if there's no items to use
            if(cb.Items.Count == 0)
            {
                cb.Enabled = false;
            }
        }

        public void UpdateUi()
        {
            var mesh = meshList[meshIDCB.SelectedIndex];

            flagsUD.Value = mesh.flags;
            unkShort0UD.Value = mesh.unkShort0;
            unkShort1UD.Value = mesh.unkShort1;
            unkByte0UD.Value = mesh.unkByte0;
            unkByte1UD.Value = mesh.unkByte1;
            unkInt0UD.Value = mesh.unkInt0;
            baseMeshNodeIdUD.Value = mesh.baseMeshNodeId;
            baseMeshDummyIdUD.Value = mesh.baseMeshDummyId;

            matIDCB.SelectedIndex = mesh.mateIndex;
            renderIDCB.SelectedIndex = mesh.rendIndex;
            shadIDCB.SelectedIndex = mesh.shadIndex;
            tsetIDCB.SelectedIndex = mesh.tsetIndex;
            vsetIDCB.SelectedIndex = mesh.vsetIndex;
            faceSetIDCB.SelectedIndex = mesh.psetIndex;
        }

        private void flagsUD_ValueChanged(object sender, EventArgs e)
        {
            var mesh = meshList[meshIDCB.SelectedIndex];
            mesh.flags = (short)flagsUD.Value;
            meshList[meshIDCB.SelectedIndex] = mesh;
        }

        private void unkShort0UD_ValueChanged(object sender, EventArgs e)
        {
            var mesh = meshList[meshIDCB.SelectedIndex];
            mesh.unkShort0 = (short)unkShort0UD.Value;
            meshList[meshIDCB.SelectedIndex] = mesh;
        }

        private void unkByte0UD_ValueChanged(object sender, EventArgs e)
        {
            var mesh = meshList[meshIDCB.SelectedIndex];
            mesh.unkByte0 = (byte)unkByte0UD.Value;
            meshList[meshIDCB.SelectedIndex] = mesh;
        }

        private void unkByte1UD_ValueChanged(object sender, EventArgs e)
        {
            var mesh = meshList[meshIDCB.SelectedIndex];
            mesh.unkByte1 = (byte)unkByte1UD.Value;
            meshList[meshIDCB.SelectedIndex] = mesh;
        }

        private void unkShort1UD_ValueChanged(object sender, EventArgs e)
        {
            var mesh = meshList[meshIDCB.SelectedIndex];
            mesh.unkShort1 = (short)unkShort1UD.Value;
            meshList[meshIDCB.SelectedIndex] = mesh;
        }

        private void baseMeshNodeIdUD_ValueChanged(object sender, EventArgs e)
        {
            var mesh = meshList[meshIDCB.SelectedIndex];
            mesh.baseMeshNodeId = (int)baseMeshNodeIdUD.Value;
            meshList[meshIDCB.SelectedIndex] = mesh;
        }

        private void baseMeshDummyIdUD_ValueChanged(object sender, EventArgs e)
        {
            var mesh = meshList[meshIDCB.SelectedIndex];
            mesh.baseMeshDummyId = (int)baseMeshDummyIdUD.Value;
            meshList[meshIDCB.SelectedIndex] = mesh;
        }

        private void unkInt0UD_ValueChanged(object sender, EventArgs e)
        {
            var mesh = meshList[meshIDCB.SelectedIndex];
            mesh.unkInt0 = (int)unkInt0UD.Value;
            meshList[meshIDCB.SelectedIndex] = mesh;
        }

        private void meshIDCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUi();
        }

        private void matIDCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            var mesh = meshList[meshIDCB.SelectedIndex];
            mesh.mateIndex = matIDCB.SelectedIndex;
            meshList[meshIDCB.SelectedIndex] = mesh;
        }

        private void renderIDCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            var mesh = meshList[meshIDCB.SelectedIndex];
            mesh.rendIndex = renderIDCB.SelectedIndex;
            meshList[meshIDCB.SelectedIndex] = mesh;
        }

        private void tsetIDCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            var mesh = meshList[meshIDCB.SelectedIndex];
            mesh.tsetIndex = tsetIDCB.SelectedIndex;
            meshList[meshIDCB.SelectedIndex] = mesh;
        }

        private void vsetIDCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            var mesh = meshList[meshIDCB.SelectedIndex];
            mesh.vsetIndex = vsetIDCB.SelectedIndex;
            meshList[meshIDCB.SelectedIndex] = mesh;
        }

        private void faceSetIDCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            var mesh = meshList[meshIDCB.SelectedIndex];
            mesh.psetIndex = faceSetIDCB.SelectedIndex;
            meshList[meshIDCB.SelectedIndex] = mesh;
        }
    }
}
