using AquaModelLibrary.Extensions.Readers;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace AquaModelLibrary.Helpers.PSO2
{
    public unsafe class VTBFMethods
    {
        public static List<Dictionary<int, object>> ReadVTBFTag(BufferedStreamReaderBE<MemoryStream> streamReader, out string tagString, out int ptrCount, out int entryCount)
        {
            List<Dictionary<int, object>> vtbfData = new List<Dictionary<int, object>>();
            bool listType = false;

            string vtc0 = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Read<int>())); //vtc0
            if (vtc0 != "vtc0")
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

        public static List<NODE> parseNODE(List<Dictionary<int, object>> nodeRaw)
        {
            List<NODE> nodeList = new List<NODE>();

            for (int i = 0; i < nodeRaw.Count; i++)
            {
                NODE node = new NODE();

                byte[] shorts = BitConverter.GetBytes((int)nodeRaw[i][0x03]);
                node.boneShort1 = (ushort)(shorts[0] * 0x100 + shorts[1]);
                node.boneShort2 = (ushort)(shorts[2] * 0x100 + shorts[3]);
                node.animatedFlag = (int)nodeRaw[i][0x0B];
                node.parentId = (int)nodeRaw[i][0x04];
                node.unkNode = (int)nodeRaw[i][0x0F];
                node.firstChild = (int)nodeRaw[i][0x05];
                node.nextSibling = (int)nodeRaw[i][0x06];
                node.const0_2 = (int)nodeRaw[i][0x0C];
                node.pos = (Vector3)nodeRaw[i][0x07];
                node.eulRot = (Vector3)nodeRaw[i][0x08];
                node.scale = (Vector3)nodeRaw[i][0x09];
                node.m1 = ((Vector4[])nodeRaw[i][0x0A])[0];
                node.m2 = ((Vector4[])nodeRaw[i][0x0A])[1];
                node.m3 = ((Vector4[])nodeRaw[i][0x0A])[2];
                node.m4 = ((Vector4[])nodeRaw[i][0x0A])[3];
                node.boneName = new AquaCommon.PSO2String();
                node.boneName.SetBytes((byte[])nodeRaw[i][0x0D]);

                nodeList.Add(node);
            }

            return nodeList;
        }

        public static byte[] toNODE(List<NODE> nodeList)
        {
            List<byte> outBytes = new List<byte>();

            for (int i = 0; i < nodeList.Count; i++)
            {
                NODE node = nodeList[i];
                if (i == 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                }
                else
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }
                AddBytes(outBytes, 0x3, 0x9, BitConverter.GetBytes(node.boneShort1 * 0x10000 + node.boneShort2));
                AddBytes(outBytes, 0x4, 0x8, BitConverter.GetBytes(node.parentId));
                AddBytes(outBytes, 0xF, 0x8, BitConverter.GetBytes(node.unkNode));
                AddBytes(outBytes, 0x5, 0x8, BitConverter.GetBytes(node.firstChild));
                AddBytes(outBytes, 0x6, 0x8, BitConverter.GetBytes(node.nextSibling));
                AddBytes(outBytes, 0x7, 0x4A, 0x1, ConvertStruct(node.pos));
                AddBytes(outBytes, 0x8, 0x4A, 0x1, ConvertStruct(node.eulRot));
                AddBytes(outBytes, 0x9, 0x4A, 0x1, ConvertStruct(node.scale));
                AddBytes(outBytes, 0xA, 0xCA, 0xA, 0x3, ConvertStruct(node.m1));
                outBytes.AddRange(ConvertStruct(node.m2));
                outBytes.AddRange(ConvertStruct(node.m3));
                outBytes.AddRange(ConvertStruct(node.m4));
                AddBytes(outBytes, 0xB, 0x9, BitConverter.GetBytes(node.animatedFlag));
                AddBytes(outBytes, 0xC, 0x8, BitConverter.GetBytes(node.const0_2));

                //Bone Name String
                string boneNameStr = node.boneName.GetString();
                AddBytes(outBytes, 0x80, 0x02, (byte)boneNameStr.Length, Encoding.UTF8.GetBytes(boneNameStr));
            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            WriteTagHeader(outBytes, "NODE", 0, (ushort)(nodeList.Count * 0xC + 1));

            return outBytes.ToArray();
        }

        public static List<NODO> parseNODO(List<Dictionary<int, object>> nodoRaw)
        {
            List<NODO> nodoList = new List<NODO>();

            if (nodoRaw[0].Keys.Count > 1)
            {
                for (int i = 0; i < nodoRaw.Count; i++)
                {
                    NODO nodo = new NODO();

                    byte[] shorts = BitConverter.GetBytes((int)nodoRaw[i][0x03]);
                    nodo.boneShort1 = (ushort)(shorts[0] * 0x100 + shorts[1]);
                    nodo.boneShort2 = (ushort)(shorts[2] * 0x100 + shorts[3]);
                    nodo.animatedFlag = (int)nodoRaw[i][0x0B];
                    nodo.parentId = (int)nodoRaw[i][0x04];
                    nodo.pos = (Vector3)nodoRaw[i][0x07];
                    nodo.eulRot = (Vector3)nodoRaw[i][0x08];
                    nodo.boneName = new AquaCommon.PSO2String();
                    nodo.boneName.SetBytes((byte[])nodoRaw[i][0x0D]);

                    nodoList.Add(nodo);
                }
            }

            return nodoList;
        }

        public static byte[] toNODO(List<NODO> nodoList)
        {
            List<byte> outBytes = new List<byte>();

            for (int i = 0; i < nodoList.Count; i++)
            {
                NODO nodo = nodoList[i];
                if (i == 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                }
                else
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }
                AddBytes(outBytes, 0x3, 0x9, BitConverter.GetBytes(nodo.boneShort1 * 0x10000 + nodo.boneShort2));
                AddBytes(outBytes, 0x4, 0x8, BitConverter.GetBytes(nodo.parentId));
                AddBytes(outBytes, 0x7, 0x4A, 0x1, ConvertStruct(nodo.pos));
                AddBytes(outBytes, 0x8, 0x4A, 0x1, ConvertStruct(nodo.eulRot));
                AddBytes(outBytes, 0xB, 0x9, BitConverter.GetBytes(nodo.animatedFlag));

                //Bone Name String
                string boneNameStr = nodo.boneName.GetString();
                AddBytes(outBytes, 0x80, 0x02, (byte)boneNameStr.Length, Encoding.UTF8.GetBytes(boneNameStr));
            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            WriteTagHeader(outBytes, "NODO", 0, (ushort)(nodoList.Count * 7 + 1));

            return outBytes.ToArray();
        }

        public static MOHeader parseMOHeader(List<Dictionary<int, object>> moRaw)
        {
            MOHeader moHeader = new MOHeader();

            moHeader.variant = (int)moRaw[0][0xE0];
            moHeader.loopPoint = (int)moRaw[0][0xE1];
            moHeader.endFrame = (int)moRaw[0][0xE2];
            moHeader.frameSpeed = (float)moRaw[0][0xE3];
            moHeader.unkInt0 = (int)moRaw[0][0xE4];
            moHeader.nodeCount = (int)moRaw[0][0xE5];
            moHeader.testString = new AquaCommon.PSO2String();
            moHeader.testString.SetBytes((byte[])moRaw[0][0xE6]);

            return moHeader;
        }

        public static byte[] toMOHeader(MOHeader moHeader, string motionType)
        {
            List<byte> outBytes = new List<byte>();

            AddBytes(outBytes, 0xE0, 0x9, BitConverter.GetBytes(moHeader.variant));
            AddBytes(outBytes, 0xE1, 0x9, BitConverter.GetBytes(moHeader.loopPoint));
            AddBytes(outBytes, 0xE2, 0x9, BitConverter.GetBytes(moHeader.endFrame));
            AddBytes(outBytes, 0xE3, 0x9, BitConverter.GetBytes(moHeader.frameSpeed));
            AddBytes(outBytes, 0xE4, 0x9, BitConverter.GetBytes(moHeader.unkInt0));
            AddBytes(outBytes, 0xE5, 0x9, BitConverter.GetBytes(moHeader.nodeCount));

            //Test String
            string testStr = moHeader.testString.GetString();
            AddBytes(outBytes, 0xE6, 0x02, (byte)testStr.Length, Encoding.UTF8.GetBytes(testStr));

            WriteTagHeader(outBytes, motionType, (ushort)moHeader.nodeCount, 0x7);

            return outBytes.ToArray();
        }

        public static MOHeader parseNDMO(List<Dictionary<int, object>> ndmoRaw)
        {
            return parseMOHeader(ndmoRaw);
        }

        public static byte[] toNDMO(MOHeader moHeader)
        {
            return toMOHeader(moHeader, "NDMO");
        }

        public static MOHeader parseSPMO(List<Dictionary<int, object>> ndmoRaw)
        {
            return parseMOHeader(ndmoRaw);
        }

        public static byte[] toSPMO(MOHeader moHeader)
        {
            return toMOHeader(moHeader, "SPMO");
        }
        public static MOHeader parseCAMO(List<Dictionary<int, object>> ndmoRaw)
        {
            return parseMOHeader(ndmoRaw);
        }

        public static byte[] toCAMO(MOHeader moHeader)
        {
            return toMOHeader(moHeader, "CAMO");
        }

        public static MSEG parseMSEG(List<Dictionary<int, object>> msegRaw)
        {
            MSEG mseg = new MSEG();

            mseg.nodeType = (int)msegRaw[0][0xE7];
            mseg.nodeDataCount = (int)msegRaw[0][0xE8];
            mseg.nodeName = new AquaCommon.PSO2String();
            mseg.nodeName.SetBytes((byte[])msegRaw[0][0xE9]);
            mseg.nodeId = (int)msegRaw[0][0xEA];

            return mseg;
        }

        public static byte[] toMSEG(MSEG mseg)
        {
            List<byte> outBytes = new List<byte>();

            AddBytes(outBytes, 0xE7, 0x9, BitConverter.GetBytes(mseg.nodeType));
            AddBytes(outBytes, 0xE8, 0x9, BitConverter.GetBytes(mseg.nodeDataCount));

            //Node name
            string nodeStr = mseg.nodeName.GetString();
            AddBytes(outBytes, 0xE9, 0x2, (byte)nodeStr.Length, Encoding.UTF8.GetBytes(nodeStr));

            AddBytes(outBytes, 0xEA, 0x9, BitConverter.GetBytes(mseg.nodeDataCount));

            WriteTagHeader(outBytes, "MSEG", 0x3, 0x4);

            return outBytes.ToArray();
        }

        public static MKEY parseMKEY(List<Dictionary<int, object>> mkeyRaw)
        {
            MKEY mkey = new MKEY();
            mkey.keyType = (int)mkeyRaw[0][0xEB];
            mkey.dataType = (int)mkeyRaw[0][0xEC];
            mkey.unkInt0 = (int)mkeyRaw[0][0xF0];
            mkey.keyCount = (int)mkeyRaw[0][0xED];

            //Get frame timings. Seemingly may not store a frame timing if there's only one frame.
            if (mkey.keyCount > 1)
            {
                for (int j = 0; j < mkey.keyCount; j++)
                {
                    mkey.frameTimings.Add(((ushort[])mkeyRaw[0][0xEF])[j]);
                }
            }
            else if (mkeyRaw[0].ContainsKey(0xEF))
            {
                mkey.frameTimings.Add((ushort)mkeyRaw[0][0xEF]);
            }

            //Get frames. The data types stored are different depending on the key count.
            switch (mkey.dataType)
            {
                //0x1 and 0x3 are Vector4 arrays essentially. 0x1 is seemingly a Vector3 with alignment padding, but could potentially have things.
                case 0x1:
                case 0x2:
                case 0x3:
                    if (mkey.keyCount > 1)
                    {
                        for (int j = 0; j < mkey.keyCount; j++)
                        {
                            mkey.vector4Keys.Add(((Vector4[])mkeyRaw[0][0xEE])[j]);
                        }
                    }
                    else
                    {
                        mkey.vector4Keys.Add((Vector4)mkeyRaw[0][0xEE]);
                    }
                    break;
                case 0x5:
                    if (mkey.keyCount > 1)
                    {
                        if (mkeyRaw[0].ContainsKey(0xF3))
                        {
                            for (int j = 0; j < mkey.keyCount; j++)
                            {
                                mkey.intKeys.Add(((int[])mkeyRaw[0][0xF3])[j]);
                            }
                        }
                        else
                        {
                            for (int j = 0; j < mkey.keyCount * 4; j += 4)
                            {
                                mkey.intKeys.Add(BitConverter.ToInt32(((byte[])mkeyRaw[0][0xF2]), j));
                            }
                        }
                    }
                    else
                    {
                        if (mkeyRaw[0].ContainsKey(0xF3))
                        {
                            mkey.intKeys.Add((int)mkeyRaw[0][0xF3]);
                        }
                        else
                        {
                            for (int j = 0; j < mkey.keyCount * 4; j += 4)
                            {
                                mkey.intKeys.Add(BitConverter.ToInt32(((byte[])mkeyRaw[0][0xF2]), j));
                            }
                        }
                    }
                    break;
                //0x4 is texture/uv related, 0x6 is Camera related - Array of floats. 0x4 seems to be used for every .aqv frame set interestingly
                case 0x4:
                case 0x6:
                    if (mkey.keyCount > 1)
                    {
                        for (int j = 0; j < mkey.keyCount; j++)
                        {
                            mkey.floatKeys.Add(((float[])mkeyRaw[0][0xF1])[j]);
                        }
                    }
                    else
                    {
                        mkey.floatKeys.Add((float)mkeyRaw[0][0xF1]);
                    }
                    break;
                default:
                    throw new Exception($"Unexpected data type: {mkey.dataType}");
            }

            return mkey;
        }

        public static byte[] toMKEY(MKEY mkey)
        {
            List<byte> outBytes = new List<byte>();

            AddBytes(outBytes, 0xEB, 0x9, BitConverter.GetBytes(mkey.keyType));
            AddBytes(outBytes, 0xEC, 0x9, BitConverter.GetBytes(mkey.dataType));
            AddBytes(outBytes, 0xF0, 0x9, BitConverter.GetBytes(mkey.unkInt0));
            AddBytes(outBytes, 0xED, 0x9, BitConverter.GetBytes(mkey.keyCount));

            //Set frame timings. The data types stored are different depending on the key count
            handleOptionalArrayHeader(outBytes, 0xEF, mkey.keyCount, 0x06);
            //Write the actual timings
            for (int j = 0; j < mkey.frameTimings.Count; j++)
            {
                outBytes.AddRange(BitConverter.GetBytes((ushort)mkey.frameTimings[j]));
            }

            //Write frame data. Types will vary.
            switch (mkey.dataType)
            {
                //0x1, 0x2, and 0x3 are Vector4 arrays essentially. 0x1 is seemingly a Vector3 with alignment padding, but could potentially have things.
                case 0x1:
                case 0x2:
                case 0x3:
                    handleOptionalArrayHeader(outBytes, 0xEE, mkey.keyCount, 0x4A);
                    for (int j = 0; j < mkey.frameTimings.Count; j++)
                    {
                        outBytes.AddRange(ConvertStruct(mkey.vector4Keys[j]));
                    }
                    break;
                case 0x5:
                    handleOptionalArrayHeader(outBytes, 0xF3, mkey.keyCount, 0x48);
                    if (mkey.intKeys.Count > 0)
                    {
                        for (int j = 0; j < mkey.frameTimings.Count; j++)
                        {

                            outBytes.AddRange(BitConverter.GetBytes(mkey.intKeys[j]));
                        }
                    }
                    else
                    {
                        outBytes.AddRange(mkey.byteKeys);
                    }
                    break;
                //0x4 is texture/uv related, 0x6 is Camera related - Array of floats. 0x4 seems to be used for every .aqv frame set interestingly
                case 0x4:
                case 0x6:
                    handleOptionalArrayHeader(outBytes, 0xF1, mkey.keyCount, 0xA);
                    for (int j = 0; j < mkey.frameTimings.Count; j++)
                    {
                        outBytes.AddRange(BitConverter.GetBytes(mkey.floatKeys[j]));
                    }
                    break;
                default:
                    throw new Exception("Unexpected data type!");
            }

            return outBytes.ToArray();
        }

        public static BODYObject parseBODY(List<Dictionary<int, object>> bodyRaw)
        {
            BODYObject body = new BODYObject();
            body.body.id = (int)bodyRaw[0][0xFF];

            body.dataString = PSO2String.GeneratePSO2String(GetObject<byte[]>(bodyRaw[0], 0)).GetString();
            body.texString1 = PSO2String.GeneratePSO2String(GetObject<byte[]>(bodyRaw[0], 1)).GetString();
            body.texString2 = PSO2String.GeneratePSO2String(GetObject<byte[]>(bodyRaw[0], 2)).GetString();
            body.texString3 = PSO2String.GeneratePSO2String(GetObject<byte[]>(bodyRaw[0], 3)).GetString();
            body.texString4 = PSO2String.GeneratePSO2String(GetObject<byte[]>(bodyRaw[0], 4)).GetString();
            body.texString5 = PSO2String.GeneratePSO2String(GetObject<byte[]>(bodyRaw[0], 5)).GetString();
            //TexString6 seemingly isn't stored in vtbf? Might correct later if that's not really the case.

            body.body2.int_24_0x9_0x9 = GetObject<int>(bodyRaw[0], 0x9);
            body.body2.costumeSoundId = GetObject<int>(bodyRaw[0], 0xA);
            body.body2.headId = GetObject<int>(bodyRaw[0], 0xD);
            body.body2.legLength = GetObject<float>(bodyRaw[0], 0x8);
            body.body2.float_4C_0xB = GetObject<float>(bodyRaw[0], 0xB);

            //Todo, handle default junk data added with NIFL

            return body;
        }

        public static BODYObject parseCARM(List<Dictionary<int, object>> carmRaw)
        {
            return parseBODY(carmRaw);
        }

        public static BODYObject parseCLEG(List<Dictionary<int, object>> clegRaw)
        {
            return parseBODY(clegRaw);
        }

        public static byte[] toBODY(BODYObject body)
        {
            List<byte> outBytes = new List<byte>();

            AddBytes(outBytes, 0xFF, 0x8, BitConverter.GetBytes(body.body.id));

            string dataStr = body.dataString;
            AddBytes(outBytes, 0x00, 0x2, (byte)dataStr.Length, Encoding.UTF8.GetBytes(dataStr));
            string texStr1 = body.texString1;
            AddBytes(outBytes, 0x01, 0x2, (byte)texStr1.Length, Encoding.UTF8.GetBytes(texStr1));
            string texStr2 = body.texString2;
            AddBytes(outBytes, 0x02, 0x2, (byte)texStr2.Length, Encoding.UTF8.GetBytes(texStr2));
            string texStr3 = body.texString3;
            AddBytes(outBytes, 0x03, 0x2, (byte)texStr3.Length, Encoding.UTF8.GetBytes(texStr3));
            string texStr4 = body.texString4;
            AddBytes(outBytes, 0x04, 0x2, (byte)texStr4.Length, Encoding.UTF8.GetBytes(texStr4));
            string texStr5 = body.texString5;
            AddBytes(outBytes, 0x05, 0x2, (byte)texStr5.Length, Encoding.UTF8.GetBytes(texStr5));

            AddBytes(outBytes, 0xA, 0x8, BitConverter.GetBytes(body.body2.costumeSoundId));
            AddBytes(outBytes, 0xB, 0xA, BitConverter.GetBytes(body.body2.float_4C_0xB));
            AddBytes(outBytes, 0xC, 0x9, BitConverter.GetBytes((int)0));
            AddBytes(outBytes, 0x6, 0x6, BitConverter.GetBytes((ushort)0x2));
            AddBytes(outBytes, 0x7, 0x6, BitConverter.GetBytes((ushort)0x0));
            AddBytes(outBytes, 0x8, 0xA, BitConverter.GetBytes(body.body2.legLength));
            AddBytes(outBytes, 0x9, 0x9, BitConverter.GetBytes(body.body2.int_24_0x9_0x9));

            WriteTagHeader(outBytes, "BODY", 0x0, 0xE);

            return outBytes.ToArray();
        }

        public static BBLYObject parseBBLY(List<Dictionary<int, object>> bblyRaw)
        {
            BBLYObject bbly = new BBLYObject();
            bbly.bbly.id = (int)bblyRaw[0][0xFF];

            bbly.texString1 = PSO2String.GeneratePSO2String(GetObject<byte[]>(bblyRaw[0], 0x10)).GetString();
            bbly.texString2 = PSO2String.GeneratePSO2String(GetObject<byte[]>(bblyRaw[0], 0x11)).GetString();
            bbly.texString3 = PSO2String.GeneratePSO2String(GetObject<byte[]>(bblyRaw[0], 0x12)).GetString();
            bbly.texString4 = PSO2String.GeneratePSO2String(GetObject<byte[]>(bblyRaw[0], 0x13)).GetString();

            //Todo, handle default junk data added with NIFL

            return bbly;
        }

        public static byte[] toBBLY(BBLYObject bbly)
        {
            List<byte> outBytes = new List<byte>();

            AddBytes(outBytes, 0xFF, 0x8, BitConverter.GetBytes(bbly.bbly.id));

            string texStr1 = bbly.texString1;
            AddBytes(outBytes, 0x01, 0x2, (byte)texStr1.Length, Encoding.UTF8.GetBytes(texStr1));
            string texStr2 = bbly.texString2;
            AddBytes(outBytes, 0x02, 0x2, (byte)texStr2.Length, Encoding.UTF8.GetBytes(texStr2));
            string texStr3 = bbly.texString3;
            AddBytes(outBytes, 0x03, 0x2, (byte)texStr3.Length, Encoding.UTF8.GetBytes(texStr3));
            string texStr4 = bbly.texString4;
            AddBytes(outBytes, 0x04, 0x2, (byte)texStr4.Length, Encoding.UTF8.GetBytes(texStr4));
            string texStr5 = bbly.texString5;
            AddBytes(outBytes, 0x05, 0x2, (byte)texStr5.Length, Encoding.UTF8.GetBytes(texStr5));

            WriteTagHeader(outBytes, "BBLY", 0x0, 0xE);

            return outBytes.ToArray();
        }

        //NIFL cmx lumps this in with BBLY
        public static BBLYObject parseBDP1(List<Dictionary<int, object>> bblyRaw)
        {
            BBLYObject bbly = new BBLYObject();
            bbly.bbly.id = (int)bblyRaw[0][0xFF];

            bbly.texString1 = PSO2String.GeneratePSO2String(GetObject<byte[]>(bblyRaw[0], 0x10)).GetString();
            bbly.texString2 = PSO2String.GeneratePSO2String(GetObject<byte[]>(bblyRaw[0], 0x11)).GetString();
            bbly.texString3 = PSO2String.GeneratePSO2String(GetObject<byte[]>(bblyRaw[0], 0x12)).GetString();
            bbly.texString4 = PSO2String.GeneratePSO2String(GetObject<byte[]>(bblyRaw[0], 0x13)).GetString();

            //Todo, handle default junk data added with NIFL

            return bbly;
        }

        public static StickerObject parseBDP2(List<Dictionary<int, object>> bdp2Raw)
        {
            StickerObject bdp2 = new StickerObject();
            bdp2.sticker.id = (int)bdp2Raw[0][0xFF];

            bdp2.texString = PSO2String.GeneratePSO2String(GetObject<byte[]>(bdp2Raw[0], 0x20)).GetString();
            bdp2.sticker.reserve0 = GetObject<int>(bdp2Raw[0], 0x21);

            return bdp2;
        }

        public static FACEObject parseFACE(List<Dictionary<int, object>> faceRaw)
        {
            FACEObject face = new FACEObject();
            face.face.id = (int)faceRaw[0][0xFF];

            face.dataString = PSO2String.GeneratePSO2String(GetObject<byte[]>(faceRaw[0], 0x70)).GetString();
            face.face2.unkInt3 = GetObject<int>(faceRaw[0], 0x71);
            face.texString1 = PSO2String.GeneratePSO2String(GetObject<byte[]>(faceRaw[0], 0x72)).GetString();
            face.texString2 = PSO2String.GeneratePSO2String(GetObject<byte[]>(faceRaw[0], 0x73)).GetString();
            face.texString3 = PSO2String.GeneratePSO2String(GetObject<byte[]>(faceRaw[0], 0x74)).GetString();
            face.texString4 = PSO2String.GeneratePSO2String(GetObject<byte[]>(faceRaw[0], 0x75)).GetString();
            face.texString5 = PSO2String.GeneratePSO2String(GetObject<byte[]>(faceRaw[0], 0x76)).GetString();

            //0x77 seemingly isn't parsed
            //0x78 seemingly isn't parsed
            face.face2.unkInt5 = GetObject<int>(faceRaw[0], 0x79);

            return face;
        }

        public static FCMNObject parseFCMN(List<Dictionary<int, object>> fcmnRaw)
        {
            FCMNObject fcmn = new FCMNObject();
            fcmn.fcmn.id = (int)fcmnRaw[0][0xFF];
            fcmn.proportionAnim = PSO2String.GeneratePSO2String(GetObject<byte[]>(fcmnRaw[0], 0xC0)).GetString();
            fcmn.faceAnim1 = PSO2String.GeneratePSO2String(GetObject<byte[]>(fcmnRaw[0], 0xC1)).GetString();

            PSO2String[] faceAnims = new PSO2String[8];
            if (fcmnRaw.Count > 1)
            {
                for (int i = 1; i < fcmnRaw.Count; i++)
                {
                    faceAnims[i - 1] = PSO2String.GeneratePSO2String(GetObject<byte[]>(fcmnRaw[0], 0xC1));
                }
            }
            fcmn.faceAnim2 = faceAnims[0].GetString();
            fcmn.faceAnim3 = faceAnims[1].GetString();
            fcmn.faceAnim4 = faceAnims[2].GetString();
            fcmn.faceAnim5 = faceAnims[3].GetString();
            fcmn.faceAnim6 = faceAnims[4].GetString();
            fcmn.faceAnim7 = faceAnims[5].GetString();
            fcmn.faceAnim8 = faceAnims[6].GetString();
            fcmn.faceAnim9 = faceAnims[7].GetString();

            return fcmn;
        }

        public static FCPObject parseFCP1(List<Dictionary<int, object>> fcp1Raw)
        {
            FCPObject fcp = new FCPObject();
            fcp.fcp.id = (int)fcp1Raw[0][0xFF];

            fcp.texString1 = PSO2String.GeneratePSO2String(GetObject<byte[]>(fcp1Raw[0], 0x80)).GetString();
            fcp.texString2 = PSO2String.GeneratePSO2String(GetObject<byte[]>(fcp1Raw[0], 0x81)).GetString();
            fcp.texString3 = PSO2String.GeneratePSO2String(GetObject<byte[]>(fcp1Raw[0], 0x82)).GetString();
            fcp.texString4 = PSO2String.GeneratePSO2String(GetObject<byte[]>(fcp1Raw[0], 0x83)).GetString();

            return fcp;
        }

        public static FCPObject parseFCP2(List<Dictionary<int, object>> fcp2Raw)
        {
            FCPObject fcp = new FCPObject();
            fcp.fcp.id = (int)fcp2Raw[0][0xFF];

            fcp.texString1 = PSO2String.GeneratePSO2String(GetObject<byte[]>(fcp2Raw[0], 0x90)).GetString();
            fcp.texString2 = PSO2String.GeneratePSO2String(GetObject<byte[]>(fcp2Raw[0], 0x92)).GetString();
            fcp.texString3 = PSO2String.GeneratePSO2String(GetObject<byte[]>(fcp2Raw[0], 0x93)).GetString();

            return fcp;
        }

        public static EYEObject parseEYE(List<Dictionary<int, object>> eyeRaw)
        {
            EYEObject eye = new EYEObject();
            eye.eye.id = (int)eyeRaw[0][0xFF];

            eye.texString1 = PSO2String.GeneratePSO2String(GetObject<byte[]>(eyeRaw[0], 0x40)).GetString();
            eye.texString2 = PSO2String.GeneratePSO2String(GetObject<byte[]>(eyeRaw[0], 0x42)).GetString();
            eye.texString3 = PSO2String.GeneratePSO2String(GetObject<byte[]>(eyeRaw[0], 0x43)).GetString();
            eye.texString4 = PSO2String.GeneratePSO2String(GetObject<byte[]>(eyeRaw[0], 0x44)).GetString();

            return eye;
        }

        public static EYEBObject parseEYEB(List<Dictionary<int, object>> eyebRaw)
        {
            EYEBObject eyeb = new EYEBObject();
            eyeb.eyeb.id = (int)eyebRaw[0][0xFF];

            eyeb.texString1 = PSO2String.GeneratePSO2String(GetObject<byte[]>(eyebRaw[0], 0x50)).GetString();

            return eyeb;
        }

        public static EYEBObject parseEYEL(List<Dictionary<int, object>> eyelRaw)
        {
            EYEBObject eyel = new EYEBObject();
            eyel.eyeb.id = (int)eyelRaw[0][0xFF];

            eyel.texString1 = PSO2String.GeneratePSO2String(GetObject<byte[]>(eyelRaw[0], 0x60)).GetString();

            return eyel;
        }

        public static HAIRObject parseHAIR(List<Dictionary<int, object>> hairRaw)
        {
            HAIRObject hair = new HAIRObject();
            hair.hair.id = (int)hairRaw[0][0xFF];

            hair.dataString = PSO2String.GeneratePSO2String(GetObject<byte[]>(hairRaw[0], 0xA0)).GetString();
            hair.texString1 = PSO2String.GeneratePSO2String(GetObject<byte[]>(hairRaw[0], 0xA1)).GetString();
            hair.texString2 = PSO2String.GeneratePSO2String(GetObject<byte[]>(hairRaw[0], 0xA2)).GetString();
            hair.texString3 = PSO2String.GeneratePSO2String(GetObject<byte[]>(hairRaw[0], 0xA3)).GetString();
            hair.texString4 = PSO2String.GeneratePSO2String(GetObject<byte[]>(hairRaw[0], 0xA4)).GetString();
            hair.texString5 = PSO2String.GeneratePSO2String(GetObject<byte[]>(hairRaw[0], 0xA5)).GetString();
            //A8, A9, and AA don't seem to be stored in the NIFL 

            return hair;
        }

        public static BCLNObject parseLN(List<Dictionary<int, object>> lnRaw)
        {
            BCLNObject bcln = new BCLNObject();
            bcln.bcln = new BCLN();
            bcln.bcln.id = (int)lnRaw[0][0xFF];
            bcln.bcln.fileId = GetObject<int>(lnRaw[0], 0x1);
            bcln.bcln.unkInt = GetObject<int>(lnRaw[0], 0x2);

            return bcln;
        }

        public static VTBF_COLObject parseCOL(List<Dictionary<int, object>> colRaw)
        {
            VTBF_COLObject vtbfCol = new VTBF_COLObject();
            vtbfCol.vtbfCol = new VTBF_COL();
            if (colRaw[0].ContainsKey(0xFF))
            {
                vtbfCol.vtbfCol.id = (int)colRaw[0][0xFF];
            }
            else
            {
                vtbfCol.vtbfCol.id = -1;
            }
            vtbfCol.vtbfCol.utf8String = (byte[])colRaw[0][0x31];

            //Convert shorts to be read as utf16 string
            if (colRaw[0].ContainsKey(0x32))
            {
                var shorts = (short[])colRaw[0][0x32];
                var bytes = new byte[shorts.Length * 2];
                Buffer.BlockCopy(shorts, 0, bytes, 0, shorts.Length * 2);

                vtbfCol.vtbfCol.utf16String = bytes;
                vtbfCol.utf16Name = Encoding.Unicode.GetString(vtbfCol.vtbfCol.utf16String);
            }

            vtbfCol.vtbfCol.colorData = (byte[])colRaw[0][0x30];
            vtbfCol.utf8Name = Encoding.UTF8.GetString(vtbfCol.vtbfCol.utf8String);

            return vtbfCol;
        }

        public static ACCEObject parseACCE(List<Dictionary<int, object>> acceRaw)
        {
            ACCEObject acce = new ACCEObject();

            acce.acce.id = (int)acceRaw[0][0xFF];

            ACCE_12Object acc12A = new ACCE_12Object();
            ACCE_12Object acc12B = new ACCE_12Object();
            ACCE_12Object acc12C = new ACCE_12Object();
            acc12A.unkShort1 = GetObject<short>(acceRaw[0], 0xC7);
            acc12B.unkShort1 = GetObject<short>(acceRaw[0], 0xD7);
            acce.acce12List.Add(acc12A);
            acce.acce12List.Add(acc12B);
            acce.acce12List.Add(acc12C);

            acce.dataString = PSO2String.GeneratePSO2String(GetObject<byte[]>(acceRaw[0], 0xF0)).GetString();
            acce.nodeAttach1 = PSO2String.GeneratePSO2String(GetObject<byte[]>(acceRaw[0], 0xF1)).GetString();
            acce.nodeAttach2 = PSO2String.GeneratePSO2String(GetObject<byte[]>(acceRaw[0], 0xF2)).GetString();
            acce.nodeAttach3 = PSO2String.GeneratePSO2String(GetObject<byte[]>(acceRaw[0], 0xF3)).GetString();
            acce.nodeAttach4 = PSO2String.GeneratePSO2String(GetObject<byte[]>(acceRaw[0], 0xF4)).GetString();
            acce.nodeAttach5 = PSO2String.GeneratePSO2String(GetObject<byte[]>(acceRaw[0], 0xF5)).GetString();
            acce.nodeAttach6 = PSO2String.GeneratePSO2String(GetObject<byte[]>(acceRaw[0], 0xF6)).GetString();
            acce.nodeAttach7 = PSO2String.GeneratePSO2String(GetObject<byte[]>(acceRaw[0], 0xF7)).GetString();

            return acce;
        }

        public static EFCT parseEFCT(List<Dictionary<int, object>> efctRaw)
        {
            EFCT efct = new EFCT();
            efct.unkVec3_0 = GetObject<Vector3>(efctRaw[0], 0x10);
            efct.unkVec3_1 = GetObject<Vector3>(efctRaw[0], 0x11);
            efct.unkVec3_2 = GetObject<Vector3>(efctRaw[0], 0x12); //May not ever be used, but we're gonna check for it

            efct.float_30 = 1.0f;

            efct.startFrame = GetObject<int>(efctRaw[0], 0x1);
            efct.endFrame = GetObject<int>(efctRaw[0], 0x2);
            efct.int_48 = GetObject<int>(efctRaw[0], 0x3);

            var color = GetObject<byte[]>(efctRaw[0], 0x42);
            for (int i = 0; i < 0x4; i++)
            {
                if (i < color.Length)
                {
                    efct.color[i] = color[i];
                }
                else
                {
                    efct.color[i] = 0;
                }
            }

            efct.boolInt_54 = GetObject<byte>(efctRaw[0], 0x4);
            efct.boolInt_58 = GetObject<byte>(efctRaw[0], 0x0);
            efct.boolInt_5C = GetObject<byte>(efctRaw[0], 0x7);

            efct.float_60 = GetObject<int>(efctRaw[0], 0x91);
            efct.float_64 = GetObject<int>(efctRaw[0], 0x92);

            efct.soundName = PSO2Stringx30.GeneratePSO2String(GetObject<byte[]>(efctRaw[0], 0x90));

            return efct;
        }

        public static EMITObject parseEMIT(List<Dictionary<int, object>> emitRaw)
        {
            EMITObject emitObject = new EMITObject();
            var emit = new EMIT();

            emit.unkVec3_00 = GetObject<Vector3>(emitRaw[0], 0x10);
            emit.unkVec3_10 = GetObject<Vector3>(emitRaw[0], 0x11);
            emit.unkVec3_20 = GetObject<Vector3>(emitRaw[0], 0x13);
            emit.unkVec3_40 = GetObject<Vector3>(emitRaw[0], 0x12);
            emit.unkVec3_50 = GetObject<Vector3>(emitRaw[0], 0x14);
            emit.unkVec3_60 = GetObject<Vector3>(emitRaw[0], 0x15);

            emit.startFrame = GetObject<int>(emitRaw[0], 0x1);
            emit.endFrame = GetObject<int>(emitRaw[0], 0x2);
            emit.int_78 = GetObject<int>(emitRaw[0], 0x20);
            emit.float_7C = GetObject<int>(emitRaw[0], 0x21);

            emit.int_80 = GetObject<byte>(emitRaw[0], 0x43);
            emit.int_84 = GetObject<int>(emitRaw[0], 0x5);
            emit.int_88 = GetObject<byte>(emitRaw[0], 0x32);
            emit.float_8C = GetObject<float>(emitRaw[0], 0x37);

            emit.float_90 = GetObject<float>(emitRaw[0], 0x35);
            emit.int_94 = GetObject<byte>(emitRaw[0], 0x33);
            emit.float_98 = GetObject<float>(emitRaw[0], 0x37);

            emit.int_B8 = -1;
            emit.int_BC = -1;

            emit.unkVec3_D0 = GetObject<Vector3>(emitRaw[0], 0x19);
            emit.int_E0 = GetObject<int>(emitRaw[0], 0x86);

            emitObject.emit = emit;

            return emitObject;
        }

        public static PTCLObject parsePTCL(List<Dictionary<int, object>> ptclRaw)
        {
            PTCLObject ptclObject = new PTCLObject();
            var ptcl = new PTCL();

            ptcl.size = GetObject<Vector3>(ptclRaw[0], 0x19);
            ptcl.sizeRandom = GetObject<Vector3>(ptclRaw[0], 0x1A);
            ptcl.rotation = GetObject<Vector3>(ptclRaw[0], 0x11);
            ptcl.rotationRandom = GetObject<Vector3>(ptclRaw[0], 0x12);
            ptcl.rotationAdd = GetObject<Vector3>(ptclRaw[0], 0x14);
            ptcl.rotationAddRandom = GetObject<Vector3>(ptclRaw[0], 0x15);
            ptcl.direction = GetObject<Vector3>(ptclRaw[0], 0x44);
            ptcl.directionRandom = GetObject<Vector3>(ptclRaw[0], 0x45);
            ptcl.gravitationalAccel = GetObject<Vector3>(ptclRaw[0], 0x50);
            ptcl.externalAccel = GetObject<Vector3>(ptclRaw[0], 0x51);
            ptcl.externalAccelRandom = GetObject<Vector3>(ptclRaw[0], 0x5C);

            ptcl.float_B0 = GetObject<float>(ptclRaw[0], 0x56);
            ptcl.float_B4 = GetObject<float>(ptclRaw[0], 0x57);
            ptcl.float_B8 = GetObject<float>(ptclRaw[0], 0x52);
            ptcl.float_BC = GetObject<float>(ptclRaw[0], 0x53);

            ptcl.int_C0 = GetObject<int>(ptclRaw[0], 0x5);
            ptcl.float_C4 = GetObject<float>(ptclRaw[0], 0x2);
            ptcl.byte_C8 = GetObject<byte>(ptclRaw[0], 0x41);
            ptcl.byte_C9 = GetObject<byte>(ptclRaw[0], 0x98);
            ptcl.byte_CA = GetObject<byte>(ptclRaw[0], 0x43);
            ptcl.byte_CB = GetObject<byte>(ptclRaw[0], 0x4F);
            ptcl.float_CC = GetObject<float>(ptclRaw[0], 0x4E);

            ptcl.speed = GetObject<float>(ptclRaw[0], 0x46);
            ptcl.speedRandom = GetObject<float>(ptclRaw[0], 0x47);

            ptcl.float_E0 = 1.0f;

            var color = GetObject<byte[]>(ptclRaw[0], 0x42);
            for (int i = 0; i < 0x4; i++)
            {
                if (i < color.Length)
                {
                    ptcl.color[i] = color[i];
                }
                else
                {
                    ptcl.color[i] = 0;
                }
            }

            ptcl.int_F0 = GetObject<byte>(ptclRaw[0], 0x4B);
            ptcl.int_F4 = GetObject<short>(ptclRaw[0], 0x4C);
            ptcl.int_F8 = GetObject<short>(ptclRaw[0], 0x4D);
            ptcl.byte_FC = GetObject<byte>(ptclRaw[0], 0x61);
            ptcl.byte_FD = GetObject<byte>(ptclRaw[0], 0x62);
            ptcl.byte_FE = GetObject<byte>(ptclRaw[0], 0x67);
            ptcl.byte_FF = GetObject<byte>(ptclRaw[0], 0x5D);

            ptcl.int_100 = GetObject<byte>(ptclRaw[0], 0x64);
            ptcl.int_104 = GetObject<byte>(ptclRaw[0], 0x66);
            ptcl.int_108 = GetObject<byte>(ptclRaw[0], 0x68);
            ptcl.short_10C = GetObject<byte>(ptclRaw[0], 0x5E);
            ptcl.short_10E = GetObject<byte>(ptclRaw[0], 0x5F);

            ptcl.field_110 = GetObject<int>(ptclRaw[0], 0x6);
            ptcl.field_114 = GetObject<short>(ptclRaw[0], 0x55);

            ptcl.float_120 = GetObject<float>(ptclRaw[0], 0x54);
            ptcl.float_124 = GetObject<float>(ptclRaw[0], 0x48);
            ptcl.float_128 = GetObject<float>(ptclRaw[0], 0x49);
            ptcl.float_12C = GetObject<float>(ptclRaw[0], 0x4A);

            ptcl.float_130 = GetObject<float>(ptclRaw[0], 0x88);

            PTCLStrings strings = new PTCLStrings();
            strings.assetName.SetBytes(GetObject<byte[]>(ptclRaw[0], 0x34));
            strings.subDirectory.SetBytes(GetObject<byte[]>(ptclRaw[0], 0x40));
            strings.diffuseTex.SetBytes(GetObject<byte[]>(ptclRaw[0], 0x63));
            strings.opacityTex.SetBytes(GetObject<byte[]>(ptclRaw[0], 0x65));
            //strings.unkString //May be unused in vtbf?

            ptclObject.strings = strings;
            ptclObject.ptcl = ptcl;

            return ptclObject;
        }

        public static CURVObject parseCURV(List<Dictionary<int, object>> curvRaw)
        {
            CURVObject curvObject = new CURVObject();
            var curv = new CURV();

            curv.type = GetObject<byte>(curvRaw[0], 0x71);
            curv.startFrame = GetObject<float>(curvRaw[0], 0x74);
            curv.int_0C = GetObject<short>(curvRaw[0], 0x73);
            curv.float_10 = GetObject<float>(curvRaw[0], 0x77);

            curv.int_14 = GetObject<int>(curvRaw[0], 0x76);
            curv.endFrame = GetObject<float>(curvRaw[0], 0x75);

            curvObject.curv = curv;

            return curvObject;
        }

        public static List<KEYS> parseKEYS(List<Dictionary<int, object>> keysRaw)
        {
            List<KEYS> keyList = new List<KEYS>();

            for (int i = 0; i < keysRaw.Count; i++)
            {
                var keys = new KEYS();

                keys.type = GetObject<byte>(keysRaw[i], 0x72);
                if (keysRaw[i].TryGetValue(0x78, out object value) == true)
                {
                    if (value is float)
                    {
                        keys.time = (float)value;
                    }
                    else if (value is short)
                    {
                        keys.time = (short)value;
                    }
                }
                keys.value = GetObject<float>(keysRaw[i], 0x79);
                keys.inParam = GetObject<float>(keysRaw[i], 0x7A);

                keys.outParam = GetObject<float>(keysRaw[i], 0x7B);

                keyList.Add(keys);
            }

            return keyList;
        }
        
        public static ADDO parseADDO(List<Dictionary<int, object>> addoRaw)
        {
            ADDO addo = new ADDO();
            addo.id = GetObject<int>(addoRaw[0], 0xFF);
            addo.leftName.SetBytes(GetObject<byte[]>(addoRaw[0], 0xF0));
            addo.leftBoneAttach.SetBytes(GetObject<byte[]>(addoRaw[0], 0xF1));
            addo.rightName.SetBytes(GetObject<byte[]>(addoRaw[0], 0xF2));
            addo.rightBoneAttach.SetBytes(GetObject<byte[]>(addoRaw[0], 0xF3));
            addo.unusedLeftName2.SetBytes(GetObject<byte[]>(addoRaw[0], 0xF4));
            addo.unusedLeftBoneAttach2.SetBytes(GetObject<byte[]>(addoRaw[0], 0xF5));
            addo.unusedRightName2.SetBytes(GetObject<byte[]>(addoRaw[0], 0xF6));
            addo.unusedRightBoneAttach2.SetBytes(GetObject<byte[]>(addoRaw[0], 0xF7));
            addo.F8 = GetObject<byte>(addoRaw[0], 0xF8);
            addo.leftEffectName.SetBytes(GetObject<byte[]>(addoRaw[0], 0xF9));
            addo.rightEffectName.SetBytes(GetObject<byte[]>(addoRaw[0], 0xFA));
            addo.leftEffectAttach.SetBytes(GetObject<byte[]>(addoRaw[0], 0xFB));
            addo.rightEffectAttach.SetBytes(GetObject<byte[]>(addoRaw[0], 0xFC));
            addo.FD = GetObject<byte>(addoRaw[0], 0xFD);
            addo.FE = GetObject<byte>(addoRaw[0], 0xFE);
            addo.E0 = GetObject<byte>(addoRaw[0], 0xE0);
            addo.extraAttach.SetBytes(GetObject<byte[]>(addoRaw[0], 0xE1));

            return addo;
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
