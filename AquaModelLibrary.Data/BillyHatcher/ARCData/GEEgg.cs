using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Data.Ninja.Model;
using AquaModelLibrary.Data.Ninja.Motion;
using AquaModelLibrary.Helpers.Readers;
using ArchiveLib;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    public class GEEGG : ARC
    {
        public PuyoFile gvm = null;
        public List<NJTextureList> texnamesList = new List<NJTextureList>();
        public Dictionary<string, NJSMotion> motions = new Dictionary<string, NJSMotion>();
        public List<NJSObject> models = new List<NJSObject>();
        public GEEGG() { }

        public GEEGG(byte[] file)
        {
            Read(file);
        }

        public GEEGG(BufferedStreamReaderBE<MemoryStream> sr)
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
            int nodeCount = 0x4B;
            for (int i = 0; i < group1FileNames.Count; i++)
            {
                var fileName = group1FileNames[i];
                sr.Seek(0x20 + group1FileReferences[i].fileOffset, SeekOrigin.Begin);
                switch (fileName)
                {
                    case "models":
                        List<int> modelPtrs = new List<int>();
                        for (int m = 0; m < 7; m++)
                        {
                            modelPtrs.Add(sr.ReadBE<int>());
                        }
                        foreach (var ptr in modelPtrs)
                        {
                            if (ptr != -1)
                            {
                                sr.Seek(0x20 + ptr, SeekOrigin.Begin);
                                models.Add(new NJSObject(sr, NinjaVariant.Ginja, true, 0x20));
                            }
                        }
                        break;
                    case "motions":
                        //Should only be filled as a motion in ge_player with a node count of 0x15
                        var motionPtr = sr.ReadBE<int>();
                        if (motionPtr != -1)
                        {
                            sr.Seek(0x20 + motionPtr, SeekOrigin.Begin);
                            nodeCount = 0;
                            models[0].CountAnimated(ref nodeCount);
                            motions.Add("motions0", new NJSMotion(sr, true, 0x20, false, nodeCount));
                            sr.Seek(0x20 + motionPtr + 4, SeekOrigin.Begin);
                            motions.Add("motions1", new NJSMotion(sr, true, 0x20, false, nodeCount));
                        }
                        break;
                    case "texlists":
                        List<int> texListOffsets = new List<int>();
                        for(int t = 0; t < 7; t++)
                        {
                            texListOffsets.Add(sr.ReadBE<int>());
                        }
                        for(int t = 0; t < 7; t++)
                        {
                            sr.Seek(0x20 + texListOffsets[t], SeekOrigin.Begin);
                            texnamesList.Add(new NJTextureList(sr, 0x20));
                        }
                        break;
                    case "textures":
                        sr.Seek(0x20 + sr.ReadBE<int>(), SeekOrigin.Begin);
                        gvm = new PuyoFile(GVMUtil.ReadGVMBytes(sr));
                        break;
                }
            }
        }

        public static List<NJSMotion> ReadMotionList(BufferedStreamReaderBE<MemoryStream> sr, int motionCount, int offset = 0x20, int nodeCount = 1)
        {
            List<NJSMotion> motList = new List<NJSMotion>();
            int[] motPtrArr = new int[motionCount];
            for (int i = 0; i < motionCount; i++)
            {
                motPtrArr[i] = sr.ReadBE<int>();
            }
            foreach (var ptr in motPtrArr)
            {
                if(ptr != -1)
                {
                    sr.Seek(offset + ptr, SeekOrigin.Begin);
                    motList.Add(new NJSMotion(sr, true, offset, false, nodeCount));
                } else
                {
                    motList.Add(null);
                }
            }

            return motList;
        }
    }
}
