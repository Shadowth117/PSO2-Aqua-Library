using AquaModelLibrary.Data.Ninja.Motion;
using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    public class ArEventPresByter : ARC
    {
        public List<NJSMotion> motions = new List<NJSMotion>();
        public List<NJSMotion> eventCamMotions = new List<NJSMotion>();
        public ArEventPresByter() { }
        public ArEventPresByter(byte[] file)
        {
            Read(file);
        }

        public ArEventPresByter(BufferedStreamReaderBE<MemoryStream> sr)
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

            List<int> motionsOffsets = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                motionsOffsets.Add(sr.ReadBE<int>());
            }
            List<int> camOffsets = new List<int>();
            for (int i = 0; i < 2; i++)
            {
                camOffsets.Add(sr.ReadBE<int>());
            }
            for (int i = 0; i < motionsOffsets.Count; i++)
            {
                sr.Seek(0x20 + motionsOffsets[i], SeekOrigin.Begin);
                motions.Add(new NJSMotion(sr, true, 0x20));
            }
            for (int i = 0; i < camOffsets.Count; i++)
            {
                sr.Seek(0x20 + camOffsets[i], SeekOrigin.Begin);
                eventCamMotions.Add(new NJSMotion(sr, true, 0x20));
            }
        }
    }
}
