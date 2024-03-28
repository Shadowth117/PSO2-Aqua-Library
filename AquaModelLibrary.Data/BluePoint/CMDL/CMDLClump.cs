using AquaModelLibrary.Helpers.Readers;
using System.Diagnostics;

namespace AquaModelLibrary.Data.BluePoint.CMDL
{
    public class CMDLClump
    {
        public uint magic;
        public CVariableTrail trail0 = null;
        public CVariableTrail trail1 = null;
        public List<byte> extraBytes = new List<byte>();
        public byte bt00;
        public short sht00;
        public short sht01;
        public short sht02;
        public float flt00;
        public float flt01;

        public CMDLClump() { }

        public CMDLClump(BufferedStreamReaderBE<MemoryStream> sr, uint check)
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
                /* DeSR */
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
                case 0x4A285982:
                    break;
                /* SOTC */
                case 0x6E799B50:
                case 0x501E51A0:
                case 0xAE5653B6:
                    sht00 = sr.ReadBE<short>();
                    sht01 = sr.ReadBE<short>();
                    return;
                case 0xCCEEF10A:
                case 0x4221B9CA:
                    sht00 = sr.ReadBE<short>();
                    sht01 = sr.ReadBE<short>();
                    sht02 = sr.ReadBE<short>();
                    return;
                case 0x5A880BFB:
                case 0xBDED10EA:
                    sht00 = sr.ReadBE<short>();
                    flt00 = sr.ReadBE<float>();
                    return;
                case 0xB3D3CBBE:
                    sht00 = sr.ReadBE<short>();
                    return;
                case 0x844F9D5C:
                    flt00 = sr.ReadBE<float>();
                    sht00 = sr.ReadBE<short>();
                    flt01 = sr.ReadBE<float>();
                    
                    //Sometimes values can just be here. With no context!
                    var test0x844F9D5C = sr.ReadBE<ushort>();
                    if (test0x844F9D5C <= 0xFF)
                    {
                        sht01 = (short)test0x844F9D5C;
                    } else
                    {
                        sr.Seek(-2, SeekOrigin.Current);
                    }
                    return;
                case 0xA03AC256: //cpid
                case 0xB7683EF2: //cclm
                    return;
                default:
                    Debug.WriteLine($"Unexpected tag: {check:X8}");
                    throw new Exception();
            }
            trail0 = new CVariableTrail(sr, limit);
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

            if(check == 0x4A285982)
            {
                sht00 = sr.ReadBE<short>();
            }
        }
    }
}
