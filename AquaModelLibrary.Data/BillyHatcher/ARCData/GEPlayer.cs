using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Helpers.Readers;
using ArchiveLib;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    public class GEPlayer : ARC
    {
        public PuyoFile gvm = null;
        public List<string> texnames = new List<string>();
        public GEPlayer() { }

        public GEPlayer(byte[] file)
        {
            Read(file);
        }

        public GEPlayer(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public override void Read(byte[] file)
        {
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        public override void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            base.Read(sr);

            for (int i = 0; i < group1FileNames.Count; i++)
            {
                var fileName = group1FileNames[i];
                sr.Seek(0x20 + group1FileReferences[i].fileOffset, SeekOrigin.Begin);
                switch (fileName)
                {
                    case "texlists":
                        texnames = ReadTexNames(sr);
                        break;
                    case "textures":
                        sr.Seek(0x20 + sr.ReadBE<int>(), SeekOrigin.Begin);
                        gvm = new PuyoFile(GVMUtil.ReadGVMBytes(sr));
                        break;
                }
            }
        }
    }
}
