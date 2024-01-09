using AquaModelLibrary.Data.PSO2.Aqua.MusFileRebootData;
using AquaModelLibrary.Data.PSO2.Aqua.MusFileRebootData.Composition;
using AquaModelLibrary.Helpers.Readers;
using System.IO;
using System.Text;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public class MusicFileReboot : AquaCommon
    {
        public int musHeaderOffset;
        public MusHeader header = null;
        public List<Composition> compositions = new List<Composition>();
        public List<TransitionData> transitionData = new List<TransitionData>();
        public List<Conditions> conditionsData = new List<Conditions>();
        public string musString1C = null;
        public string customVariables = null; //Custom variables are a comma separated list with a variable name, =, and an int value.

        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "mus\0"
            };
        }

        public MusicFileReboot() { }

        public MusicFileReboot(byte[] file) : base(file) { }

        public MusicFileReboot(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            sr.Seek(offset + sr.Read<int>(), SeekOrigin.Begin);
            header = new MusHeader(sr);

            if (header.mus.string_1COffset != 0x10 && header.mus.string_1COffset != 0)
            {
                sr.Seek(offset + header.mus.string_1COffset, SeekOrigin.Begin);
                musString1C = sr.ReadCString();
            }

            if (header.mus.customVariableStringOffset != 0x10 && header.mus.customVariableStringOffset != 0)
            {
                sr.Seek(offset + header.mus.customVariableStringOffset, SeekOrigin.Begin);
                customVariables = sr.ReadCString();
            }

            //Read compositions
            sr.Seek(offset + header.mus.compositionOffset, SeekOrigin.Begin);
            for (int i = 0; i < header.mus.compositionCount; i++)
            {
                compositions.Add(new Composition(sr, offset));
            }

            //Read transitions
            sr.Seek(offset + header.mus.transitionDataOffset, SeekOrigin.Begin);
            for (int i = 0; i < header.mus.transitionDataCount; i++)
            {
                transitionData.Add(new TransitionData(sr, offset));
            }

            //Read conditions
            sr.Seek(offset + header.mus.conditionDataOffset, SeekOrigin.Begin);
            for (int i = 0; i < header.mus.conditionDataCount; i++)
            {
                conditionsData.Add(new Conditions(sr, offset));
            }
        }

        public void SetCustomVariables(Dictionary<string, int> customVars)
        {
            StringBuilder customVarBuilder = new StringBuilder();
            int i = 0;
            foreach (var customVar in customVars)
            {
                customVarBuilder.Append($"{customVar.Key}={customVar.Value}");
                if (i != customVars.Count - 1)
                {
                    customVarBuilder.Append(',');
                }
                i++;
            }

            customVariables = customVarBuilder.ToString();
        }

        public Dictionary<string, int> GetCustomVariables()
        {
            var customVars = new Dictionary<string, int>();
            if (customVariables == null)
            {
                return customVars;
            }

            var pairs = customVariables.Split(',');
            foreach (var pair in pairs)
            {
                var pairSplit = pair.Split('=');
                customVars.Add(pairSplit[0], Int32.Parse(pairSplit[1]));
            }

            return customVars;
        }
    }
}
