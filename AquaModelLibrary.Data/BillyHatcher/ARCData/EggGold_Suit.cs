using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Data.Ninja.Model;
using AquaModelLibrary.Data.Ninja.Motion;
using AquaModelLibrary.Helpers.Readers;
using ArchiveLib;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    /// <summary>
    /// Used for egg_gold.arc and egg_suit.arc
    /// </summary>
    public class EggGold_Suit : ARC
    {

        public List<NJSObject> models = new List<NJSObject>();
        public List<NJTextureList> texLists = new List<NJTextureList>();
        public PuyoFile gvm = null;
        public CameraAnchor? cameraAnchor = null;
        public NJSMotion cameraMotion = null;
        public List<NJSMotion> motions = new List<NJSMotion>();

        public EggGold_Suit() { }

        public EggGold_Suit(byte[] file)
        {
            Read(file);
        }

        public EggGold_Suit(BufferedStreamReaderBE<MemoryStream> sr)
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

            //Read Models
            for(int i = 0; i < 2; i++)
            {
                var modelOffset = sr.ReadBE<int>();
                var bookmark = sr.Position;
                sr.Seek(modelOffset + 0x20, SeekOrigin.Begin);
                models.Add(new NJSObject(sr, NinjaVariant.Ginja, true, 0x20));
                sr.Seek(bookmark, SeekOrigin.Begin);
            }

            //Read Texlists
            for (int i = 0; i < 3; i++)
            {
                var offset = sr.ReadBE<int>();
                var bookmark = sr.Position;
                sr.Seek(offset + 0x20, SeekOrigin.Begin);
                texLists.Add(new NJTextureList(sr, 0x20));

                sr.Seek(bookmark, SeekOrigin.Begin);
            }

            //Read Textures
            sr.Seek(sr.ReadBE<int>() + 0x20, SeekOrigin.Begin);
            gvm = new PuyoFile(GVMUtil.ReadGVMBytes(sr));
            
            //Read Event, if it's there
            if(group1FileNames.Contains("event"))
            {
                var eventRef = group1FileReferences[0];
                sr.Seek(0x20 + eventRef.fileOffset, SeekOrigin.Begin);
                var anchorOffset = sr.ReadBE<int>();
                var cameraMotionOffset = sr.ReadBE<int>();
                List<int> motionOffsets = new List<int>();
                for(int i = 0; i < 9; i++)
                {
                    motionOffsets.Add(sr.ReadBE<int>());
                }

                sr.Seek(anchorOffset + 0x20, SeekOrigin.Begin);
                cameraAnchor = new CameraAnchor() { Min = sr.ReadBEV3(), Max = sr.ReadBEV3(), int_18 = sr.ReadBE<int>(), int_1C = sr.ReadBE<int>() };

                sr.Seek(cameraMotionOffset + 0x20, SeekOrigin.Begin);
                cameraMotion = new NJSMotion(sr, true, 0x20);

                foreach(var offset in motionOffsets)
                {
                    sr.Seek(offset + 0x20, SeekOrigin.Begin);
                    motions.Add(new NJSMotion(sr, true, 0x20));
                }
            }
        }
    }
}
