using AquaModelLibrary.Helpers.Readers;
using System.Text;

namespace AquaModelLibrary.Data.BluePoint.CGPR
{
    public enum CGPRSubMagic : uint
    {
        x427AC0E6 = 0xE6C07A42,
        x3BB01156 = 0x5611B03B,
        xF0C420F0 = 0xF020C4F0,
        x88BE09B0 = 0xB009BE88,
        xE7359E81 = 0x819E35E7,
        x69A26EBB = 0xBB6EA269,
    }

    public abstract class CGPRSubObject
    {
        public uint magic;

        public CGPRSubObject()
        {

        }

        public CGPRSubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
        }

        public static CGPRSubObject ReadSubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            var type = sr.Peek<uint>();
            switch (type)
            {
                case (uint)CGPRSubMagic.x427AC0E6:
                    return new _427AC0E6_SubObject(sr);
                case (uint)CGPRSubMagic.x3BB01156:
                    return new _3BB01156_SubObject(sr);
                case (uint)CGPRSubMagic.xF0C420F0:
                    return new _F0C420F0_SubObject(sr);
                case (uint)CGPRSubMagic.x88BE09B0:
                    return new _88BE09B0_SubObject(sr);
                case (uint)CGPRSubMagic.xE7359E81:
                    return new _E7359E81_SubObject(sr);
                case (uint)CGPRSubMagic.x69A26EBB:
                    return new _69A26EBB_SubObject(sr);
            }

            return null;
        }
    }

    public class _427AC0E6_SubObject : CGPRSubObject
    {
        public cgprCommonHeader mainHeader;
        public byte dataStringLength;
        public string dataString;

        public _427AC0E6_SubObject()
        {

        }

        public _427AC0E6_SubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = sr.Read<cgprCommonHeader>();
            dataStringLength = sr.Read<byte>();
            dataString = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position, dataStringLength));
            sr.Seek(dataStringLength, System.IO.SeekOrigin.Current);
        }
    }

    public class _3BB01156_SubObject : CGPRSubObject
    {
        public cgprCommonHeader mainHeader;
        public List<CGPRSubObject> subObjects = new List<CGPRSubObject>();

        public _3BB01156_SubObject()
        {

        }

        public _3BB01156_SubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = sr.Read<cgprCommonHeader>();
            ushort subStructCount = sr.Read<ushort>();
            for (int i = 0; i < subStructCount; i++)
            {
                subObjects.Add(ReadSubObject(sr));
            }
        }
    }
    public class _E7359E81_SubObject : CGPRSubObject
    {
        public cgprCommonHeader mainHeader;
        public int int_00;

        public _E7359E81_SubObject()
        {

        }

        public _E7359E81_SubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = sr.Read<cgprCommonHeader>();
            int_00 = sr.Read<int>();
        }
    }
    public class _F0C420F0_SubObject : CGPRSubObject
    {
        public cgprCommonHeader mainHeader;
        public int int_00;

        public _F0C420F0_SubObject()
        {

        }

        public _F0C420F0_SubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = sr.Read<cgprCommonHeader>();
            int_00 = sr.Read<int>();
        }
    }
    public class _88BE09B0_SubObject : CGPRSubObject
    {
        public cgprCommonHeader mainHeader;
        public byte bt_00;

        public _88BE09B0_SubObject()
        {

        }

        public _88BE09B0_SubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = sr.Read<cgprCommonHeader>();
            bt_00 = sr.Read<byte>();
        }
    }
    public class _69A26EBB_SubObject : CGPRSubObject
    {
        public cgprCommonHeader mainHeader;
        public byte bt_00;

        public _69A26EBB_SubObject()
        {

        }

        public _69A26EBB_SubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = sr.Read<cgprCommonHeader>();
            bt_00 = sr.Read<byte>();
        }
    }
}
