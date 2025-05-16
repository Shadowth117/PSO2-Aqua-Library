using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Data.Ninja.Model;
using AquaModelLibrary.Data.Ninja.Motion;
using AquaModelLibrary.Helpers.Readers;
using ArchiveLib;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    //gallery_egg.arc
    public class ArBluePresbyter
    {
        public ARCHeader header;
        public NJSObject model = null;
        public CameraAnchor cameraAnchorList;
        public List<NJSMotion> motions = new List<NJSMotion>();
        public NJSMotion camMotion = null;
        public NJTextureList texList = null;
        public PuyoFile gvm = null;

        public ArBluePresbyter() { }

        public ArBluePresbyter(byte[] file)
        {
            Read(file);
        }

        public ArBluePresbyter(BufferedStreamReaderBE<MemoryStream> sr)
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

            var modelOffset = sr.ReadBE<int>();
            var cameraAnchorOffset = sr.ReadBE<int>();
            List<int> motionOffsets = new List<int>();
            for(int i = 0; i < 7; i++)
            {
                motionOffsets.Add(sr.ReadBE<int>());
            }

            var camMotionOffset = sr.ReadBE<int>();
            var texListOffset = sr.ReadBE<int>();
            var texArrOffset = sr.ReadBE<int>();

            //Read Model
            sr.Seek(0x20 + modelOffset, SeekOrigin.Begin);
            model = new NJSObject(sr, NinjaVariant.Ginja, true, 0x20);
            model.CountAnimated(ref nodeCount);

            //Read Camera Anchor
            sr.Seek(0x20 + cameraAnchorOffset, SeekOrigin.Begin);
            cameraAnchorList = new CameraAnchor() { Min = sr.ReadBEV3(), Max = sr.ReadBEV3(), int_18 = sr.ReadBE<int>(), int_1C = sr.ReadBE<int>(), };

            //Read Anims
            for(int i = 0; i < motionOffsets.Count; i++)
            {
                sr.Seek(0x20 + motionOffsets[i], SeekOrigin.Begin);
                motions.Add(new NJSMotion(sr, true, 0x20));
            }

            //Read Camera Motion
            sr.Seek(0x20 + camMotionOffset, SeekOrigin.Begin);
            camMotion = new NJSMotion(sr, true, 0x20);

            //Texture list
            sr.Seek(0x20 + texListOffset, SeekOrigin.Begin);
            texList = new NJTextureList(sr, 0x20);

            //Texture Archive
            sr.Seek(0x20 + texArrOffset, SeekOrigin.Begin);
            gvm = new PuyoFile(GVMUtil.ReadGVMBytes(sr));
        }


    }
}
