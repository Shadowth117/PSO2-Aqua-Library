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
        public AquaModelLibrary.AquaUtil aqua = new AquaModelLibrary.AquaUtil();

        public AquaUICommon()
        {
        }


        public string confirmFile(string str = null)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select a PSO2 file",
                Filter = "All Supported Files|*.aqp;*.aqo;*.trp;*.tro;*.aqm;*.aqc;*.aqv;*.aqw;*.trm;*.trv;*.trw" +
                         "|PSO2 Model Files (*.aqp, *.aqo, *.trp, *.tro)|*.aqp;*.aqo;*.trp;*.tro" +
                         "|PSO2 Motion Files (*.aqm, *.aqv, *.aqw, *.aqc, *.trm, *.trv, *.trw)|*.aqm;*.aqv;*.aqw;*.aqc;*.trm;*.trv;*.trw"
            };
            if(str == null)
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
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
                            rend.int_0C = 1;
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

        public void averageNormals()
        {
            for (int i = 0; i < aqua.aquaModels.Count; i++)
            {
                for(int j = 0; j < aqua.aquaModels[i].models.Count; j++)
                {
                    AquaModelLibrary.AquaObjectMethods.CalcUNRMs(aqua.aquaModels[i].models[j], true, false);
                }
            }
        }

        public void toNIFLModel(string str)
        {
            //These will be output as .**p regardless and if the user really wants the o version, they can do it in 2 seconds in a hex editor.
            str = str.Replace(".aqo", ".aqp");
            str = str.Replace(".tro", ".trp");
            
            aqua.WriteClassicNIFLModel(str, str);
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
