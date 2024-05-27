using AquaModelLibrary.Data.DataTypes;
using System.Numerics;

namespace AquaModelLibrary.Data.BillyHatcher.SetData
{
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
