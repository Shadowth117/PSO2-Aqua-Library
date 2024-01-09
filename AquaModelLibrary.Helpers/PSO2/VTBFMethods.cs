using AquaModelLibrary.Helpers.Readers;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace AquaModelLibrary.Helpers.PSO2
{
    public unsafe class VTBFMethods
    {
        public const uint vtc0Start = 0x30637476;
        public static List<Dictionary<int, object>> ReadVTBFTag(BufferedStreamReaderBE<MemoryStream> streamReader, out string tagString, out int ptrCount, out int entryCount)
        {
            List<Dictionary<int, object>> vtbfData = new List<Dictionary<int, object>>();
            bool listType = false;

            int vtc0 = streamReader.Read<int>(); //vtc0
            if (vtc0 != vtc0Start)
            {
                tagString = null;
                entryCount = 0;
                ptrCount = 0;
                return null;
            }
            uint bodyLength = streamReader.Read<uint>();
            int mainTagType = streamReader.Read<int>();
            tagString = Encoding.UTF8.GetString(BitConverter.GetBytes(mainTagType));
            //Debug.WriteLine($"Start { tagString} around { streamReader.Position().ToString("X")}");

            ptrCount = streamReader.Read<short>(); //Not important for reading. Game assumedly uses this at runtime to know how many pointer ints to prepare for the block.
            entryCount = streamReader.Read<short>();

            Dictionary<int, object> vtbfDict = new Dictionary<int, object>();

            for (int i = 0; i < entryCount; i++)
            {
                byte dataId = streamReader.Read<byte>();
                byte dataType = streamReader.Read<byte>();
                byte subDataType;
                uint subDataAdditions;

                if (i == 0 && dataId == 0xFC)
                {
                    listType = true;
                }

                //Check for special ids
                if (listType)
                {
                    switch (dataId)
                    {
                        case 0xFD: //End of sequence and should be end of tag
                            vtbfData.Add(vtbfDict);
                            return vtbfData;
                        case 0xFE:
                            vtbfData.Add(vtbfDict);
                            vtbfDict = new Dictionary<int, object>();
                            break;
                        default: //Nothing needs to be done if this is a normal id. 0xFC, start of sequence, is special, but for reading purposes can also go through here.
                            break;
                    }
                }

                object data;
                switch (dataType)
                {
                    case 0x0: //Special Flag. Probably more for parsing purposes than data. 0xFC ID before this means first struct, 0xFE means a later struct
                        data = dataType;
                        break;
                    case 0x1: //Boolean? Single byte
                        data = streamReader.Read<byte>();
                        break;
                    case 0x2: //String
                        byte strLen = streamReader.Read<byte>();
                        if (strLen > 0)
                        {
                            data = streamReader.ReadBytes(streamReader.Position, strLen);
                        }
                        else
                        {
                            data = new byte[0];
                        }
                        streamReader.Seek(strLen, SeekOrigin.Current);
                        break;
                    case 0x3: //sbyte
                    case 0x4: //byte
                        data = streamReader.Read<byte>();
                        break;
                    case 0x5: //Breaks the pattern. I guess some kind of short?
                    case 0x6: //short 0x6 is signed while 0x7 is unsigned
                    case 0x7:
                        data = streamReader.Read<short>();
                        break;
                    case 0x8: //int. 0x8 is signed while 0x9 is unsigned
                    case 0x9:
                        data = streamReader.Read<int>();
                        break;
                    case 0xA: //float
                        data = streamReader.Read<float>();
                        break;
                    case 0xC: //color, BGRA?
                        data = new byte[4];
                        for (int j = 0; j < 4; j++)
                        {
                            ((byte[])data)[j] = streamReader.Read<byte>();
                        }
                        break;
                    case 0x42:
                    case 0x43: //Vector3 of bytes
                        subDataAdditions = streamReader.Read<byte>(); //Presumably the number of these consecutively
                        if (subDataAdditions == 0)
                        {
                            subDataAdditions = 4;
                        }
                        data = new byte[subDataAdditions][];
                        for (int j = 0; j < subDataAdditions; j++)
                        {
                            byte[] dataArr = new byte[3];
                            dataArr[0] = streamReader.Read<byte>();
                            dataArr[1] = streamReader.Read<byte>();
                            dataArr[2] = streamReader.Read<byte>();

                            ((byte[][])data)[j] = dataArr;
                        }
                        break;
                    case 0x48: //Vector3 of ints
                    case 0x49:
                        subDataAdditions = streamReader.Read<byte>(); //Presumably the number of these consecutively
                        data = new int[subDataAdditions][];
                        for (int j = 0; j < subDataAdditions; j++)
                        {
                            int[] dataArr = new int[3];
                            dataArr[0] = streamReader.Read<int>();
                            dataArr[1] = streamReader.Read<int>();
                            dataArr[2] = streamReader.Read<int>();

                            ((int[][])data)[j] = dataArr;
                        }
                        break;
                    case 0x4A: //Vector of floats. Observed as Vector3 and Vector4
                        subDataAdditions = streamReader.Read<byte>(); //Amount of floats past the first 2? 0x1 means Vector3, 0x2 means Vector4 total. Other amounts unobserved.

                        switch (subDataAdditions)
                        {
                            case 0x1:
                                data = streamReader.Read<Vector3>();
                                break;
                            case 0x2:
                                data = streamReader.Read<Vector4>();
                                break;
                            default:
                                Debug.WriteLine($"Unknown subDataAdditions amount {subDataAdditions} at {streamReader.Position}");
                                throw new Exception();
                        }

                        break;
                    case 0x82:
                    case 0x83: //Array of bytes
                        subDataType = streamReader.Read<byte>();      //Next entity type. 0x8 for byte, 0x10 for short
                        switch (subDataType) //The last array entry aka data count - 1.
                        {
                            case 0x8:
                                subDataAdditions = streamReader.Read<byte>() + (uint)1;
                                break;
                            case 0x10:
                                subDataAdditions = streamReader.Read<ushort>() + (uint)1;
                                break;
                            case 0x18:
                                subDataAdditions = streamReader.Read<uint>() + 1;
                                break;
                            default:
                                Debug.WriteLine($"Unknown subdataType {subDataType} at {streamReader.Position}");
                                throw new NotImplementedException();
                        }
                        data = streamReader.ReadBytes(streamReader.Position, (int)subDataAdditions);

                        streamReader.Seek(subDataAdditions, SeekOrigin.Current);
                        break;
                    case 0x84: //Theoretical array of bytes
                    case 0x85:
                        subDataType = streamReader.Read<byte>();      //Next entity type. 0x8 for byte, 0x10 for short
                        switch (subDataType) //The last array entry aka data count - 1.
                        {
                            case 0x8:
                                subDataAdditions = streamReader.Read<byte>() + (uint)1;
                                break;
                            case 0x10:
                                subDataAdditions = streamReader.Read<ushort>() + (uint)1;
                                break;
                            case 0x18:
                                subDataAdditions = streamReader.Read<uint>() + 1;
                                break;
                            default:
                                Debug.WriteLine($"Unknown subdataType {subDataType} at {streamReader.Position}");
                                throw new NotImplementedException();
                        }
                        data = streamReader.ReadBytes(streamReader.Position, (int)subDataAdditions);

                        streamReader.Seek(subDataAdditions, SeekOrigin.Current);
                        break;
                    case 0x86: //Array of ushorts?
                    case 0x87: //Array of shorts
                        subDataType = streamReader.Read<byte>();      //Next entity type. 0x8 for byte, 0x10 for short
                        switch (subDataType) //The last array entry aka data count - 1.
                        {
                            case 0x8:
                                subDataAdditions = streamReader.Read<byte>() + (uint)1;
                                break;
                            case 0x10:
                                subDataAdditions = streamReader.Read<ushort>() + (uint)1;
                                break;
                            case 0x18:
                                subDataAdditions = streamReader.Read<uint>() + 1;
                                break;
                            default:
                                Debug.WriteLine($"Unknown subdataType {subDataType.ToString("X")} at {streamReader.Position}");
                                throw new NotImplementedException();
                        }

                        if (dataType == 0x86)
                        {
                            data = new ushort[subDataAdditions];
                            for (int j = 0; j < subDataAdditions; j++)
                            {
                                ((ushort[])data)[j] = streamReader.Read<ushort>();
                            }
                        }
                        else
                        {
                            data = new short[subDataAdditions];
                            for (int j = 0; j < subDataAdditions; j++)
                            {
                                ((short[])data)[j] = streamReader.Read<short>();
                            }
                        }
                        break;
                    case 0x88:
                    case 0x89: //Array of ints. Often needs to be processed in various ways in post so we read it in bytes.
                        subDataType = streamReader.Read<byte>();      //Next entity type. 0x8 for byte, 0x10 for short
                        switch (subDataType) //The last array entry aka data count - 1.
                        {
                            case 0x8:
                                subDataAdditions = streamReader.Read<byte>() + (uint)1;
                                break;
                            case 0x10:
                                subDataAdditions = streamReader.Read<ushort>() + (uint)1;
                                break;
                            case 0x18:
                                subDataAdditions = streamReader.Read<uint>() + 1;
                                break;
                            default:
                                Debug.WriteLine($"Unknown subdataType {subDataType.ToString("X")} at {streamReader.Position}");
                                throw new NotImplementedException();
                        }
                        subDataAdditions *= 4; //The field is stored as some amount of int32s. Therefore, multiplying by 4 gives us the byte buffer length.
                        data = streamReader.ReadBytes(streamReader.Position, (int)subDataAdditions); //Read the whole vert buffer at once as byte array. We'll handle it later.
                        streamReader.Seek(subDataAdditions, SeekOrigin.Current);
                        break;
                    case 0x8A: //Array of floats
                        subDataType = streamReader.Read<byte>();      //Next entity type. 0x8 for byte, 0x10 for short
                        switch (subDataType) //The last array entry aka data count - 1.
                        {
                            case 0x8:
                                subDataAdditions = streamReader.Read<byte>() + (uint)1;
                                break;
                            case 0x10:
                                subDataAdditions = streamReader.Read<ushort>() + (uint)1;
                                break;
                            case 0x18:
                                subDataAdditions = streamReader.Read<uint>() + 1;
                                break;
                            default:
                                Debug.WriteLine($"Unknown subdataType {subDataType.ToString("X")} at {streamReader.Position}");
                                throw new NotImplementedException();
                        }
                        data = new float[subDataAdditions];
                        for (int j = 0; j < subDataAdditions; j++)
                        {
                            ((float[])data)[j] = streamReader.Read<float>();
                        }
                        break;
                    case 0xC6: //Int16 array? Seen used in .cmx files for storing unicode characters.
                        subDataType = streamReader.Read<byte>();
                        switch (subDataType) //The last array entry aka data count - 1.
                        {
                            case 0x8:
                                subDataAdditions = streamReader.Read<byte>();
                                break;
                            case 0x10:
                                subDataAdditions = streamReader.Read<ushort>();
                                break;
                            case 0x18:
                                subDataAdditions = streamReader.Read<uint>();
                                break;
                            default:
                                Debug.WriteLine($"Unknown subdataType {subDataType.ToString("X")} at {streamReader.Position}");
                                throw new NotImplementedException();
                        }
                        uint actualCount = subDataAdditions * 2 + 2;
                        data = new short[actualCount]; //Yeah something is wrong in the way the og files are written. Should be all 0s after the expected data, but still.
                        for (int j = 0; j < actualCount; j++)
                        {
                            ((short[])data)[j] = streamReader.Read<short>();
                        }
                        break;
                    case 0xCA: //Float Matrix, observed only as 4x4
                        subDataType = streamReader.Read<byte>(); //Expected to always be 0xA for float

                        switch (subDataType)
                        {
                            case 0xA:
                                subDataAdditions = streamReader.Read<byte>() + (uint)1; //last array entry id
                                break;
                            case 0x12:
                                subDataAdditions = streamReader.Read<ushort>() + (uint)1; //last array entry id
                                break;
                            default:
                                Debug.WriteLine($"Unknown subDataAdditions value {subDataType.ToString("X")}, please report!");
                                throw new NotImplementedException();
                        }

                        data = new Vector4[subDataAdditions];
                        for (int j = 0; j < subDataAdditions; j++)
                        {
                            ((Vector4[])data)[j] = streamReader.Read<Vector4>();
                        }
                        break;
                    default:
                        Debug.WriteLine($"Unknown dataType {dataType.ToString("X")} at {streamReader.Position.ToString("X")}, please report!");
                        throw new NotImplementedException();
                }

                //Really shouldn't happen, but they have this situation.
                if (vtbfDict.ContainsKey(dataId))
                {
                    vtbfData.Add(vtbfDict);
                    vtbfDict = new Dictionary<int, object>();
                    vtbfDict.Add(dataId, data);
                }
                else
                {
                    vtbfDict.Add(dataId, data);
                }
                //Debug.WriteLine($"Processed { dataType.ToString("X")} around { streamReader.Position().ToString("X")}");
            }
            //For non-list type tag data and non FD terminated lists (alpha has these)
            vtbfData.Add(vtbfDict);
            //Console.WriteLine($"Processed {tagString} around { streamReader.Position().ToString("X")}");

            return vtbfData;
        }

        public static void AnalyzeVTBF(string fileName)
        {
            using (MemoryStream stream = new MemoryStream(File.ReadAllBytes(fileName)))
            using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                int offset = 0x20; //Base offset due to NIFL header

                //Deal with deicer's extra header nonsense
                if (!type.Equals("NIFL") && !type.Equals("VTBF"))
                {
                    streamReader.Seek(0xC, SeekOrigin.Begin);
                    //Basically always 0x60, but some deicer files from the Alpha have 0x50... 
                    int headJunkSize = streamReader.Read<int>();

                    streamReader.Seek(headJunkSize - 0x10, SeekOrigin.Current);
                    type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                    offset += headJunkSize;
                }

                //Proceed based on file variant
                if (type.Equals("NIFL"))
                {
                    return;
                }
                else if (type.Equals("VTBF"))
                {
                    int dataEnd = (int)streamReader.BaseStream.Length;
                    StringBuilder output = new StringBuilder();
                    Dictionary<string, List<int>> tagTracker = new Dictionary<string, List<int>>();

                    //Seek past vtbf tag
                    streamReader.Seek(0x8, SeekOrigin.Current);          //VTBF 
                    output.AppendLine(Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Read<int>())) + ":"); //Type
                    streamReader.Seek(0x4, SeekOrigin.Current); //0x1 short and 0x4C00 short. Seem to be constants.

                    while (streamReader.Position < dataEnd)
                    {
                        var data = ReadVTBFTag(streamReader, out string tagType, out int ptrCount, out int entryCount);

                        switch (tagType)
                        {
                            default:
                                //Data being null signfies that the last thing read wasn't a proper tag. This should mean the end of the VTBF stream if nothing else.
                                if (data == null)
                                {
                                    goto FINISH;
                                }
                                else
                                {
                                    if (!tagTracker.ContainsKey(tagType))
                                    {
                                        tagTracker.Add(tagType, new List<int>());
                                    }
                                    output.AppendLine("");
                                    output.AppendLine($"{tagType} Pointers: {ptrCount} # of entries: {entryCount}");

                                    //Loop through data
                                    for (int dictId = 0; dictId < data.Count; dictId++)
                                    {
                                        var dict = data[dictId];

                                        //Many VTBF tags contain arrays of their data, but many don't. If it does we want to count those ids. We store both as a list here either way.
                                        if (data.Count > 1)
                                        {
                                            output.AppendLine("");
                                            output.AppendLine($"Set {dictId}:");
                                        }

                                        //Loop through values
                                        foreach (var pair in dict)
                                        {
                                            if (!tagTracker[tagType].Contains(pair.Key))
                                            {
                                                tagTracker[tagType].Add(pair.Key);
                                            }

                                            output.Append(pair.Key.ToString("X") + ": ");

                                            //VTBF data here can be either an array or a value of an arbitrary type. We check here if we have to iterate or not.
                                            if (pair.Value is System.Collections.ICollection)
                                            {
                                                output.Append("");
                                                string line = "";
                                                dynamic arr = pair.Value;
                                                bool first = true;
                                                foreach (var obj in arr)
                                                {
                                                    if (first == true)
                                                    {
                                                        first = false;
                                                    }
                                                    else
                                                    {
                                                        if (line == "")
                                                        {
                                                            output.AppendLine(",");
                                                        }
                                                        else
                                                        {
                                                            line += ", ";
                                                        }
                                                    }
                                                    if (obj is System.Collections.ICollection)
                                                    {
                                                        line += "<";
                                                        for (int num = 0; num < obj.Length; num++)
                                                        {
                                                            line += obj[num];
                                                            if (num + 1 != obj.Length)
                                                            {
                                                                line += ", ";
                                                            }
                                                        }
                                                        line += ">";
                                                    }
                                                    else
                                                    {
                                                        line += obj.ToString();
                                                    }

                                                    if (line.Length > 80)
                                                    {
                                                        output.Append(line);
                                                        line = "";
                                                    }
                                                }

                                                //Flush the rest here if we haven't yet
                                                if (line.Length < 80 && line != "")
                                                {
                                                    output.AppendLine(line);
                                                }
                                                else if (line == "")
                                                {
                                                    output.AppendLine("");
                                                }

                                                //Handle for potential strings since we can't easily differentiate them (Sometimes they tried to convert unicode to strings and it gets weird)
                                                if (pair.Value is byte[] && ((byte[])pair.Value).Length <= 0x30)
                                                {
                                                    output.AppendLine(pair.Key.ToString("X") + "(string): " + Encoding.UTF8.GetString(((byte[])pair.Value)));
                                                }
                                            }
                                            else
                                            {
                                                output.AppendLine(pair.Value.ToString());
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }

                FINISH:
                    output.AppendLine("");
                    output.AppendLine("*******************************");
                    output.AppendLine("Sub tag tracking per tag");
                    output.AppendLine("");
                    foreach (var pair in tagTracker)
                    {
                        output.Append(pair.Key + ": ");
                        string line = "";
                        bool first = true;
                        foreach (var num in pair.Value)
                        {
                            if (first == true)
                            {
                                first = false;
                            }
                            else
                            {
                                if (line == "")
                                {
                                    output.AppendLine(",");
                                }
                                else
                                {
                                    line += ", ";
                                }
                            }
                            line += num.ToString("X");

                            if (line.Length > 80)
                            {
                                output.Append(line);
                                line = "";
                            }
                        }
                        //Flush the rest here if we haven't yet
                        if (line.Length < 80 && line != "")
                        {
                            output.AppendLine(line);
                        }
                        else if (line == "")
                        {
                            output.AppendLine("");
                        }

                    }

                    File.WriteAllText(fileName + ".txt", output.ToString());
                }
                else
                {
                    File.WriteAllText(fileName + ".txt", $"{fileName} was not a VTBF...");
                }
            }
        }

        public static byte[] ToAQGFVTBF()
        {
            List<byte> outBytes = new List<byte>();

            outBytes.AddRange(new byte[] { 0x56, 0x54, 0x42, 0x46 }); //VTBF
            outBytes.AddRange(new byte[] { 0x10, 0, 0, 0 });
            outBytes.AddRange(new byte[] { 0x41, 0x51, 0x47, 0x46, 0x1, 0, 0, 0x4C }); //AQGF and the constants after

            return outBytes.ToArray();
        }

        public static byte[] ToROOT(string rootString = "hnd2aqg ver.1.61 Build: Feb 28 2012 18:46:06")
        {
            List<byte> outBytes = new List<byte>();

            AddBytes(outBytes, 0x0, 0x2, (byte)rootString.Length, Encoding.UTF8.GetBytes(rootString));
            WriteTagHeader(outBytes, "ROOT", 1, 1);

            return outBytes.ToArray();
        }

        //Safely retrieves objects in the case that they don't exist in the given dictionary
        public static T GetObject<T>(Dictionary<int, object> dict, int key)
        {
            if (dict.ContainsKey(key))
            {
                return (T)dict[key];
            }
            else
            {
                return default(T);
            }
        }

        public static void WriteTagHeader(List<byte> outBytes, string tagString, ushort pointerCount, ushort subtagCount)
        {
            outBytes.InsertRange(0, Encoding.UTF8.GetBytes(tagString));
            outBytes.InsertRange(0x4, BitConverter.GetBytes(pointerCount));  //Pointer count
            outBytes.InsertRange(0x6, BitConverter.GetBytes(subtagCount)); //Subtag count

            outBytes.InsertRange(0, BitConverter.GetBytes(outBytes.Count)); //Data body size
            outBytes.InsertRange(0, Encoding.UTF8.GetBytes("vtc0"));
        }

        public static int GetListOfListOfIntsIntCount(List<List<int>> intListlist)
        {
            int count = 0;
            for (int i = 0; i < intListlist.Count; i++)
            {
                count += intListlist[i].Count;
            }

            return count;
        }

        public static void HandleOptionalArrayHeader(List<byte> outBytes, byte subTagID, int vertIdCount, byte baseDataType)
        {
            outBytes.Add(subTagID);
            if (vertIdCount > 1)
            {
                outBytes.Add((byte)(baseDataType + 0x80));
                if (vertIdCount - 1 > byte.MaxValue)
                {
                    if (vertIdCount - 1 > ushort.MaxValue)
                    {
                        outBytes.Add(0x18);
                        outBytes.AddRange(BitConverter.GetBytes(vertIdCount - 1));
                    }
                    else
                    {
                        outBytes.Add(0x10);
                        outBytes.AddRange(BitConverter.GetBytes((ushort)(vertIdCount - 1)));
                    }
                }
                else
                {
                    outBytes.Add(0x08);
                    outBytes.Add((byte)(vertIdCount - 1));
                }
            }
            else
            {
                outBytes.Add(baseDataType);
            }
        }

        public static void AddBytes(List<byte> outBytes, byte id, byte dataType, byte[] data)
        {
            outBytes.Add(id);
            outBytes.Add(dataType);
            outBytes.AddRange(data);
        }

        public static void AddBytes(List<byte> outBytes, byte id, byte dataType, byte vecAmt, byte[] data)
        {
            outBytes.Add(id);
            outBytes.Add(dataType);
            outBytes.Add(vecAmt);
            outBytes.AddRange(data);
        }

        public static void AddBytes(List<byte> outBytes, byte id, byte dataType, byte subDataType, byte subDataAdditions, byte[] data)
        {
            outBytes.Add(id);
            outBytes.Add(dataType);
            outBytes.Add(subDataType);
            outBytes.Add(subDataAdditions);
            outBytes.AddRange(data);
        }

        public static ushort VTBFFlagCheck(int check)
        {
            if (check > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
