using AquaModelLibrary.AquaMethods;
using Reloaded.Memory.Streams;
using System.Collections.Generic;
using System.IO;

namespace AquaModelLibrary.Zero
{
    public static class PSZTextBin
    {
        public static void DumpNameBin(string filename)
        {
            var file = File.ReadAllBytes(filename);
            List<string> text = new List<string>();
            List<string> text2 = new List<string>();
            using (Stream stream = (Stream)new MemoryStream(file))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                var count0 = streamReader.Read<ushort>();
                List<ushort> ptrs0 = new List<ushort>();
                for(int i = 0; i < count0; i++)
                {
                    ptrs0.Add(streamReader.Read<ushort>());
                }
                var count1 = streamReader.Read<ushort>();
                List<ushort> ptrs1 = new List<ushort>();
                for (int i = 0; i < count1; i++)
                {
                    ptrs1.Add(streamReader.Read<ushort>());
                }
                var textStart = streamReader.Position();
                foreach(var ptr in ptrs0)
                {
                    streamReader.Seek(ptr + textStart, SeekOrigin.Begin);
                    text.Add(AquaGeneralMethods.ReadCStringPSZ(streamReader));
                }
                foreach (var ptr in ptrs1)
                {
                    streamReader.Seek(ptr + textStart, SeekOrigin.Begin);
                    text2.Add(AquaGeneralMethods.ReadCStringPSZ(streamReader));
                }
                File.WriteAllLines(Path.ChangeExtension(filename, "0.txt"), text);
                File.WriteAllLines(Path.ChangeExtension(filename, "1.txt"), text2);
            }
        }
    }
}
