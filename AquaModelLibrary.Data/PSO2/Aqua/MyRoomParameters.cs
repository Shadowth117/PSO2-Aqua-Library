namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public class MyRoomParameters : AquaCommon
    {
        public List<RoomGoodsObject> roomGoodsList = new List<RoomGoodsObject>();
        public List<ChipObject> chipsList = new List<ChipObject>();

        public class RoomGoodsObject
        {
            public RoomGoods goods;
            public string categoryString = null;
            public string allString = null;
            public string categoryString2 = null;
            public string functionString = null;
            public string motionType = null;
            public string unknownString = null;
        }

        public struct RoomGoods
        {
            public int id;
            public int categoryPtr;
            public int int_08;
            public byte bt_0C;
            public byte bt_0D;
            public byte bt_0E;
            public byte bt_0F;

            public int int_10;
            public int allPtr;
            public int categoryPtr2;
            public int functionStrPtr;

            public int motionPtr;
            public int int_24;
            public int unknownPtr;
        }

        public class ChipObject
        {
            public Chip chip;
            public string objectString = null;
            public string unknownString = null;
            public string collisionTypeString = null;
        }

        public struct Chip
        {
            public int objectPtr;
            public int int_04;
            public int unkStringPtr;
            public int collisionTypePtr;

            //Collision shape params?
            public float flt_10;
            public float flt_14;
            public float flt_18;
        }
    }
}
