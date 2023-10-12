using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AquaModelLibrary.BluePoint.CMDL
{
    public class CMDL_CMSHBorder
    {
        public List<BorderClump> clumps = new List<BorderClump>();

        /*
        public uint int0;
        public CVariableTrail trail0 = null;

        public uint int1;
        public CVariableTrail trail1 = null;

        public byte bt0;
        public ushort usht0;

        public uint int2;
        public CVariableTrail trail2 = null;

        public byte bt1;

        public uint int3;
        public CVariableTrail trail3 = null;
        public byte bt_2;
        public byte bt_3;

        public uint int4;
        public CVariableTrail cmshTrail = null;

        public uint int5;
        public CVariableTrail trail5 = null;
        */

        public CMDL_CMSHBorder() { }

        public CMDL_CMSHBorder(BufferedStreamReader sr)
        {
            while(true)
            {
                var check = sr.Read<uint>();
                var clump = new BorderClump(sr, check);
                clumps.Add(clump);
                if(check == 0x4A285982)
                {
                    break;
                }
            }
            /*
            int0 = sr.Read<uint>();
            trail0 = new CVariableTrail(sr);
            if (trail0.data.Count > 1)
            {
                int1 = sr.Read<uint>();
                trail1 = new CVariableTrail(sr);
            }
            bt0 = sr.Read<byte>();

            if (trail1?.data.Count > 0)
            {
                usht0 = sr.Read<ushort>();
            }

            int2 = sr.Read<uint>();
            trail2 = new CVariableTrail(sr);
            bt1 = sr.Read<byte>();

            var check = sr.Read<uint>();
            if (check == 0xEB06473D)
            {
                int5 = check;
                trail5 = new CVariableTrail(sr, 2);
                check = sr.Read<uint>();
            }
            if (check == 0x4A285982)
            {
                int4 = check;
                cmshTrail = new CVariableTrail(sr);
            }
            else
            {
                int3 = check;
                trail3 = new CVariableTrail(sr);
                if(int3 != 0x95CBEB14)
                {
                    bt_2 = sr.Read<byte>();
                    bt_3 = sr.Read<byte>();
                }
                check = sr.Read<uint>();

                if (check == 0xEB06473D)
                {
                    int5 = check;
                    trail5 = new CVariableTrail(sr, 2);
                    check = sr.Read<uint>();
                }

                int4 = check;
                cmshTrail = new CVariableTrail(sr);
            }*/
        }

        public class BorderClump
        {
            public uint magic;
            public CVariableTrail trail;
            public List<byte> extraBytes = new List<byte>();
            public BorderClump() { }
            
            public BorderClump(BufferedStreamReader sr, uint check)
            {
                Read(sr, check);
            }

            protected void Read(BufferedStreamReader sr, uint check)
            {
                magic = check;
                int limit = 4;
                int readExtra = 0;
                switch(check)
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
                if((check == 0x09258d3b || check == 0x2354F238) && sr.Peek<byte>() == 0)
                {
                    readExtra++;
                }
                for(int i = 0; i < readExtra; i++)
                {
                    extraBytes.Add(sr.Read<byte>());
                }
                var zeroCheck = sr.Peek<byte>();
                while(zeroCheck == 0)
                {
                    zeroCheck = sr.Peek<byte>();
                    if(zeroCheck == 0)
                    {
                        sr.Read<byte>();
                    }
                }
            }
        }
    }
}
