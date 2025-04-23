using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Data.Ninja.Model;
using AquaModelLibrary.Helpers.Readers;
using ArchiveLib;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    /// <summary>
    /// Used for obj_ms_fwball.arc
    /// </summary>
    public class ObjMsFwBall : ARC
    {

        public NJSObject model = null;
        public NJTextureList texList = null;
        public PuyoFile gvm = null;

        public ObjMsFwBall() { }

        public ObjMsFwBall(byte[] file)
        {
            Read(file);
        }

        public ObjMsFwBall(BufferedStreamReaderBE<MemoryStream> sr)
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
                model = new NJSObject(sr, NinjaVariant.Ginja, true, 0x20);
                sr.Seek(bookmark, SeekOrigin.Begin);
            }

            //Read Texlists
            var texListOffset = sr.ReadBE<int>();
            if (texListOffset != 0)
            {
                var bookmark = sr.Position;
                sr.Seek(texListOffset + 0x20, SeekOrigin.Begin);
                texList = new NJTextureList(sr, 0x20);

                sr.Seek(bookmark, SeekOrigin.Begin);
            }

            //Read Textures
            sr.Seek(sr.ReadBE<int>() + 0x20, SeekOrigin.Begin);
            gvm = new PuyoFile(GVMUtil.ReadGVMBytes(sr));
        }
    }
}
