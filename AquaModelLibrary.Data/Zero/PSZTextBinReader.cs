using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.Zero
{
    public static class PSZTextBinReader
    {
        public static void DumpNameBin(string filename)
        {
            var file = File.ReadAllBytes(filename);
            ReadNameBin(filename, file, out var text, out var text2);
        }

        private static void ReadNameBin(string filename, byte[] file, out List<string> text, out List<string> text2)
        {
            text = new List<string>();
            text2 = new List<string>();
            using (MemoryStream stream = new MemoryStream(file))
            using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                var count0 = streamReader.Read<ushort>();
                List<ushort> ptrs0 = new List<ushort>();
                for (int i = 0; i < count0; i++)
                {
                    ptrs0.Add(streamReader.Read<ushort>());
                }
                var count1 = streamReader.Read<ushort>();
                List<ushort> ptrs1 = new List<ushort>();
                for (int i = 0; i < count1; i++)
                {
                    ptrs1.Add(streamReader.Read<ushort>());
                }
                var textStart = streamReader.Position;
                foreach (var ptr in ptrs0)
                {
                    streamReader.Seek(ptr + textStart, SeekOrigin.Begin);
                    text.Add(streamReader.ReadCStringPSZ(0xA));
                }
                foreach (var ptr in ptrs1)
                {
                    streamReader.Seek(ptr + textStart, SeekOrigin.Begin);
                    text2.Add(streamReader.ReadCStringPSZ(0xA));
                }
                File.WriteAllLines(Path.ChangeExtension(filename, "0.txt"), text);
                File.WriteAllLines(Path.ChangeExtension(filename, "1.txt"), text2);
            }
        }
    }
}
