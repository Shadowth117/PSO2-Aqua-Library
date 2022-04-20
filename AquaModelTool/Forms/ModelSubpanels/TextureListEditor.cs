using AquaModelLibrary;
using System.Windows.Forms;

namespace AquaModelTool.Forms.ModelSubpanels
{
    public partial class TextureListEditor : UserControl
    {
        private AquaObject _aqp;
        private int texLimit = 0x10;
        public TextureListEditor(AquaObject aqp)
        {
            _aqp = aqp;
            InitializeComponent();

            //NGS models are allowed 
            //Note for NGS textures, texUsageOrder param is seemingly more of a type. In Classic PSO2 models, it's an actual order id. Unsure of importance, but may need study
            if(_aqp.tsetList.Count > 0)
            {
                SetTsetTexLimit(aqp);
                //SetTstaData();
            }
            else
            {
                texListCB.Enabled = false;
                texSlotCB.Enabled = false;
                addSlotButton.Enabled = false;
                insertSlotButton.Enabled = false;
                removeSlotButton.Enabled = false;
            }

        }

        private void SetTsetTexLimit(AquaObject aqp)
        {
            if (aqp.objc.type >= 0xC32 || _aqp.tsetList[0].texCount > 4)
            {
                texLimit = 0x10;
            }
            else
            {
                texLimit = 0x4;
            }
        }

        private void addButton_Click(object sender, System.EventArgs e)
        {

        }

        private void insertButton_Click(object sender, System.EventArgs e)
        {

        }

        private void removeButton_Click(object sender, System.EventArgs e)
        {

        }

        private void texListCB_SelectedIndexChanged(object sender, System.EventArgs e)
        {
  
        }


        private void texSlotCB_SelectedIndexChanged(object sender, System.EventArgs e)
        {

        }

        private void texRefCB_SelectedIndexChanged(object sender, System.EventArgs e)
        {

        }
    }
}
