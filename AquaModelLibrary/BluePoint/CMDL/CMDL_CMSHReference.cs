using Reloaded.Memory.Streams;
using System.IO;
using System.Text;

namespace AquaModelLibrary.BluePoint.CMDL
{
    public class CMDL_CMSHReference
    {
        public ushort usht0;
        public int int0;
        public byte cmshPathLengthLength;
        public byte cmshPathLength;
        public string cmshPath;
        public int int1;
        public CVariableTrail trail0 = null;
        public byte bt0;
        public ushort usht1;

        public CMDL_CMSHReference() { }

        public CMDL_CMSHReference(BufferedStreamReader sr)
        {
            usht0 = sr.Read<ushort>();
            int0 = sr.Read<int>();
            cmshPathLengthLength = sr.Read<byte>();
            cmshPathLength = sr.Read<byte>();
            cmshPath = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position(), cmshPathLength));
            sr.Seek(cmshPathLength, SeekOrigin.Current);
            int1 = sr.Read<int>();
            trail0 = new CVariableTrail(sr);
            bt0 = sr.Read<byte>();
            usht1 = sr.Read<ushort>();
        }
    }
}
