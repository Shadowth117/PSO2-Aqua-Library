using AquaModelLibrary.Data.BillyHatcher.Collision;
using AquaModelLibrary.Data.Ninja.Motion;
using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    /// <summary>
    /// Used for all ar_event_cam_*.arc files
    /// </summary>
    public class ArEventCam : ARC
    {
        public int int_00;
        public List<BoundsXYZ> boundsXYZs = new List<BoundsXYZ>();
        public List<NJSMotion> camMotions = new List<NJSMotion>();
        public ArEventCam() { }
        public ArEventCam(byte[] file)
        {
            Read(file);
        }

        public ArEventCam(BufferedStreamReaderBE<MemoryStream> sr)
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

            int_00 = sr.ReadBE<int>();
            var boundsListOffset = sr.ReadBE<int>();
            var motionListOffset = sr.ReadBE<int>();
            var count = (motionListOffset - boundsListOffset) / 4;
            List<int> boundsOffsets = new List<int>();
            for (int i = 0; i < count; i++)
            {
                boundsOffsets.Add(sr.ReadBE<int>());
            }
            List<int> motionsOffsets = new List<int>();
            for (int i = 0; i < count; i++)
            {
                motionsOffsets.Add(sr.ReadBE<int>());
            }
            for (int i = 0; i < boundsOffsets.Count; i++)
            {
                sr.Seek(0x20 + boundsOffsets[i], SeekOrigin.Begin);
                var bounds = new BoundsXYZ();
                bounds.Min = sr.ReadBEV3();
                bounds.Max = sr.ReadBEV3();
                boundsXYZs.Add(bounds);
            }
            for (int i = 0; i < motionsOffsets.Count; i++)
            {
                sr.Seek(0x20 + motionsOffsets[i], SeekOrigin.Begin);
                camMotions.Add(new NJSMotion(sr, true, 0x20));
            }
        }
    }
}
