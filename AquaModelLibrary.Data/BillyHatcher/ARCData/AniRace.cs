using AquaModelLibrary.Data.Ninja.Motion;
using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    /// <summary>
    /// ani_race_red.arc
    /// </summary>
    public class AniRace : ARC
    {
        public ARCHeader header;
        public NJSMotion anim = null;
        public float flt_final = 0;

        public AniRace() { }

        public AniRace(byte[] file)
        {
            Read(file);
        }

        public AniRace(BufferedStreamReaderBE<MemoryStream> sr)
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
            sr._BEReadActive = true;
            anim = new NJSMotion(sr, true, 0x20, true, nodeCount);

            sr._BEReadActive = false;
        }

    }
}
