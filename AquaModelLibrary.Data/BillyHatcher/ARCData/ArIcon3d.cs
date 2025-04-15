using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Helpers.Readers;
using ArchiveLib;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    /// <summary>
    /// Just a texture list and a gvm with one texture
    /// </summary>
    public class ARIcon3d : ARC
    {
        public List<NJTextureList> texList = new List<NJTextureList>();
        public PuyoFile gvm = null;

        public ARIcon3d() { }

        public ARIcon3d(byte[] file)
        {
            Read(file);
        }

        public ARIcon3d(BufferedStreamReaderBE<MemoryStream> sr)
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
            int nodeCount = 0;
            sr._BEReadActive = true;
            base.Read(sr);
            sr.Seek(0x20, SeekOrigin.Begin);

            var texListOffset = sr.ReadBE<int>();
            var gvmOffset = sr.ReadBE<int>();

            //Read Texlists
            sr.Seek(0x20 + texListOffset, SeekOrigin.Begin);
            texList.Add(new NJTextureList(sr, 0x20));

            sr.Seek(0x20 + gvmOffset, SeekOrigin.Begin);
            gvm = new PuyoFile(GVMUtil.ReadGVMBytes(sr));
        }
    }
}
