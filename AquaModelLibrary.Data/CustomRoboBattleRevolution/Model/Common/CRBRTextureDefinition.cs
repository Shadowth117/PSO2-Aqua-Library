using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using static AquaModelLibrary.Data.Gamecube.GCTextureInfo;

namespace AquaModelLibrary.Data.CustomRoboBattleRevolution.Model.Common
{
    /// <summary>
    /// Values map to .tpl texture values
    /// </summary>
    public class CRBRTextureDefinition
    {
        public byte[] textureBuffer = null;

        public int textureBufferOffset;
        public ushort textureWidth;
        public ushort textureHeight;
        public TPLFormat textureFormat;
        public int int_0C;

        public int int_10;
        public int int_14;

        public CRBRTextureDefinition() { }

        public CRBRTextureDefinition(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            textureBufferOffset = sr.ReadBE<int>();
            textureWidth = sr.ReadBE<ushort>();
            textureHeight = sr.ReadBE<ushort>();
            textureFormat = sr.ReadBE<TPLFormat>();
            int_0C = sr.ReadBE<int>();
            int_10 = sr.ReadBE<int>();
            int_14 = sr.ReadBE<int>();
        }

        public byte[] GetTPL()
        {
            if (textureBuffer != null)
            {
                List<byte> tpl = new List<byte>();
                ByteListExtension.AddAsBigEndian = true;
                tpl.Add(0x0);
                tpl.Add(0x20);
                tpl.Add(0xAF);
                tpl.Add(0x30);
                tpl.AddValue((int)0x1);
                tpl.AddValue((int)0xC);
                tpl.AddValue((int)0x14);

                tpl.AddValue((int)0);
                tpl.AddValue(textureWidth);
                tpl.AddValue(textureHeight);
                tpl.AddValue((int)textureFormat);
                tpl.AddValue(0x40); //Data offset

                tpl.AddValue((int)0x0);
                tpl.AddValue((int)0x0);
                tpl.AddValue((int)0x1);
                tpl.AddValue((int)0x1);

                tpl.AddValue((int)0x0);
                tpl.AddValue((int)0x0);
                tpl.AddValue((int)0x0);
                tpl.AddValue((int)0x0);
                tpl.AddRange(textureBuffer);

                ByteListExtension.Reset();

                return tpl.ToArray();
            }

            return null;
        }
    }
}
