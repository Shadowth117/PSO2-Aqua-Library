using AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData;
using AquaModelLibrary.Extensions.Readers;
using AquaModelLibrary.Helpers.Ice;
using System.Text;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public unsafe abstract class AquaCommon
    {
        public VTBF vtbf;
        public NIFL nifl;
        public REL0 rel0;

        public NOF0 nof0;
        public NEND nend;

        /// <summary>
        /// Read the aqua file header. To determine the if the file is ICE enveloped, we need the original file extension.
        /// </summary>
        public void Read(byte[] file, string _ext)
        {
            Read(file, new string[] { _ext });
        }

        /// <summary>
        /// Read the aqua file header. To determine the if the file is ICE enveloped, we need the original file extension.
        /// </summary>
        public void Read(BufferedStreamReaderBE<MemoryStream> streamReader, string _ext)
        {
            Read(streamReader, new string[] { _ext });
        }

        /// <summary>
        /// Read the aqua file header. To determine the if the file is ICE enveloped, we need the original file extension.
        /// </summary>
        public void Read(byte[] file, string[] _ext)
        {
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr, _ext);
            }
        }

        /// <summary>
        /// Read the aqua file header. To determine the if the file is ICE enveloped, we need the original file extension.
        /// </summary>
        public void Read(BufferedStreamReaderBE<MemoryStream> streamReader, string[] _ext)
        {
            string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
            int offset = 0x20; //Base offset due to NIFL header

            IceMethods.SkipIceEnvelope(streamReader, _ext, ref type, ref offset);

            //Proceed based on file variant
            if (type.Equals("NIFL"))
            {
                ReadNIFLFile(streamReader, offset);
            }
            else if (type.Equals("VTBF"))
            {
                ReadVTBFFile(streamReader, offset);
            }
        }

        public virtual void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset) { throw new NotImplementedException(); }
        public virtual void ReadVTBFFile(BufferedStreamReaderBE<MemoryStream> sr, int offset) { throw new NotImplementedException(); }

        public void ReadNIFLInfo(BufferedStreamReaderBE<MemoryStream> sr)
        {
            var fileStart = sr.Position;
            nifl = sr.Read<NIFL>();
            rel0 = sr.Read<REL0>();
            sr.Seek(fileStart + nifl.NOF0OffsetFull, SeekOrigin.Begin);
            nof0 = ReadNOF0(sr);
            sr.AlignReader(0x10);
            nend = sr.Read<NEND>();
            sr.Seek(fileStart + 0x20 + rel0.REL0DataStart, SeekOrigin.Begin);
        }

        public void ReadIceEnvelope(BufferedStreamReaderBE<MemoryStream> streamReader, ref int offset, ref string type)
        {
            //Deal with ice envelope
            streamReader.Seek(0xC, SeekOrigin.Begin);
            int headSize = streamReader.Read<int>();

            streamReader.Seek(headSize - 0x10, SeekOrigin.Current);
            type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
            offset += headSize;
        }

        public NOF0 ReadNOF0(BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            NOF0 nof0 = new NOF0();
            nof0.magic = streamReader.Read<int>();
            nof0.NOF0Size = streamReader.Read<int>();
            nof0.NOF0EntryCount = streamReader.Read<int>();
            nof0.NOF0DataSizeStart = streamReader.Read<int>();
            nof0.relAddresses = new List<int>();

            for (int nofEntry = 0; nofEntry < nof0.NOF0EntryCount; nofEntry++)
            {
                nof0.relAddresses.Add(streamReader.Read<int>());
            }

            return nof0;
        }

        public List<uint> GetNOF0PointedValues(BufferedStreamReaderBE<MemoryStream> streamReader, int offset)
        {
            List<uint> addresses = new List<uint>();

            for (int i = 0; i < nof0.relAddresses.Count; i++)
            {
                streamReader.Seek(nof0.relAddresses[i] + offset, System.IO.SeekOrigin.Begin);
                addresses.Add(streamReader.Read<uint>());
            }

            return addresses;
        }
    }
}
