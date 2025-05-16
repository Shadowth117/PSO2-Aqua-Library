using AquaModelLibrary.Data.Ninja.Model;
using AquaModelLibrary.Data.Ninja.Motion;
using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Helpers.Readers;
using ArchiveLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    public class BattleModelPlayer
    {
        public ARCHeader header;
        public List<NJSObject> models = new();
        public List<NJSMotion> motions = new();
        public List<NJTextureList> texLists = new();
        public List<PuyoFile> gvms = new();

        public BattleModelPlayer() { }

        public BattleModelPlayer(byte[] file)
        {
            Read(file);
        }

        public BattleModelPlayer(BufferedStreamReaderBE<MemoryStream> sr)
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

            var modelsCount = sr.ReadBE<int>();
            var animsCount = sr.ReadBE<int>();
            var modelsOffset = sr.ReadBE<int>();
            var animsOffset = sr.ReadBE<int>();
            var texListsOffset = sr.ReadBE<int>();
            var gvmsOffset = sr.ReadBE<int>();
            var texLists_GvmsCount = (gvmsOffset - texListsOffset) / 4;

            //Read Models
            sr.Seek(0x20 + modelsOffset, SeekOrigin.Begin);
            for (int i = 0; i < modelsCount; i++)
            {
                var bookmark = sr.Position + 4;
                sr.Seek(0x20 + sr.ReadBE<int>(), SeekOrigin.Begin);
                models.Add(new NJSObject(sr, NinjaVariant.Ginja, true, 0x20));
                sr.Seek(bookmark, SeekOrigin.Begin);
            }

            //Read Anims
            sr.Seek(0x20 + animsOffset, SeekOrigin.Begin);
            for (int i = 0; i < animsCount; i++)
            {
                var bookmark = sr.Position + 4;
                var offset = sr.ReadBE<int>();
                sr.Seek(0x20 + sr.ReadBE<int>(), SeekOrigin.Begin);
                motions.Add(new NJSMotion(sr, true, 0x20));
                sr.Seek(bookmark, SeekOrigin.Begin);
            }

            //Texture list
            sr.Seek(0x20 + texListsOffset, SeekOrigin.Begin);
            for (int i = 0; i < texLists_GvmsCount; i++)
            {
                var bookmark = sr.Position + 4;
                sr.Seek(0x20 + sr.ReadBE<int>(), SeekOrigin.Begin);
                texLists.Add(new NJTextureList(sr, 0x20));
                sr.Seek(bookmark, SeekOrigin.Begin);
            }

            //Texture Archive
            sr.Seek(0x20 + gvmsOffset, SeekOrigin.Begin);
            for (int i = 0; i < texLists_GvmsCount; i++)
            {
                var bookmark = sr.Position + 4;
                sr.Seek(0x20 + sr.ReadBE<int>(), SeekOrigin.Begin);
                gvms.Add(new PuyoFile(GVMUtil.ReadGVMBytes(sr)));
                sr.Seek(bookmark, SeekOrigin.Begin);
            }
        }
    }
}
