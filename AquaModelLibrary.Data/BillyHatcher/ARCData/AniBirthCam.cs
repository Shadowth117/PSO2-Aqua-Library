using AquaModelLibrary.Data.BillyHatcher.Collision;
using AquaModelLibrary.Data.Ninja.Motion;
using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    /// <summary>
    /// Only for ani_birth_cam.arc, an unused file in the pc release
    /// </summary>
    public class AniBirthCam : ARC
    {
        public List<BoundsXYZ> boundsXYZs = new List<BoundsXYZ>();
        public List<NJSMotion> camStartMotions = new List<NJSMotion>();
        public List<NJSMotion> camEndMotions = new List<NJSMotion>();
        public AniBirthCam() { }
        public AniBirthCam(byte[] file)
        {
            Read(file);
        }

        public AniBirthCam(BufferedStreamReaderBE<MemoryStream> sr)
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
            sr._BEReadActive = true;
            base.Read(sr);
            sr.Seek(0x20, SeekOrigin.Begin);

            var boundsListOffset = sr.ReadBE<int>();
            var startMotionListOffset = sr.ReadBE<int>();
            var endMotionListOffset = sr.ReadBE<int>();
            List<int> boundsOffsets = new List<int>();
            for (int i = 0; i < 2; i++)
            {
                boundsOffsets.Add(sr.ReadBE<int>());
            }
            List<int> startMotionOffsets = new List<int>();
            for (int i = 0; i < 2; i++)
            {
                startMotionOffsets.Add(sr.ReadBE<int>());
            }
            List<int> endMotionOffsets = new List<int>();
            for (int i = 0; i < 2; i++)
            {
                endMotionOffsets.Add(sr.ReadBE<int>());
            }
            for (int i = 0; i < boundsOffsets.Count; i++)
            {
                sr.Seek(0x20 + boundsOffsets[i], SeekOrigin.Begin);
                var bounds = new BoundsXYZ();
                bounds.Min = sr.ReadBEV3();
                bounds.Max = sr.ReadBEV3();
                boundsXYZs.Add(bounds);
            }
            for (int i = 0; i < startMotionOffsets.Count; i++)
            {
                sr.Seek(0x20 + startMotionOffsets[i], SeekOrigin.Begin);
                camStartMotions.Add(new NJSMotion(sr, true, 0x20));
            }
            for (int i = 0; i < endMotionOffsets.Count; i++)
            {
                sr.Seek(0x20 + endMotionOffsets[i], SeekOrigin.Begin);
                camEndMotions.Add(new NJSMotion(sr, true, 0x20));
            }
        }
    }
}
