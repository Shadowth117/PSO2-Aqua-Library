using AquaModelLibrary.Data.DataTypes;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.BillyHatcher.SetData
{
    public class SetObjList
    {
        public List<SetObj> setObjs = new List<SetObj>();
        public SetObjList() { }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr._BEReadActive = true;
            while(sr.Position + 0x40 < sr.BaseStream.Length)
            {
                SetObj setObj = new SetObj();
                setObj.objectId = sr.ReadBE<int>();
                setObj.Position = sr.ReadBEV3();
                setObj.BAMSRotation = new Vector3Int.Vec3Int(sr.ReadBE<int>(), sr.ReadBE<int>(), sr.ReadBE<int>());
                setObj.intProperty1 = sr.ReadBE<int>();
                setObj.intProperty2 = sr.ReadBE<int>();
                setObj.intProperty3 = sr.ReadBE<int>();
                setObj.intProperty4 = sr.ReadBE<int>();
                setObj.fltProperty1 = sr.ReadBE<float>();
                setObj.fltProperty2 = sr.ReadBE<float>();
                setObj.fltProperty3 = sr.ReadBE<float>();
                setObj.fltProperty4 = sr.ReadBE<float>();
                setObj.btProperty1 = sr.ReadBE<byte>();
                setObj.btProperty2 = sr.ReadBE<byte>();
                setObj.btProperty3 = sr.ReadBE<byte>();
                setObj.btProperty4 = sr.ReadBE<byte>();
                setObjs.Add(setObj);
            }
        }

        public byte[] GetBytes()
        {
            List<byte> outBytes = new List<byte>();
            ByteListExtension.AddAsBigEndian = true;
            foreach(var setObj in setObjs)
            {
                outBytes.AddValue(setObj.objectId);
                outBytes.AddValue(setObj.Position);
                outBytes.AddValue(setObj.BAMSRotation.X);
                outBytes.AddValue(setObj.BAMSRotation.Y);
                outBytes.AddValue(setObj.BAMSRotation.Z);
                outBytes.AddValue(setObj.intProperty1);
                outBytes.AddValue(setObj.intProperty2);
                outBytes.AddValue(setObj.intProperty3);
                outBytes.AddValue(setObj.intProperty4);
                outBytes.AddValue(setObj.fltProperty1);
                outBytes.AddValue(setObj.fltProperty2);
                outBytes.AddValue(setObj.fltProperty3);
                outBytes.AddValue(setObj.fltProperty4);
                outBytes.AddValue(setObj.btProperty1);
                outBytes.AddValue(setObj.btProperty2);
                outBytes.AddValue(setObj.btProperty3);
                outBytes.AddValue(setObj.btProperty4);
            }

            ByteListExtension.AddAsBigEndian = false;
            return outBytes.ToArray();
        }
    }
    /// <summary>
    /// For Billy Hatcher set_obj and set_design files
    /// </summary>
    public struct SetObj
    {
        public int objectId;
        public Vector3 Position;
        public Vector3Int.Vec3Int BAMSRotation;
        public int intProperty1;
        
        public int intProperty2;
        public int intProperty3;
        public int intProperty4;
        public float fltProperty1;

        public float fltProperty2;
        public float fltProperty3;
        public float fltProperty4;

        public byte btProperty1;
        public byte btProperty2;
        public byte btProperty3;
        public byte btProperty4;
    }
}
