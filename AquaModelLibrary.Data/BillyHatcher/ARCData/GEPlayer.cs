using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Data.Ninja.Model;
using AquaModelLibrary.Data.Ninja.Motion;
using AquaModelLibrary.Helpers.Readers;
using ArchiveLib;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    //For a lot of these, it seems like they have pointers to -1 for many elements. This probably means that they take from earlier in the hierarchy.
    // Ex. ge_player.arc -> ge_player1.arc -> ge_player1n.arc
    public class GEPlayer : ARC
    {
        public PuyoFile gvm = null;
        public List<string> texnames = new List<string>();
        public NJSObject dFace = null;
        public List<NJSMotion> faceMotions = new List<NJSMotion>();
        public List<NJSMotion> faceShapeMotions = new List<NJSMotion>();
        public List<NJSMotion> leftHandMotions = new List<NJSMotion>();
        public List<NJSMotion> rightHandMotions = new List<NJSMotion>();
        public Dictionary<string, NJSMotion> motions = new Dictionary<string, NJSMotion>();
        public List<NJSObject> models = new List<NJSObject>();
        public GEPlayer() { }

        public GEPlayer(byte[] file)
        {
            Read(file);
        }

        public GEPlayer(BufferedStreamReaderBE<MemoryStream> sr)
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
                    case "d_face":
                        dFace = new NJSObject(sr, NinjaVariant.Ginja, true, 0x20);
                        break;
                    case "fa_mtn":
                        faceMotions = ReadMotionList(sr, 6, 0x20, 1);
                        break;
                    case "fa_sm":
                        faceShapeMotions = ReadMotionList(sr, 6, 0x20, 1);
                        break;
                    case "lh_mtn":
                        leftHandMotions = ReadMotionList(sr, 6, 0x20, 0x16);
                        break;
                    case "rh_mtn":
                        rightHandMotions = ReadMotionList(sr, 6, 0x20, 0x16);
                        break;
                    case "models":
                        List<int> modelPtrs = new List<int>();
                        for (int m = 0; m < 5; m++)
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
                            motions.Add(fileName, new NJSMotion(sr, true, 0x20, false, 0x15));
                        }
                        break;
                    case "texlists":
                        sr.Seek(0x20 + sr.ReadBE<int>(), SeekOrigin.Begin);
                        texnames = ReadTexNames(sr);
                        break;
                    case "textures":
                        sr.Seek(0x20 + sr.ReadBE<int>(), SeekOrigin.Begin);
                        gvm = new PuyoFile(GVMUtil.ReadGVMBytes(sr));
                        break;
                    default:
                        //For ge_player.arc, the anim's nodeCount is determined by the name prefix
                        var subType = "";
                        var splitName = fileName.Split('_');

                        if (splitName.Length > 2)
                        {
                            subType = splitName[1];
                        }

                        switch (subType)
                        {
                            case "lh":
                                motions.Add(fileName, new NJSMotion(sr, true, 0x20, false, 0x16));
                                break;
                            case "rh":
                                motions.Add(fileName, new NJSMotion(sr, true, 0x20, false, 0x16));
                                break;
                            default:
                                motions.Add(fileName, new NJSMotion(sr, true, 0x20, false, 0x4B));
                                break;
                        }
                        break;
                }
            }
            if (models.Count > 0)
            {
                nodeCount = 0;
                models[0].CountAnimated(ref nodeCount);
            }

            var group1Sort = group1FileReferences.Select(x => x.fileOffset).ToList();
            group1Sort.Sort();
            var group2Sort = group2FileReferences.Select(x => x.fileOffset).ToList();
            group2Sort.Sort();

            for (int i = 0; i < group2FileNames.Count; i++)
            {
                var fileName = group2FileNames[i];
                sr.Seek(0x20 + group2FileReferences[i].fileOffset, SeekOrigin.Begin);
                var motionOffset = sr.ReadBE<int>();
                if (motionOffset != -1)
                {
                    motions.Add(fileName, new NJSMotion(sr, true, 0x20, false, nodeCount));
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
