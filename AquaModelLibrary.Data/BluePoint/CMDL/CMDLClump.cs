using AquaModelLibrary.Helpers.Readers;
using System.Diagnostics;

namespace AquaModelLibrary.Data.BluePoint.CMDL
{
    public class CMDLClump
    {
        /// <summary>
        /// Assumedly, this is a hash for the original 'type' name of the of the data clump. Types appear unique to the game they appear in. 
        /// </summary>
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
        public float flt02;

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
                    ReadDeSRCMDLClumpEnd(sr, check, limit, readExtra);
                    return;
                case 0x5C6DB65F:
                    readExtra = 3;
                    ReadDeSRCMDLClumpEnd(sr, check, limit, readExtra);
                    return;
                case 0xcf9c5789:
                case 0x233dd489:
                case 0x09258d3b:
                case 0x43C6AFBA:
                    limit = 5;
                    ReadDeSRCMDLClumpEnd(sr, check, limit, readExtra);
                    return;
                case 0xEB06473D:
                    limit = 2;
                    ReadDeSRCMDLClumpEnd(sr, check, limit, readExtra);
                    return;
                case 0x2354F238: //Should be initial tag
                case 0x95CBEB14:
                case 0x4A285982:
                    ReadDeSRCMDLClumpEnd(sr, check, limit, readExtra);
                    return;
                /* SOTC */
                case 0xBB3B0A79:
                case 0x6E799B50:
                case 0x501E51A0:
                case 0x5cf5e72d:
                case 0xA2622CF6:
                case 0xAE5653B6:
                    sht00 = sr.ReadBE<short>();
                    sht01 = sr.ReadBE<short>();

                    if ((check is 0xAE5653B6 or 0xA2622CF6) && sht00 == 0x4)
                    {
                        sht02 = sr.ReadBE<short>();
                    }
                    return;
                case 0xCCEEF10A:
                case 0x4221B9CA:
                case 0x82260250:
                case 0xA342ACB3:
                    sht00 = sr.ReadBE<short>();
                    sht01 = sr.ReadBE<short>();
                    sht02 = sr.ReadBE<short>();
                    return;
                case 0x39ea2e52:
                case 0x5A880BFB:
                case 0xBDED10EA:
                case 0xA03FA36F:
                    sht00 = sr.ReadBE<short>();
                    flt00 = sr.ReadBE<float>();
                    return;
                case 0xC24907E5:
                    sht00 = sr.ReadBE<short>();
                    flt00 = sr.ReadBE<float>();
                    flt01 = sr.ReadBE<float>();
                    flt02 = sr.ReadBE<float>();
                    return;
                case 0xB3D3CBBE:
                case 0x7CB1241A:
                    sht00 = sr.ReadBE<short>();
                    return;
                case 0x6A236525:
                    sht00 = sr.ReadBE<short>();
                    bt00 = sr.ReadBE<byte>();
                    return;
                case 0x844F9D5C:
                case 0x6BA55380:
                case 0xE57D47D0:
                    flt00 = sr.ReadBE<float>();
                    sht00 = sr.ReadBE<short>();
                    flt01 = sr.ReadBE<float>();

                    //Sometimes values can just be here. With no context!
                    var test0x844F9D5C = sr.ReadBE<ushort>();
                    if (test0x844F9D5C <= 0xFF)
                    {
                        sht01 = (short)test0x844F9D5C;
                    }
                    else
                    {
                        sr.Seek(-2, SeekOrigin.Current);
                    }
                    return;
                case 0x2CBA9737: //Unknown?
                case 0xA03AC256: //cpid
                case 0xB7683EF2: //cclm
                    return;
                default:
                    Debug.WriteLine($"Unexpected tag: {check:X8}");
                    throw new Exception();
            }
        }

        private void ReadDeSRCMDLClumpEnd(BufferedStreamReaderBE<MemoryStream> sr, uint check, int limit, int readExtra)
        {
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

            if (check == 0x4A285982)
            {
                sht00 = sr.ReadBE<short>();
            }
        }
    }
}
