using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AquaModelLibrary;

namespace AquaAutoRig
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if(args.Length != 1)
            {
                DisplayUsage();
            }
            else
            {
                var aqua = new AquaUtil();
                switch(Path.GetExtension(args[0]))
                {
                    case ".aqp":
                    case ".aqo":
                    case ".trp":
                    case ".tro":
                        string backup = Path.ChangeExtension(args[0], ".org.aqp");
                        if (File.Exists(backup))
                        {
                            File.Delete(backup);
                        }
                        File.Copy(args[0], backup);
                        var aqpName = args[0];
                        aqua.ReadModel(aqpName);
                        LegacyObj.LegacyObjIO.ExportObj(Path.ChangeExtension(aqpName, ".obj"), aqua.aquaModels[0].models[0]);
                        break;
                    case ".obj":
                        aqua.ReadModel(Path.ChangeExtension(args[0], ".org.aqp"));
                        aqua.aquaModels[0].models[0] = LegacyObj.LegacyObjIO.ImportObj(args[0], aqua.aquaModels[0].models[0]);
                        string outName = Path.ChangeExtension(args[0], ".aqp");
                        if (aqua.aquaModels[0].models[0].objc.type >= 0xC32)
                        {
                            aqua.WriteNGSNIFLModel(outName, outName);
                        } else
                        {
                            aqua.WriteClassicNIFLModel(outName, outName);
                        }
                        break;
                    default:
                        DisplayUsage();
                        break;
                }
            }
        }

        private static void DisplayUsage()
        {
            Console.WriteLine("usage: " +
                "\nAquaAutoRig.exe model.aqp" +
                "\nAquaAutoRig.exe model.obj" +
                "\nFeeding the program an aqp will create an .obj, .mtl, and .org.aqp of the same name as the given file." +
                "\nFeeding the program an .obj will have it use the .org.aqp of the same name as a base." +
                "\nProps to the anonymous creator of aqp2obj for the original program. ありがとうございます。" +
                "\nPress any key to continue.");
            Console.ReadKey();
        }
    }
}
