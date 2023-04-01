using System;
using System.Collections.Generic;
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

namespace SoulsModelTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SoulsModelToolWindow : Window
    {
        public SoulsModelToolWindow(List<string> paths, List<SoulsActionModifiers> modifiers)
        {
            InitializeComponent();
            foreach (var modifier in modifiers)
            {
                switch(modifier)
                {
                    case SoulsActionModifiers.mirror:
                        mirrorCB.IsChecked = true;
                        break;
                    case SoulsActionModifiers.useMetadata:
                        useMetaDataCB.IsChecked = true;
                        break;
                    case SoulsActionModifiers.meshNameIsMatName:
                        matNamesToMeshCB.IsChecked = true;
                        break;
                    case SoulsActionModifiers.transformMesh:
                        transformMeshCB.IsChecked = true;
                        break;
                }
            }
        }

        public List<SoulsActionModifiers> GetModifiers()
        {
            List<SoulsActionModifiers> modifiers = new List<SoulsActionModifiers>();

            if(useMetaDataCB.IsChecked == true)
            {
                modifiers.Add(SoulsActionModifiers.useMetadata);
            }
            if (mirrorCB.IsChecked == true)
            {
                modifiers.Add(SoulsActionModifiers.mirror);
            }
            if (matNamesToMeshCB.IsChecked == true)
            {
                modifiers.Add(SoulsActionModifiers.meshNameIsMatName);
            }
            if (transformMeshCB.IsChecked == true)
            {
                modifiers.Add(SoulsActionModifiers.transformMesh);
            }

            return modifiers;
        }
    }
}
