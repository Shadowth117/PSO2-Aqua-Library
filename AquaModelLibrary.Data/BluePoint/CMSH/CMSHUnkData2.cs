﻿using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint.CMSH
{
    public class CMSHUnkData2
    {
        public long position;

        public int flags;
        public int size;
        public int unk0;

        public List<byte> buffer = new List<byte>();

        public int nextSize;
        public List<byte> buffer2 = new List<byte>();

        public CMSHUnkData2()
        {

        }

        public CMSHUnkData2(BufferedStreamReaderBE<MemoryStream> sr)
        {
            position = sr.Position;

            flags = sr.Read<int>();
            size = sr.Read<int>();
            unk0 = sr.Read<int>();

            buffer.AddRange(sr.ReadBytes(sr.Position, size));
            sr.Seek(size, System.IO.SeekOrigin.Current);

            nextSize = sr.Read<int>();
            buffer2.AddRange(sr.ReadBytes(sr.Position, nextSize));
            sr.Seek(nextSize, System.IO.SeekOrigin.Current);
        }
    }
}
