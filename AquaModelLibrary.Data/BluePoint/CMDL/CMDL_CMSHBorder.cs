using AquaModelLibrary.Helpers.Readers;
using System.Diagnostics;

namespace AquaModelLibrary.BluePoint.CMDL
{
    public class CMDL_CMSHBorder
    {
        public List<BorderClump> clumps = new List<BorderClump>();

        public CMDL_CMSHBorder() { }

        public CMDL_CMSHBorder(BufferedStreamReaderBE<MemoryStream> sr)
        {
            while (true)
            {
                var check = sr.Read<uint>();
                var clump = new BorderClump(sr, check);
                clumps.Add(clump);
                if (check == 0x4A285982)
                {
                    break;
                }
            }
        }

        public class BorderClump
        {
            public uint magic;
            public CVariableTrail trail = null;
            public List<byte> extraBytes = new List<byte>();
            public BorderClump() { }

            public BorderClump(BufferedStreamReaderBE<MemoryStream> sr, uint check)
            {
                Read(sr, check);
            }

            protected void Read(BufferedStreamReaderBE<MemoryStream> sr, uint check)
            {
                magic = check;
                int limit = 4;
                int readExtra = 0;
                switch (check)
                {
                    case 0xA5428144:
                        readExtra = 2;
                        break;
                    case 0x5C6DB65F:
                        readExtra = 3;
                        break;
                    case 0xcf9c5789:
                    case 0x233dd489:
                    case 0x09258d3b:
                    case 0x43C6AFBA:
                        limit = 5;
                        break;
                    case 0xEB06473D:
                        limit = 2;
                        break;
                    case 0x2354F238: //Should be initial tag
                    case 0x95CBEB14:
                    case 0x4A285982: //Should be final tag
                        break;
                    default:
                        Debug.WriteLine($"Unexpected tag: {check:X8}");
                        throw new Exception();
                }
                trail = new CVariableTrail(sr, limit);
                if ((check == 0x09258d3b || check == 0x2354F238) && sr.Peek<byte>() == 0)
                {
                    readExtra++;
                }
                for (int i = 0; i < readExtra; i++)
                {
                    extraBytes.Add(sr.Read<byte>());
                }
                var zeroCheck = sr.Peek<byte>();
                while (zeroCheck == 0)
                {
                    zeroCheck = sr.Peek<byte>();
                    if (zeroCheck == 0)
                    {
                        sr.Read<byte>();
                    }
                }
            }
        }
    }
}
