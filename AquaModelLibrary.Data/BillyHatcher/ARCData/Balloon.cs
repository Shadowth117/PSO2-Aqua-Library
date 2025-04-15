using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Data.Ninja.Model;
using AquaModelLibrary.Helpers.Readers;
using ArchiveLib;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    /// <summary>
    /// Used for balloon.arc
    /// </summary>
    public class Balloon : ARC
    {

        public List<NJSObject> models = new List<NJSObject>();
        public List<NJTextureList> texList = new List<NJTextureList>();
        public PuyoFile gvm = null;

        public Balloon() { }

        public Balloon(byte[] file)
        {
            Read(file);
        }

        public Balloon(BufferedStreamReaderBE<MemoryStream> sr)
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
            sr._BEReadActive = true;
            base.Read(sr);
            sr.Seek(0x20, SeekOrigin.Begin);

            //Read Models
            for (int i = 0; i < 7; i++)
            {
                var offset = sr.ReadBE<int>();
                if (offset == 0)
                {
                    break;
                }
                var bookmark = sr.Position;
                sr.Seek(offset + 0x20, SeekOrigin.Begin);
                models.Add(new NJSObject(sr, NinjaVariant.Ginja, true, 0x20));

                sr.Seek(bookmark, SeekOrigin.Begin);
            }

            //Read Texlists
            for (int i = 0; i < 8; i++)
            {
                var offset = sr.ReadBE<int>();
                if (offset == 0)
                {
                    break;
                }
                var bookmark = sr.Position;
                sr.Seek(offset + 0x20, SeekOrigin.Begin);
                texList.Add(new NJTextureList(sr, 0x20));

                sr.Seek(bookmark, SeekOrigin.Begin);
            }

            //Read Textures
            sr.Seek(sr.ReadBE<int>() + 0x20, SeekOrigin.Begin);
            gvm = new PuyoFile(GVMUtil.ReadGVMBytes(sr));
        }
    }
}
