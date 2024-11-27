using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Data.Ninja.Model;
using AquaModelLibrary.Data.Ninja.Motion;
using AquaModelLibrary.Helpers.Readers;
using ArchiveLib;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    /// <summary>
    /// Container for Billy Hatcher stage and common object asset data. 
    /// </summary>
    public class GEObj_Stage : ARC
    {
        public static string[] archiveFilenames = new string[] { "geobj_black.arc", "geobj_blue.arc", "geobj_common.arc", "geobj_green.arc", "geobj_orange.arc", "geobj_purple.arc", "geobj_red.arc", "geobj_yellow.arc" };

        public Dictionary<string, NJSObject> models = new Dictionary<string, NJSObject>();
        public Dictionary<string, NJSMotion> motions = new Dictionary<string, NJSMotion>();
        public Dictionary<string, NJTextureList> texLists = new Dictionary<string, NJTextureList>();
        public Dictionary<string, CSDY> colModels = new Dictionary<string, CSDY>();
        public Dictionary<string, NJSObject> model2s = new Dictionary<string, NJSObject>();
        public Dictionary<string, NJSMotion> motion2s = new Dictionary<string, NJSMotion>();
        public Dictionary<string, NJTextureList> texList2s = new Dictionary<string, NJTextureList>();
        public Dictionary<string, CSDY> colModel2s = new Dictionary<string, CSDY>();
        public PuyoFile gvm = null;
        public GEObj_Stage(byte[] file)
        {
            Read(file);
        }

        public GEObj_Stage(BufferedStreamReaderBE<MemoryStream> sr)
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
            //If any of the areas are named d + whatever type (dmodels, dmotions etc.), it signifies a null pointer

            var modelCount = sr.ReadBE<ushort>();
            var motionCount = sr.ReadBE<ushort>();
            var textureListCount = sr.ReadBE<ushort>();
            var collisionModelCount = sr.ReadBE<ushort>();
            
            var modelsOffset = sr.ReadBE<int>();
            var motionsOffset = sr.ReadBE<int>();
            var textureListsOffset = sr.ReadBE<int>();
            var collisionModelOffset = sr.ReadBE<int>();

            var modelCount2 = sr.ReadBE<ushort>();
            var motionCount2 = sr.ReadBE<ushort>();
            var textureListCount2 = sr.ReadBE<ushort>();
            var collisionModelCount2 = sr.ReadBE<ushort>();

            var modelsOffset2 = sr.ReadBE<int>();
            var motionsOffset2 = sr.ReadBE<int>();
            var textureListsOffset2 = sr.ReadBE<int>();
            var collisionModelOffset2 = sr.ReadBE<int>();

            var gvmOffset = sr.ReadBE<int>();

            sr.Seek(modelsOffset + 0x20, SeekOrigin.Begin);
            List<int> modelOffsets = new List<int>();
            for(int i = 0; i < modelCount; i++)
            {
                modelOffsets.Add(sr.ReadBE<int>());
            }
            for(int i = 0; i < modelOffsets.Count; i++)
            {
                var offset = modelOffsets[i];
                sr.Seek(offset + 0x20, SeekOrigin.Begin);
                int nameId = -1;
                for(int j = 0; j < group1FileReferences.Count; j++)
                {
                    if (group1FileReferences[j].fileOffset == offset)
                    {
                        nameId = j;
                        break;
                    }
                }
                string name = nameId > -1 ? $"{group1FileNames[nameId]}" : $"model_{i}";
                models.Add(name, new NJSObject(sr, NinjaVariant.Ginja, true, 0x20));
            }

            sr.Seek(modelsOffset2 + 0x20, SeekOrigin.Begin);
            List<int> modelOffsets2 = new List<int>();
            for (int i = 0; i < modelCount2; i++)
            {
                modelOffsets2.Add(sr.ReadBE<int>());
            }
            for (int i = 0; i < modelOffsets2.Count; i++)
            {
                var offset = modelOffsets2[i];
                sr.Seek(offset + 0x20, SeekOrigin.Begin);
                int nameId = -1;
                for (int j = 0; j < group1FileReferences.Count; j++)
                {
                    if (group1FileReferences[j].fileOffset == offset)
                    {
                        nameId = j;
                        break;
                    }
                }
                string name = nameId > -1 ? $"{group1FileNames[nameId]}" : $"model2_{i}";
                model2s.Add(name, new NJSObject(sr, NinjaVariant.Ginja, true, 0x20));
            }

            sr.Seek(0x20 + gvmOffset, SeekOrigin.Begin);
            gvm = new PuyoFile(GVMUtil.ReadGVMBytes(sr));
        }
    }
}
