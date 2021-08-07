using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CMXPatcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string settingsPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";
        CMXPatchHandler patcher = new CMXPatchHandler();
        CommonOpenFileDialog pso2BinSelect = new CommonOpenFileDialog()
        {
            IsFolderPicker = true,
            Title = "Select pso2_bin",
        };

        public MainWindow()
        {
            InitializeComponent();
            SetFunctionality();
        }

        private void SetPSO2Bin(object sender, RoutedEventArgs e)
        {
            if (pso2BinSelect.ShowDialog() == CommonFileDialogResult.Ok)
            {
                //Ensure paths are created and ready
                Directory.CreateDirectory(settingsPath);
                File.WriteAllText(settingsPath + "settings.txt", pso2BinSelect.FileName + "\\");
                
                patcher.InitializeCMX();
                SetFunctionality();
            }
        }

        private void SetFunctionality()
        {
            ExtractMenu.IsEnabled = patcher.readyToMod;
            patchCmxButton.IsEnabled = patcher.readyToMod;
            restoreCmxButton.IsEnabled = patcher.readyToMod;
        }

        private void CloseProgram(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ExtractCostumeEntry(object sender, RoutedEventArgs e)
        {
            ExtractBodyEntry("costume", patcher.cmx.costumeDict);
        }
        private void ExtractBasewearEntry(object sender, RoutedEventArgs e)
        {
            ExtractBodyEntry("basewear", patcher.cmx.baseWearDict);
        }
        private void ExtractOuterWearEntry(object sender, RoutedEventArgs e)
        {
            ExtractBodyEntry("outerwear", patcher.cmx.outerDict);
        }
        private void ExtractCarmEntry(object sender, RoutedEventArgs e)
        {
            ExtractBodyEntry("castarm", patcher.cmx.carmDict);
        }
        private void ExtractClegEntry(object sender, RoutedEventArgs e)
        {
            ExtractBodyEntry("castleg", patcher.cmx.clegDict);
        }

        private void ExtractBodyEntry(string type, Dictionary<int, AquaModelLibrary.CharacterMakingIndex.BODYObject> dict)
        {
            var id = NumberPrompt.ShowDialog(type);
            if (id != -1)
            {
                if (dict.ContainsKey(id))
                {
                    Directory.CreateDirectory(settingsPath + "CMXEntryDumps\\");
                    string path = settingsPath + $"CMXEntryDumps\\{ type}_{id}_cmxConfig.txt";
                    try
                    {
                        File.WriteAllText(path, BODYStructHandler.ConvertToString(dict[id], type).ToString());
                        MessageBox.Show($"Wrote successfully to {path}.");
                    }
                    catch
                    {
                        MessageBox.Show($"Unable to write {path}. Ensure you have all permissions to said directory.");
                    }
                } else
                {
                    MessageBox.Show("Please Input a valid id to extract.");
                }
            } else
            {
                MessageBox.Show("Please Input a valid id to extract.");
            }
        }

        private void cmxPatchClick(object sender, RoutedEventArgs e)
        {
            patcher.InjectCMXMods();
            MessageBox.Show("CMX successfully patched.");
        }

        private void cmxRestoreClick(object sender, RoutedEventArgs e)
        {
            patcher.InjectCMXMods(true);
            MessageBox.Show("CMX successfully restored.");
        }

        private void cmxDowngradeClick(object sender, RoutedEventArgs e)
        {
            patcher.DowngradeCmx();
            MessageBox.Show("CMX successfully downgraded. Output ice written CMXPatcher BenchmarkCMX subfolder.");
        }
    }
}
