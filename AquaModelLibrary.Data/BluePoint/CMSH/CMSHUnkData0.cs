using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint.CMSH
{
    public class CMSHUnkData0
    {
        public long position;

        public int unk0;
        public int flags;
        public int unk1;
        public int count;
        public List<CMSHUNKData0Data> unkList = new List<CMSHUNKData0Data>();

        public struct CMSHUNKData0Data
        {
            public int unk0;
            public int unk1;
        }

        public CMSHUnkData0()
        {

        }

        public CMSHUnkData0(BufferedStreamReaderBE<MemoryStream> sr)
        {
            position = sr.Position;

            unk0 = sr.Read<int>();
            flags = sr.Read<int>();
            unk1 = sr.Read<int>();
            count = sr.Read<int>();
            var pos = sr.Position;
            for (int i = 0; i < count; i++)
            {
                unkList.Add(sr.Read<CMSHUNKData0Data>());
            }
        }

        public byte[] GetBytes()
        {
            List<byte> outBytes = new List<byte>();
            outBytes.AddValue(unk0);
            outBytes.AddValue(flags);
            outBytes.AddValue(unk1);
            outBytes.AddValue(unkList.Count);
            foreach(var unk in unkList)
            {
                outBytes.AddValue(unk.unk0);
                outBytes.AddValue(unk.unk1);
            }

            return outBytes.ToArray();
        }
    }
}
