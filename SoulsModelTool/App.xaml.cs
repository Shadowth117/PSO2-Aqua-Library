using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SoulsModelTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public enum SoulsModelAction
        {
            none = 0,
            toFBX = 1,
            toFlver = 2,
            toFlverDes = 3,
            toCMDL = 4, //lol
            toObj = 5,
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            bool launchUi = true;
            List<string> filePaths = new List<string>();
            List<SoulsActionModifiers> modifiers = new List<SoulsActionModifiers>();
            if (e.Args.Length > 0)
            {
                var action = SoulsModelAction.toFBX;
                launchUi = false;

                foreach (var arg in e.Args)
                {
                    var argProcessed = arg.ToLower();
                    switch(argProcessed)
                    {
                        case "-tofbx":
                            action = SoulsModelAction.toFBX;
                            break;
                        case "-toflver":
                            action = SoulsModelAction.toFlver;
                            Trace.WriteLine("toFlver not implemented. Did you mean toFlverDes?");
                            break;
                        case "-toflverdes":
                            action = SoulsModelAction.toFlverDes;
                            break;
                        case "-tocmdl":
                        case "-tocmsh":
                            action = SoulsModelAction.toCMDL;
                            Trace.WriteLine("toCMDL not implemented.");
                            break;
                        case "-toobj":
                            action = SoulsModelAction.toObj;
                            break;
                        case "-nomirror":
                            modifiers.Add(SoulsActionModifiers.mirror);
                            break;
                        case "-dontdumpmetadata":
                            modifiers.Add(SoulsActionModifiers.useMetadata);
                            break;
                        case "-meshnameismatname":
                            modifiers.Add(SoulsActionModifiers.meshNameIsMatName);
                            break;
                        case "-transformMesh":
                            modifiers.Add(SoulsActionModifiers.transformMesh);
                            break;
                        case "-launch":
                            launchUi = true;
                            break;
                        default:
                            filePaths.Add(arg);
                            break;
                    }
                }
                switch (action)
                {
                    case SoulsModelAction.toFBX:
                        break;
                    case SoulsModelAction.toObj:
                        break;
                    case SoulsModelAction.toFlverDes:
                        break;
                    case SoulsModelAction.toFlver:
                    case SoulsModelAction.toCMDL:
                    case SoulsModelAction.none:
                    default:
                        break;
                }
            }


            if(launchUi)
            {
                SoulsModelToolWindow wnd = new SoulsModelToolWindow(filePaths, modifiers);
                wnd.Show();
            }

        }
    }
}
