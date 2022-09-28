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
        public static string ReadAquaHeader(BufferedStreamReader streamReader, string ext, string variant, out int offset, AquaPackage.AFPMain afp = new AquaPackage.AFPMain())
        {
            string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
            offset = 0x20; //Base offset due to NIFL header

            ReadIceEnvelope(streamReader, ext, ref offset, ref type);

            //Deal with afp header or aqo. prefixing as needed
            if (type.Equals("afp\0"))
            {
                afp = streamReader.Read<AquaPackage.AFPMain>();
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
            else
            {
                //MessageBox.Show("Improper File Format!");
            }

            return variant;
        }

        public static void ReadIceEnvelope(BufferedStreamReader streamReader, string ext, ref int offset, ref string type)
        {
            if(ext.Contains('.'))
            {
                ext = ext.Substring(1, ext.Length - 1) + "\0";
            }

            //Deal with ice envelope nonsense
            if (type.Equals(ext))
            {
                streamReader.Seek(0xC, SeekOrigin.Begin);
                //Basically always 0x60, but some from the Alpha have 0x50... 
                int headJunkSize = streamReader.Read<int>();

                streamReader.Seek(headJunkSize - 0x10, SeekOrigin.Current);
                type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                offset += headJunkSize;
            }
        }

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
                variant = ReadAquaHeader(streamReader, ext, variant, out offset, afp);

                if (variant == "NIFL")
                {
                    var nifl = streamReader.Read<AquaCommon.NIFL>();
                    var rel = streamReader.Read<AquaCommon.REL0>();
                    streamReader.Seek(nifl.NOF0OffsetFull, SeekOrigin.Begin);
                    Trace.WriteLine(streamReader.Position());
                    AlignReader(streamReader, 0x10);
                    var nof0 = AquaCommon.readNOF0(streamReader);

                    Dictionary<int, List<int>> addresses = new Dictionary<int, List<int>>();
                    List<string> output = new List<string>();
                    output.Add(Path.GetFileName(inFilename));
                    output.Add("");
                    output.Add($"{Encoding.UTF8.GetString(BitConverter.GetBytes(rel.magic))} {rel.REL0Size:X} {rel.REL0DataStart:X} {rel.version:X}");
                    output.Add("");
                    output.Add($"{Encoding.UTF8.GetString(BitConverter.GetBytes(nof0.magic))} {nof0.NOF0Size:X} {nof0.NOF0EntryCount:X} {nof0.NOF0DataSizeStart:X}");
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

        public static void AlignReader(BufferedStreamReader streamReader, int align)
        {
            //Align to int align
            while (streamReader.Position() % align > 0)
            {
                streamReader.Read<byte>();
            }
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

        public static int[] Read4BytesToIntArray(BufferedStreamReader streamReader)
        {
            var bytes = Read4Bytes(streamReader);
            return new int[] { bytes[0], bytes[1], bytes[2], bytes[3] };
        }

        public static byte[] Read4Bytes(BufferedStreamReader streamReader)
        {
            byte[] bytes = new byte[4];
            for (int byteIndex = 0; byteIndex < 4; byteIndex++) { bytes[byteIndex] = streamReader.Read<byte>(); }

            return bytes;
        }

        public static ushort[] Read4UShorts(BufferedStreamReader streamReader)
        {
            ushort[] ushorts = new ushort[4];
            for (int ushortIndex = 0; ushortIndex < 4; ushortIndex++) { ushorts[ushortIndex] = streamReader.Read<ushort>(); }

            return ushorts;
        }

        public static short[] Read4Shorts(BufferedStreamReader streamReader)
        {
            short[] shorts = new short[4];
            for (int shortIndex = 0; shortIndex < 4; shortIndex++) { shorts[shortIndex] = streamReader.Read<short>(); }

            return shorts;
        }

        public static short[] Read2Shorts(BufferedStreamReader streamReader)
        {
            short[] shorts = new short[2];
            for (int shortIndex = 0; shortIndex < 2; shortIndex++) { shorts[shortIndex] = streamReader.Read<short>(); }

            return shorts;
        }

        public static ushort[] Read2Ushorts(BufferedStreamReader streamReader)
        {
            ushort[] shorts = new ushort[2];
            for (int shortIndex = 0; shortIndex < 2; shortIndex++) { shorts[shortIndex] = streamReader.Read<ushort>(); }

            return shorts;
        }

        public static Vector2 UshortsToVector2(ushort[] ushorts)
        {
            return new Vector2((float)((double)ushorts[0] / ushort.MaxValue), (float)((double)ushorts[1] / ushort.MaxValue));
        }

        public static float[] VectorAsArray(Vector3 vec3)
        {
            return new float[] { vec3.X, vec3.Y, vec3.Z };
        }

        public static Matrix4x4 ReadMatrix4(BufferedStreamReader streamReader)
        {
            return new Matrix4x4(streamReader.Read<float>(), streamReader.Read<float>(), streamReader.Read<float>(), streamReader.Read<float>(),
                streamReader.Read<float>(), streamReader.Read<float>(), streamReader.Read<float>(), streamReader.Read<float>(),
                streamReader.Read<float>(), streamReader.Read<float>(), streamReader.Read<float>(), streamReader.Read<float>(),
                streamReader.Read<float>(), streamReader.Read<float>(), streamReader.Read<float>(), streamReader.Read<float>());
        }

        //This shouldn't be necessary, but library binding issues in maxscript necessitated it over the Reloaded.Memory implementation. System.Runtime.CompilerServices.Unsafe causes errors otherwise.
        //Borrowed from: https://stackoverflow.com/questions/42154908/cannot-take-the-address-of-get-the-size-of-or-declare-a-pointer-to-a-managed-t
        private static byte[] ConvertStruct<T>(ref T str) where T : struct
        {
            int size = Marshal.SizeOf(str);
            IntPtr arrPtr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, arrPtr, true);
            var arr = new byte[size];
            Marshal.Copy(arrPtr, arr, 0, size);
            Marshal.FreeHGlobal(arrPtr);
            return arr;
        }

        public static byte[] ConvertStruct<T>(T str) where T : struct
        {
            return ConvertStruct(ref str);
        }

        public static string ReadCStringPSZ(BufferedStreamReader streamReader)
        {
            string str = Encoding.ASCII.GetString(streamReader.ReadBytes(streamReader.Position(), 0x60)); //Shouldn't ever be more than 0x60... in theory
            if(str.IndexOf(char.MinValue) == -1)
            {
                return str.Remove(str.IndexOf((char)0xA));
            } 
            return str.Remove(str.IndexOf(char.MinValue));
        }

        public static string ReadCString(BufferedStreamReader streamReader)
        {
            string str = Encoding.ASCII.GetString(streamReader.ReadBytes(streamReader.Position(), 0x100)); //Shouldn't ever be more than 0x60... in theory
            return str.Remove(str.IndexOf(char.MinValue));
        }

        public static string ReadUTF16String(BufferedStreamReader streamReader, long end, bool quickMode = true)
        {
            string str;
            // There's really no string limit, but it takes a long time to read these accurately. To avoid this attempt reading a short amount first. Then, go to failsafe if needed.
            if (quickMode == true)
            {
                str = Encoding.Unicode.GetString(streamReader.ReadBytes(streamReader.Position(), 0x60));
                int strEnd = str.IndexOf(char.MinValue);

                //Return the string if the buffer is valid
                if (strEnd != -1)
                {
                    return str.Remove(strEnd);
                }
            }
            str = Encoding.Unicode.GetString(streamReader.ReadBytes(streamReader.Position(), (int)(end - streamReader.Position()))); //Come up with a better way handling sometime because I hate this.
            return str.Remove(str.IndexOf(char.MinValue));
        }

        public static byte[] RemoveIceEnvelope(byte[] inData)
        {
            byte[] outData;
            var headerSize = BitConverter.ToInt32(inData, 0xC);
            outData = new byte[inData.Length - headerSize];
            Array.Copy(inData, headerSize, outData, 0, outData.Length);

            return outData;
        }

        public static string GetFileHash(string str)
        {
            if (str == null)
            {
                return "";
            }
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(new UTF8Encoding().GetBytes(str));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }

        public static string GetFileDataHash(string fileName)
        {
            if (fileName == null)
            {
                return "";
            }
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(File.ReadAllBytes(fileName));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }

        public static string GetRebootHash(string fileName)
        {
            return fileName.Substring(0, 2) + "\\" + fileName.Substring(2, fileName.Length - 2);
        }

    }
}
