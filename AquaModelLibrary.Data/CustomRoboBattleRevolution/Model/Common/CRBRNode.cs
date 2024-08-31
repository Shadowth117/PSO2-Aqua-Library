using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.CustomRoboBattleRevolution.Model.Common
{
    public class CRBRNode
    {
        public CRBRNode childNode = null;
        public CRBRNode siblingNode = null;
        public CRBRMesh crbrMesh = null;

        public int int_00;
        /// <summary>
        /// Value of 0x10 means there's a child, 0 means no child
        /// </summary>
        public byte hasChildFlag;
        /// <summary>
        /// ?
        /// </summary>
        public byte unkFlag1;
        /// <summary>
        /// ?
        /// </summary>
        public byte unkFlag2;
        /// <summary>
        /// Value of 0x80 means there's a mesh attached
        /// </summary>
        public byte meshFlag;
        public int childOffset;
        public int siblingOffset;

        public int meshOffset;
        /// <summary>
        /// Stored in radians
        /// </summary>
        public Vector3 eulerRotation;
        public Vector3 scale;
        public Vector3 translation;

        //Padding to 0x20?
        public int int_38;
        public int int_3C;

        public CRBRNode() { }

        public CRBRNode(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            int_00 = sr.ReadBE<int>();
            hasChildFlag = sr.ReadBE<byte>();
            unkFlag1 = sr.ReadBE<byte>();
            unkFlag2 = sr.ReadBE<byte>();
            meshFlag = sr.ReadBE<byte>();
            childOffset = sr.ReadBE<int>();
            siblingOffset = sr.ReadBE<int>();
            meshOffset = sr.ReadBE<int>();
            eulerRotation = sr.ReadBEV3();
            scale = sr.ReadBEV3();
            translation = sr.ReadBEV3();

            int_38 = sr.ReadBE<int>();
            int_3C = sr.ReadBE<int>();

            if(childOffset != 0)
            {
                sr.Seek(childOffset + offset, SeekOrigin.Begin);
                childNode = new CRBRNode(sr, offset);
            }

            if (siblingOffset != 0)
            {
                sr.Seek(siblingOffset + offset, SeekOrigin.Begin);
                siblingNode = new CRBRNode(sr, offset);
            }

            if (meshOffset != 0)
            {
                sr.Seek(meshOffset + offset, SeekOrigin.Begin);
                crbrMesh = new CRBRMesh(sr, offset);
            }
        }
    }
}
