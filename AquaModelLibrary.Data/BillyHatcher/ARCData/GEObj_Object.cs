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
    public class GEObj_Object : ARC
    {
        public static string[] archiveFilenames = new string[] { "geobj_cage.arc", "geobj_chicken.arc", "geobj_darkgate.arc", "geobj_emblem.arc", "geobj_goal.arc", "geobj_lightgate.arc", 
            "geobj_lightgate_blue.arc", "geobj_lightgate_green.arc", "geobj_lightgate_orange.arc", "geobj_lightgate_purple.arc", "geobj_lightgate_red.arc", "geobj_lightgate_yellow.arc", 
            "geobj_mg_leader.arc", "geobj_orange_cannon.arc", "geobj_ring.arc" };

        public Dictionary<string, NJSObject> models = new Dictionary<string, NJSObject>();
        public Dictionary<string, NJSMotion> motions = new Dictionary<string, NJSMotion>();
        public Dictionary<string, NJTextureList> texLists = new Dictionary<string, NJTextureList>();
        public PuyoFile gvm = null;
        public GEObj_Object(byte[] file)
        {
            Read(file);
        }

        public GEObj_Object(BufferedStreamReaderBE<MemoryStream> sr)
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
            var modelsOffset = sr.ReadBE<int>();
            int motionsOffset = -1;
            if (modelsOffset > 0xC)
            {
                motionsOffset = sr.ReadBE<int>();
            }
            int textureListsOffset = sr.ReadBE<int>();
            var gvmOffset = sr.ReadBE<int>();

            List<int> offsetQueue = new List<int>() { motionsOffset, textureListsOffset, gvmOffset };

            int modelCount = 1;
            offsetQueue.RemoveAt(0);
            int motionCount = HelperFunctions.GetCount(motionsOffset, offsetQueue);
            offsetQueue.RemoveAt(0);
            int textureListCount = HelperFunctions.GetCount(textureListsOffset, offsetQueue);

            if(modelsOffset > 0)
            {
                sr.Seek(modelsOffset + 0x20, SeekOrigin.Begin);
                List<int> modelOffsets = new List<int>();
                for (int i = 0; i < modelCount; i++)
                {
                    modelOffsets.Add(sr.ReadBE<int>());
                }
                for (int i = 0; i < modelOffsets.Count; i++)
                {
                    var offset = modelOffsets[i];
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
                    string name = nameId > -1 ? $"{group1FileNames[nameId]}" : $"model_{i}";
                    models.Add(name, new NJSObject(sr, NinjaVariant.Ginja, true, 0x20));
                }
            }

            if(motionsOffset > 0)
            {
                sr.Seek(motionsOffset + 0x20, SeekOrigin.Begin);
                List<int> motionOffsets = new List<int>();
                for (int i = 0; i < motionCount; i++)
                {
                    motionOffsets.Add(sr.ReadBE<int>());
                }
                for (int i = 0; i < motionOffsets.Count; i++)
                {
                    var offset = motionOffsets[i];
                    if (offset == 0)
                    {
                        break;
                    }
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
                    string name = nameId > -1 ? $"{group1FileNames[nameId]}" : $"motion_{i}";
                    motions.Add(name, new NJSMotion(sr, true, 0x20));
                }
            }

            if(textureListsOffset > 0)
            {
                sr.Seek(textureListsOffset + 0x20, SeekOrigin.Begin);
                List<int> textureListsOffsets = new List<int>();
                for (int i = 0; i < textureListCount; i++)
                {
                    textureListsOffsets.Add(sr.ReadBE<int>());
                }
                for (int i = 0; i < textureListsOffsets.Count; i++)
                {
                    var offset = textureListsOffsets[i];
                    if (offset == 0)
                    {
                        break;
                    }

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
                    string name = nameId > -1 ? $"{group1FileNames[nameId]}" : $"texList_{i}";
                    texLists.Add(name, new NJTextureList(sr, 0x20));
                }
            }

            sr.Seek(0x20 + gvmOffset, SeekOrigin.Begin);
            gvm = new PuyoFile(GVMUtil.ReadGVMBytes(sr));
        }
    }
}
