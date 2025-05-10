using AquaModelLibrary.Data.BillyHatcher.Collision;
using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Data.Ninja.Model;
using AquaModelLibrary.Data.Ninja.Motion;
using AquaModelLibrary.Helpers.Readers;
using ArchiveLib;
using System.Diagnostics;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    //Billy Hatcher ani_model_*.arc files
    public class AniModel : ARC
    {
        public List<NJSObject> models = new List<NJSObject>();
        public List<NJSMotion> motions = new List<NJSMotion>();
        public List<float> unkFloatList = new List<float>();
        public List<BoundsXYZ> boundsList = new List<BoundsXYZ>();
        public List<NJSMotion> motion2s = new List<NJSMotion>();
        public NJTextureList texList = new NJTextureList();
        public PuyoFile gvm = null;
        public PolyAnim polyAnim = null;

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

                        //Read Animations
                        var animCount = (boundingAddress - animAddress) / 4;
                        List<int> animAddresses = new List<int>();
                        sr.Seek(0x20 + animAddress, SeekOrigin.Begin);
                        for (int j = 0; j < animCount; j++)
                        {
                            animAddresses.Add(sr.ReadBE<int>());
                        }
                        foreach (var animAddr in animAddresses)
                        {
                            if(animAddr != -1)
                            {
                                sr.Seek(0x20 + animAddr, SeekOrigin.Begin);
                                motions.Add(new NJSMotion(sr, true, 0x20));
                            } else
                            {
                                motions.Add(null);
                            }
                        }

                        //Read Bounding
                        var boundCount = (anim2Address - boundingAddress) / 4;
                        List<int> boundAddresses = new List<int>();
                        sr.Seek(0x20 + boundingAddress, SeekOrigin.Begin);
                        for (int j = 0; j < boundCount; j++)
                        {
                            boundAddresses.Add(sr.ReadBE<int>());
                        }
                        foreach (var boundAddr in boundAddresses)
                        {
                            sr.Seek(0x20 + boundAddr, SeekOrigin.Begin);
                            boundsList.Add(new BoundsXYZ() { Min = sr.ReadBEV3(), Max = sr.ReadBEV3()});
                        }

                        //Read Animation 2s
                        var nextAddress = unkFloatAddress > 0 ? unkFloatAddress : texListAddress;
                        var anim2Count = (nextAddress - anim2Address) / 4;
                        List<int> anim2Addresses = new List<int>();
                        sr.Seek(0x20 + anim2Address, SeekOrigin.Begin);
                        for (int j = 0; j < anim2Count; j++)
                        {
                            anim2Addresses.Add(sr.ReadBE<int>());
                        }
                        foreach (var animAddr in anim2Addresses)
                        {
                            sr.Seek(0x20 + animAddr, SeekOrigin.Begin);
                            motion2s.Add(new NJSMotion(sr, true, 0x20));
                        }

                        //Read Weird Floats
                        if(unkFloatAddress != 0)
                        {
                            var floatCount = (texListAddress - unkFloatAddress) / 4;
                            List<int> floatAddresses = new List<int>();
                            sr.Seek(0x20 + boundingAddress, SeekOrigin.Begin);
                            for (int j = 0; j < floatCount; j++)
                            {
                                floatAddresses.Add(sr.ReadBE<int>());
                            }
                            foreach (var floatAddr in floatAddresses)
                            {
                                sr.Seek(0x20 + floatAddr, SeekOrigin.Begin);
                                unkFloatList.Add(sr.ReadBE<float>());
                            }
                        }

                        //Read Texture data
                        sr.Seek(0x20 + texListAddress, SeekOrigin.Begin);
                        sr.Seek(0x20 + sr.ReadBE<int>(), SeekOrigin.Begin);
                        texList = new NJTextureList(sr, 0x20);

                        sr.Seek(0x20 + gvmAddress, SeekOrigin.Begin);
                        gvm = new PuyoFile(GVMUtil.ReadGVMBytes(sr));
                        break;
                    case "polyanim":
                        polyAnim = new PolyAnim(sr, 0x20);
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

        public byte[] GetBytes()
        {
            List<byte> outBytes = new List<byte>();



            return outBytes.ToArray();
        }
    }
}
