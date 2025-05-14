using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Data.Ninja.Model;
using AquaModelLibrary.Helpers.Readers;
using ArchiveLib;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    /// <summary>
    /// Used for egg_gold.arc and egg_suit.arc
    /// </summary>
    public class EggGold_Suit : ARC
    {

        public List<NJSObject> model = null;
        public List<NJTextureList> texLists = new List<NJTextureList>();
        public PuyoFile gvm = null;

        public EggGold_Suit() { }

        public EggGold_Suit(byte[] file)
        {
            Read(file);
        }

        public EggGold_Suit(BufferedStreamReaderBE<MemoryStream> sr)
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

            //Read Model
            var modelOffset = sr.ReadBE<int>();
            if (modelOffset != 0)
            {
                var bookmark = sr.Position;
                sr.Seek(modelOffset + 0x20, SeekOrigin.Begin);
                //model = new NJSObject(sr, NinjaVariant.Ginja, true, 0x20);
                sr.Seek(bookmark, SeekOrigin.Begin);
            }

            //Read Texlists
            for (int i = 0; i < 2; i++)
            {
                var offset = sr.ReadBE<int>();
                if (offset == 0)
                {
                    break;
                }
                var bookmark = sr.Position;
                sr.Seek(offset + 0x20, SeekOrigin.Begin);
                texLists.Add(new NJTextureList(sr, 0x20));

                sr.Seek(bookmark, SeekOrigin.Begin);
            }

            //Read Textures
            sr.Seek(sr.ReadBE<int>() + 0x20, SeekOrigin.Begin);
            gvm = new PuyoFile(GVMUtil.ReadGVMBytes(sr));
        }
    }
}
