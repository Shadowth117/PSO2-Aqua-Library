using AquaModelLibrary.Data.PSO2.Aqua.AquaFigureData;
using AquaModelLibrary.Extensions.Readers;
using System.IO;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    //Credit to DeathCream for doing a first pass on documenting the format
    public class AquaFigure : AquaCommon
    {
        public FigHeader figHeader;
        public List<int> attachTransformPtrList = new List<int>();
        public List<int> unkPtr1List = new List<int>();
        public List<AttachTransformObject> attachTransforms = new List<AttachTransformObject>();
        public List<AttachTransformObject> attachTransformsExtra = new List<AttachTransformObject>();
        public List<StateObjects> stateStructs = new List<StateObjects>();

        public AquaFigure() { }

        public AquaFigure(byte[] file, string _ext)
        {
            Read(file, _ext);
        }

        public AquaFigure(BufferedStreamReaderBE<MemoryStream> sr, string _ext)
        {
            Read(sr, _ext);
        }

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
    }
}
