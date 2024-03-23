using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Data.Ninja.Model;
using AquaModelLibrary.Data.Ninja.Motion;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using ArchiveLib;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    //Billy Hatcher item_*.arc, lib_model_*.arc files
    //Ex. item_comb_fire.arc is identical to lib_model_01.arc
    public class ItemLibModel
    {
        public ARCHeader header;
        public NJSObject model = null;
        public NJSMotion anim = null;
        public NJTextureList texList = new NJTextureList();
        public PuyoFile gvm = null;
        public List<ItemLibBounding> boundingList = new List<ItemLibBounding>();
        public List<ItemLibEffectReference> effectList = new List<ItemLibEffectReference>();

        public ItemLibModel() { }

        public ItemLibModel(byte[] file)
        {
            Read(file);
        }

        public ItemLibModel(BufferedStreamReaderBE<MemoryStream> sr)
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
            int nodeCount = 0;

            int modelOffsetOffset = sr.ReadBE<int>();
            int animOffsetOffset = sr.ReadBE<int>();
            int texListOffset = sr.ReadBE<int>();
            int texOffset = sr.ReadBE<int>();

            int boundingCount = sr.ReadBE<int>();
            int boundingOffset = sr.ReadBE<int>();
            int effectRefCount = sr.ReadBE<int>();  //Maybe, this is a guess. 0ed on things that don't use effects
            int effectRefOffset = sr.ReadBE<int>();

            sr.Seek(0x20 + modelOffsetOffset, SeekOrigin.Begin);
            sr.Seek(0x20 + sr.ReadBE<int>(), SeekOrigin.Begin);
            model = new NJSObject(sr, NinjaVariant.Ginja, true, 0x20);
            model.CountAnimated(ref nodeCount);

            sr.Seek(0x20 + animOffsetOffset, SeekOrigin.Begin);
            sr.Seek(0x20 + sr.ReadBE<int>(), SeekOrigin.Begin);
            anim = new NJSMotion(sr, true, 0x20, true,  nodeCount);

            sr.Seek(0x20 + texListOffset, SeekOrigin.Begin);
            texList = new NJTextureList(sr, 0x20);

            sr.Seek(0x20 + texOffset, SeekOrigin.Begin);
            gvm = new PuyoFile(GVMUtil.ReadGVMBytes(sr));
            if(boundingOffset != 0)
            {
                sr.Seek(0x20 + boundingOffset, SeekOrigin.Begin);
                boundingList = new List<ItemLibBounding>();
                for (int b = 0; b < boundingCount; b++)
                {
                    ItemLibBounding bounding = new ItemLibBounding();
                    bounding.unk0 = sr.ReadBE<float>();
                    bounding.unk1 = sr.ReadBE<float>();
                    bounding.unk2 = sr.ReadBE<float>();
                    bounding.radius = sr.ReadBE<float>();
                    bounding.unkOne = sr.ReadBE<float>();
                    bounding.unk3 = sr.ReadBE<float>();
                    bounding.unk4 = sr.ReadBE<float>();
                    boundingList.Add(bounding);
                }
            }

            if (effectRefOffset != 0)
            {
                sr.Seek(0x20 + effectRefOffset, SeekOrigin.Begin);
                effectList = new List<ItemLibEffectReference>();
                for (int b = 0; b < effectRefCount; b++)
                {
                    var eff = new ItemLibEffectReference();
                    eff.unkSht0 = sr.ReadBE<short>();
                    eff.unkBt0 = sr.ReadBE<byte>();
                    eff.unkBt1 = sr.ReadBE<byte>();
                    effectList.Add(eff);
                }
            }
        }

        public byte[] GetBytes()
        {
            if(boundingList == null || boundingList.Count == 0)
            {
                boundingList = new List<ItemLibBounding>() { new ItemLibBounding() };
            }
            List<int> pofSets = new List<int>();
            if(model != null)
            {
                pofSets.Add(0x0);
            }
            if (anim != null)
            {
                pofSets.Add(0x4);
            }
            if (texList.texNames.Count > 0)
            {
                pofSets.Add(0x8);
            }
            if (gvm != null)
            {
                pofSets.Add(0xC);
            }
            if (boundingList.Count > 0)
            {
                pofSets.Add(0x14);
            }
            if (effectList.Count > 0)
            {
                pofSets.Add(0x1C);
            }
            List<byte> outBytes = new List<byte>();
            outBytes.ReserveInt("ModelOffsetOffset");
            outBytes.ReserveInt("AnimOffsetOffset");
            outBytes.ReserveInt("TexListOffset");
            outBytes.ReserveInt("TexOffset");

            outBytes.AddValue(boundingList.Count);
            outBytes.ReserveInt("BoundingOffset");
            outBytes.AddValue(effectList.Count);
            outBytes.ReserveInt("EffectRefOffset");

            if(model != null)
            {
                outBytes.FillInt("ModelOffsetOffset", outBytes.Count);
                pofSets.Add(outBytes.Count);
            }
            outBytes.ReserveInt("ModelOffset");
            if (anim != null)
            {
                outBytes.FillInt("AnimOffsetOffset", outBytes.Count);
                pofSets.Add(outBytes.Count);
            }
            outBytes.ReserveInt("AnimOffset");
            outBytes.AlignWriter(0x20);

            if (model != null)
            {
                outBytes.FillInt("ModelOffset", outBytes.Count);
                model.Write(outBytes, pofSets, true);
            }
            outBytes.AlignWriter(0x4);
            if (anim != null)
            {
                outBytes.FillInt("AnimOffset", outBytes.Count);
                anim.Write(outBytes, pofSets, NJSMotion.MotionWriteMode.BillyMode);
            }
            outBytes.AlignWriter(0x4);
            if (texList != null)
            {
                outBytes.FillInt("TexListOffset", outBytes.Count);
                texList.Write(outBytes, pofSets);
            }
            outBytes.AlignWriter(0x20);
            if (gvm != null)
            {
                outBytes.FillInt("TexOffset", outBytes.Count);
                outBytes.AddRange(gvm.GetBytes());
            }
            outBytes.AlignWriter(0x4);
            if (boundingList?.Count > 0)
            {
                outBytes.FillInt("BoundingOffset", outBytes.Count);
                foreach(var bounding in boundingList)
                {
                    outBytes.AddValue(bounding.unk0);
                    outBytes.AddValue(bounding.unk1);
                    outBytes.AddValue(bounding.unk2);
                    outBytes.AddValue(bounding.radius);
                    outBytes.AddValue(bounding.unkOne);
                    outBytes.AddValue(bounding.unk3);
                    outBytes.AddValue(bounding.unk4);
                }
            }
            outBytes.AlignWriter(0x4);
            if (boundingList?.Count > 0)
            {
                outBytes.FillInt("EffectRefOffset", outBytes.Count);
                foreach (var eff in effectList)
                {
                    outBytes.AddValue(eff.unkSht0);
                    outBytes.Add(eff.unkBt0);
                    outBytes.Add(eff.unkBt1);
                }
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

            return outBytes.ToArray();
        }
    }
}
