using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zamboni;

namespace AquaModelLibrary.Data.Utility
{
    public static class IceArchiveSelector
    {/*
        public static byte[] ShowDialog(string iceName, string filter)
        {
            var strm = new MemoryStream(File.ReadAllBytes(iceName));
            var fVarIce = IceFile.LoadIceFile(strm);
            var filteredFiles = new List<byte[]>();
            var filteredFileNames = new List<string>();
            strm.Dispose();

            List<byte[]> files = new List<byte[]>(fVarIce.groupOneFiles);
            files.AddRange(fVarIce.groupTwoFiles);

            //Loop through files to get what we need
            foreach (byte[] file in files)
            {
                var name = IceFile.getFileName(file).ToLower();
                if (name.Contains(filter))
                {
                    filteredFileNames.Add(name);
                    filteredFiles.Add(file);
                }
            }
            files = null;

            Form prompt = new Form();
            prompt.Width = 200;
            prompt.Height = 100;
            prompt.Text = $"Select a file:";
            Label label = new Label() { Text = "Select a file: ", Left = 30, Top = 1, Width = 100};
            ComboBox cb = new ComboBox() { Left = 40, Top = 18, Width = 100 };
            foreach(var name in filteredFileNames)
            {
                cb.Items.Add(name);
            }
            if(cb.Items.Count == 0)
            {
                return null;
            }
            cb.SelectedIndex = 0;
            Button confirmation = new Button() { Text = "Ok", Dock = DockStyle.Bottom };
            confirmation.Click += (sender, e) => { prompt.Close(); };

            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cb);
            prompt.Controls.Add(label);
            prompt.ShowDialog();

            fVarIce = null;

            return filteredFiles[cb.SelectedIndex];
            
        }
        */
    }
}
