using AquaModelLibrary.Data.PSO2.Aqua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace AquaModelLibrary.Data.Utility
{
    public class JSONUtility
    {
        public static void ConvertToJson(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            JsonSerializerOptions jss = new JsonSerializerOptions() { WriteIndented = true };
            string jsonData = "";
            switch (ext)
            {
                case ".aqo":
                case ".tro":
                case ".aqp":
                case ".trp":
                    var aqp = new AquaPackage(File.ReadAllBytes(filePath));
                    jsonData = JsonSerializer.Serialize(aqp, jss);
                    break;
                case ".aqn":
                case ".trn":
                    var aqn = new AquaNode(File.ReadAllBytes(filePath));
                    jsonData = JsonSerializer.Serialize(aqn, jss);
                    break;
                case ".aqm":
                case ".trm":
                    var aqm = new AquaMotion(File.ReadAllBytes(filePath));
                    jsonData = JsonSerializer.Serialize(aqm, jss);
                    break;
                case ".bti":
                    var bti = new BTI_MotionConfig(File.ReadAllBytes(filePath));
                    jsonData = JsonSerializer.Serialize(bti, jss);
                    break;
                case ".cmx":
                    var cmx = new CharacterMakingIndex(File.ReadAllBytes(filePath));
                    jsonData = JsonSerializer.Serialize(cmx, jss);
                    break;
                case ".text":
                    var text = new PSO2Text(filePath);
                    jsonData = JsonSerializer.Serialize(text, jss);
                    break;
                case ".aqe": //(Classic for now)
                    var aqe = new AquaEffect(File.ReadAllBytes(filePath));
                    jsonData = JsonSerializer.Serialize(aqe, jss);
                    break;
            }
            File.WriteAllText(filePath + ".json", jsonData);
        }
        /*
        public void ConvertFromJson(string filePath)
        {
            var ogName = filePath.Substring(0, filePath.Length - 5);
            var ext = Path.GetExtension(ogName); //GetFileNameWithoutExtension nixes the .json text
            var jsonData = File.ReadAllText(filePath);

            switch (ext)
            {
                case ".aqo":
                case ".tro":
                case ".aqp":
                case ".trp":
                    ModelSet aqp;
                    if (filePath.Contains(".ngs."))
                    {
                        aqp = JsonConvert.DeserializeObject<NGSModelSet>(jsonData).GetModelSet();
                        aquaModels.Add(aqp);
                        WriteNGSNIFLModel(ogName, ogName);
                    }
                    else if (filePath.Contains(".classic."))
                    {
                        aqp = JsonConvert.DeserializeObject<ClassicModelSet>(jsonData).GetModelSet();
                        aquaModels.Add(aqp);
                        WriteClassicNIFLModel(ogName, ogName);
                    }
                    break;
                case ".aqn":
                case ".trn":
                    var aqn = JsonConvert.DeserializeObject<AquaNode>(jsonData);
                    WriteBones(ogName, aqn);
                    break;
                case ".aqm":
                case ".trm":
                    var aqm = JsonConvert.DeserializeObject<AnimSet>(jsonData);
                    aquaMotions.Add(aqm);
                    WriteNIFLMotion(ogName);
                    break;
                case ".bti":
                    var bti = JsonConvert.DeserializeObject<AquaBTI_MotionConfig>(jsonData);
                    WriteBTI(bti, ogName);
                    break;
                case ".cmx":
                    var cmx = JsonConvert.DeserializeObject<CharacterMakingIndex>(jsonData);
                    WriteCMX(ogName, cmx, 1);
                    break;
                case ".text":
                    var text = JsonConvert.DeserializeObject<PSO2Text>(jsonData);
                    WritePSO2TextNIFL(ogName, text);
                    break;
                case ".aqe": //(Classic for now)
                    var aqe = JsonConvert.DeserializeObject<AquaEffect>(jsonData);
                    aquaEffect.Add(aqe);
                    WriteClassicNIFLEffect(ogName);
                    aquaEffect.Clear();
                    break;
            }
        }
        */
    }
}
