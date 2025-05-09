using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Helpers.Extensions;
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

        public byte[] GetBytes()
        {
            List<int> pofSets = new List<int>();
            ByteListExtension.AddAsBigEndian = true;
            List<byte> outBytes = new List<byte>();
            polyAnim.Write(outBytes, pofSets);

            //ARC enveloping
            outBytes.AlignWriter(0x4);
            var pof0Offset = outBytes.Count + 0x20;
            pofSets.Sort();
            var pof0 = POF0.GenerateRawPOF0(pofSets, true);
            outBytes.AddRange(pof0);
            //Polyanim text
            outBytes.AddRange(new byte[] { 0, 0, 0, 0,  0, 0, 0, 0,  0x70, 0x6F, 0x6C, 0x79, 0x61, 0x6E, 0x69, 0x6D, 0});

            var arcBytes = new List<byte>();
            arcBytes.AddValue(outBytes.Count + 0x20);
            arcBytes.AddValue(pof0Offset);
            arcBytes.AddValue(pof0.Length);
            arcBytes.AddValue(1);

            arcBytes.AddValue(0);
            arcBytes.Add(0x30);
            arcBytes.Add(0x31);
            arcBytes.Add(0x30);
            arcBytes.Add(0x30);
            arcBytes.AddValue(0);
            arcBytes.AddValue(0);

            outBytes.InsertRange(0, arcBytes);
            ByteListExtension.Reset();

            return outBytes.ToArray();
        }

    }
}
