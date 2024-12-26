using AquaModelLibrary.Data.DataTypes;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.BillyHatcher.SetData
{
    /// <summary>
    /// There are 2 versions of this. One has 0x50 long structures, one has 0x3C long structures. The latter isn't intentionally used for any final areas, though green_boss uses this layout. 
    /// However since green_boss has the boss first, the underwrite doesn't matter since the 2nd object's 0x14 of data doesn't affect the boss, seemingly.
    /// </summary>
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
                setEnemy.int_1C = sr.ReadBE<int>();
                setEnemy.int_20 = sr.ReadBE<int>();
                setEnemy.int_24 = sr.ReadBE<int>();
                setEnemy.int_28 = sr.ReadBE<int>();
                setEnemy.int_2C = sr.ReadBE<int>();
                setEnemy.int_30 = sr.ReadBE<int>();
                setEnemy.int_34 = sr.ReadBE<int>();
                setEnemy.int_38 = sr.ReadBE<int>();
                setEnemy.int_3C = sr.ReadBE<int>();
                setEnemy.flt_40 = sr.ReadBE<float>();
                setEnemy.flt_44 = sr.ReadBE<float>();
                setEnemy.flt_48 = sr.ReadBE<float>();
                setEnemy.flt_4C = sr.ReadBE<float>();
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
                outBytes.AddValue(setEne.int_1C);
                outBytes.AddValue(setEne.int_20);
                outBytes.AddValue(setEne.int_24);
                outBytes.AddValue(setEne.int_28);
                outBytes.AddValue(setEne.int_2C);
                outBytes.AddValue(setEne.int_30);
                outBytes.AddValue(setEne.int_34);
                outBytes.AddValue(setEne.int_38);
                outBytes.AddValue(setEne.int_3C);
                outBytes.AddValue(setEne.flt_40);
                outBytes.AddValue(setEne.flt_44);
                outBytes.AddValue(setEne.flt_48);
                outBytes.AddValue(setEne.flt_4C);
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
        public int int_1C;

        public int int_20;
        public int int_24;
        public int int_28;
        public int int_2C;

        public int int_30;
        public int int_34;
        public int int_38;
        public int int_3C;

        public float flt_40;
        public float flt_44;
        public float flt_48;
        public float flt_4C;
    }
}
