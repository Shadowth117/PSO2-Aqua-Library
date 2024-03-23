using AquaModelLibrary.Data.Ninja.Model;
using AquaModelLibrary.Data.Ninja.Motion;
using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Helpers.Readers;
using ArchiveLib;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    /// <summary>
    /// ar_ene_*.arc - These contain data for enemy models, motions, and a texture list. The GVM is stored externally with the same name, but no preceding ar_ and the .gvm extension
    /// </summary>
    public class ArEnemy
    {
        public ARCHeader header;
        public List<NJSObject> models = new List<NJSObject>();
        public List<NJSMotion> anims = new List<NJSMotion>();
        public NJTextureList texList = null;

        public ArEnemy() { }

        public ArEnemy(byte[] file)
        {
            Read(file);
        }

        public ArEnemy(BufferedStreamReaderBE<MemoryStream> sr)
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

            var modelOffset = sr.ReadBE<int>();
            var animOffset = sr.ReadBE<int>();
            var texListOffset = sr.ReadBE<int>();

            var modelCount = (animOffset - modelOffset) / 4;
            var animCount = (texListOffset - animOffset) / 4;

            //Read Models
            sr.Seek(0x20 + modelOffset, SeekOrigin.Begin);
            for (int i = 0; i < modelCount; i++)
            {
                var offset = sr.ReadBE<int>();
                var bookmark = sr.Position;
                sr.Seek(offset + 0x20, SeekOrigin.Begin);
                models.Add(new NJSObject(sr, NinjaVariant.Ginja, true, 0x20));

                sr.Seek(bookmark, SeekOrigin.Begin);
            }

            //Read Motions
            sr.Seek(0x20 + animOffset, SeekOrigin.Begin);
            for (int i = 0; i < animCount; i++)
            {
                var offset = sr.ReadBE<int>();
                var bookmark = sr.Position;
                sr.Seek(offset + 0x20, SeekOrigin.Begin);
                anims.Add(new NJSMotion(sr, true, 0x20, true, nodeCount));

                sr.Seek(bookmark, SeekOrigin.Begin);
            }

            //Read Texture List - gvm should be external
            sr.Seek(0x20 + texListOffset, SeekOrigin.Begin);
            texList = new NJTextureList(sr, 0x20);
        }
    }
}
