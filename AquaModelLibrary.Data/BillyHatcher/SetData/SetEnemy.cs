using AquaModelLibrary.Data.DataTypes;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.BillyHatcher.SetData
{
    public class SetEnemyList
    {
        public List<SetEnemy> setEnemies = new List<SetEnemy>();
        public SetEnemyList() { }
        public SetEnemyList(byte[] file)
        {
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        public SetEnemyList(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr._BEReadActive = true;
            while (sr.Position + 0x50 < sr.BaseStream.Length)
            {
                SetEnemy setEnemy = new SetEnemy();
                setEnemy.enemyId = sr.ReadBE<int>();
                setEnemy.Position = sr.ReadBEV3();
                setEnemy.BAMSRotation = new Vector3Int.Vec3Int(sr.ReadBE<int>(), sr.ReadBE<int>(), sr.ReadBE<int>());
                setEnemy.int_2C = sr.ReadBE<int>();
                setEnemy.int_30 = sr.ReadBE<int>();
                setEnemy.int_34 = sr.ReadBE<int>();
                setEnemy.int_38 = sr.ReadBE<int>();
                setEnemy.int_3C = sr.ReadBE<int>();
                setEnemy.int_40 = sr.ReadBE<int>();
                setEnemy.int_44 = sr.ReadBE<int>();
                setEnemy.int_48 = sr.ReadBE<int>();
                setEnemy.int_4C = sr.ReadBE<int>();
                setEnemy.int_50 = sr.ReadBE<int>();
                setEnemy.flt_54 = sr.ReadBE<float>();
                setEnemy.flt_58 = sr.ReadBE<float>();
                setEnemy.flt_5C = sr.ReadBE<float>();
                setEnemies.Add(setEnemy);
            }
        }

        public byte[] GetBytes()
        {
            List<byte> outBytes = new List<byte>();
            ByteListExtension.AddAsBigEndian = true;
            foreach (var setEne in setEnemies)
            {
                outBytes.AddValue(setEne.enemyId);
                outBytes.AddValue(setEne.Position);
                outBytes.AddValue(setEne.BAMSRotation.X);
                outBytes.AddValue(setEne.BAMSRotation.Y);
                outBytes.AddValue(setEne.BAMSRotation.Z);
                outBytes.AddValue(setEne.int_2C);
                outBytes.AddValue(setEne.int_30);
                outBytes.AddValue(setEne.int_34);
                outBytes.AddValue(setEne.int_38);
                outBytes.AddValue(setEne.int_3C);
                outBytes.AddValue(setEne.int_40);
                outBytes.AddValue(setEne.int_44);
                outBytes.AddValue(setEne.int_48);
                outBytes.AddValue(setEne.int_4C);
                outBytes.AddValue(setEne.int_50);
                outBytes.AddValue(setEne.flt_54);
                outBytes.AddValue(setEne.flt_58);
                outBytes.AddValue(setEne.flt_5C);
            }

            ByteListExtension.Reset();
            return outBytes.ToArray();
        }
    }

    public struct SetEnemy
    {
        public int enemyId;
        public Vector3 Position;
        public Vector3Int.Vec3Int BAMSRotation;
        public int int_2C;

        public int int_30;
        public int int_34;
        public int int_38;
        public int int_3C;

        public int int_40;
        public int int_44;
        public int int_48;
        public int int_4C;

        public int int_50;
        public float flt_54;
        public float flt_58;
        public float flt_5C;
    }
}
