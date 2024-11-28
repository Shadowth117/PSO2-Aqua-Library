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

            if (collisionModelOffset > 0)
            {
                sr.Seek(collisionModelOffset + 0x20, SeekOrigin.Begin);
                List<int> collisionModelOffsets = new List<int>();
                for (int i = 0; i < collisionModelCount; i++)
                {
                    collisionModelOffsets.Add(sr.ReadBE<int>());
                }
                for (int i = 0; i < collisionModelOffsets.Count; i++)
                {
                    var offset = collisionModelOffsets[i];
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
                    string name = nameId > -1 ? $"{group1FileNames[nameId]}" : $"collision_{i}";
                    colModels.Add(name, new CSDY(sr, false, 0x20));
                }
            }

            if (modelsOffset2 > 0)
            {
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
            }

            if(motionsOffset2 > 0)
            {
                sr.Seek(motionsOffset2 + 0x20, SeekOrigin.Begin);
                List<int> motionOffset2s = new List<int>();
                for (int i = 0; i < motionCount2; i++)
                {
                    motionOffset2s.Add(sr.ReadBE<int>());
                }
                for (int i = 0; i < motionOffset2s.Count; i++)
                {
                    var offset = motionOffset2s[i];
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
                    string name = nameId > -1 ? $"{group1FileNames[nameId]}" : $"motion2_{i}";
                    motion2s.Add(name, new NJSMotion(sr, true, 0x20));
                }
            }

            if (textureListsOffset2 > 0)
            {
                sr.Seek(textureListsOffset2 + 0x20, SeekOrigin.Begin);
                List<int> textureListsOffset2s = new List<int>();
                for (int i = 0; i < textureListCount; i++)
                {
                    textureListsOffset2s.Add(sr.ReadBE<int>());
                }
                for (int i = 0; i < textureListsOffset2s.Count; i++)
                {
                    var offset = textureListsOffset2s[i];
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
                    string name = nameId > -1 ? $"{group1FileNames[nameId]}" : $"texList2_{i}";
                    texList2s.Add(name, new NJTextureList(sr, 0x20));
                }
            }

            if (collisionModelOffset2 > 0)
            {
                sr.Seek(collisionModelOffset2 + 0x20, SeekOrigin.Begin);
                List<int> collisionModelOffset2s = new List<int>();
                for (int i = 0; i < collisionModelCount2; i++)
                {
                    collisionModelOffset2s.Add(sr.ReadBE<int>());
                }
                for (int i = 0; i < collisionModelOffset2s.Count; i++)
                {
                    var offset = collisionModelOffset2s[i];
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
                    string name = nameId > -1 ? $"{group1FileNames[nameId]}" : $"collision_{i}";
                    colModel2s.Add(name, new CSDY(sr, false, 0x20));
                }
            }

            sr.Seek(0x20 + gvmOffset, SeekOrigin.Begin);
            gvm = new PuyoFile(GVMUtil.ReadGVMBytes(sr));
        }
    }
}
