using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AquaModelTool
{
    public static class AquaUICommon
    {
        public static void openFile(string str = null)
        {
            if(str != null)
            {
                toVTBF(str);
            } else
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Select a PSO2 model file";
                openFileDialog.Filter = "PSO2 Model Files (*.aqp, *.aqo, *.trp, *.tro)|*.aqp;*.aqo;*.trp;*.tro";

                if(openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    toVTBF(openFileDialog.FileName);
                }

            }
        }

        public static void toVTBF(string str)
        {
            AquaLibrary.AquaUtil aqua = new AquaLibrary.AquaUtil();
            aqua.ReadModel(str);

            //These will be output as .**p regardless and if the user really wants the o version, they can do it in 2 seconds in a hex editor.
            str = str.Replace(".aqo", ".aqp");
            str = str.Replace(".tro", ".trp");
            
            //For now just write all back to VTBF
            string newStr = str.Replace(Path.GetExtension(str), "_VTBF" + Path.GetExtension(str));
            aqua.WriteVTBFModel(str, newStr);
        }
    }
}
