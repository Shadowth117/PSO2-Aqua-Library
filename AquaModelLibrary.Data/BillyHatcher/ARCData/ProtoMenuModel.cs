using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Data.Ninja.Model;
using AquaModelLibrary.Data.Ninja.Motion;
using AquaModelLibrary.Helpers.Readers;
using ArchiveLib;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    //Unused menu_model.arc
    public class ProtoMenuModel
    {
        public ARCHeader header;
        public List<NJSObject> models = new List<NJSObject>();
        public List<NJSMotion> motions = new List<NJSMotion>();
        public NJTextureList texList = null;
        public PuyoFile gvm = null;

        public ProtoMenuModel() { }

        public ProtoMenuModel(byte[] file)
        {
            Read(file);
        }

        public ProtoMenuModel(BufferedStreamReaderBE<MemoryStream> sr)
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
            sr._BEReadActive = true;
            header = ARC.ReadArcHeader(sr);

            var offsetToOffsets = sr.ReadBE<int>();
            var modelsOffset = sr.ReadBE<int>();
            var animsOffset = sr.ReadBE<int>();
            var texListOffset = sr.ReadBE<int>();
            var gvmOffset = sr.ReadBE<int>();

            //Read Models
            sr.Seek(0x20 + modelsOffset, SeekOrigin.Begin);
            for(int i = 0; i < 4; i++)
            {
                var bookmark = sr.Position + 4;
                sr.Seek(0x20 + sr.ReadBE<int>(), SeekOrigin.Begin);
                models.Add(new NJSObject(sr, NinjaVariant.Ginja, true, 0x20));
                sr.Seek(bookmark, SeekOrigin.Begin);
            }

            //Read Anims
            sr.Seek(0x20 + animsOffset, SeekOrigin.Begin);
            for (int i = 0; i < 4; i++)
            {
                var bookmark = sr.Position + 4;
                sr.Seek(0x20 + sr.ReadBE<int>(), SeekOrigin.Begin);
                motions.Add(new NJSMotion(sr, true, 0x20));
                sr.Seek(bookmark, SeekOrigin.Begin);
            }

            //Texture list
            sr.Seek(0x20 + texListOffset, SeekOrigin.Begin);
            texList = new NJTextureList(sr, 0x20);

            //Texture Archive
            sr.Seek(0x20 + gvmOffset, SeekOrigin.Begin);
            gvm = new PuyoFile(GVMUtil.ReadGVMBytes(sr));
        }

    }
}
