using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Text;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    public class StaffList : ARC
    {
        public List<Credit> credits = new List<Credit>();
        public StaffList() { }
        public StaffList(byte[] file)
        {
            Read(file);
        }

        public StaffList(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }
        public override void Read(byte[] file)
        {
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        public StaffList(string[] creditsText)
        {
            for (int i = 0; i < creditsText.Length; i++)
            {
                Credit credit = new Credit();
                var segments = creditsText[i].Split('|');
                credit.creditType = (CreditType)Enum.Parse(typeof(CreditType), segments[0]);
                if (segments.Length > 1)
                {
                    for (int s = 0; s < segments[1].Length; s++)
                    {
                        if (segments[1][s] != ' ')
                        {
                            credit.leadingSpaceCount = (ushort)s;
                            segments[1] = segments[1].Substring(s, segments[1].Length - s);
                            break;
                        }
                    }
                }
                credit.text = segments[1];
                credits.Add(credit);
            }
        }

        public override void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr._BEReadActive = true;
            base.Read(sr);
            sr.Seek(0x20, SeekOrigin.Begin);
            var count = sr.ReadBE<int>();
            var offset = sr.ReadBE<int>();
            sr.Seek(offset + 0x20, SeekOrigin.Begin);

            Encoding encoding = Encoding.GetEncoding("Windows-1252");
            for (int i = 0; i < count; i++)
            {
                Credit credit = new Credit();
                credit.creditType = (CreditType)sr.ReadBE<ushort>();
                credit.leadingSpaceCount = sr.ReadBE<ushort>();
                credit.textPtr = sr.ReadBE<int>();

                var bookmark = sr.Position;
                sr.Seek(0x20 + credit.textPtr, SeekOrigin.Begin);
                credit.text = sr.Read8bitEncodedString(encoding);
                sr.Seek(bookmark, SeekOrigin.Begin);

                credits.Add(credit);
            }
        }

        public byte[] GetBytes()
        {
            Dictionary<string, int> textTracker = new Dictionary<string, int>();
            ByteListExtension.AddAsBigEndian = true;
            List<int> pofSets = new List<int>();
            List<byte> outBytes = new List<byte>();
            outBytes.AddValue(credits.Count);
            pofSets.Add(outBytes.Count);
            outBytes.AddValue((int)0x8);

            Encoding encoding = Encoding.GetEncoding("Windows-1252");
            for (int i = 0; i < credits.Count; i++)
            {
                var credit = credits[i];
                outBytes.AddValue((ushort)credit.creditType);
                outBytes.AddValue((ushort)credit.leadingSpaceCount);
                if(credit.text != null && credit.text != "")
                {
                    pofSets.Add(outBytes.Count);
                }
                outBytes.ReserveInt($"Credit{i}Text");
            }
            for (int i = 0; i < credits.Count; i++)
            {
                var credit = credits[i];
                if(credit.text != null && credit.text != "")
                {
                    if(textTracker.ContainsKey(credit.text))
                    {
                        outBytes.FillInt($"Credit{i}Text", textTracker[credit.text]);
                    } else
                    {
                        outBytes.FillInt($"Credit{i}Text", outBytes.Count);
                        textTracker.Add(credit.text, outBytes.Count);
                        outBytes.AddRange(encoding.GetBytes(credit.text));
                        outBytes.Add(0);
                    }
                }
            }
            outBytes.AlignWriter(0x4);

            var pof0Offset = outBytes.Count;
            pofSets.Sort();
            var pof0 = POF0.GenerateRawPOF0(pofSets, true);
            outBytes.AddRange(pof0);

            var arcBytes = new List<byte>();
            arcBytes.AddValue(outBytes.Count + 0x20);
            arcBytes.AddValue(pof0Offset);
            arcBytes.AddValue(pof0.Length);
            arcBytes.AddValue(0);

            arcBytes.AddValue(0);
            arcBytes.Add(0x30);
            arcBytes.Add(0x31);
            arcBytes.Add(0x30);
            arcBytes.Add(0x30);
            arcBytes.AddValue(0);
            arcBytes.AddValue(0);
            outBytes.InsertRange(0, arcBytes);

            ByteListExtension.Reset();
            return outBytes.ToArray();
        }
        public List<string> GetTextLines()
        {
            List<string> creditsText = new List<string>();
            for(int i = 0; i < credits.Count; i++)
            {
                string text = $"{credits[i].creditType}|";
                for(int s = 0; s < credits[i].leadingSpaceCount; s++)
                {
                    text += " ";
                }
                text += credits[i].text;
                creditsText.Add(text);
            }

            return creditsText;
        }
    }


    public class Credit
    {
        public string text = null;

        public CreditType creditType;
        public ushort leadingSpaceCount;
        public int textPtr;
    }

    public enum CreditType : ushort
    {
        DefaultText = 0,
        BoldText = 1,
        Empty = 8,
        WAVEMASTERImage = 0x10,
        SonicTeamImage = 0x11,
        SEGAImage = 0x12,
        StaffImage = 0x13,
        GameDesignStaffImage = 0x14,
        ProgrammingStaffImage = 0x15,
        DesignStaffImage = 0x16,
        CGMovieStaffImage = 0x17,
        SoundStaffImage = 0x18,
        MusiciansImage = 0x19,
        ChantThisCharmImage = 0x1A,
        CharacterVoicesImage = 0x1B,
        SEGAStaffImage = 0x1C,
        SEGAOfAmericaStaffImage = 0x1D,
        SEGAEuropeStaffImage = 0x1E,
        SpecialThanksImage = 0x1F,
    }

}
