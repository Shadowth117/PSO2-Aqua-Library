using AquaModelLibrary.Data.BillyHatcher.Collision;
using AquaModelLibrary.Data.Ninja.Motion;
using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    /// <summary>
    /// Only for ar_cam_red_boss.arc, an unused file in the pc release
    /// </summary>
    public class ArCam : ARC
    {
        public List<BoundsXYZ> boundsXYZs = new List<BoundsXYZ>();
        public List<NJSMotion> camMotions = new List<NJSMotion>();
        public ArCam() { }
        public ArCam(byte[] file)
        {
            Read(file);
        }

        public ArCam(BufferedStreamReaderBE<MemoryStream> sr)
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

            var boundsListOffset = sr.ReadBE<int>();
            var motionListOffset = sr.ReadBE<int>();
            List<int> boundsOffsets = new List<int>();
            for(int i = 0; i < 2; i++)
            {
                boundsOffsets.Add(sr.ReadBE<int>());
            }
            List<int> motionsOffsets = new List<int>();
            for (int i = 0; i < 2; i++)
            {
                motionsOffsets.Add(sr.ReadBE<int>());
            }
            for(int i = 0; i < boundsOffsets.Count; i++)
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
