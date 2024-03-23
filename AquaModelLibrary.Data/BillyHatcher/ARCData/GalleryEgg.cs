using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Data.Ninja.Model;
using AquaModelLibrary.Data.Ninja.Motion;
using AquaModelLibrary.Helpers.Readers;
using ArchiveLib;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    //gallery_egg.arc
    public class GalleryEgg
    {
        public ARCHeader header;
        public NJSObject model = null;
        public NJSMotion anim = null;
        public List<NJTextureList> texLists = new List<NJTextureList>();
        public List<PuyoFile> texArchives = new List<PuyoFile>();

        public GalleryEgg() { }

        public GalleryEgg(byte[] file)
        {
            Read(file);
        }

        public GalleryEgg(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }
        public void Read(byte[] file)
        {
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            int nodeCount = 0;
            sr._BEReadActive = true;
            header = ARC.ReadArcHeader(sr);

            var eggCount = sr.ReadBE<int>();
            var modelOffset = sr.ReadBE<int>();
            var animOffset = sr.ReadBE<int>();
            var texListArrOffset = sr.ReadBE<int>();
            var texArrOffset = sr.ReadBE<int>();

            //Read Model
            sr.Seek(0x20 + modelOffset, SeekOrigin.Begin);
            model = new NJSObject(sr, NinjaVariant.Ginja, true, 0x20);
            model.CountAnimated(ref nodeCount);

            //Read Anim
            sr.Seek(0x20 + animOffset, SeekOrigin.Begin);
            anim = new NJSMotion(sr, true, 0x20, true, nodeCount);

            //Texture lists
            sr.Seek(0x20 + texListArrOffset, SeekOrigin.Begin);
            for(int i = 0; i < eggCount; i++)
            {
                var offset = sr.ReadBE<int>();
                var bookmark = sr.Position;
                sr.Seek(offset + 0x20, SeekOrigin.Begin);
                texLists.Add(new NJTextureList(sr, 0x20));

                sr.Seek(bookmark, SeekOrigin.Begin);
            }

            //Texture Archives
            sr.Seek(0x20 + texArrOffset, SeekOrigin.Begin);
            for (int i = 0; i < eggCount; i++)
            {
                var offset = sr.ReadBE<int>();
                var bookmark = sr.Position;
                sr.Seek(offset + 0x20, SeekOrigin.Begin);
                texArchives.Add(new PuyoFile(GVMUtil.ReadGVMBytes(sr)));

                sr.Seek(bookmark, SeekOrigin.Begin);
            }
        }

    }
}
