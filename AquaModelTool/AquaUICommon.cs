using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AquaModelTool
{
    public class AquaUICommon
    {
        OpenFileDialog openFileDialog = new OpenFileDialog()
        {
            Title = "Select a PSO2 model file",
            Filter = "PSO2 Model Files (*.aqp, *.aqo, *.trp, *.tro)|*.aqp;*.aqo;*.trp;*.tro"
        };
        public AquaModelLibrary.AquaUtil aqua = new AquaModelLibrary.AquaUtil();

        public string openFile(string str = null)
        {
            if (str != null)
            {
                aqua.aquaModels.Clear();
                aqua.ReadModel(str);
            } else
            {
                if(openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    aqua.aquaModels.Clear();
                    aqua.ReadModel(openFileDialog.FileName);
                    str = openFileDialog.FileName;
                }

            }
            return str;
        }

        public void setAllTransparent(bool setToModels)
        {
            if (setToModels)
            {
                for (int i = 0; i < aqua.aquaModels.Count; i++)
                {
                    for (int j = 0; j < aqua.aquaModels[i].models.Count; j++)
                    {
                        for (int r = 0; r < aqua.aquaModels[i].models[j].rendList.Count; r++)
                        {
                            var rend = aqua.aquaModels[i].models[j].rendList[r];
                            rend.notOpaque = 1;
                            rend.unk10 = 0;
                            aqua.aquaModels[i].models[j].rendList[r] = rend;
                        }
                        for (int m = 0; m < aqua.aquaModels[i].models[j].mateList.Count; m++)
                        {
                            var mate = aqua.aquaModels[i].models[j].mateList[m];
                            mate.alphaType.SetString("blendalpha");
                            aqua.aquaModels[i].models[j].mateList[m] = mate;
                        }
                    }
                }
            }
        }

        public void toNIFLModel(string str)
        {
            //These will be output as .**p regardless and if the user really wants the o version, they can do it in 2 seconds in a hex editor.
            str = str.Replace(".aqo", ".aqp");
            str = str.Replace(".tro", ".trp");
            
            aqua.WriteNIFLModel(str, str);
        }

        public void toVTBFModel(string str)
        {
            //These will be output as .**p regardless and if the user really wants the o version, they can do it in 2 seconds in a hex editor.
            str = str.Replace(".aqo", ".aqp");
            str = str.Replace(".tro", ".trp");

            aqua.WriteVTBFModel(str, str);
        }
    }
}
