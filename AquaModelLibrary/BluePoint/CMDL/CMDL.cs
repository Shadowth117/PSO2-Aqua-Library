using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.BluePoint.CMDL
{
    public class CMDL
    {
        public int magic;
        public int unk0;
        public int crc;

        public string shaderName;
        public List<string> texNames = new List<string>();
        public CMDL()
        {

        }
        public CMDL(BufferedStreamReader sr)
        {
            /*
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
            for (int i = 0; i < texCount; i++)
            {
                meta0List.Add(sr.Read<CMTLMeta0>());
            }
            var texCount2 = sr.Read<int>();
            for (int i = 0; i < texCount2; i++)
            {
                byte texLen = sr.Read<byte>();
                Debug.WriteLine($"{texLen}");
                texNames.Add(Encoding.UTF8.GetString(sr.ReadBytes(sr.Position(), texLen)));
                sr.Seek(texLen, System.IO.SeekOrigin.Current);
            }
            */
        }
    }
}
