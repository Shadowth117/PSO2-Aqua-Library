using AquaModelLibrary.Helpers.Readers;
using System.IO;
using System.Numerics;
using System.Text;

namespace AquaModelLibrary.Data.PSO2.MiscPSO2Structs
{
    /// <summary>
    /// .elpr - Enlighten Param
    /// </summary>
    public class ELPR
    {
        public List<ElprPiece> elprList = new List<ElprPiece>();

        public ELPR() { }

        public ELPR(byte[] file) 
        {
            Read(file);
        }

        public ELPR(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        private void Read(byte[] file)
        {
            using (MemoryStream stream = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                Read(sr);
            }
        }

        private void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            var type = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position, 4));
            sr.Seek(0x6, SeekOrigin.Current);
            var count = sr.Read<ushort>();
            sr.Seek(0x8, SeekOrigin.Current);

            for (int i = 0; i < count; i++)
            {
                ElprPiece piece = new ElprPiece();
                piece.name0x18 = sr.ReadCString();
                sr.Seek(0x18, SeekOrigin.Current);
                piece.usht_18 = sr.Read<ushort>();
                piece.usht_1A = sr.Read<ushort>();
                piece.int_1C = sr.Read<int>();
                piece.vec3_20 = sr.Read<Vector3>();
                piece.int_2C = sr.Read<int>();
                piece.vec3_30 = sr.Read<Vector3>();
                piece.int_3C = sr.Read<int>();
                elprList.Add(piece);
            }
        }

        public class ElprPiece
        {
            public string name0x18 = null; //Read as C string from 0x18 byte array, stripping from the null character at the end
            public ushort usht_18;
            public ushort usht_1A;
            public int int_1C;

            public Vector3 vec3_20;
            public int int_2C;
            public Vector3 vec3_30;
            public int int_3C;
        }

    }
}
