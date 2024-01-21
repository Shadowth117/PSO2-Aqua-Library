using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Text;

namespace AquaModelLibrary.Data.BillyHatcher
{
    public class MesBin
    {
        //Control Codes//
        //0x62 - Line break
        //0x63 - Followed by a 0x31, starts an orange highlight in a dialogue box. A second 0x0963 followed by 0x21 ends the highlight and possibly other effects.
        //0x64 - Text color. Followed by 6 bytes that make the color in RGB order. Only first byte in the 3 sets of 2 controls color, seemingly.
        //0x70 - Starts the following text in a follow up dialogue box.
        //0x77 - Always followed by 0x30? Unknown, seems to preceed many areas, but may not do anything.

        public enum BillyLanguage
        {
            Default = 0,
            Japanese = 1,
            Cyrillic = 2,
        }

        public List<string> strings = new List<string>();
        public BillyLanguage language = 0;
        public MesBin() { }

        public MesBin(string fileName, BufferedStreamReaderBE<MemoryStream> sr, bool useCyrillic)
        {
            if(useCyrillic)
            {
                language = BillyLanguage.Cyrillic;
            }
            Read(fileName, sr);
        }

        public MesBin(string fileName, BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(fileName, sr);
        }

        private void Read(string fileName, BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr._BEReadActive = true;
            Encoding encoding = Encoding.GetEncoding("Windows-1252");

            if (language == BillyLanguage.Cyrillic)
            {
                encoding = Encoding.GetEncoding("Windows-1251");
            }
            else if (!fileName.EndsWith("_e.bin") && !fileName.EndsWith("_f.bin") && !fileName.EndsWith("_g.bin") && !fileName.EndsWith("_i.bin") && !fileName.EndsWith("_s.bin"))
            {
                language = BillyLanguage.Japanese;
                encoding = Encoding.GetEncoding("shift-jis");
            }
            int fullOffsetBuffer = sr.ReadBE<int>();
            int nameStartBuffer = sr.ReadBE<int>();
            int nameCount = nameStartBuffer / 0x4;

            List<int> nameRelativeOffsets = new List<int>();
            for (int i = 0; i < nameCount; i++)
            {
                nameRelativeOffsets.Add(sr.ReadBE<int>());
            }
            int unkBuffer = sr.ReadBE<int>();
            int fakeStringOffset = sr.ReadBE<int>();

            for (int i = 0; i < nameCount; i++)
            {
                sr.Seek(fullOffsetBuffer + 0x8 + nameRelativeOffsets[i], SeekOrigin.Begin);
                List<byte> str = new List<byte>();
                string text = "";
                while (sr.Position < sr.BaseStream.Length)
                {
                    var bt = sr.Read<byte>();
                    if (bt == 0)
                    {
                        text += encoding.GetString(str.ToArray());
                        str.Clear();
                        break;
                    }
                    else if (bt == 0x9) //0x9 Designates a control code
                    {
                        text += encoding.GetString(str.ToArray());
                        str.Clear();

                        var type = sr.Read<byte>();
                        switch (type)
                        {
                            case 0x62:
                                text += "|Linebreak|";
                                break;
                            case 0x63:
                                var variant = sr.Read<byte>();
                                switch (variant)
                                {
                                    case 0x37:
                                        text += "|WhiteText|";
                                        break;
                                    case 0x36:
                                        text += "|OrangeText|";
                                        break;
                                    case 0x35:
                                        text += "|GreenText|";
                                        break;
                                    case 0x34:
                                        text += "|PinkText|";
                                        break;
                                    case 0x33:
                                        text += "|YellowText|";
                                        break;
                                    case 0x32:
                                        text += "|CyanText|";
                                        break;
                                    case 0x31:
                                        text += "|Highlight|";
                                        break;
                                    case 0x30:
                                        text += "|BlackText|";
                                        break;
                                    case 0x21:
                                        text += "|EndEffect|";
                                        break;
                                    default: throw new Exception();
                                }
                                break;
                            case 0x64:
                                var r = sr.Read<byte>(); sr.Read<byte>();
                                var g = sr.Read<byte>(); sr.Read<byte>();
                                var b = sr.Read<byte>(); sr.Read<byte>();
                                text += $"|Color#{r:X}{g:X}{b:X}|";
                                break;
                            case 0x70:
                                text += "|TextboxBreak|";
                                break;
                            case 0x77:
                                var type2 = sr.Read<byte>();
                                if (type2 == 0x30 || type2 == 0x31 || type2 == 0x32)
                                {
                                    text += $"|77{type2:X}|";
                                }
                                else
                                {
                                    throw new Exception();
                                }
                                break;
                            default:
                                throw new Exception();
                        }
                    }
                    else
                    {
                        str.Add(bt);
                    }
                }
                strings.Add(text);
            }
        }

        public byte[] GetBytesCyrillic(bool useCyrillic)
        {
            if (useCyrillic)
            {
                language = BillyLanguage.Cyrillic;
            }
            return GetBytes();
        }

        public byte[] GetBytes()
        {
            ByteListExtension.AddAsBigEndian = true;
            List<byte> outBytes = new List<byte>();
            List<byte> refs = new List<byte>();
            List<byte> strBytes = new List<byte>();
            Encoding encoding = Encoding.GetEncoding("Windows-1252");
            if (language == BillyLanguage.Cyrillic)
            {
                encoding = Encoding.GetEncoding("Windows-1251");
            }
            else if (language == BillyLanguage.Japanese)
            {
                encoding = Encoding.GetEncoding("shift-jis");
            }

            for (int i = 0; i < strings.Count; i++)
            {
                refs.AddValue(strBytes.Count);
                var splitString = strings[i].Split('|');
                bool inTag = false;
                if (strings[i][0] == '|')
                {
                    inTag = true;
                }
                for (int j = 0; j < splitString.Length; j++)
                {
                    if (inTag)
                    {
                        if (splitString[j] == "")
                        {
                            j++;
                        }

                        strBytes.Add(0x9);
                        var testStr = splitString[j].Split('#');
                        switch (testStr[0].ToLower())
                        {
                            case "whitetext":
                                strBytes.Add(0x63);
                                strBytes.Add(0x37);
                                break;
                            case "orangetext":
                                strBytes.Add(0x63);
                                strBytes.Add(0x36);
                                break;
                            case "greentext":
                                strBytes.Add(0x63);
                                strBytes.Add(0x35);
                                break;
                            case "pinktext":
                                strBytes.Add(0x63);
                                strBytes.Add(0x34);
                                break;
                            case "yellowtext":
                                strBytes.Add(0x63);
                                strBytes.Add(0x33);
                                break;
                            case "cyantext":
                                strBytes.Add(0x63);
                                strBytes.Add(0x32);
                                break;
                            case "highlight":
                                strBytes.Add(0x63);
                                strBytes.Add(0x31);
                                break;
                            case "blacktext":
                                strBytes.Add(0x63);
                                strBytes.Add(0x30);
                                break;
                            case "endeffect":
                                strBytes.Add(0x63);
                                strBytes.Add(0x21);
                                break;
                            case "linebreak":
                                strBytes.Add(0x62);
                                break;
                            case "color":
                                strBytes.Add(0x64);
                                var r = byte.Parse(testStr[1].Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                                var g = byte.Parse(testStr[1].Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                                var b = byte.Parse(testStr[1].Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                                strBytes.Add(r);
                                strBytes.Add(r);
                                strBytes.Add(g);
                                strBytes.Add(g);
                                strBytes.Add(b);
                                strBytes.Add(b);
                                break;
                            case "textboxbreak":
                                strBytes.Add(0x70);
                                break;
                            default: //Should work for any arbitrary tag values
                                var count = testStr[0].Length / 2;
                                try
                                {
                                    for (int l = 0; l < count; l++)
                                    {
                                        strBytes.Add(byte.Parse(testStr[0].Substring(l * 2, 2), System.Globalization.NumberStyles.HexNumber));
                                    }
                                }
                                catch
                                {
                                    strBytes.AddRange(encoding.GetBytes("Corrupt Text. Should only appear in unused, JP text slots"));
                                }
                                break;
                        }
                        inTag = false;
                    }
                    else
                    {
                        strBytes.AddRange(encoding.GetBytes(splitString[j]));
                        inTag = true;
                    }
                }
                strBytes.Add(0);
            }
            var endCount = strBytes.Count;
            strBytes.AlignWriter(0x4);
            refs.AddValue(strBytes.Count + 0x8);
            refs.AddValue(endCount);
            outBytes.AddValue(refs.Count);
            outBytes.AddValue(refs.Count - 0x8);
            outBytes.AddRange(refs);
            outBytes.AddRange(strBytes);

            return outBytes.ToArray();
        }
    }
}
