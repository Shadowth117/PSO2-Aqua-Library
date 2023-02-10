using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.BluePoint.CMAT
{
    public struct CMTLMeta0
    {
        public int unk0;
        public int unk1;
    }

    public class CMAT
    {
        public int magic;
        public int unk0;
        public int crc;
        public int int_0C;

        public int int_10;
        public int int_14;
        public int int_18;
        public int int_1C;

        public string shaderName;
        public List<CMTLMeta0> meta0List = new List<CMTLMeta0>();
        public List<string> texNames = new List<string>();
        public CMAT()
        {

        }
        public CMAT(BufferedStreamReader sr)
        {
            magic = sr.Read<int>();
            unk0 = sr.Read<int>();
            crc = sr.Read<int>();
            int_0C = sr.Read<int>();

            int_10 = sr.Read<int>();
            int_14 = sr.Read<int>();
            int_18 = sr.Read<int>();
            int_1C = sr.Read<int>();

            byte shaderLen = sr.Read<byte>();
            shaderName = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position(), shaderLen));
            sr.Seek(shaderLen, System.IO.SeekOrigin.Current);

            var texCount = sr.Read<int>();
            for(int i = 0; i < texCount; i++)
            {
                meta0List.Add(sr.Read<CMTLMeta0>());
            }
            var texCount2 = sr.Read<int>();
            for(int i = 0; i < texCount2; i++)
            {
                byte texLen = sr.Read<byte>();
                Debug.WriteLine($"{texLen}");
                texNames.Add(Encoding.UTF8.GetString(sr.ReadBytes(sr.Position(), texLen)));
                sr.Seek(texLen, System.IO.SeekOrigin.Current);
            }

            //TODO - Material color metadata
        }
    }
}
