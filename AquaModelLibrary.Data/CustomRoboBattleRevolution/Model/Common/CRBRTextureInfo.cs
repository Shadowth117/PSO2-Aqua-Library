using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.CustomRoboBattleRevolution.Model.Common
{
    public class CRBRTextureInfo
    {
        public CRBRTextureInfo nextTextureInfo = null;
        public CRBRTextureDefinition texture = null;

        public int int_00;
        public int nextTextureInfoOffset;
        public int int_08;
        /// <summary>
        /// Observed with a value
        /// </summary>
        public int int_0C;

        public int int_10;
        public int int_14;
        public int int_18;

        public Vector3 unkVec3;
        public int int_28;
        public int int_2C;

        public int int_30;
        public int int_34;
        public int int_38;
        public byte bt_3C;
        public byte bt_3D;
        public byte bt_3E;
        public byte bt_3F;

        public byte bt_40;
        public byte bt_41;
        public byte bt_42;
        public byte bt_43;
        public float ft_44;
        /// <summary>
        /// Observed with a value
        /// </summary>
        public int int_48;
        public int textureDefinitionOffset;
        public int int_50;
        public int int_54;
        public int int_58;

        public CRBRTextureInfo() { }

        public CRBRTextureInfo(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            int_00 = sr.ReadBE<int>();
            nextTextureInfoOffset = sr.ReadBE<int>();
            int_08 = sr.ReadBE<int>();
            int_0C = sr.ReadBE<int>();

            int_10 = sr.ReadBE<int>();
            int_14 = sr.ReadBE<int>();
            int_18 = sr.ReadBE<int>();

            unkVec3 = sr.ReadBEV3();
            int_28 = sr.ReadBE<int>();
            int_2C = sr.ReadBE<int>();

            int_30 = sr.ReadBE<int>();
            int_34 = sr.ReadBE<int>();
            int_38 = sr.ReadBE<int>();
            bt_3C = sr.ReadBE<byte>();
            bt_3D = sr.ReadBE<byte>();
            bt_3E = sr.ReadBE<byte>();
            bt_3F = sr.ReadBE<byte>();

            bt_40 = sr.ReadBE<byte>();
            bt_41 = sr.ReadBE<byte>();
            bt_42 = sr.ReadBE<byte>();
            bt_43 = sr.ReadBE<byte>();
            ft_44 = sr.ReadBE<float>();

            int_48 = sr.ReadBE<int>();
            textureDefinitionOffset = sr.ReadBE<int>();
            int_50 = sr.ReadBE<int>();
            int_54 = sr.ReadBE<int>();
            int_58 = sr.ReadBE<int>();

            if(nextTextureInfoOffset != 0)
            {
                sr.Seek(nextTextureInfoOffset + offset, SeekOrigin.Begin);
                nextTextureInfo = new CRBRTextureInfo(sr, offset);
            }

            if(textureDefinitionOffset != 0)
            {
                sr.Seek(textureDefinitionOffset + offset, SeekOrigin.Begin);
                texture = new CRBRTextureDefinition(sr, offset);
            }
        }
    }
}
