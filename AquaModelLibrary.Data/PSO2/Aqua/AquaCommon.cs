using AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData;
using AquaModelLibrary.Helpers.Ice;
using AquaModelLibrary.Helpers.Readers;
using System.Text;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public unsafe class AquaCommon
    {
        /// <summary>
        /// Offset to start of REL0 if the file is NIFL. If VTBF, offsets aren't used.
        /// </summary>
        public int offset0 = 0;
        public VTBF vtbf;
        public NIFL nifl;
        public REL0 rel0;

        public NOF0 nof0;
        public NEND nend;

        /// <summary>
        /// Get the ice envelope extension(s) for this file.
        /// </summary>
        public virtual string[] GetEnvelopeTypes() => null;

        public AquaCommon() { }

        public AquaCommon(byte[] file) { Read(file); }

        public AquaCommon(BufferedStreamReaderBE<MemoryStream> streamReader) { Read(streamReader); }

        /// <summary>
        /// Read the aqua file header. Ice envelope is not skipped if GetEnvelopeTypes is not properly defined.
        /// </summary>
        public void Read(byte[] file)
        {
            Read(file, GetEnvelopeTypes());
        }

        /// <summary>
        /// Read the aqua file header. Ice envelope is not skipped if GetEnvelopeTypes is not properly defined.
        /// </summary>
        public void Read(BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            Read(streamReader, GetEnvelopeTypes());
        }

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
            offset0 = (int)(0x20 + streamReader.Position); //Base offset due to NIFL header

            IceMethods.SkipIceEnvelope(streamReader, _ext, ref type, ref offset0);

            offset0 = (int)(0x20 + streamReader.Position);
            //Proceed based on file variant
            if (type.Equals("NIFL"))
            {
                ReadNIFLInfo(streamReader);
                ReadNIFLFile(streamReader, offset0);
            }
            else if (type.Equals("VTBF"))
            {
                ReadVTBFFile(streamReader);
            }
        }

        public virtual void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset) { throw new NotImplementedException(); }
        public virtual void ReadVTBFFile(BufferedStreamReaderBE<MemoryStream> sr) { throw new NotImplementedException(); }

        public virtual byte[] GetBytesNIFL() { throw new NotImplementedException(); }
        public virtual byte[] GetBytesVTBF() { throw new NotImplementedException(); }

        /// <summary>
        /// Reads NIFL, REL0, NOF0, and NEND, then seeks to REL0DataStart
        /// </summary>
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
            var bookmark = streamReader.Position;
            for (int i = 0; i < nof0.relAddresses.Count; i++)
            {
                streamReader.Seek(nof0.relAddresses[i] + offset, System.IO.SeekOrigin.Begin);
                addresses.Add(streamReader.Read<uint>());
            }
            streamReader.Seek(bookmark, SeekOrigin.Begin);
            return addresses;
        }

        public void DumpNOF0(BufferedStreamReaderBE<MemoryStream> streamReader, string inFilename)
        {
            Dictionary<int, List<int>> addresses = new Dictionary<int, List<int>>();
            List<string> output = new List<string>
            {
                Path.GetFileName(inFilename),
                "",
                $"REL0 Magic: {Encoding.UTF8.GetString(BitConverter.GetBytes(rel0.magic))} | REL0 Size: {rel0.REL0Size:X} | REL0 Data Start: {rel0.REL0DataStart:X} | REL0 Version: {rel0.version:X}",
                "",
                $"NOF0 Magic: {Encoding.UTF8.GetString(BitConverter.GetBytes(nof0.magic))} | NOF0 Size: {nof0.NOF0Size:X} | NOF0 Entry Count: {nof0.NOF0EntryCount:X} | NOF0 Data Size Start: {nof0.NOF0DataSizeStart:X}",
                "",
                "NOF0 Ptr Address - Ptr value",
                "",
            };
            foreach (var entry in nof0.relAddresses)
            {
                streamReader.Seek(entry + offset0, SeekOrigin.Begin);
                int ptr = streamReader.Read<int>();
                output.Add($"{entry:X} - {ptr:X}");

                if (!addresses.ContainsKey(ptr))
                {
                    addresses[ptr] = new List<int>() { entry };
                }
                else
                {
                    addresses[ptr].Add(entry);
                    addresses[ptr].Sort();
                }
            }
            output.Add("");
            output.Add("Pointer Value, followed by addresses with said value");
            output.Add("");
            var addressKeys = addresses.Keys.ToList();
            addressKeys.Sort();
            foreach (var key in addressKeys)
            {
                output.Add(key.ToString("X") + ":");
                var addressList = addresses[key];
                for (int i = 0; i < addressList.Count; i++)
                {
                    output.Add("    " + addressList[i].ToString("X"));
                }
            }

            File.WriteAllLines(inFilename + "_nof0.txt", output);
        }
    }
}
