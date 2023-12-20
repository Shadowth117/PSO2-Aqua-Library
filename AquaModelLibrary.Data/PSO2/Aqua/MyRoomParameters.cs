using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.AquaStructs
{
    public class MyRoomParameters : AquaCommon
    {
        public List<RoomGoodsObject> roomGoodsList = new List<RoomGoodsObject>();
        public List<ChipObject> chipsList = new List<ChipObject>();

        public class RoomGoodsObject
        {
            public RoomGoods goods;
            public string categoryString;
            public string allString;
            public string categoryString2;
            public string functionString;
            public string motionType;
            public string unknownString;
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
            public string objectString;
            public string unknownString;
            public string collisionTypeString;
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
