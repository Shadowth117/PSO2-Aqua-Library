using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Data.Ninja.Model;
using AquaModelLibrary.Helpers.Readers;
using ArchiveLib;
using System.Diagnostics;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    //Billy Hatcher ani_model_*.arc files
    public class AniModel : ARC
    {
        public List<NJSObject> models = new List<NJSObject>();
        public NJTextureList texList = new NJTextureList();
        public PuyoFile gvm = null;

        public AniModel() { }

        public AniModel(byte[] file)
        {
            Read(file);
        }

        public AniModel(BufferedStreamReaderBE<MemoryStream> sr)
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
            base.Read(sr);
            int nodeCount = 0;
            for (int i = 0; i < group1FileNames.Count; i++)
            {
                var fileName = group1FileNames[i];
                sr.Seek(0x20 + group1FileReferences[i].fileOffset, SeekOrigin.Begin);
                switch (fileName)
                {
                    case "AniModel":
                        var modelAddress = sr.ReadBE<int>();
                        var animAddress = sr.ReadBE<int>();
                        var boundingAddress = sr.ReadBE<int>();
                        var anim2Address = sr.ReadBE<int>();
                        var unkFloatAddress = sr.ReadBE<int>();
                        var texListAddress = sr.ReadBE<int>();
                        var gvmAddress = sr.ReadBE<int>();

                        //Read models
                        var modelCount = (animAddress - modelAddress) / 4;
                        List<int> modelAddresses = new List<int>();
                        sr.Seek(0x20 + modelAddress, SeekOrigin.Begin);
                        for(int j = 0; j < modelCount; j++)
                        {
                            modelAddresses.Add(sr.ReadBE<int>());
                        }
                        foreach(var modelAddr in modelAddresses)
                        {
                            sr.Seek(0x20 + modelAddr, SeekOrigin.Begin);
                            models.Add(new NJSObject(sr, NinjaVariant.Ginja, sr._BEReadActive, 0x20));
                        }

                        //Read Texture data
                        sr.Seek(0x20 + texListAddress, SeekOrigin.Begin);
                        sr.Seek(0x20 + sr.ReadBE<int>(), SeekOrigin.Begin);
                        texList = new NJTextureList(sr, 0x20);

                        sr.Seek(0x20 + gvmAddress, SeekOrigin.Begin);
                        gvm = new PuyoFile(GVMUtil.ReadGVMBytes(sr));
                        break;
                    case "polyanim":
                        
                        break;
                    default:
                        Debug.WriteLine($"Ignoring {fileName}");
                        break;
                }
            }
            if (models.Count > 0)
            {
                nodeCount = 0;
                models[0].CountAnimated(ref nodeCount);
            }
        }
    }
}
