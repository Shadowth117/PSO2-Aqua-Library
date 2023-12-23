using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace AquaModelLibrary.AquaMethods
{
    public static class AquaGeneralMethods
    {


        public static void DumpNOF0(string inFilename)
        {
            AquaPackage.AFPMain afp = new AquaPackage.AFPMain();
            string ext = Path.GetExtension(inFilename);
            string variant = "";
            int offset;
            if (ext.Length > 4)
            {
                ext = ext.Substring(0, 4);
            }

            using (Stream stream = (Stream)new FileStream(inFilename, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                variant = ReadAquaHeader(streamReader, ext, out offset, afp);

                if (variant == "NIFL")
                {
                    var nifl = streamReader.Read<AquaCommon.NIFL>();
                    var rel = streamReader.Read<AquaCommon.REL0>();
                    streamReader.Seek(nifl.NOF0OffsetFull, SeekOrigin.Begin);
                    Trace.WriteLine(streamReader.Position());
                    AlignReader(streamReader, 0x10);
                    var nof0 = AquaCommon.readNOF0(streamReader);

                    Dictionary<int, List<int>> addresses = new Dictionary<int, List<int>>();
                    List<string> output = new List<string>
                    {
                        Path.GetFileName(inFilename),
                        "",
                        $"REL0 Magic: {Encoding.UTF8.GetString(BitConverter.GetBytes(rel.magic))} | REL0 Size: {rel.REL0Size:X} | REL0 Data Start: {rel.REL0DataStart:X} | REL0 Version: {rel.version:X}",
                        "",
                        $"NOF0 Magic: {Encoding.UTF8.GetString(BitConverter.GetBytes(nof0.magic))} | NOF0 Size: {nof0.NOF0Size:X} | NOF0 Entry Count: {nof0.NOF0EntryCount:X} | NOF0 Data Size Start: {nof0.NOF0DataSizeStart:X}",
                        "",
                        "NOF0 Ptr Address - Ptr value",
                        "",
                    };
                    foreach (var entry in nof0.relAddresses)
                    {
                        streamReader.Seek(entry + offset, SeekOrigin.Begin);
                        int ptr = streamReader.Read<int>();
                        output.Add($"{entry:X} - {ptr:X}");
                        
                        if(!addresses.ContainsKey(ptr))
                        {
                            addresses[ptr] = new List<int>() { entry };
                         } else
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
                    foreach(var key in addressKeys)
                    {
                        output.Add(key.ToString("X") + ":");
                        var addressList = addresses[key];
                        for(int i = 0; i < addressList.Count; i++)
                        {
                            output.Add("    " + addressList[i].ToString("X"));
                        }
                    }

                    File.WriteAllLines(inFilename + "_nof0.txt", output);
                }
            }
        }

        public static int NOF0Append(List<int> nof0, int currentOffset, int countToCheck = 1, int subtractedOffset = 0)
        {
            if (countToCheck < 1)
            {
                return -1;
            }
            int newAddress = currentOffset - subtractedOffset;
            nof0.Add(newAddress);

            return newAddress;
        }

        //https://stackoverflow.com/questions/8809354/replace-first-occurrence-of-pattern-in-a-string
        public static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        //Mainly for handling pointer offsets
        public static int SetByteListInt(List<byte> outBytes, int offset, int value)
        {
            if (offset != -1)
            {
                var newBytes = BitConverter.GetBytes(value);
                for (int i = 0; i < 4; i++)
                {
                    outBytes[offset + i] = newBytes[i];
                }

                return value;
            }

            return -1;
        }

        public static int AlignWriter(List<byte> outBytes, int align)
        {
            //Align to int align
            int additions = 0;
            while (outBytes.Count % align > 0)
            {
                additions++;
                outBytes.Add(0);
            }

            return additions;
        }

        public static void AlignFileEndWrite(List<byte> outBytes, int align)
        {
            if (outBytes.Count % align == 0)
            {
                for (int i = 0; i < 0x10; i++)
                {
                    outBytes.Add(0);
                }
            }
            else
            {
                //Align to 0x10
                while (outBytes.Count % align > 0)
                {
                    outBytes.Add(0);
                }
            }
        }



    }
}
