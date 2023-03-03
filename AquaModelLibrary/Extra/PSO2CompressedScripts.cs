using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnluacNET;
using ZstdNet;

namespace AquaModelLibrary.Extra
{
    public class PSO2CompressedScripts
    {
        public string txtFile = "Extra\\PSO2Scripts\\ScriptReference.txt";
        public Dictionary<ulong, string> fileDict = new Dictionary<ulong, string>();
        public Dictionary<ulong, byte[]> storeDict = new Dictionary<ulong, byte[]>();
        public Dictionary<string, Decompiler> knownLua = new Dictionary<string, Decompiler>();
        public Dictionary<string, Decompiler> unknownLua = new Dictionary<string, Decompiler>();

        public struct CScriptsHeader
        {
            public uint magic;
            public uint zero1;
            public uint zero2;
            public uint count;
        }

        public struct CFileEntry
        {
            public ulong hash;
            public uint compressedSize;
            public uint offset;
            public uint uncompressedSize;
        }

        public PSO2CompressedScripts()
        {

        }
        public PSO2CompressedScripts(string filePath)
        {
            ParseScripts(filePath);
        }

        public void ParseScripts(string filePath)
        {
            var refText = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Substring(6), txtFile);

            //Get name references if they exist
            if (File.Exists(refText))
            {
                var text = File.ReadAllLines(refText);
                foreach (var line in text)
                {
                    string[] fileKey = line.Split(' ');
                    var key = ulong.Parse(fileKey[0], System.Globalization.NumberStyles.HexNumber);
                    if (fileDict.ContainsKey(key))
                    {
                        continue;
                    }
                    fileDict.Add(key, fileKey[1]);
                }
            }
            else
            {
                Debug.WriteLine("ScriptReference.txt does not exist");
            }

            //Extract files and apply names if applicable
            List<CFileEntry> table = new List<CFileEntry>();
            if (File.Exists(filePath))
            {
                byte[] zstdDict = new byte[0];
                ulong zstdDictHash = 0;
                using (var fileStream = new FileStream(filePath, FileMode.Open))
                using (var streamReader = new BufferedStreamReader(fileStream, 8192))
                {

                    streamReader.Seek(0xC, SeekOrigin.Begin);
                    streamReader.Read<int>(out int entryCount);

                    for (int i = 0; i < entryCount; i++)
                    {
                        CFileEntry value = new CFileEntry();

                        value.hash = streamReader.Read<ulong>();
                        value.compressedSize = streamReader.Read<uint>();
                        value.offset = streamReader.Read<uint>();
                        value.uncompressedSize = streamReader.Read<uint>();
                        table.Add(value);
                    }
                    long tableEnd = streamReader.Position();
                    int offsetThing = 0;
                    foreach (CFileEntry entry in table)
                    {
                        var data = streamReader.ReadBytes(entry.offset + tableEnd + offsetThing, (int)entry.compressedSize - offsetThing);
                        if (BitConverter.ToUInt32(data, 0) == 0xEC30A437)
                        {
                            zstdDictHash = entry.hash;
                            zstdDict = data;
                            continue;
                        }
                        storeDict.Add(entry.hash, data);
                    }
                }

                DecompressionOptions opt = new DecompressionOptions(zstdDict);
                Decompressor dec = new Decompressor(opt);
                for (int i = 0; i < table.Count; i++)
                {
                    var entry = table[i];
                    if (entry.hash == zstdDictHash)
                    {
                        continue;
                    }
                    string name;
                    bool foundName = false;
                    if (fileDict.ContainsKey(entry.hash))
                    {
                        foundName = true;
                        name = fileDict[entry.hash];
                    }
                    else
                    {
                        name = entry.hash.ToString("X") + ".lua";
                    }

                    byte[] compressedFile = storeDict[entry.hash];
                    byte[] uncompressedFile = dec.Unwrap(compressedFile, (int)entry.uncompressedSize);
                    if (uncompressedFile != null)
                    {
                        try
                        {
                            using (MemoryStream memStream = new MemoryStream())
                            {
                                var luaStream = new MemoryStream(uncompressedFile);
                                var header = new BHeader(luaStream);
                                LFunction lfunction = header.Function.Parse(luaStream, header);
                                var decompiler = new Decompiler(lfunction);
                                if(foundName)
                                {
                                    knownLua.Add(name, decompiler);
                                } else
                                {
                                    unknownLua.Add(name, decompiler);
                                }
                                /*
                                decompiler.Decompile();
                                using (var writer = new StreamWriter(memStream, new UTF8Encoding(false)))
                                {
                                    decompiler.Print(new Output(writer));
                                    writer.Flush();
                                }
                                */
                                //File.WriteAllBytes(fileOutput, memStream.ToArray());
                            }
                        }
                        catch
                        {
                            var a = 0;
                            //File.WriteAllBytes(fileOutput, uncompressedFile);
                        }
                    }
                    //scriptList.WriteLine(entry.hash.ToString("X") + "  " + name);
                }
            }
        }
    }
}
