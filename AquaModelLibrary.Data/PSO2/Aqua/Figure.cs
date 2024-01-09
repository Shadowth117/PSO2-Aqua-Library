using AquaModelLibrary.Data.PSO2.Aqua.AquaFigureData;
using AquaModelLibrary.Helpers.Readers;
using System.IO;
using System.Text;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    //Credit to DeathCream for doing a first pass on documenting the format
    public class Figure : AquaCommon
    {
        public FigHeader figHeader;
        public List<int> attachTransformPtrList = new List<int>();
        public List<int> unkPtr1List = new List<int>();
        public List<AttachTransformObject> attachTransforms = new List<AttachTransformObject>();
        public List<AttachTransformObject> attachTransformsExtra = new List<AttachTransformObject>();
        public List<StateObjects> stateStructs = new List<StateObjects>();

        public override string[] GetEnvelopeTypes()
        {
            return new string[] { "fig\0" };
        }

        public Figure() { }

        public Figure(byte[] file) : base(file) { }

        public Figure(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            figHeader = sr.Read<FigHeader>();

            //Read Attach Transforms
            if (figHeader.attachTransformPtr > 0x10)
            {
                sr.Seek(offset + figHeader.attachTransformPtr, SeekOrigin.Begin);
                for (int i = 0; i < figHeader.attachTransformCount; i++)
                {
                    int address = sr.Read<int>();
                    long bookmark = sr.Position;
                    sr.Seek(offset + address, SeekOrigin.Begin);
                    attachTransforms.Add(new AttachTransformObject(offset, sr));

                    sr.Seek(bookmark, SeekOrigin.Begin);
                }
            }

            //Read unk structs
            if (figHeader.statePtr > 0x10)
            {
                sr.Seek(offset + figHeader.statePtr, SeekOrigin.Begin);
                for (int i = 0; i < figHeader.stateCount; i++)
                {
                    int address = sr.Read<int>();
                    long bookmark = sr.Position;
                    sr.Seek(offset + address, SeekOrigin.Begin);
                    stateStructs.Add(new StateObjects(offset, sr));

                    sr.Seek(bookmark, SeekOrigin.Begin);
                }
            }
        }

        public static StringBuilder CheckFigEffectMaps(string inFilename, List<int> allTypes)
        {
            if (new FileInfo(inFilename).Length < 0x11)
            {
                return new StringBuilder();
            }
            StringBuilder dump = new StringBuilder();
            dump.AppendLine(inFilename);

            Figure fig = new Figure(File.ReadAllBytes(inFilename));

            foreach (var stateStruct in fig.stateStructs)
            {
                foreach (var eff in stateStruct.stateMap.effects)
                {
                    string mapped = "";
                    if (!FigEffectMapStructs.effectMappings.ContainsKey(eff.type))
                    {
                        mapped = "Not yet mapped!";
                    }
                    dump.AppendLine(eff.type.ToString() + " " + eff.type.ToString("X") + $" {mapped}");
                    if (!allTypes.Contains(eff.type))
                    {
                        allTypes.Add(eff.type);
                    }
                }
            }

            return dump;
        }
    }
}
