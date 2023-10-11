using Reloaded.Memory.Streams;

namespace AquaModelLibrary.BluePoint.CMDL
{
    public class CMDL_CMSHBorder
    {
        public int int0;
        public CVariableTrail trail0 = null;

        public int int1;
        public CVariableTrail trail1 = null;

        public byte bt0;
        public ushort usht0;

        public int int2;
        public CVariableTrail trail2 = null;

        public byte bt1;

        public int int3;
        public CVariableTrail trail3 = null;
        public byte bt_2;
        public byte bt_3;

        public int int4;
        public CVariableTrail cmshTrail = null;

        public CMDL_CMSHBorder() { }

        public CMDL_CMSHBorder(BufferedStreamReader sr)
        {
            int0 = sr.Read<int>();
            trail0 = new CVariableTrail(sr);
            if (trail0.data.Count > 1)
            {
                int1 = sr.Read<int>();
                trail1 = new CVariableTrail(sr);
            }
            bt0 = sr.Read<byte>();

            if (trail1?.data.Count > 0)
            {
                usht0 = sr.Read<ushort>();
            }

            int2 = sr.Read<int>();
            trail2 = new CVariableTrail(sr);
            bt1 = sr.Read<byte>();

            var check = sr.Read<int>();
            if (check == 0x4A285982)
            {
                int4 = check;
                cmshTrail = new CVariableTrail(sr);
            }
            else
            {
                int3 = check;
                trail3 = new CVariableTrail(sr);
                bt_2 = sr.Read<byte>();
                bt_3 = sr.Read<byte>();

                int4 = sr.Read<int>();
                cmshTrail = new CVariableTrail(sr);
            }
        }
    }
}
