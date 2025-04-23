using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BillyHatcher
{
    public class PAD : ARC
    {
        public PolyAnim polyAnim = null;
        public PAD() { }
        public PAD(byte[] bytes)
        {
            Read(bytes);
        }

        public PAD(BufferedStreamReaderBE<MemoryStream> sr)
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
            sr.Seek(0x20, SeekOrigin.Begin);
            polyAnim = new PolyAnim(sr, 0x20);
        }

    }
}
