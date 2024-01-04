using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Security.Cryptography;

namespace AquaModelLibrary.Data.BillyHatcher
{
    public class GPL
    {
        public GPLHeader header;
        public List<GPLEntry> entries = new List<GPLEntry>();
        public List<byte[]> rawGVRBytesList = new List<byte[]>();
        public struct GPLHeader
        {
            public int size;
            public int texCount;
            public int unkInt;
            public int dataOffset;
        }
        public struct GPLEntry
        {
            public int GVRFlags;
            public int GVRDimensions;
            /// <summary>
            ///  Filesize - 0x8 of what it would be as a gvr.
            /// </summary>
            public int offset;
            public int size;
        }

        public void LoadGVRs(List<byte[]> gvrBytesList)
        {
            entries.Clear();
            rawGVRBytesList.Clear();
            foreach (var gvr in gvrBytesList)
            {
                //Account for Global Index
                int offset = 0;
                if (gvr[0] == 0x47 && gvr[1] == 0x42 && gvr[2] == 0x49 && gvr[3] == 0x58)
                {
                    offset = 0x10;
                }
                GPLEntry entry = new GPLEntry();
                var temp = new byte[4];
                Array.Copy(gvr, offset + 8, temp, 0, 4);
                Array.Reverse(temp);
                entry.GVRFlags = BitConverter.ToInt32(temp, 0);
                Array.Copy(gvr, offset + 0xC, temp, 0, 4);
                Array.Reverse(temp);
                entry.GVRDimensions = BitConverter.ToInt32(temp, 0);
                entry.offset = -1;
                entry.size = BitConverter.ToInt32(gvr, offset + 0x4) - 0x8;
                entries.Add(entry);

                var rawGvr = new byte[entry.size];
                Array.Copy(gvr, offset + 0x10, rawGvr, 0, entry.size);
                rawGVRBytesList.Add(rawGvr);
            }
        }

        public List<byte[]> GetGVRs()
        {
            List<byte[]> GVRBytesList = new List<byte[]>();
            for (int i = 0; i < rawGVRBytesList.Count; i++)
            {
                var entry = entries[i];
                ByteListExtension.AddAsBigEndian = false;
                List<byte> gvrBytes = new List<byte>() { 0x47, 0x56, 0x52, 0x54 };
                gvrBytes.AddValue(rawGVRBytesList[i].Length + 0x8);
                ByteListExtension.AddAsBigEndian = true;
                gvrBytes.AddValue(entry.GVRFlags);
                gvrBytes.AddValue(entry.GVRDimensions);
                gvrBytes.AddRange(rawGVRBytesList[i]);
                GVRBytesList.Add(gvrBytes.ToArray());
            }

            return GVRBytesList;
        }

        public GPL() { }

        public GPL(byte[] file)
        {
            Read(file);
        }

        public GPL(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        private void Read(byte[] file)
        {
            using (MemoryStream stream = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                Read(sr);
            }
        }

        private void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr._BEReadActive = true;
            header.size = sr.ReadBE<int>();
            header.texCount = sr.ReadBE<int>();
            header.unkInt = sr.ReadBE<int>();
            header.dataOffset = sr.ReadBE<int>();

            for (int i = 0; i < header.texCount; i++)
            {
                GPLEntry entry = new GPLEntry();
                entry.GVRFlags = sr.ReadBE<int>();
                entry.GVRDimensions = sr.ReadBE<int>();
                entry.offset = sr.ReadBE<int>();
                entry.size = sr.ReadBE<int>();
                entries.Add(entry);
            }
            for (int i = 0; i < header.texCount; i++)
            {
                var entry = entries[i];
                sr.Seek(entry.offset, SeekOrigin.Begin);
                rawGVRBytesList.Add(sr.ReadBytes(sr.Position, entry.size));
            }
        }

        public byte[] GetBytes()
        {
            ByteListExtension.AddAsBigEndian = true;
            List<byte> outBytes = new List<byte>();
            outBytes.ReserveInt("totalBufferSize");
            outBytes.AddValue(rawGVRBytesList.Count);
            outBytes.AddValue(0xC);
            outBytes.ReserveInt("dataStart");

            for (int i = 0; i < entries.Count; i++)
            {
                outBytes.AddValue(entries[i].GVRFlags);
                outBytes.AddValue(entries[i].GVRDimensions);
                outBytes.ReserveInt($"gvrOffset{i}");
                outBytes.AddValue(entries[i].size);
            }
            outBytes.AlignWriter(0x20);

            Dictionary<string, int> offsetDict = new Dictionary<string, int>();
            List<string> hashList = new List<string>();
            outBytes.FillInt("dataStart", outBytes.Count);
            for (int i = 0; i < entries.Count; i++)
            {
                var rawGvr = rawGVRBytesList[i];
                byte[] byteHash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(rawGvr);
                var hash = BitConverter.ToString(byteHash).Replace("-", string.Empty).ToLower();

                hashList.Add(hash);
                if (!offsetDict.ContainsKey(hash))
                {
                    offsetDict.Add(hash, outBytes.Count);
                    outBytes.AddRange(rawGvr);
                }
            }
            for (int i = 0; i < entries.Count; i++)
            {
                outBytes.FillInt($"gvrOffset{i}", offsetDict[hashList[i]]);
            }

            outBytes.FillInt("totalBufferSize", outBytes.Count);

            return outBytes.ToArray();
        }
    }
}
