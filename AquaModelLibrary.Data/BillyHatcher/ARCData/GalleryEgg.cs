using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Data.Ninja.Model;
using AquaModelLibrary.Data.Ninja.Motion;
using AquaModelLibrary.Helpers.Extensions;
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

        public byte[] GetBytes()
        {
            ByteListExtension.AddAsBigEndian = true;
            List<int> pofSets = new List<int>();
            List<byte> outBytes = new List<byte>();

            if(model != null)
            {
                pofSets.Add(0x4);
            }
            if(anim != null)
            {
                pofSets.Add(0x8);
            }
            if (texLists != null)
            {
                pofSets.Add(0xC);
            }
            if (texArchives != null)
            {
                pofSets.Add(0x10);
            }
            outBytes.AddValue(texArchives.Count);
            outBytes.ReserveInt("ModelOffset");
            outBytes.ReserveInt("MotionOffset");
            outBytes.ReserveInt("TextureListsOffsetOffset");
            outBytes.ReserveInt("TextureArchivesOffsetOffset");
            outBytes.AlignWriter(0x20);

            outBytes.FillInt("ModelOffset", outBytes.Count);
            model.Write(outBytes, pofSets, true);

            outBytes.FillInt("MotionOffset", outBytes.Count);
            anim.Write(outBytes, pofSets, NJSMotion.MotionWriteMode.BillyMode);

            outBytes.FillInt("TextureListsOffsetOffset", outBytes.Count);
            for(int i = 0; i < texLists.Count; i++)
            {
                pofSets.Add(outBytes.Count);
                outBytes.ReserveInt($"TexList{i}Offset");
            }
            outBytes.FillInt("TextureArchivesOffsetOffset", outBytes.Count);
            for (int i = 0; i < texArchives.Count; i++)
            {
                pofSets.Add(outBytes.Count);
                outBytes.ReserveInt($"Tex{i}Offset");
            }
            outBytes.AlignWriter(0x20);
            for (int i = 0; i < texLists.Count; i++)
            {
                outBytes.FillInt($"TexList{i}Offset", outBytes.Count);
                texLists[i].Write(outBytes, pofSets);
                outBytes.AlignWriter(0x10);
            }
            outBytes.AlignWriter(0x20);

            for (int i = 0; i < texArchives.Count; i++)
            {
                outBytes.AlignWriter(0x20);
                outBytes.FillInt($"Tex{i}Offset", outBytes.Count);
                outBytes.AddRange(texArchives[i].GetBytes());
            }

            outBytes.AlignWriter(0x4);
            var pof0Offset = outBytes.Count;
            pofSets.Sort();
            var pof0 = POF0.GenerateRawPOF0(pofSets, true);
            outBytes.AddRange(pof0);

            var arcBytes = new List<byte>();
            arcBytes.AddValue(outBytes.Count + 0x20);
            arcBytes.AddValue(pof0Offset);
            arcBytes.AddValue(pof0.Length);
            arcBytes.AddValue(0);

            arcBytes.AddValue(0);
            arcBytes.Add(0x30);
            arcBytes.Add(0x31);
            arcBytes.Add(0x30);
            arcBytes.Add(0x30);
            arcBytes.AddValue(0);
            arcBytes.AddValue(0);

            outBytes.InsertRange(0, arcBytes);

            ByteListExtension.Reset();
            return outBytes.ToArray();
        }

    }
}
