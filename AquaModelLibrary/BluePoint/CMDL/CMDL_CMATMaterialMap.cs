using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.BluePoint.CMDL
{
    public class CMDL_CMATMaterialMap
    {
        public ushort usht0;
        public int int0;
        public byte cmshMaterialNameLengthLength;
        public byte cmshMaterialNameLength;
        public string cmshMaterialName;

        public int int1;
        public byte matPathLengthLength;
        public byte matPathLength;

        public string cmatPath;
        public CMDL_CMATMaterialMap() { }

        public CMDL_CMATMaterialMap(BufferedStreamReader sr)
        {
            usht0 = sr.Read<ushort>();
            int0 = sr.Read<int>();
            cmshMaterialNameLengthLength = sr.Read<byte>();
            cmshMaterialNameLength = sr.Read<byte>();
            cmshMaterialName = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position(), cmshMaterialNameLength));
            sr.Seek(cmshMaterialNameLength, SeekOrigin.Current);

            int1 = sr.Read<int>();
            matPathLengthLength = sr.Read<byte>();
            matPathLength = sr.Read<byte>();
            cmatPath = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position(), matPathLength));
            sr.Seek(matPathLength, SeekOrigin.Current);
        }
    }
}
