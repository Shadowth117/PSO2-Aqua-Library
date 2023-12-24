using AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData;
using AquaModelLibrary.Extensions.Readers;
using System.Text;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public unsafe class AquaCommon
    {
        public VTBF vtbf;
        public NIFL nifl;
        public REL0 rel0;

        public NOF0 nof0;
        public NEND nend;

        public string ReadAquaHeader(BufferedStreamReaderBE<MemoryStream> streamReader, out int offset, AFPMain afp = new AFPMain())
        {
            string variant = null;
            string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
            offset = 0x20; //Base offset due to NIFL header

            ReadIceEnvelope(streamReader, ref offset, ref type);

            //Deal with afp header or aqo. prefixing as needed
            if (type.Equals("afp\0"))
            {
                afp = streamReader.Read<AFPMain>();
                type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                offset += 0x40;
            }
            else if (type.Equals("aqo\0") || type.Equals("tro\0"))
            {
                streamReader.Seek(0x4, SeekOrigin.Current);
                type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                offset += 0x4;
            }

            if (afp.fileCount == 0)
            {
                afp.fileCount = 1;
            }

            //Proceed based on file variant
            if (type.Equals("NIFL"))
            {
                variant = "NIFL";
            }
            else if (type.Equals("VTBF"))
            {
                variant = "VTBF";
            }
            return variant;
        }

        public void ReadNIFLInfo(BufferedStreamReaderBE<MemoryStream> sr)
        {
            var fileStart = sr.Position;
            nifl = sr.Read<NIFL>();
            rel0 = sr.Read<REL0>();
            sr.Seek(fileStart + nifl.NOF0OffsetFull, SeekOrigin.Begin);
            nof0 = ReadNOF0(sr);
            nend = sr.Read<NEND>();
            sr.Seek(fileStart + 0x20 + rel0.REL0DataStart, SeekOrigin.Begin);
        }

        public void ReadIceEnvelope(BufferedStreamReaderBE<MemoryStream> streamReader, ref int offset, ref string type)
        {
            //Deal with ice envelope nonsense
            streamReader.Seek(0xC, SeekOrigin.Begin);
            //Basically always 0x60, but some from the Alpha have 0x50... 
            int headJunkSize = streamReader.Read<int>();

            streamReader.Seek(headJunkSize - 0x10, SeekOrigin.Current);
            type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
            offset += headJunkSize;
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
